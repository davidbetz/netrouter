using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using Nalarium;
using Nalarium.Configuration;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Module))]
    [ModuleMetadata("SYSTEM")]
    public class SystemModule : Module
    {
        private static readonly object _ipPacketIdentifierLock = new object();

        private Map<string, Tuple<IPAddress, Timer, string>> _pendingArpMap = new Map<string, Tuple<IPAddress, Timer, string>>();
        private readonly Map<IPAddress, IPPacketPerDestinationIdentifier> _ipPacketIdentifierMap = new Map<IPAddress, IPPacketPerDestinationIdentifier>();
        private readonly Map<IPAddress, ArpData> _arpTable = new Map<IPAddress, ArpData>();

        public Map<IPAddress, Route> RouteList { get; set; }

        private IPPacketIdentifier CreateIPPacketIdentifier(IPHeader ipHeader)
        {
            lock (_ipPacketIdentifierLock)
            {
                if (!_ipPacketIdentifierMap.ContainsKey(ipHeader.DestinationAddress))
                {
                    _ipPacketIdentifierMap[ipHeader.DestinationAddress] = new IPPacketPerDestinationIdentifier();
                }
                IPPacketPerDestinationIdentifier destinationMap = _ipPacketIdentifierMap[ipHeader.DestinationAddress];
                var key = (ushort)(destinationMap.MaxKey + 1);
                if (key == 65536)
                {
                    key = 0;
                }
                destinationMap.MaxKey = key;

                IPPacketIdentifier id = IPPacketIdentifier.Create(key, ipHeader, this);
                destinationMap.Data.Add(key, id);

                return id;
            }
        }

        internal void KillIPPacketIdentifier(IPPacketIdentifier ipPacketIdentifier)
        {
            lock (_ipPacketIdentifierLock)
            {
                _ipPacketIdentifierMap[ipPacketIdentifier.Daddr].Data.Remove(ipPacketIdentifier.Key);
            }
        }

        internal void KillArpData(ArpData arpData)
        {
            lock (_arpTable)
            {
                Log.Write("SYSTEM", "ARP", "Timing out: " + arpData.IP);
                _arpTable.Remove(arpData.IP);
            }
        }

        protected override void Initialize()
        {
            var config = NetInterop.Routing.Configuration.SystemSection.GetConfigSection();
            foreach (var item in config.ArpCache)
            {
                ArpLookup(IPAddress.From(item.Address), new IPv4Handler().LayerID);
            }

        }

        public override void Install()
        {
            RouteList = new Map<IPAddress, Route>();
            Controller.RouteDiscovered += (s, e) =>
                                          {
                                              Route existing = RouteList.Where(p => p.Value.Network == e.Route.Network).OrderByDescending(p => p.Value.Mask.GetBitCount()).FirstOrDefault().Value;
                                              if (existing == null)
                                              {
                                                  RouteList.Add(e.Route.Network, e.Route);
                                                  Log.Write("SYSTEM", "RIB", "Add: " + e.Route.Network);
                                              }
                                              else
                                              {
                                                  if (e.Route.Mask.GetBitCount() > existing.Mask.GetBitCount())
                                                  {
                                                      //++ most specific mask means different network; so AD isn't even a competitor for it
                                                      RouteList[e.Route.Network] = e.Route;
                                                      Log.Write("SYSTEM", "RIB", "Replaced via bit count: " + e.Route.Network);
                                                  }
                                                  else if (e.Route.AdministrativeDistance > RouteList[e.Route.Network].AdministrativeDistance)
                                                  {
                                                      //++ new AD is better? OK
                                                      RouteList[e.Route.Network] = e.Route;
                                                      Log.Write("SYSTEM", "RIB", "Replaced via AD: " + e.Route.Network);
                                                  }
                                              }
                                          };
            Controller.RouteRemoved += (s, e) =>
                                       {
                                           if (!RouteList.ContainsKey(e.Route.Network))
                                           {
                                               return;
                                           }
                                           RouteList.Remove(e.Route.Network);
                                           Log.Write("SYSTEM", "RIB", "Removed: " + e.Route.Network);
                                       };
        }

        protected override void AcceptHandoff(string deviceID, string nextHandlerName, PacketData packetData, Value[] parameterArray)
        {
            if (!nextHandlerName.Equals("IPV4") && !nextHandlerName.Equals("IPV6"))
            {
                packetData = HandleLayer4Processing(Controller.GetSingletonHandler(nextHandlerName), packetData,
                                                    parameterArray, out nextHandlerName);
            }
            Handler layer3Handler = Controller.GetSingletonHandler(nextHandlerName);
            IPPacketIdentifier id;
            IPHeader baseIPHeader;
            List<PacketData> packetDataList = CreatePacketData(packetData, layer3Handler, parameterArray, out baseIPHeader, out id);
            if (packetDataList == null)
            {
                Log.Write("SYSTEM", "IPV6", "Do not fragment set.");
                return;
            }
            MacAddress destinationEthernetAddress;
            if (baseIPHeader.DestinationAddress.IsMulticast)
            {
                //++ this should only be for well known, I'd think
                destinationEthernetAddress = baseIPHeader.DestinationAddress.GetMacAddress();
            }
            else
            {
                var arpData = ArpLookup(baseIPHeader.DestinationAddress, layer3Handler.LayerID);
                if (arpData == null)
                {
                    Log.Write("ICMP", "ARPNOTFOUND", baseIPHeader.DestinationAddress.StandardFormat);
                    return;
                }
                destinationEthernetAddress = arpData.Mac;
            }
            foreach (var ipPacketData in packetDataList)
            {
                PackageLayer2AndSend(deviceID, Controller.DeviceConfigurationMap[deviceID].MacAddress, destinationEthernetAddress, layer3Handler.LayerID, ipPacketData, id.StartTimer);
            }
        }

        private List<PacketData> CreatePacketData(PacketData packetData, Handler layer3Handler, Value[] parameterArray, out IPHeader baseIPHeader, out IPPacketIdentifier id)
        {
            const int ipMtu = 1500;
            var list = new List<PacketData>();
            var ipHeader = (IPHeader)layer3Handler.Build(parameterArray);
            baseIPHeader = ipHeader;
            var headerLength = ipHeader.VersionIHL * 5;
            ipHeader.TotalLength = (ushort)((ipHeader.VersionIHL * 5) + packetData.Length);

            id = CreateIPPacketIdentifier(ipHeader);
            ipHeader.Identification = id.Key;

            var remainingData = packetData.Data;
            var totalTransmitCount = remainingData.Count;
            ushort fragmentOffset = 0;

            while (remainingData.Count > 0)
            {
                if (remainingData.Count + headerLength > ipMtu)
                {
                    var firstFragmentSize = ipMtu - ipHeader.InternetHeaderLength;
                    var blockCount = (int)Math.Floor((double)firstFragmentSize / 8);
                    var copyCount = blockCount * 8;
                    var pendingTransmitData = new List<byte>(remainingData.GetRange(0, copyCount));
                    remainingData = new List<byte>(remainingData.GetRange(copyCount, remainingData.Count - copyCount));
                    ipHeader.FlagsFragmentOffset = (ushort)((fragmentOffset >> 3) | 0x2000);
                    Log.Write("SYSTEM", "IPFRAGMENT", fragmentOffset.ToString());
                    fragmentOffset += (ushort)copyCount;
                    ipHeader.TotalLength = (ushort)((ipHeader.VersionIHL * 5) + copyCount);
                    //++ contains MF bit
                    //ipHeader.FlagsFragmentOffset = (ushort)(ipHeader.FlagsFragmentOffset << 3);
                    var segmentData = PacketData.Create(pendingTransmitData);
                    segmentData = layer3Handler.GetBytes(ipHeader, segmentData);
                    list.Add(segmentData);
                }
                else
                {
                    var pendingTransmitData = remainingData;
                    ipHeader.FlagsFragmentOffset = (ushort)(fragmentOffset >> 3);
                    //ipHeader.FlagsFragmentOffset = (ushort)(fragmentOffset);
                    Log.Write("SYSTEM", "IPFRAGMENT", fragmentOffset.ToString());
                    //ipHeader.FlagsFragmentOffset = (ushort)(ipHeader.FlagsFragmentOffset << 3);
                    ipHeader.TotalLength = (ushort)((ipHeader.VersionIHL * 5) + remainingData.Count);
                    var segmentData = PacketData.Create(pendingTransmitData);
                    segmentData = layer3Handler.GetBytes(ipHeader, segmentData);
                    list.Add(segmentData);
                    remainingData.Clear();
                }
            }
            return list;
        }

        private void PackageLayer2AndSend(String deviceID, MacAddress source, MacAddress destination, ushort layer3Type, PacketData data, Action callback)
        {
            byte[] layerIdByteArray = BitConverter.GetBytes(layer3Type);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(layerIdByteArray);
            }
            var ethernetHeader =
                    (EthernetHeader)Controller.GetSingletonHandler("ETHERNETII").Build(
                                        Value.Raw("EthernetHeader:Destination", destination),
                                        Value.Raw("EthernetHeader:Source", source),
                                        Value.Raw("EthernetHeader:TypeOrLength", TypeOrLength.From(layerIdByteArray)));
            var newData = GeneratePacketData(data, ethernetHeader);
            Log.Write("System", "Send", "length:" + newData.Length + ", from: " + destination.StandardFormat + ", to: " + source.StandardFormat);
            Send(deviceID, newData);
            callback();
        }

        internal void HandleArpResponse(object sender, ModuleEventArgs args)
        {
            var arpHeader = args.HeaderPackage.GetHeader<ArpHeader>();
            var result = _pendingArpMap.FirstOrDefault(p => p.Value.Item3.Equals(args.DeviceID) && p.Value.Item1 == arpHeader.SourceIPAddress);
            if (String.IsNullOrEmpty(result.Key))
            {
                return;
            }
            lock (_arpTable)
            {
                Log.Write("SYSTEM", "ARP", "Adding: " + arpHeader.SourceIPAddress.StandardFormat + ", " + arpHeader.SourceMacAddress.StandardFormat);
                _arpTable.Add(arpHeader.SourceIPAddress, ArpData.Create(result.Value.Item3, arpHeader.SourceIPAddress, arpHeader.SourceMacAddress, this));
            }
            _pendingArpMap.Remove(result.Key);
        }

        public ArpData ArpLookup(IPAddress ipAddress, ushort layer3ID)
        {
            lock (_arpTable)
            {
                if (_arpTable.ContainsKey(ipAddress))
                {
                    return _arpTable[ipAddress];
                }
                foreach (var deviceID in Controller.DeviceConfigurationMap.Keys)
                {
                    if (!Controller.DeviceConfigurationMap[deviceID].IsEnabled ||
                       Controller.DeviceConfigurationMap[deviceID].IsLoopback)
                    {
                        continue;
                    }
                    var guid = GuidCreator.GetNewGuid();
                    var arpHandler = Controller.GetSingletonHandler("ARP") as ArpHandler;
                    var arpHeader = arpHandler.Build(
                        Value.Raw("ArpHeader:HardwareType", (ushort)1),
                        Value.Raw("ArpHeader:ProtocolType", (ushort)layer3ID),

                        Value.Raw("ArpHeader:HardwareSize", (byte)6),
                        Value.Raw("ArpHeader:ProtocolSize", (byte)4),

                        Value.Raw("ArpHeader:Operation", (ushort)1),

                        Value.Raw("ArpHeader:SourceMacAddress", Controller.DeviceConfigurationMap[deviceID].MacAddress),
                        Value.Raw("ArpHeader:SourceIPAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address),
                        Value.Raw("ArpHeader:DestinationMacAddress", MacAddress.None),
                        Value.Raw("ArpHeader:DestinationIPAddress", ipAddress));

                    var packetData = GeneratePacketData(arpHeader);
                    DateTime then = DateTime.Now;
                    var timer = new Timer
                                {
                                    Interval = 1000
                                };
                    timer.Elapsed += (s, e) =>
                                     {
                                         if (DateTime.Now - then > TimeSpan.FromSeconds(5))
                                         {
                                             _pendingArpMap.Remove(guid);
                                         }
                                     };
                    _pendingArpMap.Add(guid, new Tuple<IPAddress, Timer, string>(ipAddress, timer, deviceID));
                    PackageLayer2AndSend(deviceID, Controller.DeviceConfigurationMap[deviceID].MacAddress, MacAddress.Broadcast, new ArpHandler().LayerID, packetData, timer.Start);
                    Log.Write("SYSTEM", "ARP", deviceID + "," + ipAddress.StandardFormat);
                }
                return null;
            }
        }

        private PacketData HandleLayer4Processing(Handler nextHandler, PacketData packetData, Value[] parameterArray, out String layer3HandlerName)
        {
            layer3HandlerName = parameterArray.Any(p => p.Name.StartsWith("IPv6Header:")) ? "IPV6" : "IPV4";
            IHeader header = nextHandler.Build(parameterArray);
            return GeneratePacketData(packetData, header);
        }

        #region ICMP

        #endregion
    }
}
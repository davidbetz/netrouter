using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using Nalarium;
using NetInterop.Routing.Core;
using NetInterop.Routing.Rip.Configuration;

namespace NetInterop.Routing.Rip
{
    [Export(typeof(Module))]
    [ModuleMetadata("RIP")]
    public class RipModule : Module
    {
        [ImportMany]
        private List<Handler> _parserList;

        private Timer _timer;
        public byte Version { get; set; }

        public Map<IPAddress, RipRoute> RipDatabase { get; set; }

        public Map<string, List<Tuple<IPAddress, IPAddress>>> RoutingNetworkMap { get; set; }

        public event ModuleEvent RipEntryRemove = delegate
                                                  {
                                                  };

        protected override void SetupInterfaceData()
        {
            RoutingNetworkMap = new Map<string, List<Tuple<IPAddress, IPAddress>>>();
            RipDatabase = new Map<IPAddress, RipRoute>();
            foreach (NetworkElement networkData in RipSection.GetConfigSection().Networks)
            {
                IPAddress network = IPAddress.From(networkData.Network);
                IPAddress networkMask = network.GetClassMask();
                IPAddress classedNetwork = network.GetClassAddress();
                //++ create interface candidate list
                List<string> list = GetInterfaceListForAddress(classedNetwork, allowConnectionOverSeconaryIP: true);
                if (Collection.IsNullOrEmpty(list))
                {
                    continue;
                }
                //++ using the candidates as a basis, save each network worthy of cataloging
                foreach (string item in list)
                {
                    if (!RoutingNetworkMap.ContainsKey(item))
                    {
                        RoutingNetworkMap.Add(item, new List<Tuple<IPAddress, IPAddress>>());
                    }
                    Device device = Controller.DeviceConfigurationMap[item];
                    if (device.PrimaryIPConfiguration.Address.IsSameNetwork(network, networkMask))
                    {
                        RoutingNetworkMap[item].Add(new Tuple<IPAddress, IPAddress>(device.PrimaryIPConfiguration.Address.GetNetwork(device.PrimaryIPConfiguration.Mask), device.PrimaryIPConfiguration.Mask));
                    }
                    foreach (IPConfiguration ipData in device.SecondaryIPList)
                    {
                        if (device.PrimaryIPConfiguration.Address.IsSameNetwork(network, networkMask))
                        {
                            RoutingNetworkMap[item].Add(new Tuple<IPAddress, IPAddress>(ipData.Network, ipData.Mask));
                        }
                    }
                    ActiveInterfaceList.Add(item);
                }
            }
        }

        protected override void Initialize()
        {
            RipDatabase = new Map<IPAddress, RipRoute>();
            foreach (string deviceID in RoutingNetworkMap.Keys)
            {
                List<Tuple<IPAddress, IPAddress>> networkList = RoutingNetworkMap[deviceID];

                foreach (var network in networkList)
                {
                    RipRoute entry = RipRoute.Create(IPAddress.GetDefault(),
                                                     new RipDataHeader
                                                     {
                                                         AddressFamily = 2,
                                                         Metric = 1,
                                                         Mask = network.Item2,
                                                         Network = network.Item1,
                                                         NextHop = IPAddress.GetDefault(),
                                                         RouteTag = 0
                                                     },
                                                     deviceID,
                                                     isLocal: true
                        );
                    RipDatabase.Add(network.Item1, entry);
                    Controller.RegisterRoute(this, entry);
                }
            }
            RipSection config = RipSection.GetConfigSection();
            Version = config.Version;
            _timer = new Timer
                     {
                         Interval = config.Update * 1000
                     };
            _timer.Elapsed += (s, e) => SendFlood();
            _timer.Start();

            SendRequest();
            SendFlood();
        }

        internal void MessageReceived(object sender, ModuleEventArgs args)
        {
            HeaderPackage package = args.HeaderPackage;
            var ripPreambleHeader = package.GetHeader<RipPreambleHeader>();
            var ripDataHeaderArray = package.GetHeader<RipDataHeader[]>();
            var ipHeader = package.GetHeader<IPHeader>();
            if (ripPreambleHeader.command == (byte)RipCommand.Request)
            {
                HandleRequest(args.DeviceID, ripPreambleHeader, ripDataHeaderArray);
            }
            else if (ripPreambleHeader.command == (byte)RipCommand.Response)
            {
                HandleResponse(args.DeviceID, ripPreambleHeader, ripDataHeaderArray, ipHeader);
            }
            else
            {
                throw new InvalidOperationException("Invalid RIP command. Probably a bad packet.");
            }
        }

        private void HandleResponse(string deviceID, RipPreambleHeader ripPreambleHeader, RipDataHeader[] ripDataHeaderArray, IPHeader ipHeader)
        {
            foreach (RipDataHeader ripDataHeader in ripDataHeaderArray)
            {
                if (RipDatabase.ContainsKey(ripDataHeader.Network))
                {
                    RipRoute route = RipDatabase[ripDataHeader.Network];
                    if (ripDataHeader.Metric < route.Metric)
                    {
                        RipDatabase[ripDataHeader.Network].UpdateRipHeaderData(ipHeader.SourceAddress, ripDataHeader);
                    }
                    else if (ripDataHeader.Metric > route.Metric && ripDataHeader.NextHop == ipHeader.SourceAddress)
                    {
                        if (route.AcceptPotentialNewMetricOnNextSighting &&
                            route.PotentialNewMetric == ripDataHeader.Metric)
                        {
                            route.AcceptPotentialNewMetric();
                        }
                        else
                        {
                            route.BeginHoldDownTime(ripDataHeader);
                        }
                    }
                    route.TimeUpdated = DateTime.Now;
                    route.RaiseEvent(RipEvent.Sighted);
                }
                else
                {
                    RipRoute route = RipRoute.Create(ipHeader.SourceAddress, ripDataHeader, deviceID);
                    route.GCCollect += (s, e) =>
                                       {
                                           RipDatabase.Remove((s as RipRoute).Network);
                                           Controller.RemoveRoute(this, route);
                                       };
                    Log.Write("RIP", "RipDatabaseAdd", ripDataHeader.Network.StandardFormat);
                    RipDatabase.Add(ripDataHeader.Network, route);
                    Controller.RegisterRoute(this, route);
                }
            }
        }

        private void HandleRequest(string deviceID, RipPreambleHeader ripPreambleHeader, RipDataHeader[] ripDataHeaderArray)
        {
            //TODO: does rip flood to all interfaces upon requst from one??
            SendFlood(deviceID);
        }

        private void SendFlood()
        {
            foreach (string deviceID in ActiveInterfaceList)
            {
                if (Controller.DeviceConfigurationMap[deviceID].IsLoopback)
                {
                    continue;
                }
                SendFlood(deviceID);
            }
        }

        private void SendFlood(string deviceID)
        {
            RipDataHeader[] data = RipDatabase.Where(p => !p.Value.Interface.Equals(deviceID)).Select(p => p.Value.GetRipDataHeader(1)).ToArray();
            Log.Write("RIP", "SendingFlood", "Count:" + data.Length.ToString());
            //++ parsers build the header
            var ripPreambleHeader = (RipPreambleHeader)Controller.GetSingletonHandler("RIP").Build(
                Value.Raw("RipPreambleHeader:Command", (byte)RipCommand.Response),
                Value.Raw("RipPreambleHeader:Version", Version),
                Value.Raw("RipPreambleHeader:DataArray", data));

            PacketData packetData = GeneratePacketData(ripPreambleHeader);

            //++ send to system module for ip packaging
            Handoff(deviceID, typeof(UdpHeader), packetData,
                    Value.Raw("UdpHeader:SourcePort", (ushort)520),
                    Value.Raw("UdpHeader:DestinationPort", (ushort)520),
                    Value.Raw("IPHeader:Protocol", (byte)17),
                    Value.Raw("IPHeader:TypeOfService", (byte)0xc0),
                    Value.Raw("IPHeader:TTL", (byte)2),
                    Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address),
                    Value.Raw("IPHeader:DestinationAddress", GetDestinationAddress()));
        }

        private void SendRequest()
        {
            foreach (string deviceID in ActiveInterfaceList)
            {
                if (Controller.DeviceConfigurationMap[deviceID].IsLoopback)
                {
                    continue;
                }
                Log.Write("RIP", "SendingRequest");
                //++ parsers build the header
                var ripPreambleHeader = (RipPreambleHeader)Controller.GetSingletonHandler("RIP").Build(
                    Value.Raw("RipPreambleHeader", "Command", (byte)RipCommand.Request),
                    Value.Raw("RipPreambleHeader", "Version", Version),
                    Value.Raw("RipDataHeader", "AddressFamily", (ushort)0),
                    Value.Raw("RipDataHeader", "Metric", (uint)16));

                PacketData packetData = GeneratePacketData(ripPreambleHeader);

                //++ send to system module for ip packaging
                Handoff(deviceID, typeof(UdpHeader), packetData,
                        Value.Raw("UdpHeader:SourcePort", (ushort)520),
                        Value.Raw("UdpHeader:DestinationPort", (ushort)520),
                        Value.Raw("IPHeader:Protocol", (byte)17),
                        Value.Raw("IPHeader:TypeOfService", (byte)0xc0),
                        Value.Raw("IPHeader:TTL", (byte)2),
                        Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address),
                        Value.Raw("IPHeader:DestinationAddress", GetDestinationAddress()));
            }
        }

        private IPAddress GetDestinationAddress()
        {
            if (Version == 2)
            {
                return IPAddress.From("224.0.0.9");
            }
            else
            {
                return IPAddress.From("255.255.255.255");
            }
        }

        internal void RequestReceived(string deviceID)
        {
        }
    }
}
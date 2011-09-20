using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;
using NetInterop.Routing.Core;
using NetInterop.Routing.Ospf.Configuration;

namespace NetInterop.Routing.Ospf
{
    [Export(typeof(Module))]
    [ModuleMetadata("OSPF")]
    public class OspfModule : Module
    {
        public const byte TOS = 0xc0;

        [ImportMany]
        private List<Handler> _parserList;

        public IPAddress RouterID { get; set; }
        private Map<uint, Area> AreaMap { get; set; }
        public Map<IPAddress, IPAddress> VirtualLinkMap { get; set; }
        public Map<IPAddress, Route> ExternalRouteMap { get; set; }
        public Map<IPAddress, Route> ASExternalRouteMap { get; set; }
        public Map<string, OspfLsaExternalHeader> ASExternalLSAMap { get; set; }

        //public int DDSequenceNumber { get; set; }

        private Map<string, Interface> OspfInterfaceMap { get; set; }

        public RouterLSAOptions RouterLSAOptions
        {
            get
            {
                var options = new RouterLSAOptions();
                if (AreaMap.Any(p => p.Key == 0) && AreaMap.Any(p => p.Key != 0))
                {
                    options |= RouterLSAOptions.Abr;
                }
                if (ASExternalLSAMap.Count > 0)
                {
                    options |= RouterLSAOptions.Asbr;
                }
                if (VirtualLinkMap.Count > 0)
                {
                    options |= RouterLSAOptions.VirtualEndpoint;
                }
                return options;
            }
        }

        //internal Stack<Tuple<IPHeader, ospfHeader, ospfHelloHeader>> HelloMessageList { get; set; }

        protected override void SetupInterfaceData()
        {
            ASExternalLSAMap = new Map<string, OspfLsaExternalHeader>();
            VirtualLinkMap = new Map<IPAddress, IPAddress>();
            AreaMap = new Map<uint, Area>();
            AreaMap.Add(0, Area.Create(this, Controller, 0));
            OspfInterfaceMap = new Map<string, Interface>();
            //+
            var networkInterfaceMap = new Map<string, IPConfiguration>();
            //+
            OspfSection config = OspfSection.GetConfigSection();
            foreach (NetworkElement networkData in config.Networks)
            {
                //++ get broad list of interfaces by networks
                IPAddress network = IPAddress.From(networkData.Network);
                IPAddress wildcardMask = IPAddress.From(networkData.Mask).FlipBits();
                List<string> deviceIDList = GetInterfaceListForAddress(network, wildcardMask)
                    .Where(p => Controller.DeviceConfigurationMap[p].IsEnabled)
                    .Where(p =>
                           {
                               if (config.Interfaces.PassiveDefault)
                               {
                                   if (config.Interfaces.Any(d => d.IsPassive))
                                   {
                                       return false;
                                   }
                                   else if (!config.Interfaces.Any(d => d.IsPassive))
                                   {
                                       return true;
                                   }
                               }
                               return !config.Interfaces.Any(d => d.IsPassive);
                           }).ToList();
                if (Collection.IsNullOrEmpty(deviceIDList))
                {
                    continue;
                }
                deviceIDList
                    .Select(deviceID => new
                                        {
                                            device = Controller.DeviceConfigurationMap[deviceID],
                                            deviceID
                                        })
                    .ToList()
                    .ForEach(p => networkInterfaceMap.Add(p.deviceID, IPConfiguration.Create(
                        p.device.PrimaryIPConfiguration.Address,
                        p.device.PrimaryIPConfiguration.Mask)));
                ActiveInterfaceList.AddRange(deviceIDList.Where(p => !ActiveInterfaceList.Contains(p)));
            }
            foreach (string deviceID in networkInterfaceMap.Keys)
            {
                IPConfiguration ipConfiguration = networkInterfaceMap[deviceID];
                //++ find matching "network commands"
                List<NetworkElement> networkElementList = config.Networks.
                    Select(networkData => new { main = networkData, mask = IPAddress.From(networkData.Mask).FlipBits() })
                    .Where(networkData =>
                        IPAddress.GetNetwork(IPAddress.From(networkData.main.Network), networkData.mask)
                            .IsSameNetwork(ipConfiguration.Address, networkData.mask)
                    )
                    .Select(p => p.main)
                    .ToList();
                if (networkElementList.Count == 0)
                {
                    continue;
                }
                //++ find most specific maask;
                //++    inverted mask, so look for weakest here; that would be the strongest wildard mask
                NetworkElement networkElement = networkElementList.OrderBy(p => IPAddress.From(p.Mask)).First();
                //++ normalize area style
                uint areaID = GetArea(networkElement.Area);
                //++ ensure area exists
                if (!AreaMap.ContainsKey(areaID))
                {
                    AreaMap.Add(areaID, Area.Create(this, Controller, areaID));
                }
                Area area = AreaMap[areaID];
                //++ create interface
                InterfaceElement interfaceConfig = config.Interfaces.FirstOrDefault(p => p.DeviceID.Equals(deviceID, StringComparison.InvariantCultureIgnoreCase))
                    ?? new InterfaceElement
                    {
                    };
                Interface ospfInterface = Interface.Create(this, Controller, deviceID, area, interfaceConfig);
                Controller.DeviceConfigurationMap[deviceID].StateChanged += (s, e) =>
                                                                            {
                                                                                //++ ospf is only usable when the lower-level interface has a state of full
                                                                                if (e.InterfaceState == Routing.InterfaceState.L2UpL3Up)
                                                                                {
                                                                                    //++ per-state machine, only do this when in the down state
                                                                                    if (ospfInterface.InterfaceState == InterfaceState.Down)
                                                                                    {
                                                                                        ospfInterface.RaiseEvent(InterfaceEventType.InterfaceUp);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    ospfInterface.RaiseEvent(InterfaceEventType.InterfaceDown);
                                                                                }
                                                                            };
                area.InterfaceList.Add(ospfInterface);
                OspfInterfaceMap.Add(deviceID, ospfInterface);
            }
        }

        private uint GetArea(string area)
        {
            return area.Contains(".") ? IPAddress.From(area).GetUInt32() : Parser.ParseUInt32(area);
        }

        protected override void Initialize()
        {
            OspfSection config = OspfSection.GetConfigSection();
            //++ find router-id
            Device loopback = Controller.LoopbackMap.OrderBy(p => p.Value.PrimaryIPConfiguration.Address).FirstOrDefault().Value;
            RouterID = loopback != null
                           ? loopback.PrimaryIPConfiguration.Address
                           : Controller.DeviceConfigurationMap.OrderBy(p => p.Value.PrimaryIPConfiguration.Address).FirstOrDefault().Value.PrimaryIPConfiguration.Address;
            OspfInterfaceMap.GetValueList().ForEach(p => p.Init());
            foreach (uint key in AreaMap.Keys)
            {
                Area area = AreaMap[key];
                AreaElement areaConfig = config.Areas.FirstOrDefault(p => Parser.ParseInt32(p.Area) == area.Number);
                if (areaConfig != null)
                {
                    if (areaConfig.AreaType == AreaType.Stub)
                    {
                        area.ExternalRoutingCapability = false;
                    }
                }
                area.InitLSBD();
            }
            foreach (string deviceID in OspfInterfaceMap.Keys)
            {
                //++ is not administratively down
                if (Controller.DeviceConfigurationMap[deviceID].IsEnabled)
                {
                    OspfInterfaceMap[deviceID].RaiseEvent(InterfaceEventType.InterfaceUp);
                }
            }
            //NeighborMap = new Map<IPAddress, Neighbor>();
            //HelloMessageList = new Stack<Tuple<IPHeader, ospfHeader, ospfHelloHeader>>();
            //Options |= OspfHelloOptions.ContainsLLSDataBlock | OspfHelloOptions.ExternalRoutingCapable;

            //_timer = new Timer
            //{
            //    Interval = HelloInterval * 1000
            //};
            //_timer.Elapsed += (s, e) => SendHello();

            //OspfInterfaceMap = new OspfInterface
            //{
            //    WaitTimerLength = RouterDeadInterval
            //};
            //OspfInterfaceMap.InterfaceUp += (s, e) => _timer.Start();
            //OspfInterfaceMap.RaiseEvent(InterfaceEventType.InterfaceUp);
            //SendHello();
        }

        //public void ContinueSendDBD(string deviceID, IPAddress saddr, OspfDBDHeader ospfDBDHeader)
        //    throw new NotImplementedException();
        //{
        //}
        internal void ContinueSend(string deviceID, IPAddress destinationAddress, params object[] parameterArray)
        {
            parameterArray = parameterArray ?? new object[] { };
            var valueArray = parameterArray.Where(p => p is Value).Cast<Value>().ToArray();
            var headerArray = parameterArray.Where(p => p is IHeader).Cast<IHeader>().ToArray();
            //++ parsers build the header
            var ospfHeader = (OspfHeader)Controller.GetSingletonHandler("OSPF").Build(valueArray);

            var headerList = new List<IHeader>(headerArray);
            headerList.Add(ospfHeader);
            PacketData packetData = GeneratePacketData(headerList.ToArray());

            //++ send to system module for ip packaging
            Handoff(deviceID, typeof(IPHeader), packetData,
                    Value.Raw("IPHeader:TypeOfService", (byte)0xc0),
                    Value.Raw("IPHeader:TTL", (byte)1),
                    Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address),
                    Value.Raw("IPHeader:DestinationAddress", destinationAddress),
                    Value.Raw("IPHeader:Protocol", (byte)89));
        }

        public void HelloReceived(Object sender, ModuleEventArgs args)
        {
            OspfInterfaceMap[args.DeviceID].HelloReceived(args);
        }

        public void DBDReceived(Object sender, ModuleEventArgs args)
        {
            OspfInterfaceMap[args.DeviceID].DBDReceived(args);
        }

        #region ProcessNewHello

        //private void HandleHelloMessage(IPHeader ipHeader, OspfHeader ospfHeader, OspfHelloHeader ospfHelloHeader)
        //{
        //    return;
        //    Neighbor neighbor = NeighborMap[ospfHeader.rid] ?? Neighbor.Create(ipHeader, ospfHeader, ospfHelloHeader);
        //    NeighborEventArgs args = NeighborEventArgs.Create(ipHeader, ospfHeader, ospfHelloHeader);
        //    neighbor.RaiseEvent(NeighborEventType.HelloReceived, args);
        //    if (ospfHelloHeader.neighbor.Any(p => p == RouterID))
        //    {
        //        neighbor.RaiseEvent(NeighborEventType.TwoWayReceived, args);
        //    }
        //    if (neighbor.IsNew && IsPotentialNeighbor(ipHeader, ospfHeader, ospfHelloHeader))
        //    {
        //        NeighborMap.Add(neighbor.RouterId, neighbor);
        //        neighbor.IsNew = false;
        //    }
        //    else
        //    {
        //        Log.Write("OSPF", "HELLO", string.Format("Incompatible OSPF hello message from .", ipHeader.saddr.StandardFormat));
        //    }
        //}

        private bool IsPotentialNeighbor(IPHeader ipHeader, OspfHeader ospfHeader, OspfHelloHeader ospfHelloHeader)
        {
            //if (!ospfHeader.aid.Equals(Area))
            //{
            //    return false;
            //}
            //if (ospfHelloHeader.interval != HelloInterval)
            //{
            //    return false;
            //}
            //if (ospfHelloHeader.routerdeadintvl != RouterDeadInterval)
            //{
            //    return false;
            //}
            //if (((ospfHelloHeader.Options & OspfHelloOptions.ExternalRoutingCapable) == OspfHelloOptions.ExternalRoutingCapable) == IsStub)
            //{
            //    return false;
            //}
            return true;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    internal class Area
    {
        private RoutingController _controller;

        internal OspfModule Module { get; set; }

        private Area()
        {
            InterfaceList = new List<Interface>();
            ExternalRoutingCapability = true;
            RouterLSAMap = new Map<string, OspfLsaRouterHeader>();
            NetworkLSAMap = new Map<string, OspfLsaNetworkHeader>();
            SummaryLSAMap = new Map<string, OspfLsaSummaryHeader>();
        }

        public Map<IPAddress, string> LSDB { get; set; }

        public uint Number { get; private set; }

        public List<IPConfiguration> RangeList { get; private set; }

        public List<Interface> InterfaceList { get; private set; }

        public Map<string, OspfLsaRouterHeader> RouterLSAMap { get; private set; }

        public Map<string, OspfLsaNetworkHeader> NetworkLSAMap { get; private set; }

        public Map<string, OspfLsaSummaryHeader> SummaryLSAMap { get; private set; }

        public bool TransitCapability { get; private set; }

        public bool ExternalRoutingCapability { get; set; }

        public ushort StubCost { get; private set; }

        public void InitLSBD()
        {
            //+ local-router LSA
            var header = new OspfLsaHeader
            {
                LSAge = 0,
                AdvertisingRouter = Module.RouterID,
                Options = (byte)GetOptions(supportOBit: true, removeLLSDataBlock: true),
                OspfLsaType = OspfLsaType.Route,
                LSID = Module.RouterID,
                SequenceNumber = Constant.InitialSequenceNumber
            };
            var routerLsa = new OspfLsaRouterHeader
            {
                CommonHeader = header,
                RouterLSAOptions = Module.RouterLSAOptions,
                LinkCount = (ushort)InterfaceList.Count
            };
            RouterLSAMap.Add(header.Key, routerLsa);
            var lsaLinkList = new List<OspfLsaRouterLinkHeader>();
            foreach (var interfc in InterfaceList)
            {
                var link = new OspfLsaRouterLinkHeader
                {
                    TypeOfService = 0,
                    Metric = 1
                };
                if (_controller.DeviceConfigurationMap[interfc.DeviceID].IsLoopback)
                {
                    link.OspfLsaLinkType = OspfLsaLinkType.Stub;
                }
                else
                {
                    link.OspfLsaLinkType = OspfLsaLinkType.Transit;
                }
                lsaLinkList.Add(link);
                switch (link.OspfLsaLinkType)
                {
                    case OspfLsaLinkType.PointToPoint:
                        link.LinkID = interfc.RemoteRouterID;
                        link.LinkData = interfc.IPAddress;
                        //TODO: something about unnumbered here, p207 "
                        break;
                    case OspfLsaLinkType.Transit:
                        link.LinkID = interfc.DR;
                        link.LinkData = interfc.IPAddress;
                        break;
                    case OspfLsaLinkType.Stub:
                        link.LinkID = interfc.IPAddress.GetNetwork(interfc.Mask);
                        link.LinkData = interfc.Mask;
                        break;
                    case OspfLsaLinkType.VirtualLink:
                        link.LinkID = interfc.RemoteRouterID;
                        link.LinkData = interfc.IPAddress;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            routerLsa.LinkList = lsaLinkList;
        }

        public OspfOptions GetOptions(Boolean supportOBit = false, Boolean removeLLSDataBlock = false)
        {
            var options = new OspfOptions();
            if (!removeLLSDataBlock)
            {
                options |= OspfOptions.ContainsLLSDataBlock;
            }
            if (supportOBit)
            {
                options |= OspfOptions.O;
            }
            if (ExternalRoutingCapability)
            {
                options |= OspfOptions.ExternalRoutingCapable;
            }
            return options;
        }

        public static Area Create(OspfModule module, RoutingController controller, uint number)
        {
            return new Area
                   {
                       Module = module,
                       _controller = controller,
                       Number = number
                   };
        }
    }
}
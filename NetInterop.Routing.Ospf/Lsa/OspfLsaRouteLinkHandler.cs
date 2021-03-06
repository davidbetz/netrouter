using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSAROUTELINK", "OSPFLSAROUTE")]
    public class OspfLsaRouteLinkHandler : Handler
    {
        protected override Boolean CheckForNext()
        {
            return CheckForIteration(OspfLsaRouteHandler.CurrentLinkIndexProperty, OspfLsaRouteHandler.LinkCountProperty);
        }

        public override Handler Parse()
        {
            var header = new OspfLsaRouterLinkHeader();
            header.LinkID = LoadAndScroll<IPAddress>();
            header.LinkData = LoadAndScroll<IPAddress>();
            header.Type = LoadAndScroll<Byte>();
            header.TypeOfService = LoadAndScroll<Byte>();
            header.Metric = LoadUInt16ReversingEndian();

            GetValue<OspfLsaRouterHeader>(OspfLsaRouteHandler.LsaRouteHeaderProperty).LinkList.Add(header);

            return GetNextHandler();
        }
    }
}
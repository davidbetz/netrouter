using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSAROUTE", "OSPFLSA")]
    public class OspfLsaRouteHandler : Handler
    {
        public static GlobalProperty LsaRouteHeaderProperty = GlobalProperty.Register("LsaRouteHeader",
                                                                                      typeof(OspfLsaRouterHeader),
                                                                                      typeof(OspfLsaRouteHandler));

        public static GlobalProperty CurrentLinkIndexProperty = GlobalProperty.Register("CurrentLinkIndex",
                                                                                        typeof(int),
                                                                                        typeof(OspfLsaRouteHandler),
                                                                                        new GlobalPropertyMetadata(
                                                                                            GlobalPropertyMetadataOptions.InternalUseOnly));

        public static GlobalProperty LinkCountProperty = GlobalProperty.Register("LinkCount",
                                                                                 typeof(int),
                                                                                 typeof(OspfLsaRouteHandler),
                                                                                 new GlobalPropertyMetadata(
                                                                                     GlobalPropertyMetadataOptions.InternalUseOnly));

        protected override Boolean CheckForNext()
        {
            return GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty).OspfLsaType == OspfLsaType.Route;
        }

        public override Handler Parse()
        {
            var header = new OspfLsaRouterHeader();
            header.Options = LoadAndScroll<Byte>();
            Scroll<Byte>();
            header.LinkCount = LoadUInt16ReversingEndian();
            header.CommonHeader = GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty);
            header.LinkList = new List<OspfLsaRouterLinkHeader>();

            SetValue(LsaRouteHeaderProperty, header);

            if (header.LinkCount > 0)
            {
                return DeclareInteration(LinkCountProperty, header.LinkCount, CurrentLinkIndexProperty);
            }

            return GetNextHandler();
        }
    }
}
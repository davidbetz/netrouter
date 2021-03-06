using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSAEXTERNAL", "OSPFLSA")]
    public class OfpsLsaExternalHandler : Handler
    {
        public static GlobalProperty LsaExternalHeaderProperty = GlobalProperty.Register("LsaExternalHeader",
                                                                                         typeof(OspfLsaExternalHeader),
                                                                                         typeof(OfpsLsaExternalHandler));

        protected override Boolean CheckForNext()
        {
            return GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty).OspfLsaType == OspfLsaType.External;
        }

        public override Handler Parse()
        {
            var header = new OspfLsaExternalHeader();
            header.NetworkMask = LoadUInt32ReversingEndian();
            header.Options1 = LoadAndScroll<Byte>();
            header.Metric1 = LoadAndScroll<Byte>();
            header.Metric2 = LoadAndScroll<Byte>();
            header.Metric3 = LoadAndScroll<Byte>();
            header.ForwardingAddress1 = LoadUInt32ReversingEndian();
            header.ExternalRouteTag1 = LoadUInt32ReversingEndian();
            header.Options2 = LoadAndScroll<Byte>();
            header.TypeOfService1 = LoadAndScroll<Byte>();
            header.TypeOfService2 = LoadAndScroll<Byte>();
            header.TypeOfService3 = LoadAndScroll<Byte>();
            header.ForwardingAddress2 = LoadUInt32ReversingEndian();
            header.ExternalRouteTag2 = LoadUInt32ReversingEndian();
            header.CommonHeader = GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty);

            SetValue(LsaExternalHeaderProperty, header);

            return GetNextHandler();
        }
    }
}
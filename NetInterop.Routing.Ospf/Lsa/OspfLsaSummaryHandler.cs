using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSASUMMARY", "OSPFLSA")]
    public class OspfLsaSummaryHandler : Handler
    {
        public static GlobalProperty LsaSummaryHeaderProperty = GlobalProperty.Register("LsaSummaryHeader",
                                                                                        typeof(OspfLsaSummaryHeader),
                                                                                        typeof(OspfLsaSummaryHandler));

        protected override Boolean CheckForNext()
        {
            OspfLsaType type = GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty).OspfLsaType;
            return type == OspfLsaType.SummaryNetwork || type == OspfLsaType.SummaryAsbr;
        }

        public override Handler Parse()
        {
            var header = new OspfLsaSummaryHeader();
            header.NetworkMask = LoadUInt32ReversingEndian();
            Scroll<Byte>();
            header.Metric1 = LoadAndScroll<Byte>();
            header.Metric2 = LoadAndScroll<Byte>();
            header.Metric3 = LoadAndScroll<Byte>();
            header.TypeOfService = LoadAndScroll<Byte>();
            header.TypeOfServiceMetric1 = LoadAndScroll<Byte>();
            header.TypeOfServiceMetric2 = LoadAndScroll<Byte>();
            header.TypeOfServiceMetric3 = LoadAndScroll<Byte>();
            header.CommonHeader = GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty);

            SetValue(LsaSummaryHeaderProperty, header);

            return GetNextHandler();
        }
    }
}
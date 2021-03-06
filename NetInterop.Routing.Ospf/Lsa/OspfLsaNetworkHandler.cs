using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSANETWORK", "OSPFLSA")]
    public class OspfLsaNetworkHandler : Handler
    {
        public static GlobalProperty LsaNetworkHeaderProperty = GlobalProperty.Register("LsaNetworkHeader",
                                                                                        typeof(OspfLsaNetworkHeader),
                                                                                        typeof(OspfLsaNetworkHandler));

        protected override Boolean CheckForNext()
        {
            return GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty).OspfLsaType == OspfLsaType.Network;
        }

        public override Handler Parse()
        {
            var header = new OspfLsaNetworkHeader();
            header.NetworkMask = LoadUInt32ReversingEndian();
            header.AttachedRouter = LoadUInt32ReversingEndian();
            header.CommonHeader = GetValue<OspfLsaHeader>(OspfLsaCommonHandler.LsaCommonHeaderProperty);

            SetValue(LsaNetworkHeaderProperty, header);

            return GetNextHandler();
        }
    }
}
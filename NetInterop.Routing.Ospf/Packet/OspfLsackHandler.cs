using System.ComponentModel.Composition;
using NetInterop.Routing.Ospf.Lsa;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSACK", "OSPF")]
    public class OspfLsackHandler : Handler
    {
        public static GlobalProperty CurrentLinkIndexProperty = GlobalProperty.Register("CurrentLinkIndex", typeof(int),
                                                                                        typeof(OspfLsackHandler),
                                                                                        new GlobalPropertyMetadata(
                                                                                            GlobalPropertyMetadataOptions.InternalUseOnly));

        public static GlobalProperty LinkCountProperty = GlobalProperty.Register("LinkCount", typeof(int),
                                                                                 typeof(OspfLsackHandler),
                                                                                 new GlobalPropertyMetadata(
                                                                                     GlobalPropertyMetadataOptions.InternalUseOnly));

        protected override bool CheckForNext()
        {
            return GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType == OspfPacketType.LinkStateAcknowledgement;
        }

        public override Handler Parse()
        {
            //int lsaHeaderSpace = OspfHandler.OspfHeaderProperty.Size;
            //int lsaHeaderCount = lsaHeaderSpace / OspfLsaCommonHandler.LsaCommonHeaderProperty.Size;

            //return DeclareInteration(LinkCountProperty, lsaHeaderCount, CurrentLinkIndexProperty);
            return null;
        }
    }
}
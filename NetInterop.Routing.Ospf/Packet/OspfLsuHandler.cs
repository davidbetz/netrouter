using System.ComponentModel.Composition;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSU", "OSPF")]
    public class OspfLsuHandler : Handler
    {
        public static GlobalProperty LsuHeaderProperty = GlobalProperty.Register("LsuHeader", typeof(OspfLsuHeader),
                                                                                 typeof(OspfLsuHandler));

        public static GlobalProperty CurrentLinkIndexProperty = GlobalProperty.Register("CurrentLinkIndex", typeof(int),
                                                                                        typeof(OspfLsuHandler),
                                                                                        new GlobalPropertyMetadata(
                                                                                            GlobalPropertyMetadataOptions.InternalUseOnly));

        public static GlobalProperty LinkCountProperty = GlobalProperty.Register("LinkCount", typeof(int),
                                                                                 typeof(OspfLsuHandler),
                                                                                 new GlobalPropertyMetadata(
                                                                                     GlobalPropertyMetadataOptions.InternalUseOnly));

        protected override bool CheckForNext()
        {
            return GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType == OspfPacketType.LinkStateUpdate;
        }

        public override Handler Parse()
        {
            var header = new OspfLsuHeader();
            header.LSACount = LoadUInt32ReversingEndian();

            SetValue(LsuHeaderProperty, header);

            if (header.LSACount > 0)
            {
                return DeclareInteration(OspfHandler.LinkCountProperty, (int)header.LSACount, OspfHandler.CurrentLinkIndexProperty);
            }

            return GetNextHandler();
        }
    }
}
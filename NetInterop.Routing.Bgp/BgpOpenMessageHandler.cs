using System;

namespace NetInterop.Routing.Bgp
{
    [HandlerMetadata("BGPOPEN", "BGP")]
    public class BgpOpenMessageHandler : InterpretiveHandler
    {
        public static GlobalProperty BgpOpenHeaderProperty = GlobalProperty.Register("BgpOpenHeader",
                                                                                     typeof(BgpOpenHeader),
                                                                                     typeof(BgpHandler),
                                                                                     new GlobalPropertyMetadata(
                                                                                         GlobalPropertyMetadataOptions.
                                                                                             Interpretive));

        public override Interpreter Interpreter
        {
            get
            {
                return new BgpOpenMessageTlvInterpreter();
            }
        }

        protected override bool CheckForNext()
        {
            return GetValue<BgpHeader>(BgpHandler.BgpCommonHeaderProperty).BgpMessageType == BgpMessageType.Open;
        }

        public override Handler Parse()
        {
            var header = new BgpOpenHeader();
            header.Version = LoadAndScroll<Byte>();
            header.asn = LoadUInt16ReversingEndian();
            header.Holdtime = LoadUInt16ReversingEndian();
            header.ID = LoadUInt32ReversingEndian();
            header.OptionalParameterLength = LoadAndScroll<Byte>();

            //++ load raw data
            header.OptionalParameter = new byte[header.OptionalParameterLength];
            for (int i = 0; i < header.OptionalParameterLength; i++)
            {
                header.OptionalParameter[i] = LoadAndScroll<Byte>();
            }

            return GetNextHandler();
        }
    }
}
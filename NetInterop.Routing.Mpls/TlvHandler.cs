using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Mpls
{
    [Export(typeof(Handler))]
    [HandlerMetadata("LDPMESSAGETLV", "LDPMESSAGE")]
    public class TlvHandler : InterpretiveHandler
    {
        public static GlobalProperty TlvProperty = GlobalProperty.Register("Tlv", typeof(LdpTlv), typeof(TlvHandler),
                                                                           new GlobalPropertyMetadata(
                                                                               GlobalPropertyMetadataOptions.
                                                                                   Interpretive));

        //TODO: This could be a problem for the iterations, since the parent would be this current class. Perhaps I need to extend the shared parent concept.

        public override Interpreter Interpreter
        {
            get
            {
                return new MplsTlvInterpreter();
            }
        }

        protected override Boolean CheckForNext()
        {
            return true;
        }

        public override Handler Parse()
        {
            var header = new LdpTlv();
            header.Type = LoadUInt16ReversingEndian();
            header.Length = LoadUInt16ReversingEndian();
            header.Value = new byte[header.Length];
            for (int i = 0; i < header.Length; i++)
            {
                header.Value[i] = LoadAndScroll<Byte>();
            }

            SetValue(TlvProperty, header);
            SetValue(RootHandler.SharedParentProperty, true);

            int currentLength = ((UInt16)GetValue(LdpHandler.CurrentLengthProperty)) + header.Length;
            var totalLength = (UInt16)GetValue(LdpHandler.TotalLengthProperty);
            if (currentLength < totalLength)
            {
                SetValue(LdpHandler.CurrentLengthProperty, (UInt16)currentLength);
                SetValue(RootHandler.SharedParentProperty, true);
                return GetNextHandler();
            }

            return null;
        }
    }
}
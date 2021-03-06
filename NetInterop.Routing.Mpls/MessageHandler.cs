using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Mpls
{
    [Export(typeof(Handler))]
    [HandlerMetadata("LDPMESSAGE", "LDP")]
    public class MessageHandler : Handler
    {
        public static GlobalProperty MessageProperty = GlobalProperty.Register("Message", typeof(LdpMessage),
                                                                               typeof(MessageHandler));

        protected override Boolean CheckForNext()
        {
            return true;
        }

        public override Handler Parse()
        {
            var header = new LdpMessage();
            header.Type = LoadUInt16ReversingEndian();
            header.Length = LoadUInt16ReversingEndian();
            header.ID = LoadUInt32ReversingEndian();

            SetValue(MessageProperty, header);
            SetValue(LdpHandler.CurrentLengthProperty,
                     (UInt16)(((UInt16)GetValue(LdpHandler.CurrentLengthProperty)) + 12));

            return GetNextHandler();
        }
    }
}
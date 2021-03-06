using System;
using System.ComponentModel.Composition;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Mpls
{
    [Export(typeof(Handler))]
    [HandlerMetadata("LDP", "UDP")]
    public class LdpHandler : Handler
    {
        public static GlobalProperty LdpHeaderProperty = GlobalProperty.Register("LdpHeader", typeof(LdpHeader),
                                                                                 typeof(LdpHandler));

        public static GlobalProperty TotalLengthProperty = GlobalProperty.Register("TotalLength", typeof(UInt16),
                                                                                   typeof(LdpHandler),
                                                                                   new GlobalPropertyMetadata(
                                                                                       GlobalPropertyMetadataOptions.
                                                                                           InternalUseOnly));

        public static GlobalProperty CurrentLengthProperty = GlobalProperty.Register("CurrentLength", typeof(UInt16),
                                                                                     typeof(LdpHandler),
                                                                                     new GlobalPropertyMetadata(
                                                                                         GlobalPropertyMetadataOptions.
                                                                                             InternalUseOnly));

        protected override Boolean CheckForNext()
        {
            return ((UdpHeader)GetValue(UdpHandler.UdpHeaderProperty)).DestinationPort == 676;
        }

        public override Handler Parse()
        {
            var header = new LdpHeader();
            header.Version = LoadUInt16ReversingEndian();
            header.PduLength = LoadUInt16ReversingEndian();
            header.LSRID = LoadAndScroll<IPAddress>();
            header.LabelSpaceID = LoadUInt16ReversingEndian();

            SetValue(LdpHeaderProperty, header);
            SetValue(TotalLengthProperty, header.PduLength);
            SetValue(CurrentLengthProperty, (UInt16)10);

            return GetNextHandler();
        }
    }
}
using System;
using NetInterop.Routing.Core;
using NetInterop.Routing.Tcp;

namespace NetInterop.Routing.Bgp
{
    [HandlerMetadata("BGP", "TCP")]
    public class BgpHandler : Handler
    {
        public static GlobalProperty BgpCommonHeaderProperty = GlobalProperty.Register("BgpCommon", typeof(BgpHeader), typeof(BgpHandler));

        protected override bool CheckForNext()
        {
            return ((TcpHeader)GetValue(TcpHandler.TcpHeaderProperty)).DestinationPort == 179;
        }

        public override Handler Parse()
        {
            var header = new BgpHeader();
            Scroll<Int32>();
            Scroll<Int32>();
            Scroll<Int32>();
            Scroll<Int32>();
            header.Length = LoadUInt16ReversingEndian();
            header.type = LoadAndScroll<Byte>();

            SetValue(BgpCommonHeaderProperty, header);

            return GetNextHandler();
        }
    }
}
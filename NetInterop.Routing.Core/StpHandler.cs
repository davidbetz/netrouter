using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("STP", "IEEE8023")]
    public class StpHandler : Handler
    {
        public static GlobalProperty StpHeaderProperty = GlobalProperty.Register("StpHeader", typeof(Bpdu),
                                                                                 typeof(StpHandler));

        protected override Boolean CheckForNext()
        {
            var llch = ((Llc)GetValue(IEEE8023Handler.LlcHeaderProperty));
            return llch.dsap == 66 && llch.ssap == 66;
        }

        public override Handler Parse()
        {
            var bpdu = new Bpdu();

            bpdu.ProtocolID = LoadAndScroll<UInt16>();

            bpdu.Version = LoadAndScroll<Byte>();
            bpdu.Type = LoadAndScroll<Byte>();
            bpdu.Flags = LoadAndScroll<Byte>();

            bpdu.RootPriority = LoadUInt16ReversingEndian();
            bpdu.RootAddress = LoadAndScroll<MacAddress>();

            bpdu.RootPathCost = LoadAndScroll<UInt32>();

            bpdu.SenderPriority = LoadUInt16ReversingEndian();
            bpdu.SenderAddress = LoadAndScroll<MacAddress>();

            bpdu.PortID = LoadAndScroll<UInt16>();
            bpdu.MessageAge = LoadAndScroll<UInt16>();
            bpdu.MaxAge = LoadAndScroll<UInt16>();
            bpdu.HelloTime = LoadAndScroll<UInt16>();
            bpdu.ForwardDelay = LoadAndScroll<UInt16>();

            SetValue(StpHeaderProperty, bpdu);

            return GetNextHandler();
        }
    }
}
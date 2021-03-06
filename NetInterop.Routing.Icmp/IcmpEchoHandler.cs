using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using Nalarium;

namespace NetInterop.Routing.Icmp
{
    [Export(typeof(Handler))]
    [HandlerMetadata("ICMPECHO", "ICMP")]
    public class IcmpEchoHandler : Handler
    {
        public static GlobalProperty IcmpEchoHeaderProperty = GlobalProperty.Register("IcmpEchoHeader", typeof(IcmpEchoHeader),
                                                                                  typeof(IcmpEchoHandler));

        private readonly Map<Tuple<int, int>, Timer> _sequenceTrackingMap = new Map<Tuple<int, int>, Timer>();

        protected override Boolean CheckForNext()
        {
            var header = (IcmpHeader)GetValue(IcmpHandler.IcmpHeaderProperty);
            return (header.Type == 8 || header.Type == 0) && header.Code == 0;
        }

        public override void Initialize(Module owningModule)
        {
            SeriesCompleted += (owningModule as IcmpModule).ProcessEcho;
        }

        public override Handler Parse()
        {
            var header = LoadHeader<IcmpEchoHeader>(
                            "identifier", 2,
                            "seqnum", 2
                            );

            SetValue(IcmpEchoHeaderProperty, header);

            return null;
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var header = CreateHeader<IcmpEchoHeader>(parameterArray);
            header.Data = System.Text.UTF8Encoding.UTF8.GetBytes("abcdabcdabcdabcdabcdabcdabcdabcd");
            return header;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var icmpEchoHeader = (IcmpEchoHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(icmpEchoHeader.Identifier));
            currentData.AddRange(GetBytes(icmpEchoHeader.SequenceNumber));
            currentData.AddRange(icmpEchoHeader.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
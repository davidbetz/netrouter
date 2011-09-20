using System;
using Nalarium;

namespace NetInterop.Routing.Icmp
{
    public struct IcmpEchoHeader : IHeader
    {
        public const byte MessageType = 8;
        public const byte ReplyMessageType = 0;
        public const byte Code = 0;

        public ushort Identifier;
        public ushort SequenceNumber;

        public byte[] Data;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
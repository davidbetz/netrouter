using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct IPv6Header : IHeader
    {
        public IPv6Address DestinationAddress;
        public byte HopLimit;
        public ushort Length;
        public byte NextHeader;
        public IPv6Address SourceAddress;
        public UInt32 VersionClassFlow;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
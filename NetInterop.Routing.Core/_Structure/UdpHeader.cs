using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct UdpHeader : IHeader
    {
        public ushort Crc;
        public ushort DestinationPort;
        public ushort Len;
        public ushort SourcePort;

        public IPAddress PseudoSourcePort { get; set; }
        public IPAddress PseudoDestinationPort1 { get; set; }
        public IPAddress PseudoDestinationPort2 { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
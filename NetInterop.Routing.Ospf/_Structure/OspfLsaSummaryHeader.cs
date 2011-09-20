using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaSummaryHeader : IHeader, IOspfLsaHeader
    {
        public byte Metric1;
        public byte Metric2;
        public byte Metric3;
        public UInt32 NetworkMask;
        public byte TypeOfService;
        public byte TypeOfServiceMetric1;
        public byte TypeOfServiceMetric2;
        public byte TypeOfServiceMetric3;
        public OspfLsaHeader CommonHeader { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
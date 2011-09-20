using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaNetworkHeader : IHeader, IOspfLsaHeader
    {
        public UInt32 AttachedRouter;
        public UInt32 NetworkMask;
        public OspfLsaHeader CommonHeader { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
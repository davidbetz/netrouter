using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsuHeader : IHeader
    {
        [FieldLabel("Link Count")]
        public UInt32 LSACount;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
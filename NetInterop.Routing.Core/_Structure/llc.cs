using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct Llc : IHeader
    {
        public byte cf;
        public byte dsap;
        public byte ssap;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
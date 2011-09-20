using System;
using System.Globalization;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct EthernetHeader : IHeader
    {
        public MacAddress Destination;
        public MacAddress Source;
        public TypeOrLength TypeOrLength;

        public int TypeOrLengthInt32
        {
            get
            {
                return Int32.Parse(TypeOrLength.Segment1.ToString("00") + TypeOrLength.Segment2.ToString("00"), NumberStyles.HexNumber);
            }
        }

        [FieldLabel("Type")]
        public int Type
        {
            get
            {
                return (TypeOrLength.Segment1 * 8) + TypeOrLength.Segment2;
            }
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
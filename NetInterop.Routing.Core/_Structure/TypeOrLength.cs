using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct TypeOrLength : IHeader, IReversable
    {
        public byte Segment1;
        public byte Segment2;

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static TypeOrLength From(byte[] array)
        {
            if (array == null)
            {
                return new TypeOrLength();
            }
            return new TypeOrLength
                   {
                       Segment1 = array[0],
                       Segment2 = array[1]
                   };
        }

        public static byte[] GetBytes(TypeOrLength typeOrLength)
        {
            var array = new byte[2];
            array[0] = typeOrLength.Segment1;
            array[1] = typeOrLength.Segment2;
            return array;
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
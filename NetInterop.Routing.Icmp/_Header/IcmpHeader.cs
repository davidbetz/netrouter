using System;
using Nalarium;
using NetInterop.Routing.Icmp;

namespace NetInterop.Routing.Icmp
{
    public struct IcmpHeader : IHeader
    {
        [FieldSizeIgnore]
        public ushort BodySize;

        public byte Code;
        public ushort Crc;

        [FieldSizeIgnore]
        public byte[] Data;

        public byte Type;

        //[FieldLabel("Type/Code")]
        //public IcmpTypeCode IcmpTypeCode
        //{
        //    get
        //    {
        //        return IcmpTypeCode.FindByTypeAndCode(type, code);
        //    }
        //}

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
//using System;
//using Nalarium;

//namespace NetInterop.Routing.Core
//{
//    public struct EigrpHeader : IHeader
//    {
//        public UInt32 ack;
//        public UInt32 asn;
//        public ushort crc;
//        public ushort eigrpver;
//        public UInt32 flags;
//        public ushort iosver;
//        public byte op;
//        public ushort param_holdtime;
//        public byte param_k1;
//        public byte param_k2;
//        public byte param_k3;
//        public byte param_k4;
//        public byte param_k5;
//        public byte param_reserved;
//        public ushort param_size;
//        public ushort param_type;
//        public UInt32 seq;
//        public ushort softsize;
//        public ushort softver;
//        public byte version;

//        public override String ToString()
//        {
//            return PairAndSeriesBuilder.CreateSeries(this);
//        }
//    }
//}
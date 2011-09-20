using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfHeader : IHeader
    {
        public struct Member
        {
            public const String AreaID = "AreaID";
            public const String Auth1 = "Auth1";
            public const String Auth2 = "Auth2";
            public const String AuthType = "AuthType";
            public const String Crc = "Crc";
            public const String Length = "Length";
            public const String RouterID = "RouterID";
            public const String Type = "Type";
            public const String Version = "Version";
        }

        public IPAddress AreaID;
        public UInt32 Auth1;
        public UInt32 Auth2;
        public ushort AuthType;
        public ushort Crc;
        public ushort Length;
        public IPAddress RouterID;
        public byte Type;
        public byte Version;

        [FieldOverride("type")]
        public OspfPacketType OspfPacketType
        {
            get
            {
                return (OspfPacketType)Type;
            }
        }

        [FieldOverride("authtype")]
        public OspfAuthType OspfAuthType
        {
            get
            {
                return (OspfAuthType)AuthType;
            }
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
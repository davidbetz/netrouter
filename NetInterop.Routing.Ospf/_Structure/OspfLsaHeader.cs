using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaHeader : IHeader
    {
        public IPAddress AdvertisingRouter;
        public ushort Crc;
        public ushort Length;
        public ushort LSAge;
        public IPAddress LSID;
        public byte LSType;
        public byte Options;
        public uint SequenceNumber;

        public string Key
        {
            get
            {
                return LSType + "_" + LSID + "_" + AdvertisingRouter;
            }
        }

        [FieldOverride("lstype")]
        public OspfLsaType OspfLsaType
        {
            get
            {
                return (OspfLsaType)LSType;
            }
            set
            {
                LSType = (byte)value;
            }
        }

        public bool IsSame(OspfLsaHeader header)
        {
            return header.LSType == LSType && header.LSID == LSID && header.AdvertisingRouter == AdvertisingRouter;
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
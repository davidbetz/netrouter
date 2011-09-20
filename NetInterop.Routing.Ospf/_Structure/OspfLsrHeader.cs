using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsrHeader : IHeader
    {
        public struct Member
        {
            public const String AdvertisingRouter = "AdvertisingRouter";
            public const String LSID = "LSID";
            public const String Type = "Type";
        }

        public IPAddress AdvertisingRouter;
        public IPAddress LSID;
        public UInt32 Type;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
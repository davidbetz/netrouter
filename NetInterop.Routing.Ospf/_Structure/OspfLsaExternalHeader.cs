using System;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaExternalHeader : IHeader, IOspfLsaHeader
    {
        public UInt32 ExternalRouteTag1;
        public UInt32 ExternalRouteTag2;
        public UInt32 ForwardingAddress1;
        public UInt32 ForwardingAddress2;
        public byte TypeOfService1;
        public byte TypeOfService2;
        public byte TypeOfService3;
        public byte Metric1;
        public byte Metric2;
        public byte Metric3;
        public UInt32 NetworkMask;
        public byte Options1;
        public byte Options2;
        public OspfLsaHeader CommonHeader { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
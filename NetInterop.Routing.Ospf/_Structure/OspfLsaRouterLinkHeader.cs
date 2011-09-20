using System;
using System.Net;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaRouterLinkHeader : IHeader
    {
        public IPAddress LinkData;
        public IPAddress LinkID;
        public ushort Metric;
        public byte TypeOfService;
        public byte Type;

        //[FieldLabel("ID")]
        //public IPAddress LinkIDAsIPAddress
        //{
        //    get
        //    {
        //        return DataConverter.ReverseIpAddress(new IPAddress(lnkid));
        //    }
        //}

        //[FieldLabel("Data")]
        //public IPAddress LinkDataAsIPAddress
        //{
        //    get
        //    {
        //        return DataConverter.ReverseIpAddress(new IPAddress(lnkid));
        //    }
        //}

        [FieldLabel("Type")]
        public OspfLsaLinkType OspfLsaLinkType
        {
            get
            {
                return (OspfLsaLinkType)Type;
            }
            set
            {
                Type = (byte)value;
            }
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
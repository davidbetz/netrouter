using System;
using System.Collections.Generic;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfLsaRouterHeader : IHeader, IOspfLsaHeader
    {
        public byte Options;
        public ushort LinkCount;
        public OspfLsaHeader CommonHeader { get; set; }

        public List<OspfLsaRouterLinkHeader> LinkList { get; set; }

        [FieldOverride("flags")]
        public RouterLSAOptions RouterLSAOptions
        {
            get
            {
                var routerLSAOptions = new RouterLSAOptions();
                routerLSAOptions = routerLSAOptions | (RouterLSAOptions)((Options & 0x01));
                routerLSAOptions = routerLSAOptions | (RouterLSAOptions)((Options & 0x02));
                routerLSAOptions = routerLSAOptions | (RouterLSAOptions)((Options & 0x04));
                return routerLSAOptions;
            }
            set
            {
                Options = 0;
                foreach (var item in Enum.GetValues(typeof(DBDOptions)))
                {
                    var v = (byte)item;
                    if (((byte)Options & v) == v)
                    {
                        Options |= v;
                    }
                }
            }
        }

        [FieldOverride("options")]
        public String OptionString
        {
            get
            {
                return Convert.ToString(Options, 2);
            }
        }

        public bool IsVirtualLink
        {
            get
            {
                return (Options & 0x04) == 1;
            }
        }

        public bool IsAsbr
        {
            get
            {
                return (Options & 0x02) == 1;
            }
        }

        public bool IsAbr
        {
            get
            {
                return (Options & 0x01) == 1;
            }
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
using System;
using System.Collections;
using Nalarium;

namespace NetInterop.Routing.Ospf
{
    public struct OspfHelloHeader : IHeader
    {
        public IPAddress Bdr;
        public IPAddress Dr;
        public ushort Interval;
        public IPAddress Mask;
        public IPAddress[] Neighbor;
        public byte Options;
        public uint RouterDeadInterval;
        public byte RouterPriority;

        [FieldOverride("options")]
        public OspfOptions OspfOptions
        {
            get
            {
                var f = new OspfOptions();
                f = f | (OspfOptions)((Options & 0x01));
                f = f | (OspfOptions)((Options & 0x02));
                f = f | (OspfOptions)((Options & 0x04));
                f = f | (OspfOptions)((Options & 0x08));
                f = f | (OspfOptions)((Options & 0x10));
                f = f | (OspfOptions)((Options & 0x20));
                f = f | (OspfOptions)((Options & 0x40));
                f = f | (OspfOptions)((Options & 0x80));
                return f;
            }
        }

        public static byte GetByteFromFlags(OspfOptions options)
        {
            byte result = 0;
            foreach (var item in Enum.GetValues(typeof(OspfOptions)))
            {
                var value = (byte)item;
                if (((byte)options & value) == value)
                {
                    result |= value;
                }
            }
            return result;
        }

        public int Length { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
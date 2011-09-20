using System;

namespace NetInterop.Parsing
{
    public struct arp_header : IHeader
    {
        [FieldLabel("Destination IP Address")] public ip_address daddr;
        [FieldLabel("Destination MAC Address")] public mac_address dst;
        public Byte hwsize;
        public UInt16 hwtype;
        public UInt16 opcode;
        public Byte protsize;
        public UInt16 prottype;

        [FieldLabel("Source IP Address")] public ip_address saddr;
        [FieldLabel("Source MAC Address")] public mac_address src;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    };
}
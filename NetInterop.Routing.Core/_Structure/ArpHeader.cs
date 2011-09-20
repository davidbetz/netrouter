using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct ArpHeader : IHeader
    {
        [FieldLabel("Destination IP Address")]
        public IPAddress DestinationIPAddress;

        [FieldLabel("Destination MAC Address")]
        public MacAddress DestinationMacAddress;

        public byte HardwareSize;
        public ushort HardwareType;
        public ushort Operation;
        public byte ProtocolSize;
        public ushort ProtocolType;

        [FieldLabel("Source IP Address")]
        public IPAddress SourceIPAddress;

        [FieldLabel("Source MAC Address")]
        public MacAddress SourceMacAddress;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    };
}
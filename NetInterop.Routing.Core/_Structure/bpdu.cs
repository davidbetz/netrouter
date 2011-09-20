using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct Bpdu : IHeader
    {
        public byte Flags;
        public ushort ForwardDelay;
        public ushort HelloTime;
        public ushort MaxAge;
        public ushort MessageAge;
        public ushort PortID;
        public ushort ProtocolID;
        public MacAddress RootAddress;
        public ushort RootPriority;
        public UInt32 RootPathCost;
        public MacAddress SenderAddress;
        public ushort SenderPriority;
        public byte Type;
        public byte Version;

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
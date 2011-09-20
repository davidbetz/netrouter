using Nalarium;

namespace NetInterop.Routing.Core
{
    public class IPPacketPerDestinationIdentifier
    {
        public IPPacketPerDestinationIdentifier()
        {
            MaxKey = 0;
            Data = new Map<ushort, IPPacketIdentifier>();
        }

        public ushort MaxKey { get; set; }

        public Map<ushort, IPPacketIdentifier> Data { get; set; }
    }
}
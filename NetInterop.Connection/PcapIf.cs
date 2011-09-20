using System;

namespace NetInterop.Connection
{
    public struct PcapIf
    {
        public IntPtr next; //++ pcap_if
        public string name; /* name to hand to "pcap_open_live()" */
        public string description; /* textual description of interface, or NULL */
        public IntPtr addresses; //++ pcap_addr
        public UInt32 flags; /* PCAP_IF_ interface flags */
    };
}
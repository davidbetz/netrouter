using System;

namespace NetInterop.Connection
{
    public struct PcapAddr
    {
        public IntPtr next; //++ pcap_addr
        public IntPtr addr; //++ sockaddr
        public IntPtr netmask; //++ sockaddr
        public IntPtr broadaddr; //++ sockaddr
        public IntPtr dstaddr; //++ sockaddr
    };
}
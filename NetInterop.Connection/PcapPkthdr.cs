using System;

namespace NetInterop.Connection
{
    public struct PcapPkthdr
    {
        public TimeValue ts;	/* time stamp */
        public UInt64 caplen;	/* length of portion present */
        public UInt64 len;	/* length this packet (off wire) */
    }
}
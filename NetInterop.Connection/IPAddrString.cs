using System;
using System.Runtime.InteropServices;

namespace NetInterop.Connection
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IPAddrString
    {
        public IntPtr Next;
        public IPAddressString IpAddress;
        public IPAddressString IpMask;
        public Int32 Context;
    }
}
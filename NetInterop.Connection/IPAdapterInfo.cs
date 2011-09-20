using System;
using System.Runtime.InteropServices;

namespace NetInterop.Connection
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IPAdapterInfo
    {
        public IntPtr Next;
        public Int32 ComboIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IPAdapterConst.MaxAdapterNameLength + 4)]
        public string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IPAdapterConst.MaxAdapterDescriptionLength + 4)]
        public string Description;
        public UInt32 AddressLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IPAdapterConst.MaxAdapterAddressLength)]
        public byte[] Address;
        public Int32 Index;
        public UInt32 Type;
        public UInt32 DhcpEnabled;
        public IntPtr CurrentIpAddress;
        public IPAddrString IpAddressList;
        public IPAddrString GatewayList;
        public IPAddrString DhcpServer;
        public bool HaveWins;
        public IPAddrString PrimaryWinsServer;
        public IPAddrString SecondaryWinsServer;
        public Int32 LeaseObtained;
        public Int32 LeaseExpires;
    }
}
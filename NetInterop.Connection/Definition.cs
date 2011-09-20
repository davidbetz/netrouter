﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using bpf_u_int32 = System.UInt64;

namespace NetInterop.Connection
{
    //public struct bpf_program
    //{
    //    public u_int bf_len;
    //    public bpf_insn bf_insns;
    //};

    //public struct bpf_insn
    //{
    //    public short code;
    //    public char jt;
    //    public char jf;
    //    public bpf_u_int32 k;
    //};

    public static class Definition
    {
        public static void UseReferenceFolder()
        {
            SetDllDirectory(@"F:\_HG\NetMon\_REFERENCE\");
        }

        public delegate void packet_handler(int id, IntPtr pcap_pkthdr, IntPtr pkt_data);

        [DllImport("Iphlpapi.dll")]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pOutBufLen);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int pcap_findalldevs(ref IntPtr alldev, StringBuilder errbuf);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr pcap_open_live(StringBuilder device, int length, int promiscuous, int timeout, StringBuilder errorBuffer); //++ return: pcap_t *

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void pcap_freealldevs(IntPtr pcap_if_t);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int pcap_loop(IntPtr pcap_t, int count, packet_handler packet_handler, int id);

        //[DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public static extern void pcap_breakloop(IntPtr pcap_t);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern StringBuilder pcap_geterr(IntPtr pcap_t);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int pcap_sendpacket(IntPtr pcap_t, IntPtr buffer, int size);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void pcap_close(IntPtr pcap_t);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int pcap_compile(IntPtr pcap_t, IntPtr bpf_program, StringBuilder str, int optimize, bpf_u_int32 netmask);

        [DllImport("wpcap.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int pcap_setfilter(IntPtr pcap_t, IntPtr bpf_program);

        //public static void GetAdapters()
        //{
        //    const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        //    const int ERROR_BUFFER_OVERFLOW = 111;
        //    const int MAX_ADAPTER_NAME_LENGTH = 256;
        //    const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        //    const int MIB_IF_TYPE_OTHER = 1;
        //    const int MIB_IF_TYPE_ETHERNET = 6;
        //    const int MIB_IF_TYPE_TOKENRING = 9;
        //    const int MIB_IF_TYPE_FDDI = 15;
        //    const int MIB_IF_TYPE_PPP = 23;
        //    const int MIB_IF_TYPE_LOOPBACK = 24;
        //    const int MIB_IF_TYPE_SLIP = 28;

        //    long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
        //    IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

        //    int ret = GetAdaptersInfo(pArray, ref structSize);

        //    if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
        //    {
        //        // Buffer was too small, reallocate the correct size for the buffer.
        //        pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

        //        ret = GetAdaptersInfo(pArray, ref structSize);
        //    } // if

        //    if (ret == 0)
        //    {
        //        // Call Succeeded
        //        IntPtr pEntry = pArray;

        //        do
        //        {
        //            // Retrieve the adapter info from the memory address
        //            IP_ADAPTER_INFO entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

        //            // ***Do something with the data HERE!***
        //            Console.WriteLine("\n");
        //            Console.WriteLine("Index: {0}", entry.Index.ToString());

        //            // Adapter Type
        //            string tmpString = string.Empty;
        //            switch (entry.Type)
        //            {
        //                case MIB_IF_TYPE_ETHERNET: tmpString = "Ethernet"; break;
        //                case MIB_IF_TYPE_TOKENRING: tmpString = "Token Ring"; break;
        //                case MIB_IF_TYPE_FDDI: tmpString = "FDDI"; break;
        //                case MIB_IF_TYPE_PPP: tmpString = "PPP"; break;
        //                case MIB_IF_TYPE_LOOPBACK: tmpString = "Loopback"; break;
        //                case MIB_IF_TYPE_SLIP: tmpString = "Slip"; break;
        //                default: tmpString = "Other/Unknown"; break;
        //            } // switch
        //            Console.WriteLine("Adapter Type: {0}", tmpString);

        //            Console.WriteLine("Name: {0}", entry.AdapterName);
        //            Console.WriteLine("Desc: {0}\n", entry.Description);

        //            Console.WriteLine("DHCP Enabled: {0}", (entry.DhcpEnabled == 1) ? "Yes" : "No");

        //            if (entry.DhcpEnabled == 1)
        //            {
        //                Console.WriteLine("DHCP Server : {0}", entry.DhcpServer.IpAddress.Address);

        //                // Lease Obtained (convert from "time_t" to C# DateTime)
        //                DateTime pdatDate = new DateTime(1970, 1, 1).AddSeconds(entry.LeaseObtained).ToLocalTime();
        //                Console.WriteLine("Lease Obtained: {0}", pdatDate.ToString());

        //                // Lease Expires (convert from "time_t" to C# DateTime)
        //                pdatDate = new DateTime(1970, 1, 1).AddSeconds(entry.LeaseExpires).ToLocalTime();
        //                Console.WriteLine("Lease Expires : {0}\n", pdatDate.ToString());
        //            } // if DhcpEnabled

        //            Console.WriteLine("IP Address     : {0}", entry.IpAddressList.IpAddress.Address);
        //            Console.WriteLine("Subnet Mask    : {0}", entry.IpAddressList.IpMask.Address);
        //            Console.WriteLine("Default Gateway: {0}", entry.GatewayList.IpAddress.Address);

        //            // MAC Address (data is in a byte[])
        //            tmpString = string.Empty;
        //            for (int i = 0; i < entry.AddressLength - 1; i++)
        //            {
        //                tmpString += string.Format("{0:X2}-", entry.Address[i]);
        //            }
        //            Console.WriteLine("MAC Address    : {0}{1:X2}\n", tmpString, entry.Address[entry.AddressLength - 1]);

        //            Console.WriteLine("Has WINS: {0}", entry.HaveWins ? "Yes" : "No");
        //            if (entry.HaveWins)
        //            {
        //                Console.WriteLine("Primary WINS Server  : {0}", entry.PrimaryWinsServer.IpAddress.Address);
        //                Console.WriteLine("Secondary WINS Server: {0}", entry.SecondaryWinsServer.IpAddress.Address);
        //            } // HaveWins

        //            // Get next adapter (if any)
        //            pEntry = entry.Next;

        //        }
        //        while (pEntry != IntPtr.Zero);

        //        Marshal.FreeHGlobal(pArray);

        //    } // if
        //    else
        //    {
        //        Marshal.FreeHGlobal(pArray);
        //        throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
        //    }

        //} // GetAdapters
    }

}

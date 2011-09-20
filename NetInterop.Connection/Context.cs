using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Nalarium;
using bpf_u_int32 = System.UInt32;

namespace NetInterop.Connection
{
    public class Context
    {
        public static event Write VerboseOutput = delegate
        {
        };

        public event Received Received = delegate
        {
        };

        public readonly static Dictionary<Int32, Received> ReceivedArray = new Dictionary<Int32, Received>();
        private static Dictionary<Int32, Tuple<String, String, String, List<Tuple<String, String, Boolean>>>> _deviceDictionary;
        private static Dictionary<Int32, String> _deviceIndexDictionary;

        private const int PCAP_ERRBUF_SIZE = 256;

        private int _index;
        private IntPtr _adhandle = IntPtr.Zero;
        private int _deviceNumber;
        private string _deviceID;

        public static Context Create()
        {
            Definition.UseReferenceFolder();
            return new Context();
        }

        public static void OnVerboseOutput(String text)
        {
            VerboseOutput(text);
        }
        public Dictionary<Int32, String> GetDeviceIndexDictionary()
        {
            if (_deviceIndexDictionary == null)
            {
                _deviceIndexDictionary = new Dictionary<int, string>();
                lock (_deviceIndexDictionary)
                {
                    var alldevs = new IntPtr();
                    var errbuf = new StringBuilder();
                    int i = 0;

                    if (Definition.pcap_findalldevs(ref alldevs, errbuf) == -1)
                    {
                        OnVerboseOutput(String.Format("Error in pcap_findalldevs: {0}\n", errbuf.ToString()));
                        return null;
                    }

                    var d = Marshaller.ToStructure<PcapIf>(alldevs);
                    do
                    {
                        if (!String.IsNullOrEmpty(d.description))
                        {
                            int index = d.name.IndexOf("{");
                            int indexEnd = d.name.IndexOf("}");
                            _deviceIndexDictionary.Add(i++, d.name.Substring(index + 1, indexEnd - (index + 1)));
                        }
                        d = Marshaller.ToStructure<PcapIf>(d.next);
                    } while (d.next != IntPtr.Zero);
                    //{
                    //    if (d.description)
                    //    {
                    //        data.Add(i++,d.name);
                    //    }
                    //}
                }
            }
            return _deviceIndexDictionary;
        }

        public Dictionary<Int32, Tuple<String, String, String, List<Tuple<String, String, Boolean>>>> GetDeviceDictionary()
        {
            if (_deviceDictionary == null)
            {
                _deviceDictionary = new Dictionary<int, Tuple<string, string, string, List<Tuple<string, string, bool>>>>();
                lock (_deviceDictionary)
                {
                    //Definition.GetAdapters();
                    var alldevs = new IntPtr();
                    var errbuf = new StringBuilder();
                    int i = 0;

                    if (Definition.pcap_findalldevs(ref alldevs, errbuf) == -1)
                    {
                        OnVerboseOutput(String.Format("Error in pcap_findalldevs: {0}\n", errbuf.ToString()));
                        return null;
                    }

                    Int64 size = Marshal.SizeOf(typeof(IPAdapterInfo));
                    var pArray = Marshal.AllocHGlobal(new IntPtr(size));

                    int ret = Definition.GetAdaptersInfo(pArray, ref size);

                    if (ret == IPAdapterConst.ErrorBufferOverflow) // ERROR_BUFFER_OVERFLOW == 111
                    {
                        // Buffer was too small, reallocate the correct size for the buffer.
                        pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(size));

                        ret = Definition.GetAdaptersInfo(pArray, ref size);
                    } // if

                    var data = new Dictionary<Int32, Tuple<String, String, String, List<Tuple<String, String, Boolean>>>>();
                    if (ret == 0)
                    {
                        //Definition.GetAdaptersInfo(pArray, ref size);
                        var sb = new StringBuilder();
                        IntPtr entry = pArray;
                        do
                        {
                            var pos = Marshaller.ToStructure<IPAdapterInfo>(entry);
                            sb = new System.Text.StringBuilder();
                            //Int32.Parse(type_or_length.segment1.ToString("00") + type_or_length.segment2.ToString("00"), NumberStyles.HexNumber)
                            sb.Append(System.String.Format("{0}", pos.Address[0]));
                            for (int j = 1; j < pos.AddressLength; j++)
                            {
                                sb.Append(":");
                                sb.Append(System.String.Format("{0}", pos.Address[j]));
                            }
                            IPAddrString ip = pos.IpAddressList;
                            var list = new List<Tuple<String, String, Boolean>>
                                       {
                                           new Tuple<String, String, Boolean>(pos.IpAddressList.IpAddress.Address,
                                                                              pos.IpAddressList.IpMask.Address, true)
                                       };
                            //ip = Marshaller.ToStructure<IP_ADDR_STRING>(ip.Next);
                            while (ip.Next != IntPtr.Zero)
                            {
                                ip = Marshaller.ToStructure<IPAddrString>(ip.Next);
                                list.Add(new Tuple<String, String, Boolean>(ip.IpAddress.Address, ip.IpMask.Address, false));
                            }
                            //for (int j=1; j<pos.IpAddressList; j++) {
                            _deviceDictionary.Add(i++, new Tuple<String, String, String, List<Tuple<String, String, Boolean>>>(pos.AdapterName, pos.Description, sb.ToString(), list));

                            entry = pos.Next;
                        } while (entry != IntPtr.Zero);
                    }
                }
            }
            return _deviceDictionary;
        }

        public int Initialize(int deviceNumber, String filter)
        {
            _index = deviceNumber;
            lock (ReceivedArray)
            {
                ReceivedArray.Add(_index, Received);
            }

            string pNewCharStr = filter;

            var alldevs = new IntPtr();
            var errbuf = new StringBuilder();

            /* Retrieve the device list */
            if (Definition.pcap_findalldevs(ref alldevs, errbuf) == -1)
            {
                OnVerboseOutput(String.Format("Error in pcap_findalldevs: {0}", errbuf));
                return 0;
            }
            if (deviceNumber < 0)
            {
                OnVerboseOutput("Interface number out of range");
                Definition.pcap_freealldevs(alldevs);
                return -1;
            }

            IntPtr dPtr = alldevs;
            PcapIf d = Marshaller.ToStructure<PcapIf>(dPtr);
            int i = 0;
            while (i < deviceNumber && d.next != IntPtr.Zero)
            {
                d = Marshaller.ToStructure<PcapIf>(d.next);
                i++;
            }
            if ((_adhandle = Definition.pcap_open_live(new StringBuilder(d.name),
                                                      65536,
                                                      1,
                                                      1000,
                                                      errbuf
                                )) == IntPtr.Zero)
            {
                OnVerboseOutput(String.Format("Unable to open the adapter. {0} is not supported by WinPcap\n", d.name));
                Definition.pcap_freealldevs(alldevs);
                return -1;
            }

            OnVerboseOutput(String.Format("listening on {0}...\n", d.description));

            Definition.pcap_freealldevs(alldevs);

            if (!String.IsNullOrEmpty(pNewCharStr))
            {
                bpf_u_int32 netmask;
                IntPtr addressesPtr = d.addresses;
                if (addressesPtr != IntPtr.Zero)
                {
                    var addresses = Marshaller.ToStructure<PcapAddr>(addressesPtr);
                    var sock = Marshaller.ToStructure<SockAddr>(addresses.netmask);
                    netmask = sock.data;
                    //netmask = ((struct sockaddr_in *)(d.addresses.netmask)).sin_addr.S_un.S_addr;
                }
                else
                {
                    netmask = 0xffffff;
                }

                var dontcare = new IntPtr();
                if (Definition.pcap_compile(_adhandle, dontcare, new StringBuilder(pNewCharStr), 1, netmask) < 0)
                {
                    OnVerboseOutput("Error compiling filter: wrong syntax.\n");

                    Definition.pcap_close(_adhandle);
                    return -3;
                }

                if (Definition.pcap_setfilter(_adhandle, dontcare) < 0)
                {
                    OnVerboseOutput("\nError setting the filter\n");

                    Definition.pcap_close(_adhandle);
                    return -4;
                }
            }
            return 0;
        }

        public void Run(String deviceID)
        {
            Run(deviceID, string.Empty);
        }
        public void Run(int deviceNumber)
        {
            Run(deviceNumber, string.Empty);
        }

        public void Run(String deviceID, String filter)
        {
            var data = GetDeviceIndexDictionary();
            if (!data.Any(p => p.Value.Equals(deviceID)))
            {
                return;
            }
            int index = data.First(p => p.Value.Equals(deviceID)).Key;
            Run(deviceID, index, filter);
            Run(deviceID, 0, filter);
        }

        public void Run(int deviceNumber, String filter)
        {
            Run(GetDeviceIndexDictionary()[deviceNumber], deviceNumber, filter);
        }

        private void Run(String deviceID, int deviceNumber, String filter)
        {
            _deviceID = deviceID;
            _deviceNumber = deviceNumber;
            Initialize(deviceNumber, filter);
            if (_adhandle != IntPtr.Zero)
            {
                Definition.pcap_loop(_adhandle, 0, packet_handler, _index);
                Definition.pcap_close(_adhandle);
            }
            else
            {
                OnVerboseOutput("Monitor not initialized.\n");
            }
        }

        //void packet_handler(u_char *user, const struct pcap_pkthdr *header, const u_char *pkt_data)
        public void packet_handler(int index, IntPtr pcap_pkthdr, IntPtr pkt_data)
        {
            if (!ReceivedArray.ContainsKey(index))
            {
                return;
            }
            var header = Marshaller.ToStructure<PcapPkthdr>(pcap_pkthdr);
            ReceivedArray[index](_deviceID, (int)header.len, pkt_data);
        }

        public void Send(int size, IntPtr packet)
        {
            if (Definition.pcap_sendpacket(_adhandle, packet, size) != 0)
            {
                OnVerboseOutput(String.Format("\nError sending the packet: {0}\n", Definition.pcap_geterr(_adhandle)));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("IPV4", "ETHERNETII")]
    public class IPv4Handler : Handler, ILayer3Handler
    {
        public static GlobalProperty IPv4HeaderProperty = GlobalProperty.Register("IPv4Header", typeof(IPHeader), typeof(IPv4Handler));

        public override ushort LayerID
        {
            get
            {
                return 2048;
            }
        }

        protected override Boolean CheckForNext()
        {
            return GetValue<EthernetHeader>(EthernetIIHandler.EthernetHeaderProperty).TypeOrLengthInt32 == LayerID;
        }

        public override Handler Parse()
        {
            var header = new IPHeader();

            header.VersionIHL = LoadAndScroll<Byte>();
            header.TypeOfService = LoadAndScroll<Byte>();
            header.TotalLength = LoadUInt16ReversingEndian();
            header.Identification = LoadUInt16ReversingEndian();
            header.FlagsFragmentOffset = LoadUInt16ReversingEndian();
            header.TTL = LoadAndScroll<Byte>();
            header.Protocol = LoadAndScroll<Byte>();
            header.Crc = LoadUInt16ReversingEndian();
            header.SourceAddress = LoadAndScroll<IPAddress>();
            header.DestinationAddress = LoadAndScroll<IPAddress>();

            Offset += header.InternetHeaderLength - 20;

            SetValue(IPv4HeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ipHeader = CreateHeader<IPHeader>(parameterArray);
            ipHeader.SourceAddress = (IPAddress)parameterArray.First(p => p.Scope.Equals("IPHeader") && p.Name.Equals("SourceAddress")).AsObject;
            ipHeader.VersionIHL = 4;
            return ipHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ipHeader = (IPHeader)header;
            var currentData = new List<byte>();
            currentData.Add((byte)((ipHeader.VersionIHL << 4) | 5));
            currentData.Add(ipHeader.TypeOfService);
            //currentData.AddRange(GetBytes((UInt16)(packetData.Data.Count + 20)));
            currentData.AddRange(GetBytes(ipHeader.TotalLength));
            currentData.AddRange(GetBytes(ipHeader.Identification));
            //++ flags (3 bits-- left to right)
            //Bit 0: reserved, must be zero
            //Bit 1: (DF) 0 = May Fragment,  1 = Don't Fragment.
            //Bit 2: (MF) 0 = Last Fragment, 1 = More Fragments.
            //++ offset (13 bits)
            currentData.AddRange(GetBytes(ipHeader.FlagsFragmentOffset));
            currentData.Add(ipHeader.TTL);
            currentData.Add(ipHeader.Protocol);
            currentData.AddRange(new byte[]
                                 {
                                     0, 0
                                 });
            currentData.AddRange(ipHeader.SourceAddress.GetBytes());
            currentData.AddRange(ipHeader.DestinationAddress.GetBytes());
            byte[] crc = GetCrc(currentData.ToArray());
            currentData[10] = crc[1];
            currentData[11] = crc[0];
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }

        private byte[] GetCrc(byte[] array)
        {
            long sum = 0;
            for (int i = 0; i < (array.Length); i += 2)
            {
                var word16 = (ushort)(((array[i] << 8) & 0xFF00) + (array[i + 1] & 0xFF));
                sum += word16;
            }

            while ((sum >> 16) != 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            sum = ~sum;

            return BitConverter.GetBytes((ushort)sum);
        }
    }
}
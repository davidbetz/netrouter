using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("UDP", "IPV4", "IPV6")]
    public class UdpHandler : Handler
    {
        public static GlobalProperty UdpHeaderProperty = GlobalProperty.Register("UdpHeader", typeof(UdpHeader),
                                                                                 typeof(UdpHandler));


        protected override Boolean CheckForNext()
        {
            if (HandlerNameStack.Contains("IPV4"))
            {
                return ((IPHeader)GetValue(IPv4Handler.IPv4HeaderProperty)).Protocol == 17;
            }
            else if (HandlerNameStack.Contains("IPV6"))
            {
                //return ((ipv6_header)GetValue(IPv6Handler.IPv6HeaderProperty)).proto == 17;
            }
            return false;
        }

        public override Handler Parse()
        {
            var header = new UdpHeader();
            header.SourcePort = LoadUInt16ReversingEndian();
            header.DestinationPort = LoadUInt16ReversingEndian();
            header.Len = LoadUInt16ReversingEndian();
            header.Crc = LoadUInt16ReversingEndian();

            SetValue(UdpHeaderProperty, header);

            return GetNextHandler();
            //return GetNextHandler(header);
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var udpHeader = CreateHeader<UdpHeader>(parameterArray);
            udpHeader.PseudoDestinationPort1 = (IPAddress)parameterArray.First(p => p.Scope.Equals("IPHeader") && p.Name.Equals("DestinationAddress")).AsObject;
            udpHeader.PseudoSourcePort = (IPAddress)parameterArray.First(p => p.Scope.Equals("IPHeader") && p.Name.Equals("SourceAddress")).AsObject;
            return udpHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var udpHeader = (UdpHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(udpHeader.SourcePort));
            currentData.AddRange(GetBytes(udpHeader.DestinationPort));
            currentData.AddRange(GetBytes((ushort)(UdpHeaderProperty.Size + packetData.Data.Count)));
            currentData.AddRange(new byte[]
                                 {
                                     0, 0
                                 });
            //+ crc
            List<byte> crcData = GetPseudoHeaderBytes(udpHeader, (uint)(packetData.Data.Count + UdpHeaderProperty.Size));
            crcData.AddRange(currentData);
            crcData.AddRange(packetData.Data);
            if (crcData.Count % 2 == 1)
            {
                crcData.Add(0);
            }
            byte[] crc = Checksum.GetCrc(crcData.ToArray());
            currentData[6] = crc[1];
            currentData[7] = crc[0];
            //+
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }

        private List<byte> GetPseudoHeaderBytes(UdpHeader udpHeader, uint length)
        {
            var currentData = new List<byte>();
            currentData.AddRange(udpHeader.PseudoSourcePort.GetBytes());
            currentData.AddRange(udpHeader.PseudoDestinationPort1.GetBytes());
            currentData.Add(0);
            currentData.Add(17);
            currentData.AddRange(GetBytes(length));
            return currentData;
        }
    }
}
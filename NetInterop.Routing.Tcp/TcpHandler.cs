using Nalarium;
using NetInterop.Routing.Core;
using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Tcp
{
    [Export(typeof(Handler))]
    [HandlerMetadata("TCP", "IPV4", "IPV6")]
    public class TcpHandler : Handler
    {
        public static GlobalProperty TcpHeaderProperty = GlobalProperty.Register("TcpHeader", typeof(TcpHeader), typeof(TcpHandler));
        private static readonly Map<TcpOptionKind, TcpOption> TcpOptionMap = new Map<TcpOptionKind, TcpOption>();
        private readonly Map<Tuple<IPAddress, Byte>, TcpStreamSegment> _tcpStreamSegmentMap = new Map<Tuple<IPAddress, Byte>, TcpStreamSegment>();

        static TcpHandler()
        {
            TcpOptionMap.Add(TcpOptionKind.EOL, TcpOption.Create(1, "EOL", 0793, "EndOfOptionList"));
            TcpOptionMap.Add(TcpOptionKind.NOP, TcpOption.Create(1, "NOP", 0793, "NoOperation"));
            TcpOptionMap.Add(TcpOptionKind.MSS, TcpOption.Create(4, "MSS", 0793, "MaximumSegmentSize"));
            TcpOptionMap.Add(TcpOptionKind.WSOPT, TcpOption.Create(3, "WSOPT", 1323, "WindowScalingFactor"));
            TcpOptionMap.Add(TcpOptionKind.SACKPermitted, TcpOption.Create(2, "SACKPermitted", 2018, "Sender Supports SACK options"));
            TcpOptionMap.Add(TcpOptionKind.SACK, TcpOption.Create(0, "SACK", 2018, "SACK block"));
            TcpOptionMap.Add(TcpOptionKind.TSOPT, TcpOption.Create(10, "TSOPT", 1323, "Timestamps"));
            TcpOptionMap.Add(TcpOptionKind.UTO, TcpOption.Create(4, "UTO", 5482, "User Timeout"));
            TcpOptionMap.Add(TcpOptionKind.TCPAO, TcpOption.Create(0, "TCPAO", 5925, "Authentication"));
            TcpOptionMap.Add(TcpOptionKind.Experimental1, TcpOption.Create(0, "Experimental", 4727, "Experimental1"));
            TcpOptionMap.Add(TcpOptionKind.Experimental2, TcpOption.Create(0, "Experimental", 4727, "Experimental2"));
        }

        public override ushort LayerID
        {
            get
            {
                return 6;
            }
        }

        protected override Boolean CheckForNext()
        {
            if (HandlerNameStack.Contains("IPV4"))
            {
                return ((IPHeader)GetValue(IPv4Handler.IPv4HeaderProperty)).Protocol == LayerID;
            }
            else if (HandlerNameStack.Contains("IPV6"))
            {
                //return ((ipv6_header)GetValue(IPv6Handler.IPv6HeaderProperty)).proto == 6;
            }
            return false;
        }

        public override Handler Parse()
        {
            var initialOffset = Offset;
            var header = new TcpHeader();
            header.SourcePort = LoadUInt16ReversingEndian();
            header.DestinationPort = LoadUInt16ReversingEndian();
            header.SequenceNumber = LoadUInt32ReversingEndian();
            header.Ack = LoadUInt32ReversingEndian();
            header.Flags = LoadUInt16ReversingEndian();
            if (header.TcpFlags != TcpFlags.Syn)
            {
                return null;
            }
            header.WindowSize = LoadUInt16ReversingEndian();
            header.Crc = LoadUInt16ReversingEndian();
            header.UrgentPointer = LoadUInt16ReversingEndian();
            int optionsSize = header.Length - 20;
            bool done = false;
            int optionsOffset = 0;
            while (!done)
            {
                var nextByte = (TcpOptionKind)Load<Byte>();
                var tcpOption = TcpOptionMap[nextByte];
                var byteData = LoadByteData(tcpOption.Size);
                switch (nextByte)
                {
                    case TcpOptionKind.EOL:
                        done = true;
                        break;
                    case TcpOptionKind.NOP:
                        header.NOP = byteData;
                        break;
                    case TcpOptionKind.MSS:
                        header.MSS = byteData;
                        break;
                    case TcpOptionKind.WSOPT:
                        header.WSOPT = byteData;
                        break;
                    case TcpOptionKind.SACKPermitted:
                        header.SACKPermitted = byteData;
                        break;
                    case TcpOptionKind.SACK:
                        header.SACK = byteData;
                        break;
                    case TcpOptionKind.TSOPT:
                        header.TSOPT = byteData;
                        break;
                    case TcpOptionKind.UTO:
                        header.UTO = byteData;
                        break;
                    case TcpOptionKind.TCPAO:
                        header.TCPAO = byteData;
                        break;
                    case TcpOptionKind.Experimental1:
                        header.Experimental1 = byteData;
                        break;
                    case TcpOptionKind.Experimental2:
                        header.Experimental2 = byteData;
                        break;
                }
                optionsOffset += tcpOption.Size;
                if (optionsOffset == optionsSize)
                {
                    done = true;
                }
            }
            var finalOffset = Offset;
            var collectedCount = finalOffset - initialOffset;
            while (collectedCount % 4 != 0)
            {
                Scroll<byte>();
                collectedCount++;
            }

            SetValue(TcpHeaderProperty, header);

            return GetNextHandler();
        }
    }
}
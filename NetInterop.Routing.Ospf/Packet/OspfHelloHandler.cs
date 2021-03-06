using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFHELLO", "OSPF")]
    [HeaderOwner(typeof(OspfHelloHeader))]
    public class OspfHelloHandler : Handler
    {
        public static GlobalProperty HelloHeaderProperty = GlobalProperty.Register("HelloHeader",
                                                                                   typeof(OspfHelloHeader),
                                                                                   typeof(OspfHelloHandler));

        protected override Boolean CheckForNext()
        {
            return GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType == OspfPacketType.Hello;
        }

        public override Handler Parse()
        {
            int esize = Marshal.SizeOf(typeof(EthernetHeader));
            var ipHeader = GetValue<IPHeader>(IPv4Handler.IPv4HeaderProperty);
            ushort tlen = GetValue<IPHeader>(IPv4Handler.IPv4HeaderProperty).TotalLength;
            int ihl = GetValue<IPHeader>(IPv4Handler.IPv4HeaderProperty).InternetHeaderLength;
            ushort ospfLength = GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).Length;

            int startOffset = Offset;
            var header = new OspfHelloHeader();
            header.Mask = LoadAndScroll<IPAddress>();
            header.Interval = LoadUInt16ReversingEndian();
            header.Options = LoadAndScroll<Byte>();
            header.RouterPriority = LoadAndScroll<Byte>();
            header.RouterDeadInterval = LoadUInt32ReversingEndian();
            header.Dr = LoadAndScroll<IPAddress>();
            header.Bdr = LoadAndScroll<IPAddress>();
            int headerEndOffset = Offset;
            int sizeLeftForHelloHeader = (ospfLength - 24) - (headerEndOffset - startOffset);
            var neighborCount = (int)Math.Floor((double)sizeLeftForHelloHeader / 4);
            header.Neighbor = new IPAddress[neighborCount];

            for (int i = 0; i < neighborCount; i++)
            {
                header.Neighbor[i] = LoadAndScroll<IPAddress>();
            }

            SetValue(HelloHeaderProperty, header);

            return GetNextHandler();
        }

        public override void Initialize(Module module)
        {
            SeriesCompleted += (module as OspfModule).HelloReceived;
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            return CreateHeader<OspfHelloHeader>(parameterArray);
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ospfHelloHeader = (OspfHelloHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetNotReversedBytes(ospfHelloHeader.Mask));
            currentData.AddRange(GetBytes(ospfHelloHeader.Interval));
            currentData.Add(ospfHelloHeader.Options);
            currentData.Add(ospfHelloHeader.RouterPriority);
            currentData.AddRange(GetBytes(ospfHelloHeader.RouterDeadInterval));
            currentData.AddRange(ospfHelloHeader.Dr.GetBytes());
            currentData.AddRange(ospfHelloHeader.Bdr.GetBytes());
            if (ospfHelloHeader.Neighbor != null)
            {
                foreach (IPAddress neighbor in ospfHelloHeader.Neighbor)
                {
                    currentData.AddRange(neighbor.GetBytes());
                }
            }
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
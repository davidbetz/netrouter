using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("IPv6", "ETHERNETII")]
    public class IPv6Handler : Handler, ILayer3Handler
    {
        public static GlobalProperty IPv6HeaderProperty = GlobalProperty.Register("IPv6Header", typeof(IPv6Header), typeof(IPv6Handler));

        public override ushort LayerID
        {
            get
            {
                return 34525;
            }
        }

        protected override Boolean CheckForNext()
        {
            return GetValue<EthernetHeader>(EthernetIIHandler.EthernetHeaderProperty).TypeOrLengthInt32 == LayerID;
        }

        public override Handler Parse()
        {
            var header = new IPv6Header();

            header.VersionClassFlow = LoadUInt32ReversingEndian();
            header.Length = LoadUInt16ReversingEndian();
            header.NextHeader = LoadAndScroll<Byte>();
            header.HopLimit = LoadAndScroll<Byte>();
            header.SourceAddress = LoadAndScroll<IPv6Address>();
            header.DestinationAddress = LoadAndScroll<IPv6Address>();

            SetValue(IPv6HeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ipv6_header = CreateHeader<IPv6Header>(parameterArray);
            ipv6_header.SourceAddress = Controller.PrimaryIPv6Address.Item1;
            return ipv6_header;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            return packetData;

            var ipv6_header = (IPv6Header)header;
            var currentData = new List<byte>();
            //++ more here
            currentData.AddRange(ipv6_header.SourceAddress.GetBytes());
            currentData.AddRange(ipv6_header.DestinationAddress.GetBytes());
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
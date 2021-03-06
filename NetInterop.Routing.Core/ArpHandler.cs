using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("ARP", "ETHERNETII")]
    public class ArpHandler : Handler
    {
        public static GlobalProperty ArpHeaderProperty = GlobalProperty.Register("ArpHeader", typeof(ArpHeader), typeof(ArpHandler));

        public override ushort LayerID
        {
            get
            {
                return 2054;
            }
        }

        protected override Boolean CheckForNext()
        {
            return GetValue<EthernetHeader>(EthernetIIHandler.EthernetHeaderProperty).TypeOrLengthInt32 == LayerID;
        }

        public override Handler Parse()
        {
            var header = new ArpHeader();
            header.HardwareType = LoadUInt16ReversingEndian();
            header.ProtocolType = LoadUInt16ReversingEndian();

            header.HardwareSize = LoadAndScroll<Byte>();
            header.ProtocolSize = LoadAndScroll<Byte>();
            header.Operation = LoadUInt16ReversingEndian();

            header.SourceMacAddress = LoadAndScroll<MacAddress>();
            header.SourceIPAddress = LoadAndScroll<IPAddress>();
            header.DestinationMacAddress = LoadAndScroll<MacAddress>();
            header.DestinationIPAddress = LoadAndScroll<IPAddress>();

            SetValue(ArpHeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var arpHeader = CreateHeader<ArpHeader>(parameterArray);
            return arpHeader;
        }

        public override void Initialize(Module module)
        {
            SeriesCompleted += (s, e) =>
            {
                var systemModule = (module as SystemModule);
                var arpHeader = e.HeaderPackage.GetHeader<ArpHeader>();
                if (arpHeader.Operation == 2)
                {
                    systemModule.HandleArpResponse(s, e);
                }
            };
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var arpHeader = (ArpHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(arpHeader.HardwareType));
            currentData.AddRange(GetBytes(arpHeader.ProtocolType));

            currentData.Add(arpHeader.HardwareSize);
            currentData.Add(arpHeader.ProtocolSize);

            currentData.AddRange(GetBytes(arpHeader.Operation));


            currentData.AddRange(arpHeader.SourceMacAddress.GetBytes());
            currentData.AddRange(arpHeader.SourceIPAddress.GetBytes());
            currentData.AddRange(arpHeader.DestinationMacAddress.GetBytes());
            currentData.AddRange(arpHeader.DestinationIPAddress.GetBytes());

            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
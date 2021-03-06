using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Rip
{
    [Export(typeof(Handler))]
    [HandlerMetadata("RIP", "UDP")]
    public class RipHandler : Handler
    {
        public static GlobalProperty RipPreambleHeaderProperty = GlobalProperty.Register("RipPreambleHeader", typeof(RipPreambleHeader),
                                                                                         typeof(RipHandler));

        public static GlobalProperty RipDataHeaderProperty = GlobalProperty.Register("RipDataHeader", typeof(RipDataHeader[]),
                                                                                     typeof(RipHandler));


        protected override Boolean CheckForNext()
        {
            return ((UdpHeader)GetValue(UdpHandler.UdpHeaderProperty)).DestinationPort == 520;
        }

        public override Handler Parse()
        {
            var udpHeader = GetValue<UdpHeader>(UdpHandler.UdpHeaderProperty);

            var ripPreambleHeader = LoadHeader<RipPreambleHeader>("command", 1, "version", 1, "domain", 2);
            int bodySize = udpHeader.Len - UdpHandler.UdpHeaderProperty.Size - RipPreambleHeaderProperty.Size;
            int dataSectionCount = bodySize / RipDataHeaderProperty.Size;
            var list = new List<RipDataHeader>();
            for (int i = 0; i < dataSectionCount; i++)
            {
                list.Add(LoadHeader<RipDataHeader>("AddressFamily", 2,
                                                     "RouteTag", 2,
                                                     "Network", typeof(IPAddress),
                                                     "Mask", typeof(IPAddress),
                                                     "NextHop", typeof(IPAddress),
                                                     "Metric", 4));
            }

            SetValue(RipPreambleHeaderProperty, ripPreambleHeader);
            SetValue(RipDataHeaderProperty, list.ToArray());

            return GetNextHandler();
        }

        public override void Initialize(Module module)
        {
            SeriesCompleted += (module as RipModule).MessageReceived;
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ripPreambleHeader = CreateHeader<RipPreambleHeader>(parameterArray);
            if (ripPreambleHeader.command == (byte)RipCommand.Response)
            {
                ripPreambleHeader.DataArray = (module as RipModule).RipDatabase.Select(p => p.Value.GetRipDataHeader()).ToArray();
            }
            else
            {
                ripPreambleHeader.DataArray = new[]
                                                 {
                                                     CreateHeader<RipDataHeader>(parameterArray)
                                                 };
            }
            return ripPreambleHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ripPreambleHeader = (RipPreambleHeader)header;
            var currentData = new List<byte>();
            currentData.Add(ripPreambleHeader.command);
            currentData.Add(ripPreambleHeader.Version);
            currentData.AddRange(GetBytes(ripPreambleHeader.Domain));
            foreach (var ripDataHeader in ripPreambleHeader.DataArray)
            {
                currentData.AddRange(GetBytes(ripDataHeader.AddressFamily));
                currentData.AddRange(GetBytes(ripDataHeader.RouteTag));
                currentData.AddRange(ripDataHeader.Network.GetBytes());
                currentData.AddRange(ripDataHeader.Mask.GetBytes());
                currentData.AddRange(ripDataHeader.NextHop.GetBytes());
                currentData.AddRange(GetBytes(ripDataHeader.Metric));
            }
            return packetData.UpdateData(currentData);
        }
    }
}
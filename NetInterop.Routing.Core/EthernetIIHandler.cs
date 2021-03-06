using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("ETHERNETII", "L2SELECTION")]
    [HeaderOwner(typeof(EthernetHeader))]
    public class EthernetIIHandler : Handler
    {
        public static GlobalProperty EthernetHeaderProperty = GlobalProperty.Register("EthernetHeader", typeof(EthernetHeader), typeof(EthernetIIHandler),
                                                                                      new GlobalPropertyMetadata(
                                                                                          GlobalPropertyMetadataOptions.Preferred));

        protected override Boolean CheckForNext()
        {
            return GetValue<EthernetHeader>(L2SelectionHandler.EthernetHeaderProperty).TypeOrLengthInt32 >= 1536;
        }

        public override Handler Parse()
        {
            MoveHeader(L2SelectionHandler.EthernetHeaderProperty, EthernetHeaderProperty);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ethernetHeader = CreateHeader<EthernetHeader>(parameterArray);
            //ethernetHeader.src = Controller.InterfaceMacAddress;
            return ethernetHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ethernetHeader = (EthernetHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(ethernetHeader.Destination.GetBytes());
            currentData.AddRange(ethernetHeader.Source.GetBytes());
            currentData.AddRange(ethernetHeader.TypeOrLength.GetBytes());
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;
using NetInterop.Routing.Ospf.Lsa;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFDBD", "OSPF")]
    public class OspfDbdHandler : Handler
    {
        public static GlobalProperty DbdHeaderProperty = GlobalProperty.Register("DbdHeader", typeof(OspfDbdHeader),
                                                                                 typeof(OspfDbdHandler));

        protected override Boolean CheckForNext()
        {
            return GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType == OspfPacketType.DatabaseDescriptor;
        }

        public override void Initialize(Module module)
        {
            SeriesCompleted += (module as OspfModule).DBDReceived;
        }

        public override Handler Parse()
        {
            var header = new OspfDbdHeader();
            header.Mtu = LoadUInt16ReversingEndian();
            header.Options = LoadAndScroll<Byte>();
            header.Flags = LoadAndScroll<Byte>();
            header.SequenceNumber = LoadUInt32ReversingEndian();
            header.LsaList = new List<OspfLsaHeader>();

            SetValue(DbdHeaderProperty, header);

            int length = GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).Length;
            int lsaHeaderSpace = length - OspfHandler.OspfHeaderProperty.Size - DbdHeaderProperty.Size;
            int lsaHeaderCount = lsaHeaderSpace / OspfLsaCommonHandler.LsaCommonHeaderProperty.Size;
            header.LsaHeaderCount = lsaHeaderCount;

            return DeclareInteration(OspfHandler.LinkCountProperty, lsaHeaderCount, OspfHandler.CurrentLinkIndexProperty);
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            return CreateHeader<OspfDbdHeader>(parameterArray);
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ospfDBDHeader = (OspfDbdHeader)header;

            if (ospfDBDHeader.LsaList != null)
            {
                foreach (var ospfLsaHeader in ospfDBDHeader.LsaList)
                {
                    packetData = Controller.GetSingletonHandler("OSPFLSA").GetBytes(ospfLsaHeader, packetData);
                }
            }
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(ospfDBDHeader.Mtu));
            currentData.AddRange(GetBytes(ospfDBDHeader.Options));
            currentData.AddRange(GetBytes(ospfDBDHeader.Flags));
            currentData.AddRange(GetBytes(ospfDBDHeader.SequenceNumber));
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
using System;
using System.Linq;
using System.ComponentModel.Composition;
using NetInterop.Routing.Ospf.Packet;
using System.Collections.Generic;

namespace NetInterop.Routing.Ospf.Lsa
{
    [Export(typeof(Handler))]
    [DoNotIncludeInDataTree]
    [HandlerMetadata("OSPFLSA", "OSPFDBD", "OSPFLSU", "OSPFLSACK")]
    public class OspfLsaCommonHandler : Handler
    {
        public static GlobalProperty LsaCommonHeaderProperty = GlobalProperty.Register("LsaCommonHeader",
                                                                                       typeof(OspfLsaHeader),
                                                                                       typeof(OspfLsaCommonHandler));

        protected override Boolean CheckForNext()
        {
            return CheckForIteration(OspfHandler.CurrentLinkIndexProperty, OspfHandler.LinkCountProperty);
        }

        public override Handler Parse()
        {
            var header = new OspfLsaHeader();
            header.LSAge = LoadUInt16ReversingEndian();
            header.Options = LoadAndScroll<Byte>();
            header.LSType = LoadAndScroll<Byte>();
            header.LSID = LoadAndScroll<IPAddress>();
            header.AdvertisingRouter = LoadAndScroll<IPAddress>();
            header.SequenceNumber = LoadUInt32ReversingEndian();
            header.Crc = LoadUInt16ReversingEndian();
            header.Length = LoadUInt16ReversingEndian();

            if (HandlerStack.Any(p => p.Name.Equals("OSPFDBD")))
            {
                //++ this blows up now... donno why
                //GetValue<OspfDbdHeader>(OspfDbdHandler.DbdHeaderProperty).LsaList.Add(header);
                return null;
            }
            SetValue(LsaCommonHeaderProperty, header);

            return GetNextHandler();
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            ushort size = (ushort)packetData.Data.Count;

            var ospfLsaHeader = (OspfLsaHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(ospfLsaHeader.LSAge));
            currentData.AddRange(GetBytes(ospfLsaHeader.Options));
            currentData.AddRange(GetBytes(ospfLsaHeader.LSType));
            currentData.AddRange(ospfLsaHeader.LSID.GetBytes());
            currentData.AddRange(ospfLsaHeader.AdvertisingRouter.GetBytes());
            currentData.AddRange(GetBytes(ospfLsaHeader.SequenceNumber));
            currentData.AddRange(new byte[] { 0, 0 });
            currentData.AddRange(GetBytes((ushort)(20 + size)));
            byte[] crc = Nalarium.Checksum.GetCrc(currentData.ToArray());
            currentData[16] = crc[1];
            currentData[17] = crc[0];
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
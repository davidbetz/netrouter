using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Ospf
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPF", "IPV4")]
    [HeaderOwner(typeof(OspfHeader))]
    public class OspfHandler : Handler
    {
        public static GlobalProperty OspfHeaderProperty = GlobalProperty.Register("OspfHeader", typeof(OspfHeader), typeof(OspfHandler));


        public static GlobalProperty CurrentLinkIndexProperty = GlobalProperty.Register("CurrentLinkIndex", typeof(int),
                                                                                        typeof(OspfHandler),
                                                                                        new GlobalPropertyMetadata(
                                                                                            GlobalPropertyMetadataOptions.InternalUseOnly));

        public static GlobalProperty LinkCountProperty = GlobalProperty.Register("LinkCount", typeof(int),
                                                                                 typeof(OspfHandler),
                                                                                 new GlobalPropertyMetadata(
                                                                                     GlobalPropertyMetadataOptions.InternalUseOnly));
        protected override bool CheckForNext()
        {
            return ((IPHeader)GetValue(IPv4Handler.IPv4HeaderProperty)).Protocol == 89;
        }

        public override Handler Parse()
        {
            var header = LoadHeader<OspfHeader>(OspfHeader.Member.Version, 1,
                                                 OspfHeader.Member.Type, 1,
                                                 OspfHeader.Member.Length, 2,
                                                 OspfHeader.Member.RouterID, typeof(IPAddress),
                                                 OspfHeader.Member.AreaID, typeof(IPAddress),
                                                 OspfHeader.Member.Crc, 2,
                                                 OspfHeader.Member.AuthType, 2,
                                                 OspfHeader.Member.Auth1, 4,
                                                 OspfHeader.Member.Auth2, 4);

            SetValue(OspfHeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ospfModule = module as OspfModule;
            var ospfHeader = CreateHeader<OspfHeader>(parameterArray);
            ospfHeader.Version = 2;
            ospfHeader.RouterID = ospfModule.RouterID;
            //ospfHeader.aid = ospfModule.Area;
            ospfHeader.AuthType = 0;
            ospfHeader.Auth1 = 0;
            ospfHeader.Auth2 = 0;
            return ospfHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ospfHeader = (OspfHeader)header;
            var currentData = new List<byte>();
            currentData.Add(ospfHeader.Version);
            currentData.Add(ospfHeader.Type);
            byte[] array = GetBytes((UInt16)(packetData.Data.Count + 24 - packetData.GetProperty<ushort>("OspfLlsDataBlockHeaderSize")));
            currentData.AddRange(array);
            currentData.AddRange(GetNotReversedBytes(ospfHeader.RouterID));
            currentData.AddRange(GetBytes(ospfHeader.AreaID));
            //currentData.AddRange(new byte[] { 1, 2 });
            //currentData.AddRange(new byte[] { 3, 4 });
            //currentData.AddRange(new byte[] { 5, 6, 7, 8 });
            //currentData.AddRange(new byte[] { 9, 0xa, 0xb, 0xc });
            currentData.AddRange(new byte[]
                                 {
                                     0, 0
                                 });
            currentData.AddRange(new byte[]
                                 {
                                     0, 0
                                 });
            currentData.AddRange(new byte[]
                                 {
                                     0, 0, 0, 0
                                 });
            currentData.AddRange(new byte[]
                                 {
                                     0, 0, 0, 0
                                 });
            currentData.AddRange(packetData.Data);
            byte[] crc = Checksum.GetCrc(currentData.ToArray());
            currentData[12] = crc[1];
            currentData[13] = crc[0];
            return packetData.UpdateData(currentData);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLLSDATABLOCK", "OSPFHELLO", "OSPFDBD")]
    public class OspfLlsDataBlockHandler : InterpretiveHandler
    {
        public static GlobalProperty LlsDataBlockHeaderProperty = GlobalProperty.Register("LlsDataBlock",
                                                                                          typeof(OspfLlsDataBlockHeader),
                                                                                          typeof(OspfLlsDataBlockHandler),
                                                                                          new GlobalPropertyMetadata(
                                                                                              GlobalPropertyMetadataOptions.
                                                                                                  Interpretive));

        public static GlobalProperty TotalLengthProperty = GlobalProperty.Register("TotalLength", typeof(UInt16),
                                                                                   typeof(OspfLlsDataBlockHandler),
                                                                                   new GlobalPropertyMetadata(
                                                                                       GlobalPropertyMetadataOptions.
                                                                                           InternalUseOnly));

        public static GlobalProperty CurrentLengthProperty = GlobalProperty.Register("CurrentLength", typeof(UInt16),
                                                                                     typeof(OspfLlsDataBlockHandler),
                                                                                     new GlobalPropertyMetadata(
                                                                                         GlobalPropertyMetadataOptions.
                                                                                             InternalUseOnly));

        public override Interpreter Interpreter
        {
            //TODO: the mpls model seems bettter-- that is, have a tlv parser and the interpreter interprets the single ones.
            get
            {
                return new OspfLlsDataBlockTlvInterpreter();
            }
        }

        protected override Boolean CheckForNext()
        {
            OspfOptions options;
            switch (GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType)
            {
                case OspfPacketType.Hello:
                    options = GetValue<OspfHelloHeader>(OspfHelloHandler.HelloHeaderProperty).OspfOptions;
                    break;
                case OspfPacketType.DatabaseDescriptor:
                    options = GetValue<OspfDbdHeader>(OspfDbdHandler.DbdHeaderProperty).OspfOptions;
                    break;
                default:
                    return false;
            }
            if ((options & OspfOptions.ContainsLLSDataBlock) == OspfOptions.ContainsLLSDataBlock)
            {
                if ((GetValue<IPHeader>(IPv4Handler.IPv4HeaderProperty).TotalLength -
                     GetValue<IPHeader>(IPv4Handler.IPv4HeaderProperty).InternetHeaderLength -
                     GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).Length) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override Handler Parse()
        {
            var header = LoadHeader<OspfLlsDataBlockHeader>("crc", 2, "length", 2);

            ////++ load raw data
            //var tlvAreaLength = (header.length * 4) - 4;
            //header.OspfLlsDataBlockTlv = new byte[tlvAreaLength];
            //for (int i = 0; i < tlvAreaLength; i++)
            //{
            //    header.OspfLlsDataBlockTlv[i] = LoadAndScroll<Byte>();
            //}

            SetValue(LlsDataBlockHeaderProperty, header);
            SetValue(TotalLengthProperty, header.Length);
            SetValue(CurrentLengthProperty, (UInt16)4);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ospfLlsDataBlockHeader = CreateHeader<OspfLlsDataBlockHeader>(parameterArray);
            //ospfLlsDataBlockHeader.OspfLlsDataBlockTlv = new OspfLlsDataBlockTlv[] { (OspfLlsDataBlockTlv)Controller.GetSingletonHandler("OSPFLLSDATABLOCKTLV").Build(module) };
            //ospfLlsDataBlockHeader.length = (ushort)(ospfLlsDataBlockHeader.OspfLlsDataBlockTlv.Sum(p => p.length) + 4);
            return ospfLlsDataBlockHeader;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ospfLlsDataBlockHeader = (OspfLlsDataBlockHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(new byte[]
                                 {
                                     0, 0
                                 });
            var size = (ushort)(packetData.Data.Count + 4);
            packetData.AddProperty("OspfLlsDataBlockHeaderSize", size);
            currentData.AddRange(GetBytes((ushort)(size / 4)));
            byte[] crc = Checksum.GetCrc(currentData.ToArray());
            currentData[0] = 0xff;
            currentData[1] = 0xf6;
            //foreach (var ospfLlsDataBlockTlv in OspfLlsDataBlockHeader.OspfLlsDataBlockTlv)
            //{
            //    var subData = new List<byte>();
            //    Controller.GetSingletonHandler("OSPFLLSDATABLOCKTLV").GetBytes(ospfLlsDataBlockTlv, subData);
            //    currentData.AddRange(subData);
            //}
            currentData.AddRange(packetData.Data);
            return packetData.UpdateData(currentData);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLLSDATABLOCKTLV", "OSPFLLSDATABLOCK")]
    public class OspfLlsDataBlockTlvHandler : InterpretiveHandler
    {
        public static GlobalProperty TlvProperty = GlobalProperty.Register("Tlv", typeof(OspfLlsDataBlockTlv), typeof(OspfLlsDataBlockTlvHandler),
                                                                           new GlobalPropertyMetadata(
                                                                               GlobalPropertyMetadataOptions.
                                                                                   Interpretive));

        //TODO: This could be a problem for the iterations, since the parent would be this current class. Perhaps I need to extend the shared parent concept.

        public override Interpreter Interpreter
        {
            get
            {
                return new OspfLlsDataBlockTlvInterpreter();
            }
        }

        protected override Boolean CheckForNext()
        {
            return true;
        }

        public override Handler Parse()
        {
            var header = new OspfLlsDataBlockTlv();
            header.Type = LoadUInt16ReversingEndian();
            header.Length = LoadUInt16ReversingEndian();
            header.Value = new byte[header.Length];
            for (int i = 0; i < header.Length; i++)
            {
                header.Value[i] = LoadAndScroll<Byte>();
            }

            SetValue(TlvProperty, header);
            SetValue(RootHandler.SharedParentProperty, true);

            int currentLength = ((UInt16)GetValue(OspfLlsDataBlockHandler.CurrentLengthProperty)) + header.Length;
            var totalLength = (UInt16)GetValue(OspfLlsDataBlockHandler.TotalLengthProperty);
            if (currentLength < totalLength)
            {
                SetValue(OspfLlsDataBlockHandler.CurrentLengthProperty, (UInt16)currentLength);
                SetValue(RootHandler.SharedParentProperty, true);
                return GetNextHandler();
            }

            return null;
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            //+ for now, extended options tlv only (and always)
            var ospfLlsDataBlockTlv = CreateHeader<OspfLlsDataBlockTlv>(parameterArray);
            ospfLlsDataBlockTlv.Type = 1;
            ospfLlsDataBlockTlv.Length = 4;
            ospfLlsDataBlockTlv.Value = new byte[]
                                            {
                                                0, 0, 0, 1
                                            };
            return ospfLlsDataBlockTlv;
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            if (!Collection.IsNullOrEmpty(packetData.Data))
            {
                //TODO: if this becomes iterative, this will have to change
                throw new InvalidOperationException("There should never be data after the TLV area of an OSPF LLS data block");
            }
            var ospfLlsDataBlockTlv = (OspfLlsDataBlockTlv)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(ospfLlsDataBlockTlv.Type));
            currentData.AddRange(GetBytes(ospfLlsDataBlockTlv.Length));
            currentData.AddRange(ospfLlsDataBlockTlv.Value);
            return packetData.UpdateData(currentData);
        }
    }
}
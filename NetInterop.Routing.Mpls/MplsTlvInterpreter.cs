namespace NetInterop.Routing.Mpls
{
    public class MplsTlvInterpreter : Interpreter
    {
        private LdpTlv _data;

        public override object Interpret(object obj)
        {
            TryGetData(obj, out _data);

            switch (_data.Type)
            {
                    //case 0x100: return GetFecData();
                    //case 0x101: return GetAddressListData();
                    //case 0x103: return GetHopCountData();
                    //case 0x104: return GetPathVectorData();
                    //case 0x200: return GetGenericLabelData();
                    //case 0x201: return GetATMLabelData();
                    //case 0x202: return GetFrameRelayLabelData();
                    //case 0x300: return GetStatusData();
                    //case 0x301: return GetExtendedStatusData();
                    //case 0x302: return GetReturnedPDUData();
                    //case 0x303: return GetReturnedMessageData();
                case 0x400:
                    return GetCommonHelloParametersData();
                case 0x401:
                    return GetTransportAddressData();
                    //case 0x402: return GetConfigurationSequenceNumberData();
                    //case 0x500: return GetCommonSessionParametersData();
                    //case 0x501: return GetATMSessionParametersData();
                    //case 0x502: return GetFrameRelaySessionParametersData();
                    //case 0x600: return GetLabelRequestMessageIDData();
                default:
                    return obj;
            }
        }

        private object GetCommonHelloParametersData()
        {
            ushort holdTime = ReadUInt16(_data.Value, 0);
            ushort options = ReadUInt16(_data.Value, 2);
            return new
                   {
                       type = _data.TypeString,
                       length = _data.Length,
                       holdTime,
                       options
                   };
        }

        private object GetTransportAddressData()
        {
            var ipAddress = new IPAddress();
            ipAddress.octet1 = _data.Value[0];
            ipAddress.octet2 = _data.Value[1];
            ipAddress.octet3 = _data.Value[2];
            ipAddress.octet4 = _data.Value[3];
            return new
                   {
                       type = _data.TypeString,
                       length = _data.Length,
                       ipAddress
                   };
        }
    }
}
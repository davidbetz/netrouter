namespace NetInterop.Routing.Ospf.Packet
{
    public class OspfLlsDataBlockTlvInterpreter : Interpreter
    {
        private OspfLlsDataBlockTlv _data;

        public override object Interpret(object obj)
        {
            TryGetData(obj, out _data);

            switch (_data.Type)
            {
                case 1:
                    return GetExtendedOptionData();
                case 2:
                    return CryptographicAuthenticationData();
                default:
                    return obj;
            }
        }

        private object GetExtendedOptionData()
        {
            return new
                   {
                       options = GetBytes(_data.Value, _data.Length)
                   };
            //ushort holdTime = ReadUInt16(_data.value, 0);
            //ushort options = ReadUInt16(_data.value, 2);
            //return new
            //{
            //    type = _data.Type,
            //    _data.length,
            //    holdTime,
            //    options
            //};
        }

        private object CryptographicAuthenticationData()
        {
            return new
                   {
                   };
            //var ipAddress = new ipAddress();
            //ipAddress.octet1 = _data.value[0];
            //ipAddress.octet2 = _data.value[1];
            //ipAddress.octet3 = _data.value[2];
            //ipAddress.octet4 = _data.value[3];
            //return new
            //{
            //    type = _data.Type,
            //    _data.length,
            //    ipAddress
            //};
        }

        //private OspfLlsDataBlockTlv _data;

        //public override object Interpret(object obj)
        //{
        //    if (obj == null)
        //    {
        //        return new object { };
        //    }
        //    TryGetData(obj, out _data);

        //    var result = new List<OspfLlsDataBlockTlv>();

        //    var tlvAreaLength = (_data.length * 4) - 4;
        //    if (tlvAreaLength == 0)
        //    {
        //        return result;
        //    }
        //    for (int i = 0; i < tlvAreaLength; )
        //    {
        //        var tlv = new OspfLlsDataBlockTlv();
        //        tlv.type = _data.OspfLlsDataBlockTlv[i++];
        //        tlv.length = _data.OspfLlsDataBlockTlv[i++];
        //        tlv.value = new byte[tlv.length];

        //        for (int j = 0; j < tlv.length; j++)
        //        {
        //            i++;
        //            tlv.value[j] = _data.OspfLlsDataBlockTlv[j];
        //        }

        //        result.Add(tlv);
        //    }

        //    return result;
        //}
    }
}
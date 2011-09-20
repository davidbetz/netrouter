using System.Collections.Generic;

namespace NetInterop.Routing.Bgp
{
    public class BgpOpenMessageTlvInterpreter : Interpreter
    {
        private BgpOpenHeader _data;

        public override object Interpret(object obj)
        {
            TryGetData(obj, out _data);

            var result = new List<BgpTlv>();

            for (int i = 0; i < _data.OptionalParameterLength; i++)
            {
                var tlv = new BgpTlv();
                tlv.Type = _data.OptionalParameter[i++];
                tlv.Length = _data.OptionalParameter[i++];
                tlv.Value = new byte[tlv.Length];

                for (int j = 0; j < tlv.Length; j++)
                {
                    i++;
                    tlv.Value[j] = _data.OptionalParameter[j];
                }

                result.Add(tlv);
            }

            return result;
        }
    }
}
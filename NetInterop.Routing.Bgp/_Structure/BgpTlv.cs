using System;

namespace NetInterop.Routing.Bgp
{
    public struct BgpTlv : IHeader
    {
        /// <summary>
        /// Parameter Type is a one octet field that unambiguously
        /// identifies individual parameters.
        /// </summary>
        public byte Length;

        /// <summary>
        /// Parameter Length is a one
        /// octet field that contains the length of the Parameter Value
        /// field in octets. 
        /// </summary>
        public ushort Type;

        /// <summary>
        /// Parameter Value is a variable length field
        /// that is interpreted according to the value of the Parameter
        /// Type field.
        /// </summary>
        public Byte[] Value;
    }
}
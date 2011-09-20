using System;

namespace NetInterop.Routing.Bgp
{
    public struct BgpHeader : IHeader
    {
        /// <summary>
        /// This 2-octet unsigned integer indicates the total length of the
        ///  message, including the header in octets.  Thus, it allows one
        ///  to locate the (Marker field of the) next message in the TCP
        ///  stream.  The value of the Length field MUST always be at least
        ///  19 and no greater than 4096, and MAY be further constrained,
        ///  depending on the message type.  "padding" of extra data after
        ///  the message is not allowed.  Therefore, the Length field MUST
        ///  have the smallest value required, given the rest of the
        ///  message.
        /// </summary>
        public ushort Length;

        /// <summary>
        /// This 16-octet field is included for compatibility; it MUST be
        /// set to all ones.
        /// </summary>
        public UInt32 Marker1;
        public UInt32 Marker2;
        public UInt32 Marker3;
        public UInt32 Marker4;

        public byte type;

        [FieldOverride("type")]
        public BgpMessageType BgpMessageType
        {
            get
            {
                return (BgpMessageType)type;
            }
        }
    }
}
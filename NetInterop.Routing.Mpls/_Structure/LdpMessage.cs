using System;

namespace NetInterop.Routing.Mpls
{
    public struct LdpMessage : IHeader
    {
        public UInt32 ID;
        public ushort Length;
        public ushort Type;

        [FieldOverride("type")]
        public String TypeString
        {
            get
            {
                switch (Type)
                {
                    case 0x001:
                        return "Notification";
                    case 0x100:
                        return "Hello";
                    case 0x200:
                        return "Initialization";
                    case 0x201:
                        return "Keep Alive";
                    case 0x300:
                        return "Address";
                    case 0x301:
                        return "Address Withdraw";
                    case 0x400:
                        return "Label Mapping";
                    case 0x401:
                        return "Label Request";
                    case 0x404:
                        return "Label Abort request";
                    case 0x402:
                        return "Label Withdraw";
                    case 0x403:
                        return "Label Release";
                    default:
                        return "Unknown Message Name";
                }
            }
        }
    }
}
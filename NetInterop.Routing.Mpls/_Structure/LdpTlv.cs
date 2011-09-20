using System;

namespace NetInterop.Routing.Mpls
{
    public struct LdpTlv : IHeader
    {
        public ushort Length;
        public ushort Type;
        public Byte[] Value;

        [FieldOverride("type")]
        public String TypeString
        {
            get
            {
                switch (Type)
                {
                    case 0x100:
                        return "Fec";
                    case 0x101:
                        return "Address List";
                    case 0x103:
                        return "Hop Count";
                    case 0x104:
                        return "Path Vector";
                    case 0x200:
                        return "Generic Label";
                    case 0x201:
                        return "ATM Label";
                    case 0x202:
                        return "Frame Relay Label";
                    case 0x300:
                        return "Status";
                    case 0x301:
                        return "Extended Status";
                    case 0x302:
                        return "Returned PDU";
                    case 0x303:
                        return "Returned Message";
                    case 0x400:
                        return "Common Hello Parameters.";
                    case 0x401:
                        return "Transport Address";
                    case 0x402:
                        return "Configuration Sequence Number";
                    case 0x500:
                        return "Common Session Parameters";
                    case 0x501:
                        return "ATM Session Parameters";
                    case 0x502:
                        return "Frame Relay Session Parameters";
                    case 0x600:
                        return "Label Request Message ID";
                    default:
                        return "Unknown Message Name";
                }
            }
        }
    }
}
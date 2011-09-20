using System;

namespace NetInterop.Routing.Ospf
{
    [Flags]
    public enum OspfOptions : byte
    {
        Dn = 0x80,
        O = 0x40,
        Dc = 0x20,
        ContainsLLSDataBlock = 0x10,
        Np = 0x08,
        Mc = 0x04,
        ExternalRoutingCapable = 0x02,
        Mt = 0x01
    }
}
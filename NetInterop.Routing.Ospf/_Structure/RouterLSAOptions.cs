using System;

namespace NetInterop.Routing.Ospf
{
    [Flags]
    public enum RouterLSAOptions : byte
    {
        Abr = 0x01,
        Asbr = 0x02,
        VirtualEndpoint = 0x04
    }
}

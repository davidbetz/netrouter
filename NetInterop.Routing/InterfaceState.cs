using System;

namespace NetInterop.Routing
{
    [Flags]
    public enum InterfaceState
    {
        Down = 0,
        L2Up = 0x01,
        L3Up = 0x02,
        L2UpL3Up = ~0
    }
}
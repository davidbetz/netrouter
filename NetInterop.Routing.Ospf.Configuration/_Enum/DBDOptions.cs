using System;

namespace NetInterop.Routing.Ospf
{
    [Flags]
    public enum DBDOptions : byte
    {
        Master = 0x01,
        More = 0x02,
        Initial = 0x04
    }
}
using System;

namespace NetInterop.Routing
{
    [Flags]
    public enum GlobalPropertyMetadataOptions
    {
        InternalUseOnly = 0x01,
        Interpretive = 0x02,
        Preferred = 0x04
    }
}
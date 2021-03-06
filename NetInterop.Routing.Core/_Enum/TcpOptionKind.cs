using System;

namespace NetInterop.Routing.Core
{
    public enum TcpOptionKind : byte
    {
        EOL = 0,
        NOP = 1,
        MSS = 2,
        WSOPT = 3,
        SACKPermitted = 4,
        SACK = 5,
        TSOPT = 8,
        UTO = 28,
        TCPAO = 29,
        Experimental1 = 253,
        Experimental2 = 24
    }
}
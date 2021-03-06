using System;

namespace NetInterop.Routing.Core
{
    [Flags]
    public enum TcpFlags
    {
        Fin = 0x01,
        Syn = 0x02,
        Reset = 0x04,
        Push = 0x08,
        Ack = 0x10,
        Urgent = 0x20,
        ECE = 0x40,
        CWR = 0x80,
        NS = 0x100
    }
}
using System;

namespace NetInterop.Connection
{
    public delegate void Received(String deviceID, int len, IntPtr ptr);
}
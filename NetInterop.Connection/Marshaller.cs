using System;
using System.Runtime.InteropServices;

namespace NetInterop.Connection
{
    public static class Marshaller
    {
        public static T ToStructure<T>(IntPtr ptr) where T : struct
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
    }
}
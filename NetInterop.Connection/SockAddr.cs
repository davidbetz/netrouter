
namespace NetInterop.Connection
{
    public struct SockAddr
    {
        public short sin_family;
        public short sin_port;
        public uint data;
        public string sin_zero;
    };
}
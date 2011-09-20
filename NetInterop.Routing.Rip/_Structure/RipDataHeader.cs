using System.Collections.Generic;

namespace NetInterop.Routing.Rip
{
    public struct RipDataHeader : IHeader, IReversable
    {
        public ushort AddressFamily;
        public IPAddress Mask;
        public uint Metric;
        public IPAddress Network;
        public IPAddress NextHop;
        public ushort RouteTag;

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static byte[] GetBytes(RipDataHeader ripDataHeader)
        {
            var list = new List<byte>();
            list.AddRange(ByteReader.GetBytes(ripDataHeader.AddressFamily));
            list.AddRange(ByteReader.GetBytes(ripDataHeader.RouteTag));
            list.AddRange(ripDataHeader.Network.GetBytes());
            list.AddRange(ripDataHeader.Mask.GetBytes());
            list.AddRange(ripDataHeader.NextHop.GetBytes());
            list.AddRange(ByteReader.GetBytes(ripDataHeader.Metric));
            return list.ToArray();
        }
    }
}
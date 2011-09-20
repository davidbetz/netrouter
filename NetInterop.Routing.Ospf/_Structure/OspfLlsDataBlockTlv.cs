namespace NetInterop.Routing.Ospf
{
    /// <summary>
    /// Naote that TLVs are always padded to 32-bit boundary, but padding
    /// bytes are not included in TLV Length field (though it is included in
    /// the LLS Data Length field of the LLS block header).
    /// </summary>
    public struct OspfLlsDataBlockTlv : IHeader, IReversable
    {
        public ushort Length;
        public ushort Type;
        public byte[] Value;

        #region IReversable Members

        public byte[] GetBytes()
        {
            var array = new byte[4 + Length];
            return array;
        }

        #endregion
    }
}
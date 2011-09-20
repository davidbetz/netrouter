namespace NetInterop.Routing.Ospf
{
    public struct OspfLlsDataBlockHeader : IHeader
    {
        /// <summary>
        /// The Checksum field contains the standard IP checksum of the entire
        /// contents of the LLS block.
        /// </summary>
        public ushort Crc;

        /// <summary>
        /// (in 32-bit words)
        /// Implementations should not use the Length field in the IP packet
        /// header to determine the length of the LLS data block.
        /// </summary>
        public ushort Length;

        public OspfLlsDataBlockTlv[] OspfLlsDataBlockTlv;
    }
}
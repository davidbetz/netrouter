namespace NetInterop.Routing.Mpls
{
    public struct LdpHeader : IHeader
    {
        public ushort LabelSpaceID;
        public IPAddress LSRID;
        public ushort PduLength;
        public ushort Version;
    }
}
namespace NetInterop.Routing.Bgp
{
    public struct BgpOpenHeader : IHeader
    {
        public ushort asn;

        /// <summary>
        /// This 2-octet unsigned integer indicates the number of seconds
        /// the sender proposes for the value of the Hold Timer.  Upon
        /// receipt of an OPEN message, a BGP speaker MUST calculate the
        /// value of the Hold Timer by using the smaller of its configured
        /// Hold Time and the Hold Time received in the OPEN message.  The
        /// Hold Time MUST be either zero or at least three seconds.  An
        /// implementation MAY reject connections on the basis of the Hold
        /// Time. The calculated value indicates the maximum number of
        /// seconds that may elapse between the receipt of successive
        /// KEEPALIVE and/or UPDATE messages from the sender.
        /// </summary>
        public ushort Holdtime;

        public uint ID;

        public byte[] OptionalParameter;
        public byte OptionalParameterLength;
        public byte Version;
    }
}
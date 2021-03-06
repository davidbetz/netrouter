using System;

namespace NetInterop.Routing.Http
{
    public struct HttpIdentification
    {
        public static readonly HttpIdentification HttpIdentifier = From(0x48, 0x54, 0x54, 0x50);

        public byte octet1;
        public byte octet2;
        public byte octet3;
        public byte octet4;

        public static Boolean operator ==(HttpIdentification a, HttpIdentification b)
        {
            return a.octet1 == b.octet1
                   && a.octet2 == b.octet2
                   && a.octet3 == b.octet3
                   && a.octet4 == b.octet4;
        }

        public static bool operator !=(HttpIdentification a, HttpIdentification b)
        {
            return !(a == b);
        }

        public static HttpIdentification From(byte p1, byte p2, byte p3, byte p4)
        {
            return new HttpIdentification
                {
                    octet1 = p1,
                    octet2 = p2,
                    octet3 = p3,
                    octet4 = p4
                };
        }
    }
}
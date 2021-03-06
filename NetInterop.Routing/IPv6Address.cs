using System;
using System.Numerics;
using Nalarium;

namespace NetInterop.Routing
{
    public struct IPv6Address : IHeader, IReversable, IComparable<IPv6Address>, IHasStandardFormat, IEquatable<IPv6Address>
    {
        public byte octet1;
        public byte octet2;
        public byte octet3;
        public byte octet4;
        public byte octet5;
        public byte octet6;
        public byte octet7;
        public byte octet8;
        public byte octet9;
        public byte octet10;
        public byte octet11;
        public byte octet12;
        public byte octet13;
        public byte octet14;
        public byte octet15;
        public byte octet16;

        #region IComparable<IPv6Address> Members

        public int CompareTo(IPv6Address other)
        {
            BigInteger first = GetBigInteger(this);
            BigInteger second = GetBigInteger(other);
            if (first < second)
            {
                return -1;
            }
            else if (first > second)
            {
                return 1;
            }
            return 0;
        }

        #endregion

        #region IEquatable<IPv6Address> Members

        public bool Equals(IPv6Address other)
        {
            if (octet1 == other.octet1 &&
                octet2 == other.octet2 &&
                octet3 == other.octet3 &&
                octet4 == other.octet4 &&
                octet5 == other.octet5 &&
                octet6 == other.octet6 &&
                octet7 == other.octet7 &&
                octet8 == other.octet8 &&
                octet9 == other.octet9 &&
                octet10 == other.octet10 &&
                octet11 == other.octet11 &&
                octet12 == other.octet12 &&
                octet13 == other.octet13 &&
                octet14 == other.octet14 &&
                octet15 == other.octet15 &&
                octet16 == other.octet16)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region IHasStandardFormat Members

        public String StandardFormat
        {
            get
            {
                return string.Format("{0:X2}{1:X2}:{2:X2}{3:X2}:{4:X2}{5:X2}:{6:X2}{7:X2}:{8:X2}{9:X2}:{10:X2}{11:X2}:{12:X2}{13:X2}:{14:X2}{15:X2}",
                                     octet1,
                                     octet2,
                                     octet3,
                                     octet4,
                                     octet5,
                                     octet6,
                                     octet7,
                                     octet8,
                                     octet9,
                                     octet10,
                                     octet11,
                                     octet12,
                                     octet13,
                                     octet14,
                                     octet15,
                                     octet16);
            }
        }

        #endregion

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static bool operator ==(IPv6Address address1, IPv6Address address2)
        {
            return address1.Equals(address2);
        }

        public static bool operator !=(IPv6Address address1, IPv6Address address2)
        {
            return !address1.Equals(address2);
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }

        public static IPv6Address From(byte[] partArray)
        {
            if (Collection.IsNullOrTooSmall(partArray, 4))
            {
                return new IPv6Address();
            }
            return new IPv6Address
                   {
                       octet1 = partArray[0],
                       octet2 = partArray[1],
                       octet3 = partArray[2],
                       octet4 = partArray[3],
                       octet5 = partArray[4],
                       octet6 = partArray[5],
                       octet7 = partArray[6],
                       octet8 = partArray[7],
                       octet9 = partArray[8],
                       octet10 = partArray[9],
                       octet11 = partArray[10],
                       octet12 = partArray[11],
                       octet13 = partArray[12],
                       octet14 = partArray[13],
                       octet15 = partArray[14],
                       octet16 = partArray[15]
                   };
        }

        public static IPv6Address From(byte p1, byte p2, byte p3, byte p4,
                                        byte p5, byte p6, byte p7, byte p8,
                                        byte p9, byte p10, byte p11, byte p12,
                                        byte p13, byte p14, byte p15, byte p16)
        {
            return new IPv6Address
                   {
                       octet1 = p1,
                       octet2 = p2,
                       octet3 = p3,
                       octet4 = p4,
                       octet5 = p5,
                       octet6 = p6,
                       octet7 = p7,
                       octet8 = p8,
                       octet9 = p9,
                       octet10 = p10,
                       octet11 = p11,
                       octet12 = p12,
                       octet13 = p13,
                       octet14 = p14,
                       octet15 = p15,
                       octet16 = p16
                   };
        }

        public static IPv6Address From(string ipAddressAsText)
        {
            if (String.IsNullOrEmpty(ipAddressAsText))
            {
                return new IPv6Address();
            }
            string[] partArray = ipAddressAsText.Split(':');
            if (partArray.Length != 8)
            {
                return new IPv6Address();
            }
            var buffer = new byte[16];
            int i = 0;
            foreach (string item in partArray)
            {
                byte[] array = BitConverter.GetBytes(Parser.ParseUInt16(item));
                buffer[i++] = array[0];
                buffer[i++] = array[1];
            }
            return From(buffer);
        }

        public static BigInteger GetBigInteger(IPv6Address ipv6Address)
        {
            return new BigInteger(ipv6Address.GetBytes());
        }

        public static byte[] GetBytes(IPv6Address ipv6Address)
        {
            return new[]
                   {
                       ipv6Address.octet1,
                       ipv6Address.octet2,
                       ipv6Address.octet3,
                       ipv6Address.octet4,
                       ipv6Address.octet5,
                       ipv6Address.octet6,
                       ipv6Address.octet7,
                       ipv6Address.octet8,
                       ipv6Address.octet9,
                       ipv6Address.octet10,
                       ipv6Address.octet11,
                       ipv6Address.octet12,
                       ipv6Address.octet13,
                       ipv6Address.octet14,
                       ipv6Address.octet15,
                       ipv6Address.octet16
                   };
        }
    }
}
using System;
using System.Linq;
using Nalarium;

namespace NetInterop.Routing
{
    public struct IPAddress : IHeader, IReversable, IComparable<IPAddress>, IHasStandardFormat, IEquatable<IPAddress>
    {
        public static readonly IPAddress NetworkAMask = From(0xff, 0, 0, 0);
        public static readonly IPAddress NetworkBMask = From(0xff, 0xff, 0, 0);
        public static readonly IPAddress NetworkCMask = From(0xff, 0xff, 0xff, 0);
        public static readonly IPAddress Broadcast = From(0xff, 0xff, 0xff, 0xff);

        public byte octet1;
        public byte octet2;
        public byte octet3;
        public byte octet4;

        public bool IsGlobalMulticast
        {
            get
            {
                return IsMulticast && !IsLocalMulticast;
            }
        }

        public bool IsLocalMulticast
        {
            get
            {
                return (octet1 & 0xEF) == 0xEF;
            }
        }

        public bool IsMulticast
        {
            get
            {
                return (octet1 & 0xE0) == 0xE0;
            }
        }

        public Boolean IsMask
        {
            get
            {
                return GetBitCount() != -1;
            }
        }

        #region IComparable<IPAddress> Members

        public int CompareTo(IPAddress other)
        {
            uint first = GetUInt32(this);
            uint second = GetUInt32(other);
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

        #region IEquatable<IPAddress> Members

        public bool Equals(IPAddress other)
        {
            return other.octet1 == octet1 && other.octet2 == octet2 && other.octet3 == octet3 && other.octet4 == octet4;
        }

        #endregion

        #region IHasStandardFormat Members

        public String StandardFormat
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}",
                                     octet1,
                                     octet2,
                                     octet3,
                                     octet4);
            }
        }

        #endregion

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static bool operator <(IPAddress address1, IPAddress address2)
        {
            return address1.GetUInt32() < address2.GetUInt32();
        }

        public static bool operator >(IPAddress address1, IPAddress address2)
        {
            return address1.GetUInt32() > address2.GetUInt32();
        }

        public static bool operator ==(IPAddress address1, IPAddress address2)
        {
            return address1.Equals(address2);
        }

        public static bool operator !=(IPAddress address1, IPAddress address2)
        {
            return !address1.Equals(address2);
        }

        public override String ToString()
        {
            return StandardFormat;
        }

        public static IPAddress From(byte[] partArray)
        {
            if (Collection.IsNullOrTooSmall(partArray, 4))
            {
                return new IPAddress();
            }
            return new IPAddress
                   {
                       octet1 = partArray[0],
                       octet2 = partArray[1],
                       octet3 = partArray[2],
                       octet4 = partArray[3]
                   };
        }

        public static IPAddress From(byte p1, byte p2, byte p3, byte p4)
        {
            return new IPAddress
                   {
                       octet1 = p1,
                       octet2 = p2,
                       octet3 = p3,
                       octet4 = p4
                   };
        }

        public static IPAddress GetDefault()
        {
            return From(0, 0, 0, 0);
        }

        public static IPAddress From(string ipAddressAsText)
        {
            if (String.IsNullOrEmpty(ipAddressAsText))
            {
                return new IPAddress();
            }
            string[] partArray = ipAddressAsText.Split('.');
            if (partArray.Length != 4)
            {
                return new IPAddress();
            }
            return new IPAddress
                   {
                       octet1 = Parser.ParseByte(partArray[0]),
                       octet2 = Parser.ParseByte(partArray[1]),
                       octet3 = Parser.ParseByte(partArray[2]),
                       octet4 = Parser.ParseByte(partArray[3])
                   };
        }

        public static uint GetUInt32(IPAddress ip_address)
        {
            return (uint)(ip_address.octet1 << 24) + (uint)(ip_address.octet2 << 16) + (uint)(ip_address.octet3 << 8) + (uint)(ip_address.octet4);
        }

        public uint GetUInt32()
        {
            return GetUInt32(this);
        }

        public static byte[] GetBytes(IPAddress ip_address)
        {
            var array = new byte[4];
            array[0] = ip_address.octet1;
            array[1] = ip_address.octet2;
            array[2] = ip_address.octet3;
            array[3] = ip_address.octet4;
            return array;
        }

        public bool IsSameNetwork(IPAddress address, IPAddress mask)
        {
            IPAddress network1 = GetNetwork(mask);
            IPAddress network2 = address.GetNetwork(mask);

            return network1 == network2;
        }

        public IPAddress FlipBits()
        {
            return From(GetBytes().Select(p => (byte)~p).ToArray());
        }

        public static char GetClass(IPAddress ip_address)
        {
            byte b = ip_address.GetBytes()[0];
            return (b & 0xE0) == 0xE0 ? 'D' :
                                                (b & 0xC0) == 0xC0 ? 'C' :
                                                                             (b & 0x80) == 0x80 ? 'B' :
                                                                                                          'A';
        }

        public char GetClass()
        {
            return GetClass(this);
        }

        public static IPAddress GetClassMask(IPAddress ip_address)
        {
            switch (ip_address.GetClass())
            {
                case 'A':
                    return NetworkAMask;
                case 'B':
                    return NetworkAMask;
                case 'C':
                    return NetworkAMask;
            }
            return From("0.0.0.0");
        }

        public IPAddress GetClassMask()
        {
            return GetClassMask(this);
        }

        public static IPAddress GetNetwork(IPAddress address, IPAddress mask)
        {
            byte[] currentBytes = address.GetBytes();
            var otherBytes = new byte[4];
            byte[] maskBytes = mask.GetBytes();
            for (int i = 0; i < 4; i++)
            {
                otherBytes[i] = (byte)(currentBytes[i] & maskBytes[i]);
            }
            return From(otherBytes);
        }

        public IPAddress GetNetwork(IPAddress mask)
        {
            return GetNetwork(this, mask);
        }

        public static IPAddress GetClassAddress(IPAddress ipAddress)
        {
            return GetNetwork(ipAddress, ipAddress.GetClassMask());
        }

        public IPAddress GetClassAddress()
        {
            return GetClassAddress(this);
        }

        public static MacAddress GetMacAddress(IPAddress ipAddress)
        {
            var segment4 = (byte)(ipAddress.octet3 & 127);
            return MacAddress.From(new byte[]
                                    {
                                        1, 0, 94, segment4, ipAddress.octet3, ipAddress.octet4
                                    });
        }

        public MacAddress GetMacAddress()
        {
            return GetMacAddress(this);
        }

        public int GetBitCount()
        {
            int count = 0;
            byte[] byteArray = GetBytes();
            for (int i = 0; i < 4; i++)
            {
                switch (byteArray[i])
                {
                    case 128:
                        count++;
                        break;
                    case 192:
                        count += 2;
                        break;
                    case 224:
                        count += 3;
                        break;
                    case 240:
                        count += 4;
                        break;
                    case 248:
                        count += 5;
                        break;
                    case 252:
                        count += 6;
                        break;
                    case 254:
                        count += 7;
                        break;
                    case 255:
                        count += 8;
                        break;
                    case 0:
                        continue;
                    default:
                        return -1;
                }
            }
            return count;
        }

        public bool IsHostAddress(IPAddress primaryMask)
        {
            throw new NotImplementedException();
            //TODO: finish this
            return true;
        }
    }
}
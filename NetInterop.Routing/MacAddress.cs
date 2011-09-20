using System;
using Nalarium;

namespace NetInterop.Routing
{
    public struct MacAddress : IHeader, IReversable, IHasStandardFormat, IEquatable<MacAddress>
    {
        public static readonly MacAddress Broadcast = From(255, 255, 255, 255, 255, 255);
        public static readonly MacAddress None = From(0, 0, 0, 0, 0, 0);

        public byte segment1;
        public byte segment2;
        public byte segment3;
        public byte segment4;
        public byte segment5;
        public byte segment6;

        #region IEquatable<MacAddress> Members

        public bool Equals(MacAddress other)
        {
            return
                other.segment1 == segment1 &&
                other.segment2 == segment2 &&
                other.segment3 == segment3 &&
                other.segment4 == segment4 &&
                other.segment5 == segment5 &&
                other.segment6 == segment6;
        }

        #endregion

        #region IHasStandardFormat Members

        public String StandardFormat
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
                                     segment1,
                                     segment2,
                                     segment3,
                                     segment4,
                                     segment5,
                                     segment6);
            }
        }

        #endregion

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static bool operator ==(MacAddress address1, MacAddress address2)
        {
            return address1.Equals(address2);
        }

        public static bool operator !=(MacAddress address1, MacAddress address2)
        {
            return !address1.Equals(address2);
        }

        public static bool IsSameMacAddress(MacAddress macAddress, byte[] array)
        {
            if (array == null)
            {
                return false;
            }
            if (array.Length != 6)
            {
                return false;
            }
            if (
                macAddress.segment1 == array[0] &&
                macAddress.segment2 == array[1] &&
                macAddress.segment3 == array[2] &&
                macAddress.segment4 == array[3] &&
                macAddress.segment5 == array[4] &&
                macAddress.segment6 == array[5])
            {
                return true;
            }
            return false;
        }

        public static MacAddress From(byte[] partArray)
        {
            if (Collection.IsNullOrTooSmall(partArray, 6))
            {
                return new MacAddress();
            }
            return new MacAddress
                   {
                       segment1 = partArray[0],
                       segment2 = partArray[1],
                       segment3 = partArray[2],
                       segment4 = partArray[3],
                       segment5 = partArray[4],
                       segment6 = partArray[5]
                   };
        }

        public static MacAddress From(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6)
        {
            return new MacAddress
                   {
                       segment1 = p1,
                       segment2 = p2,
                       segment3 = p3,
                       segment4 = p4,
                       segment5 = p5,
                       segment6 = p6
                   };
        }

        public static MacAddress From(string macAsText)
        {
            if (String.IsNullOrEmpty(macAsText))
            {
                return new MacAddress();
            }
            char separator = macAsText.Contains(":") ? ':' : ' ';
            string[] partArray = macAsText.Split(separator);
            if (partArray.Length != 6)
            {
                return new MacAddress();
            }
            return new MacAddress
                   {
                       segment1 = Parser.ParseByte(partArray[0]),
                       segment2 = Parser.ParseByte(partArray[1]),
                       segment3 = Parser.ParseByte(partArray[2]),
                       segment4 = Parser.ParseByte(partArray[3]),
                       segment5 = Parser.ParseByte(partArray[4]),
                       segment6 = Parser.ParseByte(partArray[5])
                   };
        }

        public static byte[] GetBytes(MacAddress macAddress)
        {
            var array = new byte[6];
            array[0] = macAddress.segment1;
            array[1] = macAddress.segment2;
            array[2] = macAddress.segment3;
            array[3] = macAddress.segment4;
            array[4] = macAddress.segment5;
            array[5] = macAddress.segment6;
            return array;
        }

        public override String ToString()
        {
            return StandardFormat;
        }
    }
}
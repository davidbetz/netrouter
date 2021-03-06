using System;

namespace NetInterop.Routing
{
    public static class ByteReader
    {
        private static byte[] GetReverseBytes(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }

        public static byte[] GetNotReversedBytes(object data)
        {
            byte[] array = GetBytes(data);
            return GetReverseBytes(array);
        }

        public static byte[] GetBytes(object data)
        {
            Type type = data.GetType();
            var r = data as IReversable;
            byte[] result = null;
            if (r != null)
            {
                result = r.GetBytes();
            }
            else if (data is byte)
            {
                result = new[]
                         {
                             (byte)data
                         };
            }
            else if (data is UInt16)
            {
                result = BitConverter.GetBytes((UInt16)data);
            }
            else if (data is UInt32)
            {
                result = BitConverter.GetBytes((UInt32)data);
            }
            else if (data is UInt64)
            {
                result = BitConverter.GetBytes((UInt64)data);
            }
            else if (data is Int16)
            {
                result = BitConverter.GetBytes((Int16)data);
            }
            else if (data is Int32)
            {
                result = BitConverter.GetBytes((Int32)data);
            }
            else if (data is Int64)
            {
                result = BitConverter.GetBytes((Int64)data);
            }
            else if (data is byte[])
            {
                result = data as byte[];
            }
            else if (result == null)
            {
                throw new InvalidCastException(string.Format("Unknown data type {0}.", data.GetType().FullName));
            }
            if (BitConverter.IsLittleEndian)
            {
                return GetReverseBytes(result);
            }
            return result;
        }
    }
}
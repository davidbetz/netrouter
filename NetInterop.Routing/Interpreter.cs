using System;

namespace NetInterop.Routing
{
    public abstract class Interpreter
    {
        public abstract object Interpret(object data);

        protected Boolean TryGetData<T>(object obj, out T d)
        {
            d = default(T);
            if (obj == null)
            {
                return false;
            }
            if (!(obj is T))
            {
                return false;
            }
            d = (T)obj;
            return true;
        }

        protected byte[] GetBytes(byte[] data, int length)
        {
            var array = new byte[length];
            Array.Copy(data, array, length);
            return array;
        }

        protected ushort ReadUInt16(byte[] data, int offset)
        {
            return (UInt16)((data[0 + offset] << 8) + data[1 + offset]);
        }

        protected UInt32 ReadUInt32(byte[] data, int offset)
        {
            return
                (UInt32)
                ((data[0 + offset] << 24) + (data[1 + offset] << 16) + (data[2 + offset] << 8) + data[3 + offset]);
        }
    }
}
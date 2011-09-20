using System.Collections.Generic;

namespace NetInterop.Routing.Rip
{
    public struct RipPreambleHeader : IHeader, IReversable
    {
        public byte command;

        [FieldSizeIgnore]
        public RipDataHeader[] DataArray;

        public ushort Domain;
        public byte Version;

        public RipCommand RipCommand
        {
            get
            {
                return (RipCommand)command;
            }
        }

        #region IReversable Members

        public byte[] GetBytes()
        {
            return GetBytes(this);
        }

        #endregion

        public static byte[] GetBytes(RipPreambleHeader ripPreambleHeader)
        {
            var list = new List<byte>();
            list.Add(ripPreambleHeader.command);
            list.Add(ripPreambleHeader.Version);
            list.AddRange(ByteReader.GetBytes(ripPreambleHeader.Domain));
            return list.ToArray();
        }
    }
}
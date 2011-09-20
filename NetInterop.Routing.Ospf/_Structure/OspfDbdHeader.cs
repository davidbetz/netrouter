using System;
using Nalarium;
using System.Collections.Generic;

namespace NetInterop.Routing.Ospf
{
    public struct OspfDbdHeader : IHeader
    {
        public byte Flags;
        public ushort Mtu;
        public byte Options;
        public UInt32 SequenceNumber;

        public List<OspfLsaHeader> LsaList { get; set; }

        public static ushort EthernetAndP2PMtu
        {
            get
            {
                return 1500;
            }
        }

        [FieldOverride("options")]
        public OspfOptions OspfOptions
        {
            get
            {
                var f = new OspfOptions();
                f = f | (OspfOptions)((Options & 0x01));
                f = f | (OspfOptions)((Options & 0x02));
                f = f | (OspfOptions)((Options & 0x04));
                f = f | (OspfOptions)((Options & 0x08));
                f = f | (OspfOptions)((Options & 0x10));
                f = f | (OspfOptions)((Options & 0x20));
                f = f | (OspfOptions)((Options & 0x40));
                f = f | (OspfOptions)((Options & 0x80));
                return f;
            }
        }

        [FieldOverride("flags")]
        public DBDOptions DBDOptions
        {
            get
            {
                var dbdOptions = new DBDOptions();
                dbdOptions = dbdOptions | (DBDOptions)((Flags & 0x01));
                dbdOptions = dbdOptions | (DBDOptions)((Flags & 0x02));
                dbdOptions = dbdOptions | (DBDOptions)((Flags & 0x04));
                return dbdOptions;
            }
            set
            {
                Flags = 0;
                foreach (var item in Enum.GetValues(typeof(DBDOptions)))
                {
                    var v = (byte)item;
                    if (((byte)Options & v) == v)
                    {
                        Flags |= v;
                    }
                }
            }
        }

        public Boolean IsInitial
        {
            get
            {
                return (DBDOptions & DBDOptions.Initial) == DBDOptions.Initial;
            }
            set
            {
                if (value)
                {
                    DBDOptions |= DBDOptions.Initial;
                }
                else
                {
                    unchecked
                    {
                        DBDOptions &= ~DBDOptions.Initial;
                    }
                }
            }
        }

        public Boolean HasMore
        {
            get
            {
                return (DBDOptions & DBDOptions.More) == DBDOptions.More;
            }
            set
            {
                if (value)
                {
                    DBDOptions |= DBDOptions.More;
                }
                else
                {
                    unchecked
                    {
                        DBDOptions &= ~DBDOptions.More;
                    }
                }
            }
        }

        public Boolean IsMaster
        {
            get
            {
                return (DBDOptions & DBDOptions.Master) == DBDOptions.Master;
            }
            set
            {
                if (value)
                {
                    DBDOptions |= DBDOptions.Master;
                }
                else
                {
                    unchecked
                    {
                        DBDOptions &= ~DBDOptions.Master;
                    }
                }
            }
        }

        public int LsaHeaderCount { get; set; }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
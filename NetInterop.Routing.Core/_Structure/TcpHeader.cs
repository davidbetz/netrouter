using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct TcpHeader : IHeader
    {
        public UInt32 Ack;
        public ushort Crc;
        public ushort DestinationPort;
        public ushort Flags;
        public UInt32 SequenceNumber;
        public ushort SourcePort;
        public ushort UrgentPointer;
        public ushort WindowSize;
        public Byte[] NOP;
        public Byte[] MSS;
        public Byte[] WSOPT;
        public Byte[] SACKPermitted;
        public Byte[] SACK;
        public Byte[] TSOPT;
        public Byte[] UTO;
        public Byte[] TCPAO;
        public Byte[] Experimental1;
        public Byte[] Experimental2;

        [FieldOverride("flags")]
        public TcpFlags TcpFlags
        {
            get
            {
                var f = new TcpFlags();
                f = f | (TcpFlags)((Flags & 0x01));
                f = f | (TcpFlags)((Flags & 0x02));
                f = f | (TcpFlags)((Flags & 0x04));
                f = f | (TcpFlags)((Flags & 0x08));
                f = f | (TcpFlags)((Flags & 0x10));
                f = f | (TcpFlags)((Flags & 0x20));
                f = f | (TcpFlags)((Flags & 0x40));
                f = f | (TcpFlags)((Flags & 0x80));
                f = f | (TcpFlags)((Flags & 0x100));
                return f;
            }
        }

        [FieldOverride("flags")]
        public int Length
        {
            get
            {
                return ((Flags & 0x0f000) >> 12) * 4;
            }
        }

        public int NoOperation
        {
            get
            {
                return 0;
            }
        }

        public int MaximumSegmentSize
        {
            get
            {
                return 0;
            }
        }

        public int WindowScalingFactor
        {
            get
            {
                return 0;
            }
        }

        public int SenderSupportsSACK
        {
            get
            {
                return 0;
            }
        }

        public int SACKBlock
        {
            get
            {
                return 0;
            }
        }

        public int Timestamps
        {
            get
            {
                return 0;
            }
        }

        public int UserTimeout
        {
            get
            {
                return 0;
            }
        }

        public int Authentication
        {
            get
            {
                return 0;
            }
        }

        public override String ToString()
        {
            return PairAndSeriesBuilder.CreateSeries(this);
        }
    }
}
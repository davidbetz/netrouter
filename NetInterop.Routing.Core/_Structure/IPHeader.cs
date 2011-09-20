using System;
using Nalarium;

namespace NetInterop.Routing.Core
{
    public struct IPHeader : IHeader
    {
        public ushort Crc;
        public IPAddress DestinationAddress;
        public ushort FlagsFragmentOffset;
        public ushort Identification;
        public UInt32 OptionPadding;
        public byte Protocol;
        public IPAddress SourceAddress;
        public ushort TotalLength;
        public byte TypeOfService;
        public byte TTL;
        public byte VersionIHL;

        [FieldLabel("Version")]
        public UInt32 Version
        {
            get
            {
                return (UInt32)((VersionIHL & 0xF0) >> 4);
            }
        }

        public int InternetHeaderLength
        {
            get
            {
                return ((VersionIHL & 0xf) * 4);
            }
        }

        [FieldLabel("Do not fragment")]
        public Boolean DNF
        {
            get
            {
                return (FlagsFragmentOffset & 0x4000) == 0x4000;
            }
            set
            {
                if (value)
                {
                    FlagsFragmentOffset |= 0x4000;
                }
                else
                {
                    FlagsFragmentOffset &= 0x3FFF;
                }
            }
        }

        [FieldLabel("More fragments")]
        public Boolean MF
        {
            get
            {
                return (FlagsFragmentOffset & 0x2000) == 0x2000;
            }
            set
            {
                if (value)
                {
                    FlagsFragmentOffset |= 0x2000;
                }
                else
                {
                    FlagsFragmentOffset &= 0xDFFF;
                }
            }
        }

        //public override String ToString()
        //{
        //    return PairAndSeriesBuilder.CreateSeries(this);
        //}
    }
}
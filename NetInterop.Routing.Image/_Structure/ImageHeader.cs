using System.Collections.Generic;
using Nalarium;

namespace NetInterop.Routing.Image
{
    public struct ImageHeader : IHeader
    {
        public byte Operation;
        public byte[] Data;
    }
}
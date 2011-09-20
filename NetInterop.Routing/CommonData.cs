using System;

namespace NetInterop.Routing
{
    internal class CommonData
    {
        public IntPtr Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        //public List<Handler> HandlerStack { get; set; }
        //public Dictionary<Tuple<GlobalProperty, Type>, Object> GlobalPropertyData { get; set; }
    }
}
using System;
using System.Diagnostics;

namespace NetInterop.Routing
{
    [DebuggerDisplay("{Name}")]
    public class HandlerDataValue
    {
        public String Name { get; set; }
        public Object Value { get; set; }
    }
}
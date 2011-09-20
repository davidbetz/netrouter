using System;
using System.Collections.Generic;

namespace NetInterop.Routing
{
    public class ParsedEventArgs : EventArgs
    {
        public int Index { get; set; }

        public List<Handler> SerialHandlerList { get; set; }

        public List<HandlerData> HandlerData { get; set; }

        public String Summary { get; set; }
    }
}
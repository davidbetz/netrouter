using System;

namespace NetInterop.Routing
{
    public class ModuleFoundEventArgs : EventArgs
    {
        public String Name { get; set; }
        public Handler Handler { get; set; }
    }
}
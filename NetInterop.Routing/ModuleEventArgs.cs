using System;

namespace NetInterop.Routing
{
    public class ModuleEventArgs : EventArgs
    {
        public String DeviceID { get; set; }

        public HeaderPackage HeaderPackage { get; set; }
    }
}
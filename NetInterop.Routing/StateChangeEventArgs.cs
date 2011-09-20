using System;

namespace NetInterop.Routing
{
    public class StateChangeEventArgs : EventArgs
    {
        public InterfaceState InterfaceState { get; set; }
    }
}
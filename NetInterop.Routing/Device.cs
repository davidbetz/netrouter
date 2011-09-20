using System;
using System.Collections.Generic;

namespace NetInterop.Routing
{
    public class Device
    {
        private InterfaceState _interfaceState;
        private IPConfiguration _primaryIPConfiguration;
        public string ID { get; set; }
        public int Index { get; set; }
        public string Description { get; set; }
        public Boolean IsLoopback { get; set; }

        public MacAddress MacAddress { get; set; }

        public IPAddress Gateway { get; set; }
        public IPv6Address GatewayV6 { get; set; }

        public List<IPConfiguration> SecondaryIPList { get; set; }
        public List<IPv6Configuration> IPv6List { get; set; }

        //++ this controls administratively down
        public bool IsEnabled { get; set; }

        public IPConfiguration PrimaryIPConfiguration
        {
            get
            {
                return _primaryIPConfiguration;
            }
            set
            {
                _primaryIPConfiguration = value;
                _primaryIPConfiguration.Changed += (s, e) => Validatev4AddressAndMask();
            }
        }

        public InterfaceState InterfaceState { get; set; }

        public event StateChange StateChanged = delegate
                                                {
                                                };

        private void Validatev4AddressAndMask()
        {
            InterfaceState _after = InterfaceState;
            if ((_after & InterfaceState.L2Up) != InterfaceState.L2Up)
            {
                return;
            }
            if (_primaryIPConfiguration.IsHost)
            {
                _after |= InterfaceState.L3Up;
            }
            if (_after != InterfaceState)
            {
                OnStateChanged();
            }
        }

        public void OnStateChanged(InterfaceState interfaceState)
        {
            InterfaceState = interfaceState;
            OnStateChanged();
        }

        public void OnStateChanged()
        {
            StateChange handler = StateChanged;
            if (handler != null)
            {
                handler(this, new StateChangeEventArgs
                              {
                                  InterfaceState = InterfaceState
                              });
            }
        }
    }
}
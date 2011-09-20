using System;
using System.Diagnostics;

namespace NetInterop.Routing
{
    [DebuggerDisplay("{Address}, {Mask}")]
    public class IPConfiguration
    {
        private IPAddress _address;
        private IPAddress _mask;

        public IPAddress Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
                Changed(this, null);
            }
        }

        public IPAddress Mask
        {
            get
            {
                return _mask;
            }
            set
            {
                _mask = value;
                Changed(this, null);
            }
        }

        public bool IsHost
        {
            get
            {
                return _address.IsHostAddress(_mask);
            }
        }

        public IPAddress Network
        {
            get
            {
                return Address.GetNetwork(Mask);
            }
        }

        public event EventHandler Changed = delegate
                                            {
                                            };

        public static IPConfiguration Create(IPAddress address, IPAddress mask)
        {
            return new IPConfiguration
                   {
                       Address = address,
                       Mask = mask
                   };
        }
    }
}
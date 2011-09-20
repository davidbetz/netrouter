using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class IPv6AddressCollection : CommentableCollection<IPv6AddressElement>
    {
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as IPv6AddressElement).Address;
        }
    }
}
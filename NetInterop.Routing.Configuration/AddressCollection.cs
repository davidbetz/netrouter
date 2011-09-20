using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class AddressCollection : CommentableCollection<AddressElement>
    {
        protected override Object GetElementKey(ConfigurationElement element)
        {
            var e = element as AddressElement;
            return e.ID + e.IP;
        }
    }
}
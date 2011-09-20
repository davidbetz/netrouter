using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class SecondaryCollection : CommentableCollection<SecondaryElement>
    {
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as SecondaryElement).Address;
        }
    }
}
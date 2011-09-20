using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class RouterCollection : CommentableCollection<RouterElement>
    {
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as RouterElement).Name;
        }
    }
}
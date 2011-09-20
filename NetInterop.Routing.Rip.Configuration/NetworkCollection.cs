using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Rip.Configuration
{
    public class NetworkCollection : CommentableCollection<NetworkElement>
    {
        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as NetworkElement).Network;
        }
    }
}
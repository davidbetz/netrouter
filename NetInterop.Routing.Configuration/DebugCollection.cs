using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class DebugCollection : CommentableCollection<DebugElement>
    {
        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as DebugElement).Category;
        }
    }
}
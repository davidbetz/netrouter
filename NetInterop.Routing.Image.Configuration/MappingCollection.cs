using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Image.Configuration
{
    public class MappingCollection : CommentableCollection<MappingElement>
    {
        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as MappingElement).Virtual;
        }
    }
}
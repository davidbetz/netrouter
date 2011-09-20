using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    public class AreaCollection : CommentableCollection<AreaElement>
    {
        protected override string ElementName
        {
            get
            {
                return "setup";
            }
        }

        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as AreaElement).Area;
        }
    }
}
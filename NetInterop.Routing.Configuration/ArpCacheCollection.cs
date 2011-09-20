using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    public class ArpCacheCollection : CommentableCollection<ArpCacheElement>
    {
        [ConfigurationProperty("gratuitousOnStartup", IsRequired = false, DefaultValue = false)]
        public bool GratuitousOnStartup
        {
            get
            {
                return (bool)this["gratuitousOnStartup"];
            }
        }

        //- #GetElementKey -//
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as ArpCacheElement).Address;
        }
    }
}
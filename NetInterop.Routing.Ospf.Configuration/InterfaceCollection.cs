using System;
using System.Configuration;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    public class InterfaceCollection : CommentableCollection<InterfaceElement>
    {
        protected override string ElementName
        {
            get
            {
                return "settings";
            }
        }
        [ConfigurationProperty("passiveDefault", IsRequired = false, DefaultValue = false)]
        public bool PassiveDefault
        {
            get
            {
                return (bool)this["passiveDefault"];
            }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return (element as InterfaceElement).DeviceID;
        }
    }
}
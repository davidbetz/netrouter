#region Copyright

//+ Nalarium Pro 3.0 - Web Module
//+ Copyright © Jampad Technology, Inc. 2008-2010

#endregion

using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Ospf.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class OspfSection : ConfigurationSection
    {
        [ConfigurationProperty("areas")]
        [ConfigurationCollection(typeof(AreaElement), AddItemName = "setup")]
        public AreaCollection Areas
        {
            get
            {
                return (AreaCollection)this["areas"];
            }
        }

        [ConfigurationProperty("interfaces")]
        [ConfigurationCollection(typeof(InterfaceElement), AddItemName = "settings")]
        public InterfaceCollection Interfaces
        {
            get
            {
                return (InterfaceCollection)this["interfaces"];
            }
        }

        [ConfigurationProperty("networks")]
        [ConfigurationCollection(typeof(NetworkElement), AddItemName = "add")]
        public NetworkCollection Networks
        {
            get
            {
                return (NetworkCollection)this["networks"];
            }
        }

        //+
        //- @GetConfigSection -//
        /// <summary>
        /// Gets the config section.
        /// </summary>
        /// <returns>Configuration section</returns>
        public static OspfSection GetConfigSection()
        {
            return GetConfigSection<OspfSection>("netinterop/ospf");
        }
    }
}
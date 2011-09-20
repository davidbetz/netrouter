#region Copyright

//+ Nalarium Pro 3.0 - Web Module
//+ Copyright © Jampad Technology, Inc. 2008-2010

#endregion

using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class SystemSection : ConfigurationSection
    {
        [ConfigurationProperty("arpCache")]
        [ConfigurationCollection(typeof(ArpCacheElement), AddItemName = "address")]
        public ArpCacheCollection ArpCache
        {
            get
            {
                return (ArpCacheCollection)this["arpCache"];
            }
        }
        [ConfigurationProperty("routers")]
        [ConfigurationCollection(typeof(RouterElement), AddItemName = "add")]
        public RouterCollection Routers
        {
            get
            {
                return (RouterCollection)this["routers"];
            }
        }

        [ConfigurationProperty("debug")]
        [ConfigurationCollection(typeof(DebugElement), AddItemName = "add")]
        public DebugCollection DebugEntries
        {
            get
            {
                return (DebugCollection)this["debug"];
            }
        }

        [ConfigurationProperty("interfaces")]
        [ConfigurationCollection(typeof(InterfaceElement), AddItemName = "add")]
        public InterfaceCollection Interfaces
        {
            get
            {
                return (InterfaceCollection)this["interfaces"];
            }
        }

        [ConfigurationProperty("addresses")]
        [ConfigurationCollection(typeof(AddressElement), AddItemName = "add")]
        public AddressCollection Addresses
        {
            get
            {
                return (AddressCollection)this["addresses"];
            }
        }

        [ConfigurationProperty("loopbacks")]
        [ConfigurationCollection(typeof(LoopbackElement), AddItemName = "add")]
        public LoopbackCollection Loopbacks
        {
            get
            {
                return (LoopbackCollection)this["loopbacks"];
            }
        }

        //+
        //- @GetConfigSection -//
        /// <summary>
        /// Gets the config section.
        /// </summary>
        /// <returns>Configuration section</returns>
        public static SystemSection GetConfigSection()
        {
    //Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver resolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    //Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
    //settings.ContractResolver = resolver;
    //var a = Newtonsoft.Json.JsonConvert.SerializeObject(NetInterop.Routing.Configuration.SystemSection.GetConfigSection(), Newtonsoft.Json.Formatting.Indented, settings);
            return GetConfigSection<SystemSection>("netinterop/system");
        }
    }
}
using System;
using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Rip.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class RipSection : ConfigurationSection
    {
        [ConfigurationProperty("version", IsRequired = true, DefaultValue = (byte)1)]
        public byte Version
        {
            get
            {
                return (byte)this["version"];
            }
        }

        [ConfigurationProperty("update", IsRequired = false, DefaultValue = (UInt16)30)]
        public ushort Update
        {
            get
            {
                return (UInt16)this["update"];
            }
        }

        [ConfigurationProperty("invalid", IsRequired = false, DefaultValue = (UInt16)180)]
        public ushort Invalid
        {
            get
            {
                return (UInt16)this["invalid"];
            }
        }

        [ConfigurationProperty("holddown", IsRequired = false, DefaultValue = (UInt16)180)]
        public ushort Holddown
        {
            get
            {
                return (UInt16)this["holddown"];
            }
        }

        [ConfigurationProperty("flush", IsRequired = false, DefaultValue = (UInt16)60)]
        public ushort Flush
        {
            get
            {
                return (UInt16)this["flush"];
            }
        }

        [ConfigurationProperty("autoSummary", IsRequired = false)]
        public Boolean AutoSummary
        {
            get
            {
                return (Boolean)this["autoSummary"];
            }
        }

        //- @Sequences -//
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
        public static RipSection GetConfigSection()
        {
            return GetConfigSection<RipSection>("netinterop/rip");
        }
    }
}
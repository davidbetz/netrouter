#region Copyright

//+ Nalarium Pro 3.0 - Web Module
//+ Copyright © Jampad Technology, Inc. 2008-2010

#endregion

using System;
using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Http.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class HttpSection : ConfigurationSection
    {

        [ConfigurationProperty("port", IsRequired = true)]
        public Int32 Port
        {
            get
            {
                return (int)this["port"];
            }
        }

        //+
        //- @GetConfigSection -//
        /// <summary>
        /// Gets the config section.
        /// </summary>
        /// <returns>Configuration section</returns>
        public static HttpSection GetConfigSection()
        {
            return GetConfigSection<HttpSection>("netinterop/http");
        }
    }
}
#region Copyright

//+ Nalarium Pro 3.0 - Web Module
//+ Copyright © Jampad Technology, Inc. 2008-2010

#endregion

using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Tcp.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class TcpSection : ConfigurationSection
    {
        //- @GetConfigSection -//
        /// <summary>
        /// Gets the config section.
        /// </summary>
        /// <returns>Configuration section</returns>
        public static TcpSection GetConfigSection()
        {
            return GetConfigSection<TcpSection>("netinterop/tcp");
        }
    }
}
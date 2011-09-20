using System;
using System.Configuration;
using ConfigurationSection = Nalarium.Configuration.ConfigurationSection;

namespace NetInterop.Routing.Image.Configuration
{
    /// <summary>
    /// Provides access to the configuration section.
    /// </summary>
    public class ImageSection : ConfigurationSection
    {
        //- @Sequences -//
        [ConfigurationProperty("mappings")]
        [ConfigurationCollection(typeof(MappingElement), AddItemName = "add")]
        public MappingCollection Mappings
        {
            get
            {
                return (MappingCollection)this["mappings"];
            }
        }

        //+
        //- @GetConfigSection -//
        /// <summary>
        /// Gets the config section.
        /// </summary>
        /// <returns>Configuration section</returns>
        public static ImageSection GetConfigSection()
        {
            return GetConfigSection<ImageSection>("netinterop/image");
        }
    }
}
using System;
using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Image.Configuration
{
    [DebuggerDisplay("{virtual}, {physical}")]
    public class MappingElement : CommentableElement
    {
        [ConfigurationProperty("virtual", IsRequired = true)]
        public string Virtual
        {
            get
            {
                return (string)this["virtual"];
            }
        }

        [ConfigurationProperty("physical", IsRequired = true)]
        public string Physical
        {
            get
            {
                return (string)this["physical"];
            }
        }
    }
}
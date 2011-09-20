using System;
using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    [DebuggerDisplay("{Version}")]
    public class AreaElement : CommentableElement
    {
        [ConfigurationProperty("area", IsRequired = true)]
        public String Area
        {
            get
            {
                return (string)this["area"];
            }
        }

        [ConfigurationProperty("hello", IsRequired = false)]
        public ushort Hello
        {
            get
            {
                return (UInt16)this["hello"];
            }
        }

        [ConfigurationProperty("dead", IsRequired = false)]
        public UInt32 Dead
        {
            get
            {
                return (UInt32)this["dead"];
            }
        }

        [ConfigurationProperty("type", IsRequired = false, DefaultValue = AreaType.Normal)]
        public AreaType AreaType
        {
            get
            {
                return (AreaType)this["type"];
            }
        }
    }
}
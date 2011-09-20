using System;
using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    [DebuggerDisplay("{Area}, {Network}, {Mask}")]
    public class NetworkElement : CommentableElement
    {
        [ConfigurationProperty("area", IsRequired = true)]
        public string Area
        {
            get
            {
                return (string)this["area"];
            }
        }

        [ConfigurationProperty("network", IsRequired = true)]
        public String Network
        {
            get
            {
                return (string)this["network"];
            }
        }

        [ConfigurationProperty("mask", IsRequired = true)]
        public String Mask
        {
            get
            {
                return (string)this["mask"];
            }
        }
    }
}
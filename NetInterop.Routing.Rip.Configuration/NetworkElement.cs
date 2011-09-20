using System;
using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Rip.Configuration
{
    [DebuggerDisplay("{Version}")]
    public class NetworkElement : CommentableElement
    {
        [ConfigurationProperty("network", IsRequired = true)]
        public string Network
        {
            get
            {
                return (string)this["network"];
            }
        }

        [ConfigurationProperty("metric", IsRequired = false, DefaultValue = (UInt32)1)]
        public UInt32 Metric
        {
            get
            {
                return (UInt32)this["metric"];
            }
        }
    }
}
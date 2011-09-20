using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    [DebuggerDisplay("{Address} {Mask}")]
    public class SecondaryElement : CommentableElement
    {
        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get
            {
                return (string)this["address"];
            }
        }

        [ConfigurationProperty("mask", IsKey = true)]
        public string Mask
        {
            get
            {
                return (string)this["mask"];
            }
        }
    }
}
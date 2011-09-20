using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    [DebuggerDisplay("{Address} {Mask}")]
    public class IPv6AddressElement : CommentableElement
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
        public int Mask
        {
            get
            {
                return (int)this["mask"];
            }
        }
    }
}
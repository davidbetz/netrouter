using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    [DebuggerDisplay("{ID}")]
    public class InterfaceElement : CommentableElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string ID
        {
            get
            {
                return (string)this["id"];
            }
        }

        [ConfigurationProperty("disabled", IsRequired = false, DefaultValue = false)]
        public bool IsDisabled
        {
            get
            {
                return (bool)this["disabled"];
            }
        }

        [ConfigurationProperty("ipMtu", IsRequired = false, DefaultValue = 1500)]
        public int IPMtu
        {
            get
            {
                return (int)this["ipMtu"];
            }
        }
    }
}
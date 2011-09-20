using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    [DebuggerDisplay("{Category}")]
    public class DebugElement : CommentableElement
    {
        [ConfigurationProperty("category", IsRequired = true)]
        public string Category
        {
            get
            {
                return (string)this["category"];
            }
        }
    }
}
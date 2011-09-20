using System;
using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Configuration
{
    [DebuggerDisplay("{Name}")]
    public class AddressElement : CommentableElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string ID
        {
            get
            {
                return (string)this["id"];
            }
        }

        [ConfigurationProperty("index", IsKey = true)]
        public int Index
        {
            get
            {
                return (int)this["index"];
            }
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        [ConfigurationProperty("gateway", IsRequired = false)]
        public String Gateway
        {
            get
            {
                return (string)this["gateway"];
            }
        }

        [ConfigurationProperty("ip", IsRequired = false)]
        public String IP
        {
            get
            {
                return (string)this["ip"];
            }
        }

        [ConfigurationProperty("mask", IsRequired = false)]
        public String Mask
        {
            get
            {
                return (string)this["mask"];
            }
        }

        //TODO: IPv6 on Cisco can handle multiple privary ipv6 addresses-- perhaps make this a collection?
        [ConfigurationProperty("v6ip", IsRequired = false)]
        public String V6IP
        {
            get
            {
                return (string)this["v6ip"];
            }
        }

        [ConfigurationProperty("v6mask", IsRequired = false)]
        public byte V6Mask
        {
            get
            {
                return (byte)this["v6mask"];
            }
        }

        [ConfigurationProperty("mac", IsRequired = false)]
        public String Mac
        {
            get
            {
                return (String)this["mac"];
            }
        }

        [ConfigurationProperty("secondaries")]
        [ConfigurationCollection(typeof(SecondaryElement), AddItemName = "add")]
        public SecondaryCollection Secondaries
        {
            get
            {
                return (SecondaryCollection)this["secondaries"];
            }
        }

        [ConfigurationProperty("ipv6Addresses")]
        [ConfigurationCollection(typeof(IPv6AddressElement), AddItemName = "add")]
        public IPv6AddressCollection IPv6Addresses
        {
            get
            {
                return (IPv6AddressCollection)this["ipv6Addresses"];
            }
        }
    }
}
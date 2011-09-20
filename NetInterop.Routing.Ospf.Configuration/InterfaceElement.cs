using System.Configuration;
using System.Diagnostics;
using Nalarium.Configuration;

namespace NetInterop.Routing.Ospf.Configuration
{
    [DebuggerDisplay("{DeviceID} {Type}")]
    public class InterfaceElement : CommentableElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string DeviceID
        {
            get
            {
                return (string)this["id"];
            }
        }

        [ConfigurationProperty("type", IsRequired = false, DefaultValue = OspfNetworkType.Broadcast)]
        public OspfNetworkType Type
        {
            get
            {
                return (OspfNetworkType)this["type"];
            }
        }

        [ConfigurationProperty("hello", IsRequired = false)]
        public ushort HelloInterval
        {
            get
            {
                return (ushort)this["hello"];
            }
        }

        [ConfigurationProperty("dead", IsRequired = false)]
        public uint RouterDeadInterval
        {
            get
            {
                return (uint)this["dead"];
            }
        }

        [ConfigurationProperty("priority", IsRequired = false, DefaultValue = (byte)1)]
        public byte Priority
        {
            get
            {
                return (byte)this["priority"];
            }
        }

        [ConfigurationProperty("cost", IsRequired = false)]
        public uint Cost
        {
            get
            {
                return (uint)this["cost"];
            }
        }

        [ConfigurationProperty("delay", IsRequired = false, DefaultValue = (byte)1)]
        public byte InfTransDelay
        {
            get
            {
                return (byte)this["delay"];
            }
        }

        [ConfigurationProperty("rxmt", IsRequired = false)]
        public ushort RxmtInterval
        {
            get
            {
                return (ushort)this["rxmt"];
            }
        }

        [ConfigurationProperty("authType", IsRequired = false)]
        public OspfAuthType OspfAuthType
        {
            get
            {
                return (OspfAuthType)this["authType"];
            }
        }

        [ConfigurationProperty("passive", IsRequired = false, DefaultValue = false)]
        public bool IsPassive
        {
            get
            {
                return (bool)this["passive"];
            }
        }
    }
}
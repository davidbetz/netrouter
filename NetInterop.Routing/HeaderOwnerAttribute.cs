using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HeaderOwnerAttribute : Attribute
    {
        public HeaderOwnerAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
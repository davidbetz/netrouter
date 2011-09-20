using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModuleMetadataAttribute : Attribute
    {
        public ModuleMetadataAttribute(string name)
        {
            Name = name;
        }

        public String Name { get; set; }
    }
}
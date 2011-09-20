using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class AssemblyModuleAttribute : Attribute
    {
        public AssemblyModuleAttribute(string name)
        {
            Name = name;
        }

        public String Name { get; set; }
    }
}
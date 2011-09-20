using System;
using System.Collections.Generic;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HandlerMetadataAttribute : Attribute
    {
        public HandlerMetadataAttribute(string name, params string[] parentNameParameterArray)
        {
            Name = name;
            ParentNameList = new List<string>(parentNameParameterArray);
        }

        public String Name { get; set; }
        public List<String> ParentNameList { get; set; }
    }
}
using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldOverrideAttribute : Attribute
    {
        public FieldOverrideAttribute(String fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; set; }
    }
}
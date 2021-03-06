using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FieldLabelAttribute : Attribute
    {
        public FieldLabelAttribute(String label)
        {
            Label = label;
        }

        public string Label { get; set; }
    }
}
using System;

namespace NetInterop.Routing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DoNotIncludeInDataTreeAttribute : Attribute
    {
    }
}
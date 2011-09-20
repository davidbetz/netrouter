using System;
using System.Collections.Generic;
using System.Linq;
using Nalarium;

namespace NetInterop.Routing
{
    public class HeaderPackage
    {
        public HeaderPackage()
        {
            Data = new Map<Type, List<object>>();
        }

        public Map<Type, List<object>> Data { get; set; }

        public T GetHeader<T>()
        {
            Type type = typeof(T);
            if (!Data.ContainsKey(type))
            {
                return default(T);
            }
            if (!type.IsArray && Data[type].Count > 1)
            {
                throw new InvalidOperationException("Array found, but not requested.");
            }
            return (T)Data[type].FirstOrDefault();
        }

        public object GetHeaderArray<T>()
        {
            Type type = typeof(T);
            if (!type.IsArray)
            {
                throw new InvalidOperationException("Type must be an array.");
            }
            if (!Data.ContainsKey(type))
            {
                return null;
            }
            return Data[type].ToArray();
        }
    }
}
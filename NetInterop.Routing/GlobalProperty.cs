using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetInterop.Routing
{
    [DebuggerDisplay("{Name}, OT: {OwnerType}")]
    public class GlobalProperty
    {
        private int _size;

        private GlobalProperty(String name, Type type, Type ownerType, int size, GlobalPropertyMetadata metadata)
        {
            Name = name;
            Type = type;
            OwnerType = ownerType;
            Size = size;
            IsArray = type.IsArray;
            GlobalPropertyMetadata = metadata ?? new GlobalPropertyMetadata();
        }

        internal String Name { get; set; }

        internal Type Type { get; set; }

        internal Type OwnerType { get; set; }

        /// <summary>
        /// Size of property type in bytes. If array, size of component type in bytes.
        /// </summary>
        public int Size
        {
            get
            {
                if (_size == -1)
                {
                    throw new InvalidOperationException("Attempt to get size of unsizable objet. Possible type error.");
                }
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        /// <summary>
        /// True is array. False is not.
        /// </summary>
        public Boolean IsArray { get; set; }

        internal GlobalPropertyMetadata GlobalPropertyMetadata { get; set; }

        public static GlobalProperty Register(String name, Type propertyType, Type ownerType)
        {
            return Register(name, propertyType, ownerType, null);
        }

        public static GlobalProperty Register(String name, Type propertyType, Type ownerType,
                                              GlobalPropertyMetadata metadata)
        {
            int size = 0;
            Type typeToSize;
            if (propertyType.IsArray)
            {
                typeToSize = propertyType.GetElementType();
            }
            else
            {
                typeToSize = propertyType;
            }
            if (typeToSize.IsValueType && Nullable.GetUnderlyingType(typeToSize) == null)
            {
                foreach (FieldInfo fieldInfo in typeToSize.GetFields())
                {
                    object[] attributeData = fieldInfo.GetCustomAttributes(typeof(FieldSizeIgnoreAttribute), false);
                    //++ if there is an array, it's impossible to size anyways
                    if (fieldInfo.FieldType.IsArray && attributeData.Length == 0)
                    {
                        size = -1;
                        break;
                    }
                    if (attributeData.Length == 0)
                    {
                        size += Marshal.SizeOf(fieldInfo.FieldType);
                    }
                }
            }
            return new GlobalProperty(name, propertyType, ownerType, size, metadata);
        }
    }
}
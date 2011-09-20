using System;
using System.Collections.Generic;
using Nalarium;

namespace NetInterop.Routing
{
    public class PacketData
    {
        private PacketData()
        {
            Data = new List<byte>();
            _propertyMap = new Map<String, Value>();
        }

        public List<byte> Data { get; private set; }
        private readonly Map<String, Value> _propertyMap;

        public int Length
        {
            get
            {
                return Data.Count;
            }
        }

        public static PacketData Create()
        {
            return new PacketData();
        }
        public static PacketData Create(List<byte> currentData)
        {
            return new PacketData().UpdateData(currentData);
        }

        public PacketData UpdateData(List<byte> currentData)
        {
            Data = new List<byte>(currentData);

            return this;
        }

        public void AddProperty(string name, object value)
        {
            _propertyMap.Add(name, Value.Raw(value));
        }

        public T GetProperty<T>(string name)
        {
            return (T)_propertyMap[name].AsObject;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Nalarium;
using Nalarium.Reflection;

namespace NetInterop.Routing
{
    [DebuggerDisplay("{LayerID}, {Name}")]
    public abstract class Handler
    {
        //public ModuleTrackingInformation ModuleTrackingInformation { get; set; }

        private readonly Dictionary<GlobalProperty, Object> _data = new Dictionary<GlobalProperty, Object>();

        public ModuleEvent Completed = delegate
                                       {
                                       };

        public ModuleEvent SeriesCompleted;
        private bool _isMetadataLoaded;
        private string _moduleName;
        private List<Handler> _parserStack;

        public virtual ushort LayerID { get; private set; }

        public Handler Parent { get; set; }
        internal List<Handler> Children { get; set; }

        protected internal RoutingController Controller { get; set; }
        protected internal HandlerTrackingInformation HandlerTrackingInformation { get; set; }

        protected internal List<string> ParentNameList
        {
            get
            {
                return HandlerTrackingInformation.ParentNameList;
            }
        }

        public String Name
        {
            get
            {
                return HandlerTrackingInformation.Name;
            }
        }

        public String ModuleName
        {
            get
            {
                return HandlerTrackingInformation.ModuleTrackingInformation.Module.Name;
            }
        }

        public Module Module
        {
            get
            {
                return HandlerTrackingInformation.ModuleTrackingInformation.Module;
            }
        }

        internal CommonData CommonData { get; set; }

        protected int Offset
        {
            get
            {
                return CommonData.Offset;
            }
            set
            {
                CommonData.Offset = value;
            }
        }

        protected IntPtr Buffer
        {
            get
            {
                return CommonData.Buffer;
            }
            set
            {
                CommonData.Buffer = value;
            }
        }

        protected int Length
        {
            get
            {
                return CommonData.Length;
            }
            set
            {
                CommonData.Length = value;
            }
        }

        protected internal List<string> HandlerNameStack
        {
            get
            {
                return _parserStack.Select(p => p.Name).ToList();
            }
        }

        protected internal List<Handler> HandlerStack
        {
            get
            {
                return _parserStack;
                if (Collection.IsNullOrTooSmall(_parserStack, 1))
                {
                    _parserStack = new List<Handler>();
                    Handler find = Parent;
                    _parserStack.Add(find);
                    while ((find = Parent) != null)
                    {
                        _parserStack.Add(find);
                    }
                }
                return _parserStack;
            }
            internal set
            {
                _parserStack = value;
            }
        }

        public Device InterfaceData
        {
            get
            {
                return Controller.DeviceConfigurationMap[DeviceID];
            }
        }

        public string DeviceID { get; set; }

        public bool CanRun
        {
            get
            {
                return HandlerTrackingInformation.ModuleTrackingInformation.Module.IsEnabled;
            }
        }

        public virtual Result Handle(Module module)
        {
            return HeaderResult<IHeader>();
        }

        internal void OnCompleted()
        {
            //Completed(this, new ModuleEventArgs
            //{
            //    HeaderPackage = Controller.GetHeaderPackage(GetType(), this)
            //});
        }

        internal void OnSeriesCompleted(string deviceID)
        {
            if (SeriesCompleted != null)
            {
                SeriesCompleted(this, new ModuleEventArgs
                                      {
                                          DeviceID = deviceID,
                                          HeaderPackage = Controller.GetHeaderPackage(GetType(), this)
                                      });
            }
        }

        protected internal abstract Boolean CheckForNext();

        public abstract Handler Parse();

        protected Handler GetNextHandler()
        {
            return Controller.GetNextHandler(this);
        }

        internal void Initialize(string deviceID, RoutingController pc, CommonData common = null, List<Handler> parserStack = null)
        {
            Controller = pc;
            Children = new List<Handler>();

            CommonData = common;
            _parserStack = parserStack;

            DeviceID = deviceID;

            //HandlerNameStack = new List<string>();

            SetValue(RootHandler.SharedParentProperty, false);

            Initialize(HandlerTrackingInformation.ModuleTrackingInformation.Module);
        }

        public virtual void Initialize(Module owningModule)
        {
        }

        protected Handler SetValue(GlobalProperty key, object value)
        {
            if (value == null)
            {
                return this;
            }
            if (GetType() == key.OwnerType)
            {
                Type valueType = value.GetType();
                if (!key.Type.IsAssignableFrom(valueType))
                {
                    throw new InvalidCastException(
                        String.Format("Value type {0} is not compatible with global property type {1}", valueType,
                                      key.Type));
                }
                _data[key] = value;
            }
            else
            {
                if (Parent != null)
                {
                    return Parent.SetValue(key, value);
                }
            }
            return this;
        }

        public T GetValue<T>(GlobalProperty key)
        {
            object obj = GetValue(key);
            if (!(obj is T))
            {
                return default(T);
            }
            return (T)obj;
        }

        public object GetValue(GlobalProperty key)
        {
            if (GetType() == key.OwnerType)
            {
                if (!_data.ContainsKey(key))
                {
                    return null;
                }
                return _data[key];
            }
            if (Parent != null)
            {
                return Parent.GetValue(key);
            }
            return null;
        }

        protected Handler DeclareInteration(GlobalProperty maxProperty, int maxCount, GlobalProperty indexProperty)
        {
            if (maxCount == 0)
            {
                return null;
            }
            return DeclareInteration(maxProperty, maxCount, indexProperty, 0);
        }

        protected Handler DeclareInteration(GlobalProperty maxProperty, int maxCount, GlobalProperty indexProperty,
                                            int indexStart)
        {
            if (maxCount == 0)
            {
                return null;
            }
            SetValue(maxProperty, maxCount);
            SetValue(indexProperty, indexStart);
            return GetNextHandler();
        }

        protected bool CheckForIteration(GlobalProperty indexProperty, GlobalProperty maxProperty)
        {
            var index = (int)GetValue(indexProperty);
            var max = (int)GetValue(maxProperty);

            if (index < max)
            {
                SetValue(indexProperty, index + 1);
                SetValue(RootHandler.SharedParentProperty, true);
                return true;
            }

            return false;
        }

        public T LoadAndScroll<T>()
        {
            var t = Load<T>();
            Scroll<T>();
            return t;
        }

        public object LoadAndScroll(Type type)
        {
            object t = Load(type);
            Scroll(type);
            return t;
        }

        protected ushort LoadUInt16ReversingEndian()
        {
            var result =
                (UInt16)
                (((Byte)Marshal.PtrToStructure(Buffer + Offset, typeof(Byte)) << 8) +
                 (Byte)Marshal.PtrToStructure(Buffer + Offset + 1, typeof(Byte)));
            Scroll<Int16>();
            return result;
        }

        protected UInt32 LoadUInt32ReversingEndian()
        {
            var result =
                (UInt32)
                (((Byte)(Marshal.PtrToStructure(Buffer + Offset, typeof(Byte))) << 24) +
                 ((Byte)(Marshal.PtrToStructure(Buffer + Offset + 1, typeof(Byte))) << 16) +
                 ((Byte)(Marshal.PtrToStructure(Buffer + Offset + 2, typeof(Byte))) << 8) +
                 (Byte)(Marshal.PtrToStructure(Buffer + Offset + 3, typeof(Byte))));
            Scroll<Int32>();
            return result;
        }

        protected Byte[] LoadByteData(int count)
        {
            var byteList = new List<Byte>();
            for (int i = 0; i < count; i++)
            {
                byteList.Add(LoadAndScroll<Byte>());
            }
            var byteArray = byteList.ToArray();
            return byteArray;
        }

        protected T LoadHeader<T>(params object[] parameterArray) where T : IHeader, new()
        {
            var t = new T();
            if (parameterArray == null)
            {
                return default(T);
            }
            if (parameterArray.Length % 2 != 0)
            {
                return default(T);
            }
            Type type = typeof(T);
            FieldInfo[] fieldArray = type.GetFields();
            for (int i = 0; i < parameterArray.Length; i += 2)
            {
                string name = Parser.ParseString(parameterArray[i]);
                object size = parameterArray[i + 1];
                var parameterType = size as Type;
                if (String.IsNullOrEmpty(name))
                {
                    if (parameterType != null)
                    {
                        Scroll(parameterType);
                    }
                    else
                    {
                        switch (Parser.ParseInt32(size))
                        {
                            case 1:
                                Scroll<Byte>();
                                break;
                            case 2:
                                Scroll<UInt16>();
                                break;
                            case 4:
                                Scroll<UInt32>();
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    FieldInfo fieldInfo = fieldArray.First(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    var tr = __makeref(t);
                    if (parameterType != null)
                    {
                        //++ separated for debugging
                        object data = LoadAndScroll(parameterType);
                        fieldInfo.SetValueDirect(tr, data);
                    }
                    else
                    {
                        switch (Parser.ParseInt32(size))
                        {
                            case 1:
                                fieldInfo.SetValueDirect(tr, LoadAndScroll<Byte>());
                                break;
                            case 2:
                                fieldInfo.SetValueDirect(tr, LoadUInt16ReversingEndian());
                                break;
                            case 4:
                                fieldInfo.SetValueDirect(tr, LoadUInt32ReversingEndian());
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return t;
        }

        protected Byte[] Copy(int length)
        {
            var dst = new Byte[length];
            Marshal.Copy(Buffer, dst, Offset, length);
            return dst;
        }

        protected T Load<T>()
        {
            return (T)Load(typeof(T));
        }

        protected object Load(Type type)
        {
            return Marshal.PtrToStructure((Buffer + Offset), type);
        }

        protected void Scroll<T>()
        {
            Scroll(typeof(T));
        }

        protected void Scroll(Type type)
        {
            int size = Marshal.SizeOf(type);
            Offset += size;
        }

        public HandlerData CollectData(RootHandler rootHandler, List<HandlerData> flatData)
        {
            var data = new HandlerData
                       {
                           Name = Name
                       };
            if (RoutingController.HandlerGlobalPropertyDictionary.ContainsKey(Name))
            {
                List<GlobalProperty> globalPropertyList = RoutingController.HandlerGlobalPropertyDictionary[Name];
                if (globalPropertyList != null)
                {
                    foreach (GlobalProperty item in globalPropertyList)
                    {
                        if ((item.GlobalPropertyMetadata.GlobalPropertyMetadataOptions &
                             GlobalPropertyMetadataOptions.InternalUseOnly) !=
                            GlobalPropertyMetadataOptions.InternalUseOnly)
                        {
                            object value;
                            if ((item.GlobalPropertyMetadata.GlobalPropertyMetadataOptions &
                                 GlobalPropertyMetadataOptions.Interpretive) ==
                                GlobalPropertyMetadataOptions.Interpretive)
                            {
                                if (item.OwnerType.BaseType == typeof(InterpretiveHandler))
                                {
                                    var ip = Activator.CreateInstance(item.OwnerType) as InterpretiveHandler;
                                    value = ip.Interpret(GetValue(item));
                                }
                                else
                                {
                                    value = GetValue(item);
                                }
                            }
                            else
                            {
                                value = GetValue(item);
                            }
                            data.PropertyList.Add(new HandlerDataValue
                                                  {
                                                      Name = item.Name,
                                                      Value = value
                                                  });
                        }
                    }
                }
            }
            foreach (Handler childHandler in Children)
            {
                //TODO: gotta rework all this
                HandlerData childData = childHandler.CollectData(rootHandler, flatData);
                rootHandler.HandlerList.Add(childHandler);
                if (AttributeReader.ReadTypeAttribute<DoNotIncludeInDataTreeAttribute>(childHandler) == null)
                {
                    flatData.Insert(0, childData);
                }
            }
            return data;
        }

        protected byte[] GetNotReversedBytes(object data)
        {
            return ByteReader.GetNotReversedBytes(data);
        }

        protected byte[] GetBytes(object data)
        {
            return ByteReader.GetBytes(data);
        }

        public virtual PacketData GetBytes(IHeader header, PacketData packetData)
        {
            return packetData;
        }

        protected Result EmptyResult()
        {
            return new EmptyResult();
        }

        protected Result ResponseResult(IHeader header)
        {
            return new ResponseResult(header);
        }

        public override String ToString()
        {
            return "";
        }

        protected HeaderResult HeaderResult<T>()
        {
            return new HeaderResult
                   {
                       HeaderPackage = Controller.GetHeaderPackage<T>(this)
                   };
        }

        public IHeader Build(params Value[] parameterArray)
        {
            Module.ValidateParameterArray(parameterArray);
            return Build(HandlerTrackingInformation.ModuleTrackingInformation.Module, parameterArray);
        }

        protected virtual IHeader Build(Module module, params Value[] parameterArray)
        {
            throw new NotImplementedException(String.Format("Build not implemented for {0}.", Name));
        }

        protected Handler CancelProcessing(String message)
        {
            return new CancelProcessingHandler(message);
        }

        public T CreateHeader<T>(params Value[] parameterArray) where T : IHeader
        {
            T t = default(T);
            if (parameterArray == null)
            {
                return t;
            }
            Type type = typeof(T);
            FieldInfo[] fieldInfoArray = type.GetFields();
            foreach (Value item in parameterArray)
            {
                if (!type.Name.Equals(item.Scope))
                {
                    continue;
                }
                string name = item.Name;
                FieldInfo fieldInfo = fieldInfoArray.FirstOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (fieldInfo != null)
                {
                    var tr = __makeref(t);
                    fieldInfo.SetValueDirect(tr, item.AsObject);
                }
            }
            return t;
        }

        public void MoveHeader(GlobalProperty fromProperty, GlobalProperty toProperty)
        {
            SetValue(toProperty, GetValue(fromProperty));
            SetValue(fromProperty, null);
        }

        protected byte[] LoadUntilCharacterFound(params char[] c)
        {
            throw new NotImplementedException();
        }
        public object LoadUntilCharacterFound(string p)
        {
            throw new NotImplementedException();
        }

        protected byte[] LoadRest()
        {
            return Copy(CommonData.Length - Offset);
        }

        protected void SendToParentForBuild(IHeader header, params Value[] parameterArray)
        {
        }
    }
}
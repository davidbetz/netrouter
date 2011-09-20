using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Nalarium;
using Nalarium.Activation;
using NetInterop.Routing.Configuration;
using Timer = System.Timers.Timer;

namespace NetInterop.Routing
{
    public class RoutingController : IPartImportsSatisfiedNotification
    {
        private static readonly object Lock = new object();
        private static readonly object threadLock = new object();

        internal static readonly List<string> DebugCategoryList = new List<string>();

        private static RoutingController _controller;

        public static Write VerboseOutput = delegate
                                            {
                                            };

        private static readonly object _sendLock = new object();
        private readonly Type _globalPropertyType = typeof(GlobalProperty);
        private int _count = 1;
        private List<Thread> _mainThreadList;

        [ImportMany]
        private List<Module> _moduleList;

        private Map<string, Monitor> _monitorList;

        [ImportMany]
        private List<Handler> _parserList;

        private Timer _timer;
        private bool _usingConfiguredPrimaryIPAddress;

        static RoutingController()
        {
            HandlerGlobalPropertyDictionary = new Dictionary<string, List<GlobalProperty>>();
        }

        private RoutingController()
        {
            HandlerIndex = new Map<string, HandlerTrackingInformation>();
            HandlerTypeNameMap = new Map<string, string>();
            DeviceConfigurationMap = new Map<string, Device>();
            IPAddressList = new List<Tuple<IPAddress, IPAddress, bool>>();
            LoopbackMap = new Map<string, Device>();
        }

        public bool EnableExtensiveOutput { get; set; }

        public Map<string, Device> LoopbackMap { get; set; }

        internal static Dictionary<String, List<GlobalProperty>> HandlerGlobalPropertyDictionary { get; private set; }

        private SortedDictionary<String, Type> ModuleDictionary { get; set; }

        private Dictionary<String, Module> ModuleSingletonDictionary { get; set; }

        private Map<String, HandlerTrackingInformation> HandlerIndex { get; set; }

        public List<GlobalProperty> FlatGlobalPropertyIndex { get; set; }

        private Map<string, ModuleTrackingInformation> ModuleTrackingInformationMap { get; set; }

        public List<Tuple<IPAddress, IPAddress, bool>> IPAddressList { get; set; }

        public Map<string, Device> DeviceConfigurationMap { get; set; }

        public MacAddress InterfaceMacAddress { get; set; }

        public List<Tuple<IPv6Address, IPv6Address, bool>> IPv6AddressList { get; set; }

        public Map<string, string> HandlerTypeNameMap { get; set; }

        public Tuple<IPAddress, IPAddress, bool> PrimaryIPAddress
        {
            get
            {
                if (IPAddressList.Count(p => !p.Item3) == 0)
                {
                    return IPAddressList.OrderByDescending(p => p.Item1).First();
                }
                return IPAddressList.Where(p => !p.Item3).OrderByDescending(p => p.Item1).First();
            }
        }

        public Tuple<IPv6Address, IPv6Address, bool> PrimaryIPv6Address
        {
            get
            {
                if (IPv6AddressList.Count(p => !p.Item3) == 0)
                {
                    return IPv6AddressList.OrderByDescending(p => p.Item1).First();
                }
                return IPv6AddressList.Where(p => !p.Item3).OrderByDescending(p => p.Item1).First();
            }
        }

        protected static bool IsEnabled { get; set; }
        public IPConfiguration ActiveIPConfiguration { get; set; }
        public Boolean ShowSummary { get; set; }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            var config = SystemSection.GetConfigSection();
            ModuleTrackingInformationMap = new Map<string, ModuleTrackingInformation>();
            foreach (Type moduleType in _moduleList
                .Select(p => new
                             {
                                 t = p.GetType(),
                                 p
                             })
                .OrderBy(p => p.t.FullName.Length)
                .Select(p => p.t))
            {
                var module = ObjectCreator.CreateAs<Module>(moduleType);
                ModuleTrackingInformation information = ModuleTrackingInformation.Create(module, moduleType);
                if (!module.Name.Equals("SYSTEM") &&
                    !module.Name.Equals("INTERNAL") &&
                    !module.Name.Equals("ICMP") &&
                    !config.Routers.Any(o => o.Name.Equals(module.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                ModuleTrackingInformationMap.Add(module.Name, information);
                module.Controller = _controller;
                module.InstallAll();
            }
            foreach (Type parserType in _parserList.Select(p => p.GetType()))
            {
                var attribute = Nalarium.Reflection.AttributeReader.ReadAssemblyAttribute<AssemblyModuleAttribute>(parserType);
                if (!attribute.Name.Equals("SYSTEM") &&
                    !attribute.Name.Equals("INTERNAL") &&
                    !attribute.Name.Equals("ICMP") &&
                    !config.Routers.Any(o => o.Name.Equals(attribute.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                string key = ModuleTrackingInformationMap
                    .OrderByDescending(p => p.Value.Namespace.Length)
                    .FirstOrDefault(p => parserType.Namespace.StartsWith(p.Value.Namespace)).Key;
                ModuleTrackingInformation moduleTrackingInformation = ModuleTrackingInformationMap[key];
                if (moduleTrackingInformation.HandlerTrackingInformationMap == null)
                {
                    moduleTrackingInformation.HandlerTrackingInformationMap = new Map<string, HandlerTrackingInformation>();
                }
                var parser = ObjectCreator.CreateAs<Handler>(parserType);
                parser.Controller = this;
                HandlerTrackingInformation parserTrackingInformation = HandlerTrackingInformation.Create(parser, parserType, moduleTrackingInformation);
                HandlerTypeNameMap.Add(parserType.FullName, parser.Name);
                IndexHandler(parserTrackingInformation);
                moduleTrackingInformation.HandlerTrackingInformationMap.Add(parser.Name, parserTrackingInformation);
            }
            foreach (var key in HandlerIndex.Keys)
            {
                var info = HandlerIndex[key];
                if (info.ParentNameList.Count == 1)
                {
                    var parentName = info.ParentNameList[0];
                    var parent = HandlerIndex[parentName];
                    if (parent != null)
                    {
                        info.SingletonHandler.Parent = parent.SingletonHandler;
                    }
                }
            }
            //foreach (var type in pendingRemovalList)
            //{
            //    _parserList.Single(p=>p.GetType().FullName.Equals
            //}
            foreach (string key in HandlerIndex.Where(p => p.Key != "ROOT" && p.Key != "DUMMY").Select(p => p.Key))
            {
                RegisterGlobalPropertyInformation(HandlerIndex[key]);
            }
            FlatGlobalPropertyIndex = HandlerGlobalPropertyDictionary.SelectMany(p => p.Value).ToList();
        }

        #endregion

        public event Parsed Parsed;

        public static RoutingController Create()
        {
            lock (Lock)
            {
                foreach (DebugElement item in SystemSection.GetConfigSection().DebugEntries)
                {
                    DebugCategoryList.Add(item.Category.ToUpper());
                }
                _controller = new RoutingController();
                string routingFolder = ConfigurationManager.AppSettings["routingFolder"];
                if (String.IsNullOrEmpty(routingFolder))
                {
                    throw new ConfigurationErrorsException("routingFolder missing.");
                }
                if (!Directory.Exists(routingFolder))
                {
                    throw new ConfigurationErrorsException(string.Format("routingFolder does not exist ({0}).", routingFolder));
                }
                //_controller.HandlerIndex["ROOT"] = HandlerTrackingInformation.Create(new RootHandler(), typeof(RootHandler), new ModuleTrackingInformation());
                //_controller.HandlerIndex["DUMMY"] = HandlerTrackingInformation.Create(new DummyHandler(), typeof(DummyHandler), new ModuleTrackingInformation());
                using (var dc = new DirectoryCatalog(routingFolder))
                {
                    using (var ac = new AssemblyCatalog(Assembly.GetExecutingAssembly()))
                    {
                        using (var aa = new AggregateCatalog())
                        {
                            aa.Catalogs.Add(dc);
                            aa.Catalogs.Add(ac);
                            var cc = new CompositionContainer(aa);
                            try
                            {
                                cc.ComposeParts(_controller);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                return _controller;
            }
        }

        internal void Send(string deviceID, List<byte> data)
        {
            lock (_sendLock)
            {
                byte[] array = data.ToArray();
                GCHandle pinnedArray = GCHandle.Alloc(array, GCHandleType.Pinned);
                _monitorList[deviceID].Send(array.Length, pinnedArray.AddrOfPinnedObject());
                pinnedArray.Free();
            }
        }

        protected internal Handler GetNextHandler(Handler currentHandler)
        {
            HandlerTrackingInformation information = HandlerIndex[currentHandler.Name];
            List<string> potentialHandler = information.PotentialNextHandlerList;
            foreach (string item in potentialHandler)
            {
                Handler parser = CreateHandler(HandlerIndex[item].Type, HandlerIndex[item].ModuleTrackingInformation);
                parser.Parent = currentHandler;
                parser.CommonData = currentHandler.CommonData;
                parser.HandlerStack = currentHandler.HandlerStack;
                if (parser.CheckForNext())
                {
                    return parser;
                }
            }
            return null;
        }

        internal void RegisterGlobalPropertyInformation(HandlerTrackingInformation information)
        {
            lock (Lock)
            {
                Handler parser = information.SingletonHandler;
                foreach (string parentName in parser.ParentNameList)
                {
                    HandlerIndex[parentName].PotentialNextHandlerList.Add(parser.Name);
                }

                //TODO: how about only allowing an IHeader to be used in one globalproperty ever. This would enforce uniqueness... for stuff like IPv6/IPv4 which may seem to need dupe, just use a common base class with an empty class.
                var globalPropertyList = new List<GlobalProperty>();
                HandlerGlobalPropertyDictionary.Add(parser.Name, globalPropertyList);
                FieldInfo[] fieldInfoArray = information.Type.GetFields(BindingFlags.Static | BindingFlags.Public);
                globalPropertyList.AddRange(from fieldInfo in fieldInfoArray
                                            where _globalPropertyType.IsAssignableFrom(fieldInfo.FieldType)
                                            select fieldInfo.GetValue(null) as GlobalProperty);
                information.HeaderList = globalPropertyList.Where(p => typeof(IHeader).IsAssignableFrom(globalPropertyList[0].Type)).Select(p => p.Type.Name).ToList();
            }
        }

        public void IndexHandler(HandlerTrackingInformation information)
        {
            string name = information.SingletonHandler.Name;
            if (HandlerIndex.ContainsKey(information.SingletonHandler.Name))
            {
                throw new InvalidOperationException(string.Format("Handler {0} already registered.", name));
            }
            HandlerIndex.Add(name, information);
        }

        public Handler GetHandler(string name, ModuleTrackingInformation moduleTrackingInformation)
        {
            if (String.IsNullOrEmpty(name))
            {
                return CreateHandler(HandlerIndex["DUMMY"].Type, moduleTrackingInformation);
            }
            name = name.ToUpper();

            if (!HandlerIndex.ContainsKey(name))
            {
                return CreateHandler(HandlerIndex["DUMMY"].Type, moduleTrackingInformation);
            }
            return CreateHandler(HandlerIndex[name].Type, moduleTrackingInformation);
        }

        private Handler CreateHandler(Type type, ModuleTrackingInformation moduleTrackingInformation)
        {
            var parser = (Handler)Activator.CreateInstance(type);
            parser.HandlerTrackingInformation = moduleTrackingInformation.HandlerTrackingInformationMap[HandlerTypeNameMap[type.FullName]];
            return parser;
        }

        public void Begin(string deviceID, int length, IntPtr data)
        {
            lock (Lock)
            {
                _count++;
                var common = new CommonData
                             {
                                 Buffer = data,
                                 Offset = 0,
                                 Length = length
                             };
                Handler parser = null;
                Handler rootHandler = GetSingletonHandler("ROOT");
                Handler nextHandler = rootHandler;
                var sb = new StringBuilder();
                String owningModule = string.Empty;
                var parserStack = new List<Handler>();
                var previousHandlerStack = new List<string>();
                do
                {
                    if (parser != null)
                    {
                        if (parser.GetValue<bool?>(RootHandler.SharedParentProperty) ?? false)
                        {
                            parser.Parent.Children.Add(nextHandler);
                            nextHandler.Parent = parser.Parent;
                        }
                        else
                        {
                            parser.Children.Add(nextHandler);
                            nextHandler.Parent = parser;
                        }
                    }
                    parser = nextHandler;
                    parser.Initialize(deviceID, this, common, parserStack);
                    if (!parser.Name.EndsWith("ROOT"))
                    {
                        sb.Append(parser.Name + " ");
                    }
                    parserStack.Add(parser);
                    nextHandler = parser.Parse();
                    var cancelProcessingHandler = nextHandler as CancelProcessingHandler;
                    if (cancelProcessingHandler != null)
                    {
                        Log.Write("Process", "Cancellation", cancelProcessingHandler.Message);
                        return;
                    }
                    parser.OnCompleted();
                    owningModule = parser.ModuleName;
                    //Func<KeyValuePair<string, List<string>>, Boolean> containingHandlerName = p => p.Value.Contains(parser.Name);
                    //if (ModuleHandlerOwnershipDictionary.Any(containingHandlerName))
                    //{
                    //    owningModule = ModuleHandlerOwnershipDictionary.First(containingHandlerName).Key;
                    //}
                } while (nextHandler != null);
                for (int i = parserStack.Count - 1; i >= 0; i--)
                {
                    parserStack[i].OnSeriesCompleted(deviceID);
                }
                if (ShowSummary)
                {
                    string summary = sb.ToString();
                    if (!String.IsNullOrEmpty(owningModule))
                    {
                        OnModuleFound(owningModule, parser);
                        summary = "(" + owningModule + ") " + summary;
                    }
                    OnParsed(rootHandler as RootHandler, summary);
                }
            }
        }

        public void Enable()
        {
            Enable(null);
        }

        public void Enable(string filter)
        {
            var wh = new AutoResetEvent(false);
            _monitorList = new Map<string, Monitor>();
            VerboseOutput("Starting monitor..." + Environment.NewLine);
            bool hasInterfaces = SetupDeviceInformation();
            if (!hasInterfaces)
            {
                throw new InvalidOperationException("Nothing to do. No interfaces configured.");
            }
            _mainThreadList = new List<Thread>();
            foreach (string key in DeviceConfigurationMap.Keys)
            {
                Device device = DeviceConfigurationMap[key];
                if (device.IsLoopback)
                {
                    continue;
                }
                Monitor monitor = Monitor.Create();
                string copy = key;
                monitor.ActiveCallbackIntPtr = (s, p) => Begin(copy, s, p);
                //monitor.Initialize(3, filter);
                monitor.Initialize(DeviceConfigurationMap[key].Index, filter);
                _monitorList.Add(key, monitor);

                _mainThreadList.Add(new Thread(() =>
                                               {
                                                   VerboseOutput(string.Format("Monitor started for {0}." + Environment.NewLine, device.ID));
                                                   monitor.RunIt();
                                                   VerboseOutput(string.Format("Monitor stopped for {0}." + Environment.NewLine, device.ID));
                                               }));
            }
            Monitor.VerboseOutput += (t) => VerboseOutput(t);

            //_monitor.Initialize(4, filter);

            foreach (string info in ModuleTrackingInformationMap.Keys)
            {
                ModuleTrackingInformationMap[info].Module.Start();
            }

            IsEnabled = true;
            _mainThreadList.ForEach(p => p.Start());
            _timer = new Timer
                     {
                         Interval = 1000
                     };
            _timer.Elapsed += (s, e) =>
                              {
                                  lock (threadLock)
                                  {
                                      if (IsEnabled)
                                      {
                                          return;
                                      }
                                      VerboseOutput("Stopping monitor..." + Environment.NewLine);
                                      _monitorList.GetValueList().ForEach(p => p.Stop());
                                      _timer.Stop();
                                      wh.Set();
                                  }
                              };
            _timer.Start();
            wh.WaitOne();
        }

        private bool SetupDeviceInformation()
        {
            Dictionary<int, Tuple<string, string, string, List<Tuple<string, string, bool>>>> deviceList = Monitor.GetDeviceDictionary();
            Dictionary<int, string> deviceIndexList = Monitor.GetDeviceIndexDictionary();
            var foundDeviceList = new List<String>();
            bool hasInterfaces = false;
            foreach (int key in deviceList.Keys)
            {
                Tuple<string, string, string, List<Tuple<string, string, bool>>> device = deviceList[key];
                string deviceID = device.Item1.Replace("{", string.Empty).Replace("}", string.Empty);
                if (!SystemSection.GetConfigSection().Interfaces.Any(p => p.ID.Replace("{", string.Empty).Replace("}", string.Empty).Equals(deviceID)))
                {
                    continue;
                }
                foundDeviceList.Add(deviceID);
                hasInterfaces = true;
                var list = new List<IPConfiguration>();
                var v6List = new List<IPv6Configuration>();
                DeviceConfigurationMap.Add(deviceID,
                                           new Device
                                           {
                                               ID = deviceID,
                                               Index = deviceIndexList.FirstOrDefault(p => p.Value.Contains(device.Item1)).Key,
                                               Description = device.Item2,
                                               PrimaryIPConfiguration = IPConfiguration.Create(
                                                   IPAddress.From(device.Item4.First(p => p.Item3).Item1),
                                                   IPAddress.From(device.Item4.First(p => p.Item3).Item2)
                                                   ),
                                               MacAddress = MacAddress.From(device.Item3),
                                               SecondaryIPList = list,
                                               //TODO: do this
                                               IPv6List = v6List
                                           });
                list.AddRange(device.Item4.Where(p => !p.Item3).Select(p => new IPConfiguration
                                                                            {
                                                                                Address = IPAddress.From(p.Item1),
                                                                                Mask = IPAddress.From(p.Item2),
                                                                            }));
            }
            bool foundPrimary = false;
            //TODO: check for overlapping networks
            //IEnumerable<InterfaceElement> interfaceData = SystemSection.GetConfigSection().Interfaces.Where(p => !DeviceConfigurationMap.ContainsKey(p.ID));
            foreach (InterfaceElement interfaceElement in SystemSection.GetConfigSection().Interfaces)
            {
                string deviceID = interfaceElement.ID;
                if (!foundDeviceList.Contains(deviceID))
                {
                    continue;
                }
                if (interfaceElement.IPMtu < 68)
                {
                    throw new InvalidOperationException("Minimum MTU size is 68 octets.");
                }
                if (foundDeviceList.Contains(deviceID))
                {
                    DeviceConfigurationMap[deviceID].IsEnabled = !interfaceElement.IsDisabled;
                    continue;
                }
                hasInterfaces = true;
                InterfaceElement element = interfaceElement;
                var device = new Device();
                DeviceConfigurationMap.Add(deviceID, device);
                foundDeviceList.Add(deviceID);
                foreach (AddressElement address in SystemSection.GetConfigSection().Addresses.Where(p => p.ID == element.ID))
                {
                    var list = new List<IPConfiguration>();
                    var listv6 = new List<IPv6Configuration>();
                    device = new Device
                             {
                                 Index = address.Index,
                                 Description = address.Name,
                                 PrimaryIPConfiguration = IPConfiguration.Create(
                                     IPAddress.From(address.IP),
                                     IPAddress.From(address.Mask)
                                     ),
                                 MacAddress = MacAddress.From(address.Mac),
                                 SecondaryIPList = list,
                                 IPv6List = listv6
                             };
                    list.AddRange(address.Secondaries.Select(secondary => new IPConfiguration
                                                                          {
                                                                              Address = IPAddress.From(secondary.Address),
                                                                              Mask = IPAddress.From(secondary.Mask)
                                                                          }));
                    listv6.AddRange(address.IPv6Addresses.Select(v6 => new IPv6Configuration
                                                                       {
                                                                           Address = IPv6Address.From(v6.Address),
                                                                           Mask = v6.Mask
                                                                       }));
                }
            }
            if (!hasInterfaces)
            {
                return false;
            }
            //TODO: check for overlapping networks
            foreach (LoopbackElement interfaceElement in SystemSection.GetConfigSection().Loopbacks)
            {
                var list = new List<IPConfiguration>();
                var v6List = new List<IPv6Configuration>();
                var device = new Device
                             {
                                 ID = interfaceElement.ID.ToString(),
                                 Index = interfaceElement.ID,
                                 PrimaryIPConfiguration = IPConfiguration.Create(
                                     IPAddress.From(interfaceElement.IP),
                                     IPAddress.From(interfaceElement.Mask)
                                     ),
                                 SecondaryIPList = list,
                                 IPv6List = v6List,
                                 Description = "Loopback",
                                 IsLoopback = true,
                                 IsEnabled = true
                             };
                LoopbackMap.Add(device.ID, device);
                DeviceConfigurationMap.Add(device.ID, device);
                foundDeviceList.Add(device.ID);
                list.AddRange(interfaceElement.Secondaries.Select(secondary => new IPConfiguration
                                                                               {
                                                                                   Address = IPAddress.From(secondary.Address),
                                                                                   Mask = IPAddress.From(secondary.Mask)
                                                                               }));
                v6List.AddRange(interfaceElement.IPv6Addresses.Select(v6 => new IPv6Configuration
                                                                            {
                                                                                Address = IPv6Address.From(v6.Address),
                                                                                Mask = v6.Mask
                                                                            }));
            }
            return true;
        }

        public void Disable()
        {
            foreach (string info in ModuleTrackingInformationMap.Keys)
            {
                ModuleTrackingInformationMap[info].Module.Stop();
            }
            lock (threadLock)
            {
                IsEnabled = false;
            }
        }

        internal HeaderPackage GetHeaderPackage<T>(Handler currentHandler)
        {
            return GetHeaderPackage(typeof(T), currentHandler);
        }

        internal HeaderPackage GetHeaderPackage(Type parserType, Handler currentHandler)
        {
            var package = new HeaderPackage();

            Handler parser = currentHandler;

            do
            {
                string name = parser.Name;
                if (!HandlerGlobalPropertyDictionary.ContainsKey(name))
                {
                    continue;
                }
                List<GlobalProperty> globalPropertyList = HandlerGlobalPropertyDictionary[name];
                if (globalPropertyList == null)
                {
                    continue;
                }
                foreach (GlobalProperty item in globalPropertyList.Where(item => (item.GlobalPropertyMetadata.GlobalPropertyMetadataOptions &
                                                                                  GlobalPropertyMetadataOptions.InternalUseOnly) !=
                                                                                 GlobalPropertyMetadataOptions.InternalUseOnly))
                {
                    //TODO: this isn't current compatible with the non-typed interpreter results; may need a IView type (based on type IHeader) or something
                    object value;
                    //if ((item.GlobalPropertyMetadata.GlobalPropertyMetadataOptions &
                    //     GlobalPropertyMetadataOptions.Interpretive) ==
                    //    GlobalPropertyMetadataOptions.Interpretive)
                    //{
                    //    if (item.OwnerType.BaseType == typeof(InterpretiveHandler))
                    //    {
                    //        var ip = Activator.CreateInstance(item.OwnerType) as InterpretiveHandler;
                    //        value = ip.Interpret(parser.GetValue(item));
                    //    }
                    //    else
                    //    {
                    //        value = parser.GetValue(item);
                    //    }
                    //}
                    //else
                    //{
                    value = parser.GetValue(item);
                    //}
                    if (!package.Data.ContainsKey(item.Type))
                    {
                        package.Data.Add(item.Type, new List<object>());
                    }
                    package.Data[item.Type].Add(value);
                }
            } while ((parser = parser.Parent) != null);

            return package;
        }

        public void OnParsed(RootHandler rootHandler, String summary)
        {
            if (Parsed != null)
            {
                HandlerData collectedData;
                rootHandler.HandlerList = new List<Handler>();
                var flatData = new List<HandlerData>();
                if (EnableExtensiveOutput)
                {
                    collectedData = rootHandler.CollectData(rootHandler, flatData);
                }
                Parsed(this, new ParsedEventArgs
                             {
                                 Index = _count,
                                 SerialHandlerList = rootHandler.HandlerList,
                                 HandlerData = flatData,
                                 Summary = summary
                             });
            }
        }

        public event ModuleFound ModuleFound = delegate
                                               {
                                               };

        private void OnModuleFound(String moduleName, Handler terminalHandler)
        {
            ModuleFound(this, new ModuleFoundEventArgs
                              {
                                  Name = moduleName,
                                  Handler = terminalHandler
                              });
        }

        private void ParseController_ModuleFound(object sender, ModuleFoundEventArgs args)
        {
            //string moduleName = args.Name.ToUpper();
            //Handler terminalHandler = args.Handler;
            //foreach (string key in ModuleCallbackDictionary[moduleName].Keys)
            //{
            //    Map<string, List<ModuleEvent>> map = ModuleCallbackDictionary[key];
            //    if (map.ContainsKey(terminalHandler.Name))
            //    {
            //        List<ModuleEvent> list = map[terminalHandler.Name];
            //        foreach (ModuleEvent item in list)
            //        {
            //            item(null, new ModuleEventArgs
            //                           {
            //                               HeaderPackage = null
            //                           });
            //        }
            //    }
            //    //var result = callback;
            //    //if (result != null)
            //    //{
            //    //    //TODO: Do something...
            //    //}
            //}
            //foreach (var callback in CallbackDictionary[moduleName])
            //{
            //    var result = callback(terminalHandler);
            //    if (result != null)
            //    {
            //        //TODO: Do something...
            //    }
            //}
        }

        internal Module GetModule(string moduleName)
        {
            if (!ModuleTrackingInformationMap.ContainsKey(moduleName))
            {
                return null;
            }
            return ModuleTrackingInformationMap[moduleName].Module;
        }

        internal Handler GetHeaderOwner(Type type)
        {
            IEnumerable<GlobalProperty> propertyEnumeration = FlatGlobalPropertyIndex.Where(o => o.Type.FullName == type.FullName);
            GlobalProperty property;
            if (propertyEnumeration.Count() > 1)
            {
                Func<GlobalProperty, Boolean> isPreferred = p => (p.GlobalPropertyMetadata.GlobalPropertyMetadataOptions & GlobalPropertyMetadataOptions.Preferred) == GlobalPropertyMetadataOptions.Preferred;
                if (propertyEnumeration.Count(isPreferred) > 1)
                {
                    throw new InvalidOperationException(string.Format("Property may not be preferred in more than one place. {0}", type.FullName));
                }
                property = propertyEnumeration.FirstOrDefault(isPreferred);
            }
            else
            {
                property = propertyEnumeration.FirstOrDefault();
            }
            if (property != null)
            {
                return HandlerIndex.FirstOrDefault(p => p.Value.Type == property.OwnerType).Value.SingletonHandler;
            }
            throw new InvalidOperationException("property not found.");
        }

        public Module GetHandlerOwner(Type type)
        {
            return (from key in ModuleTrackingInformationMap.Keys
                    where ModuleTrackingInformationMap[key].HandlerTrackingInformationMap.Any(o => o.Value.Type == type)
                    select ModuleTrackingInformationMap[key].Module).FirstOrDefault();
        }

        public Handler GetSingletonHandler(string name)
        {
            name = name.ToUpper();
            return HandlerIndex[name].SingletonHandler;
        }

        internal List<string> GetInterfaceListForAddress(IPAddress address, bool allowConnectionOverSeconaryIP = false)
        {
            IPAddress mask;
            switch (address.GetClass())
            {
                case 'A':
                    mask = IPAddress.NetworkAMask;
                    break;
                case 'B':
                    mask = IPAddress.NetworkBMask;
                    break;
                case 'C':
                    mask = IPAddress.NetworkCMask;
                    break;
                default:
                    throw new InvalidOperationException("Mask not implemented");
            }
            return GetInterfaceListForAddress(address, mask, allowConnectionOverSeconaryIP);
        }

        internal List<string> GetInterfaceListForAddress(IPAddress address, IPAddress mask, bool allowConnectionOverSeconaryIP = false)
        {
            var list = new List<string>();
            foreach (string key in DeviceConfigurationMap.Keys)
            {
                Device device = DeviceConfigurationMap[key];
                if (device.PrimaryIPConfiguration.Address.IsSameNetwork(address, mask))
                {
                    list.Add(key);
                }
                if (allowConnectionOverSeconaryIP)
                {
                    list.AddRange(from secondary in device.SecondaryIPList
                                  where secondary.Address.IsSameNetwork(address, mask)
                                  select key);
                }
            }
            return list;
        }

        public void EnableInterface(string deviceID)
        {
            DeviceConfigurationMap[deviceID].OnStateChanged(InterfaceState.L2Up);
        }

        public void DisableInterface(string deviceID)
        {
            DeviceConfigurationMap[deviceID].OnStateChanged(InterfaceState.Down);
        }

        public event RouteEvent RouteDiscovered = delegate
                                                  {
                                                  };

        public event RouteEvent RouteRemoved = delegate
                                               {
                                               };

        public void RegisterRoute(Module module, Route entry)
        {
            RouteDiscovered(module, new RouteEventArgs
                                    {
                                        Route = entry
                                    });
        }

        public void RemoveRoute(Module module, Route entry)
        {
            RouteRemoved(module, new RouteEventArgs
                                 {
                                     Route = entry
                                 });
        }

        public Handler GetHandlerByScopedName(string handlerName)
        {
            var partArray = handlerName.Split(':');
            var moduleTrackingInformation = ModuleTrackingInformationMap[partArray[0].ToUpper()];
            if (moduleTrackingInformation == null)
            {
                return null;
            }
            //if (!moduleTrackingInformation.Module.IsInitialized)
            //{
            //    moduleTrackingInformation.Module.Initialize();
            //    moduleTrackingInformation.Module.IsInitialized = true;
            //}
            var handlerTrackingInformation = moduleTrackingInformation.HandlerTrackingInformationMap[partArray[1].ToUpper()];
            if (handlerTrackingInformation == null)
            {
                return null;
            }
            //if (!handlerTrackingInformation.SingletonHandler.IsInitialized)
            //{
            //    handlerTrackingInformation.SingletonHandler.Initialize(moduleTrackingInformation.Module);
            //    handlerTrackingInformation.SingletonHandler.IsInitialized = true;
            //}
            return handlerTrackingInformation.SingletonHandler as Handler;
        }

        public void Submit(string deviceID, string handlerName, params Value[] parameterArray)
        {
            var handler = GetHandlerByScopedName(handlerName) as Handler;
            if (handler == null)
            {
                throw new InvalidOperationException(handlerName + " not found");
            }
            handler.Module.Handoff(deviceID, handler, PacketData.Create(), parameterArray);
        }

        public Value[] OverrideDefaultValueData(Value[] valueArray, params Value[] parameterArray)
        {
            var valueList = new List<Value>(valueArray);
            var parameterList = new List<Value>(parameterArray);
            foreach (var parameter in parameterArray)
            {
                if (!valueList.Any(p => p.Name.Equals(parameter.Name)))
                {
                    valueList.Add(parameter);
                }
            }
            return valueList.ToArray();
        }
    }
}
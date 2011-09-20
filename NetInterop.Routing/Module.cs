using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nalarium;

namespace NetInterop.Routing
{
    public abstract class Module
    {
        //[ImportMany]
        //private List<Handler> _parserList;

        private string _name;

        public List<string> ActiveInterfaceList { get; set; }

        protected internal RoutingController Controller { get; set; }

        public ModuleTrackingInformation ModuleTrackingInformation { get; set; }

        protected internal bool IsEnabled { get; set; }

        public String Name { get; internal set; }
        internal Thread MainThread { get; set; }

        public virtual void Install()
        {
        }

        public void InstallAll()
        {
            ActiveInterfaceList = new List<string>();
            Install();
        }

        public void Start()
        {
            InitializeAll();
            MainThread.Start();
        }

        private void InitializeAll()
        {
            MainThread = new Thread(ThreadStart);
            SetupInterfaceData();
            CheckForActiveInterfaceList();
            Initialize();
        }

        protected internal virtual void SetupInterfaceData()
        {
        }

        private void CheckForActiveInterfaceList()
        {
            if (ActiveInterfaceList.Count > 0)
            {
                IsEnabled = true;
            }
            else
            {
                return;
            }
        }

        protected internal virtual void Initialize()
        {
        }

        private void ThreadStart()
        {
            //Controller.VerboseOutput("Stopped module " + Name + "." + Environment.NewLine);
        }

        internal void Stop()
        {
            RoutingController.VerboseOutput("Stopping module " + Name + "..." + Environment.NewLine);
            IsEnabled = false;
        }

        protected PacketData GeneratePacketData(params IHeader[] parameterArray)
        {
            return GeneratePacketData(PacketData.Create(), parameterArray);
        }

        protected PacketData GeneratePacketData(PacketData packetData, params IHeader[] parameterArray)
        {
            if (parameterArray == null)
            {
                return null;
            }
            foreach (IHeader header in parameterArray)
            {
                Type type = header.GetType();
                Handler parser = Controller.GetHeaderOwner(type);
                packetData = parser.GetBytes(header, packetData);
            }
            return packetData;
            //++ scan through each, getting type. Lookup header type in a dictionary that was populated from 
            //++ attributes on various types declaring header ownership.

            //++ for L2-- this will be per interface and the singletons will become PER listening interface and those singletons will know their L2 type.

            //++ also for L2, this setup will know the interface and thus also know the L2-- this will effectively hide all MAC address stuff
        }

        protected void Send(string deviceID, PacketData packetData)
        {
            Controller.Send(deviceID, packetData.Data);
        }

        internal static void ValidateParameterArray(Value[] parameterArray)
        {
            Func<Value, Boolean> doesNotContainScope = p => !p.IsScoped;
            if (parameterArray.Any(doesNotContainScope))
            {
                throw new ArgumentException(string.Format("Argument not scoped ({0}).", parameterArray.First(doesNotContainScope)));
            }
        }

        public void Handoff(string deviceID, Type type, PacketData packetData, params Value[] parameterArray)
        {
            Handoff(deviceID, Controller.GetHeaderOwner(type), packetData, parameterArray);
        }
        public void Handoff(string deviceID, Handler handler, PacketData packetData, params Value[] parameterArray)
        {
            ValidateParameterArray(parameterArray);
            Module module = Controller.GetHandlerOwner(handler.GetType());
            module.AcceptHandoff(deviceID, handler.Name, packetData, parameterArray);
        }

        protected virtual void AcceptHandoff(string deviceID, string nextHandlerName, PacketData packetData, Value[] parameterArray)
        {
        }

        protected List<string> GetInterfaceListForAddress(IPAddress address, bool allowConnectionOverSeconaryIP = false)
        {
            return Controller.GetInterfaceListForAddress(address);
        }

        protected List<string> GetInterfaceListForAddress(IPAddress address, IPAddress mask, bool allowConnectionOverSeconaryIP = false)
        {
            return Controller.GetInterfaceListForAddress(address, mask);
        }

        public void Build(string deviceID, string handlerName, Value[] parameterArray)
        {
            throw new NotImplementedException();
        }

        protected List<IHeader> CatalogHeaderListInBranch(string nextHandlerName, params Value[] parameterArray)
        {
            Handler handler = null;
            var headerList = new List<IHeader>();
            handler = Controller.GetSingletonHandler(nextHandlerName);
            do
            {
                if (handler != null)
                {
                    headerList.Add(handler.Build(parameterArray));
                    handler = handler.Parent;
                }
            } while (handler != null && handler.ModuleName.Equals(Name));
            return headerList;
        }
    }
}
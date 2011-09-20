using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Nalarium;
using NetInterop.Routing.Core;
using NetInterop.Routing.Ospf.Configuration;

namespace NetInterop.Routing.Ospf
{
    internal class Interface
    {
        private static readonly object DecisionLock = new object();
        private DateTime _beginHelloTime;
        private DateTime _beginWaitTime;
        private InterfaceElement _config;
        private RoutingController _controller;
        private Timer _helloTimer;
        private StateMachine<InterfaceState, InterfaceEventType> _stateMachine;
        private Timer _waitTimer;

        private Interface()
        {
            NeighborMap = new Map<IPAddress, Neighbor>();
        }

        internal string DeviceID { get; set; }

        public OspfModule Module { get; set; }

        public int WaitTimerLength { get; set; }

        public OspfNetworkType OspfNetworkType { get; set; }

        private InterfaceState _interfaceState;

        internal InterfaceState InterfaceState
        {
            get
            {
                return _interfaceState;
            }
            set
            {
                _interfaceState = value;
                Log.Write("OSPF", "INTERFACE", "State change: " + value);
            }
        }

        public IPAddress IPAddress
        {
            get
            {
                return _controller.DeviceConfigurationMap[DeviceID].PrimaryIPConfiguration.Address;
            }
        }

        public IPAddress Mask
        {
            get
            {
                return _controller.DeviceConfigurationMap[DeviceID].PrimaryIPConfiguration.Mask;
            }
        }

        internal Area Area { get; set; }

        public uint AreaNumber
        {
            get
            {
                return Area.Number;
            }
        }

        public ushort HelloInterval { get; set; }

        public uint RouterDeadInterval { get; set; }

        public byte InfTransDelay { get; set; }

        public byte Priority { get; set; }

        public bool IsDREligible
        {
            get
            {
                return Priority != 0;
            }
        }

        public Map<IPAddress, Neighbor> NeighborMap { get; set; }

        ///// <summary>
        ///// For DR/BDR election.
        ///// </summary>
        //private Neighbor NeighborSelf { get; set; }

        public IPAddress RemoteRouterID { get; set; }

        public IPAddress DR { get; set; }

        public IPAddress BDR { get; set; }

        public bool IsDR
        {
            get
            {
                return DR == IPAddress;
            }
        }

        public bool IsBDR
        {
            get
            {
                return BDR == IPAddress;
            }
        }

        public uint Cost { get; set; }

        public ushort RxmtInterval { get; set; }

        public OspfAuthType OspfAuthType { get; set; }

        public byte[] AuthenticationKey { get; set; }

        public static Interface Create(OspfModule module, RoutingController controller, string deviceID, Area area, InterfaceElement config)
        {
            var interfc = new Interface
                          {
                              Module = module,
                              _controller = controller,
                              DeviceID = deviceID,
                              Area = area,
                              _config = config,
                              InterfaceState = InterfaceState.Down
                          };
            interfc.Priority = config.Priority;
            interfc.OspfNetworkType = config.Type;
            interfc.HelloInterval = config.HelloInterval;
            interfc.RouterDeadInterval = config.RouterDeadInterval;
            interfc.RxmtInterval = config.RxmtInterval;
            //interfc.NeighborSelf = Neighbor.Create(module, controller, interfc, controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address, module.RouterID,
            //                                       new OspfOptions(), config.Priority);
            return interfc;
        }

        internal void Init()
        {
            DR = IPAddress.GetDefault();
            BDR = IPAddress.GetDefault();
            _stateMachine = new StateMachine<InterfaceState, InterfaceEventType>();
            _stateMachine.Add(InterfaceState.Down, InterfaceEventType.InterfaceUp, InterfaceUp);
            _stateMachine.Add(InterfaceState.Waiting, InterfaceEventType.BackupSeen, BackupSeen);
            _stateMachine.Add(InterfaceState.Waiting, InterfaceEventType.WaitTimer, WaitTimer);
            _stateMachine.Add(InterfaceState.DROther, InterfaceEventType.NeighborChange, NeighborChange);
            _stateMachine.Add(InterfaceState.DR, InterfaceEventType.NeighborChange, NeighborChange);
            _stateMachine.Add(InterfaceState.Backup, InterfaceEventType.NeighborChange, NeighborChange);
            _stateMachine.Add(InterfaceEventType.InterfaceDown, InterfaceDown);
            _stateMachine.Add(InterfaceEventType.LoopInd, LoopInd, Ospf.InterfaceState.Loopback);
            _stateMachine.Add(InterfaceState.Loopback, InterfaceEventType.UnloopInd, UnloopInd, Ospf.InterfaceState.Down);

            Device device = _controller.DeviceConfigurationMap[DeviceID];
            if (device.IsEnabled && device.InterfaceState == Routing.InterfaceState.L2UpL3Up)
            {
                RaiseEvent(InterfaceEventType.InterfaceUp);
            }
        }

        public void InterfaceUp(InterfaceState state, InterfaceEventType eventType, object data)
        {
            if (_controller.DeviceConfigurationMap[DeviceID].IsLoopback)
            {
                RaiseEvent(InterfaceEventType.LoopInd);
                return;
            }
            _helloTimer = new Timer();
            Timer helloTimer = _helloTimer;
            helloTimer.Interval = 1000;
            helloTimer.Elapsed += HelloElapsed;
            helloTimer.Start();
            switch (OspfNetworkType)
            {
                case OspfNetworkType.PointToPoint:
                case OspfNetworkType.PointToMultiPoint:
                case OspfNetworkType.VirtualLink:
                    InterfaceState = InterfaceState.PointToPoint;
                    break;
                default:
                    if (Priority == 0)
                    {
                        InterfaceState = InterfaceState.DROther;
                    }
                    else
                    {
                        InterfaceState = InterfaceState.Waiting;
                        _beginWaitTime = DateTime.Now;
                        _waitTimer = new Timer();
                        _waitTimer.Interval = 1000;
                        _waitTimer.Elapsed += (s, e) =>
                        {
                            if ((DateTime.Now - _beginWaitTime).Seconds < RouterDeadInterval)
                            {
                                return;
                            }
                            _waitTimer.Stop();
                            CalculateDRAndBDR();
                        };
                        if (OspfNetworkType == OspfNetworkType.NBMA)
                        {
                            SetupManualNeighborData();
                        }
                        _waitTimer.Start();
                    }
                    break;
            }
        }

        private void CalculateDRAndBDR()
        {
            lock (DecisionLock)
            {
                DR = IPAddress.GetDefault();
                BDR = IPAddress.GetDefault();
                var neighborData = NeighborMap.Values
                    .Where(p => ((int)p.OspfNeighborState) >= (int)OspfNeighborState.TwoWay &&
                                p.Priority != 0)
                    .Select(p => new Tuple<IPAddress, IPAddress, IPAddress, byte, IPAddress>(p.Address, p.DR, p.BDR, p.Priority, p.RouterID))
                    .ToList();
                neighborData.Add(new Tuple<IPAddress, IPAddress, IPAddress, byte, IPAddress>(IPAddress, DR, BDR, Priority, Module.RouterID));
                var sortedData = neighborData
                    .OrderByDescending(p => p.Item4)
                    .OrderByDescending(p => p.Item5).ToList();
                var haveNoDRClaim = sortedData.Where(p => p.Item2 != p.Item1);
                var bdrData = sortedData.Where(p => p.Item3 == p.Item1).FirstOrDefault() ?? haveNoDRClaim.FirstOrDefault();
                var drData = sortedData.Where(p => p.Item2 == p.Item1).FirstOrDefault() ?? bdrData;
                IPAddress claimedBdr;
                IPAddress claimedDr;
                if ((Module.RouterID != DR && Module.RouterID == drData.Item1) ||
                    (Module.RouterID != BDR && Module.RouterID == bdrData.Item1) ||
                    (Module.RouterID == DR && Module.RouterID != drData.Item1) ||
                    (Module.RouterID == BDR && Module.RouterID != bdrData.Item1))
                {
                    claimedBdr = bdrData.Item2;
                    claimedDr = drData.Item2;
                    neighborData = NeighborMap.Values
                        .Where(p => ((int)p.OspfNeighborState) >= (int)OspfNeighborState.TwoWay &&
                                    p.Priority != 0)
                        .Select(p => new Tuple<IPAddress, IPAddress, IPAddress, byte, IPAddress>(p.Address, p.DR, p.BDR, p.Priority, p.RouterID))
                        .ToList();
                    neighborData.Add(new Tuple<IPAddress, IPAddress, IPAddress, byte, IPAddress>(IPAddress, claimedBdr, claimedDr, Priority, Module.RouterID));
                    sortedData = neighborData
                    .OrderByDescending(p => p.Item4)
                    .OrderByDescending(p => p.Item5).ToList();
                    haveNoDRClaim = sortedData.Where(p => p.Item2 != p.Item1);
                    bdrData = sortedData.Where(p => p.Item3 == p.Item1).FirstOrDefault() ?? haveNoDRClaim.FirstOrDefault();
                    drData = sortedData.Where(p => p.Item2 == p.Item1).FirstOrDefault() ?? bdrData;
                }
                if (Module.RouterID == drData.Item1)
                {
                    InterfaceState = InterfaceState.DR;
                }
                else if (Module.RouterID == bdrData.Item1)
                {
                    InterfaceState = InterfaceState.Backup;
                }
                else
                {
                    InterfaceState = InterfaceState.DROther;
                }
                DR = drData.Item1;
                BDR = bdrData.Item1;
                Log.Write("OSPF", "CalculateDRAndBDR", string.Format("DR: {0}, BDR: {1}", DR, BDR));
                if ((Module.RouterID != DR && Module.RouterID == drData.Item1) ||
                    (Module.RouterID != BDR && Module.RouterID == bdrData.Item1) ||
                    (Module.RouterID == DR && Module.RouterID != drData.Item1) ||
                    (Module.RouterID == BDR && Module.RouterID != bdrData.Item1))
                {
                    NeighborMap.Values
                        .Where(p => ((int)p.OspfNeighborState) >= (int)OspfNeighborState.TwoWay &&
                                    p.Priority != 0)
                        .ToList().ForEach(p => p.RaiseEvent(NeighborEventType.AdjOK, new Tuple<IPAddress, IPAddress, IPAddress>(drData.Item1, bdrData.Item1, IPAddress)));
                }
            }
        }

        private void PrepareHello()
        {
            if (OspfNetworkType == OspfNetworkType.Broadcast || OspfNetworkType == OspfNetworkType.PointToPoint)
            {
                SendHello(Constant.AllSPFRouters);
            }
            else if (OspfNetworkType == OspfNetworkType.VirtualLink)
            {
                //++ unicast
                //SendHello(whateverhere);
            }
            else if (OspfNetworkType == OspfNetworkType.PointToMultiPoint)
            {
                foreach (IPAddress key in NeighborMap.Keys)
                {
                    //++ unicast
                    //SendHello(whateverhere);
                }
            }
        }

        private void SendHello(IPAddress address)
        {
            //++ parsers build the header
            var ospfLlsDataBlockTlv = (OspfLlsDataBlockTlv)_controller.GetSingletonHandler("OSPFLLSDATABLOCKTLV").Build();
            var ospfLlsDataBlockHeader = (OspfLlsDataBlockHeader)_controller.GetSingletonHandler("OSPFLLSDATABLOCK").Build();
            var header = (OspfHelloHeader)_controller.GetSingletonHandler("OSPFHELLO").Build(
                Value.Raw("OspfHelloHeader:Mask", Mask),
                Value.Raw("OspfHelloHeader:Interval", HelloInterval),
                Value.Raw("OspfHelloHeader:RouterDeadInterval", RouterDeadInterval),
                Value.Raw("OspfHelloHeader:RouterPriority", Priority),
                Value.Raw("OspfHelloHeader:Options", OspfHelloHeader.GetByteFromFlags(Area.GetOptions())),
                Value.Raw("OspfHelloHeader:Dr", DR),
                Value.Raw("OspfHelloHeader:Bdr", BDR),
                Value.Raw("OspfHelloHeader:Neighbor", NeighborMap.Select(p => p.Value.RouterID).ToArray()));
            Module.ContinueSend(DeviceID, address, ospfLlsDataBlockTlv, ospfLlsDataBlockHeader, header,
                Value.Raw("OspfHeader:Type", (byte)OspfPacketType.Hello));
        }

        private void BackupSeen(InterfaceState state, InterfaceEventType eventType, object data)
        {
            _waitTimer.Stop();
            CalculateDRAndBDR();
        }

        private void WaitTimer(InterfaceState state, InterfaceEventType eventType, object data)
        {
            CalculateDRAndBDR();
        }

        private void NeighborChange(InterfaceState state, InterfaceEventType eventType, object data)
        {
            CalculateDRAndBDR();
        }

        private void InterfaceDown(InterfaceState state, InterfaceEventType eventType, object data)
        {
        }

        private void LoopInd(InterfaceState state, InterfaceEventType eventType, object data)
        {
        }

        private void UnloopInd(InterfaceState state, InterfaceEventType eventType, object data)
        {
        }

        private static void SetupManualNeighborData()
        {
            //TODO: for NBMA
        }

        private void HelloElapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - _beginHelloTime).Seconds < HelloInterval)
            {
                return;
            }
            PrepareHello();
            _beginHelloTime = DateTime.Now;
        }

        internal void RaiseEvent(InterfaceEventType eventType, object data = null)
        {
            Log.Write("OSPF", "INTERFACEEVENT", string.Format("State: {0}, Event: {1}", InterfaceState, eventType));
            _stateMachine.RaiseEvent(InterfaceState, eventType, data);
        }

        public void HelloReceived(ModuleEventArgs args)
        {
            HeaderPackage package = args.HeaderPackage;
            var ipHeader = package.GetHeader<IPHeader>();
            var ospfHeader = package.GetHeader<OspfHeader>();
            var ospfHelloHeader = package.GetHeader<OspfHelloHeader>();
            if (ospfHelloHeader.Interval != HelloInterval)
            {
                Log.Write("OSPF", "INTERFACE",
                          string.Format("Mismatched hello interval. Local:{0}, Remote:{1}", HelloInterval,
                                        ospfHelloHeader.Interval));
                return;
            }
            if (ospfHelloHeader.RouterDeadInterval != RouterDeadInterval)
            {
                Log.Write("OSPF", "INTERFACE",
                          string.Format("Mismatched router dead interval. Local:{0}, Remote:{1}", RouterDeadInterval,
                                        ospfHelloHeader.RouterDeadInterval));
                return;
            }
            if (OspfNetworkType != OspfNetworkType.PointToPoint && OspfNetworkType != OspfNetworkType.VirtualLink)
            {
                if (ospfHelloHeader.Mask != Mask)
                {
                    Log.Write("OSPF", "INTERFACE",
                              string.Format("Mismatched mask. Local:{0}, Remote:{1}", Mask,
                                            ospfHelloHeader.Mask));
                    return;
                }
            }
            bool remoteERC = (ospfHelloHeader.OspfOptions & OspfOptions.ExternalRoutingCapable) == OspfOptions.ExternalRoutingCapable;
            if (remoteERC != Area.ExternalRoutingCapability)
            {
                Log.Write("OSPF", "INTERFACE",
                          string.Format("Mismatched external routing capability. Local:{0}, Remote:{1}", Area.ExternalRoutingCapability,
                                        remoteERC));
                return;
            }
            IPAddress source;
            IPAddress id;
            GetSourceAndID(ospfHeader, ipHeader, out source, out id);
            if (!NeighborMap.ContainsKey(id))
            {
                NeighborMap.Add(id, Neighbor.Create(Module, _controller, this, source, id, ospfHelloHeader.OspfOptions, ospfHelloHeader.RouterPriority, ospfHelloHeader.Dr, ospfHelloHeader.Bdr));
            }
            lock (DecisionLock)
            {
                if (InterfaceState == InterfaceState.Waiting)
                {
                    if (ospfHelloHeader.Dr == ipHeader.SourceAddress &&
                        ospfHelloHeader.Bdr == IPAddress.GetDefault())
                    {
                        RaiseEvent(InterfaceEventType.BackupSeen, new Tuple<IPHeader, OspfHeader, OspfHelloHeader>(ipHeader, ospfHeader, ospfHelloHeader));
                    }
                    else if (ospfHelloHeader.Bdr == ipHeader.SourceAddress)
                    {
                        RaiseEvent(InterfaceEventType.BackupSeen, new Tuple<IPHeader, OspfHeader, OspfHelloHeader>(ipHeader, ospfHeader, ospfHelloHeader));
                    }
                }
                else if ((ospfHelloHeader.Dr == ipHeader.SourceAddress && DR != ipHeader.SourceAddress) ||
                         (ospfHelloHeader.Dr != ipHeader.SourceAddress && DR == ipHeader.SourceAddress)
                    )
                {
                    RaiseEvent(InterfaceEventType.NeighborChange, new Tuple<IPHeader, OspfHeader, OspfHelloHeader>(ipHeader, ospfHeader, ospfHelloHeader));
                }
                else if ((ospfHelloHeader.Bdr == ipHeader.SourceAddress && BDR != ipHeader.SourceAddress) ||
                         (ospfHelloHeader.Bdr != ipHeader.SourceAddress && BDR == ipHeader.SourceAddress)
                    )
                {
                    RaiseEvent(InterfaceEventType.NeighborChange, new Tuple<IPHeader, OspfHeader, OspfHelloHeader>(ipHeader, ospfHeader, ospfHelloHeader));
                }
            }
            Neighbor neighbor = NeighborMap[id];
            neighbor.OnHelloReceive(ipHeader, ospfHeader, ospfHelloHeader);
        }

        public void DBDReceived(ModuleEventArgs args)
        {
            HeaderPackage package = args.HeaderPackage;
            var ipHeader = package.GetHeader<IPHeader>();
            var ospfHeader = package.GetHeader<OspfHeader>();
            var ospfDBDHeader = package.GetHeader<OspfDbdHeader>();
            OspfLsaHeader[] ospfLsaHeaderArray = null;
            if (ospfDBDHeader.LsaHeaderCount > 0)
            {
                ospfLsaHeaderArray = (OspfLsaHeader[])package.GetHeaderArray<OspfLsaHeader>();
            }
            IPAddress source;
            IPAddress id;
            GetSourceAndID(ospfHeader, ipHeader, out source, out id);
            Neighbor neighbor = NeighborMap[id];
            if (neighbor == null)
            {
                return;
            }
            neighbor.OnDBDReceive(ipHeader, ospfHeader,
                                  new Tuple<OspfDbdHeader, OspfLsaHeader[]>(ospfDBDHeader, ospfLsaHeaderArray));
        }

        private void GetSourceAndID(OspfHeader ospfHeader, IPHeader ipHeader, out IPAddress source, out IPAddress id)
        {
            switch (OspfNetworkType)
            {
                case OspfNetworkType.PointToMultiPoint:
                case OspfNetworkType.Broadcast:
                case OspfNetworkType.NBMA:
                    source = ipHeader.SourceAddress;
                    id = ospfHeader.RouterID;
                    break;
                case OspfNetworkType.VirtualLink:
                    source = ospfHeader.RouterID;
                    id = ospfHeader.RouterID;
                    break;
                default:
                case OspfNetworkType.PointToPoint:
                    source = ospfHeader.RouterID;
                    id = ipHeader.SourceAddress;
                    break;
            }
        }
    }
}
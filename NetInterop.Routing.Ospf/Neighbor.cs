using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Ospf
{
    [DebuggerDisplay("Address: {Address}, RouterID: {RouterID}, DR: {DR}, BDR: {BDR}")]
    internal class Neighbor
    {
        private static readonly object Lock = new object();

        private StateMachine<OspfNeighborState, NeighborEventType> _stateMachine;
        private RoutingController _controller;
        private Timer _dbdResendTimer;
        private bool _hasAttemptedAdjacency;
        private uint _helloInterval;
        private Timer _inactivityTimer;
        private Timer _sendTimer;
        private DateTime _dbdSendTimerLastSend;

        private bool _isNew = true;
        private byte _priority;
        private Map<uint, Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv>> _dbdSendList;
        private MapEntry<uint, Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv>> _dbdPendingAck;

        private OspfModule Module { get; set; }
        private Interface Interface { get; set; }


        private MasterSlaveState LocalMSState { get; set; }
        private OspfOptions DiscoveredOptions { get; set; }

        private uint DDSequenceNumber { get; set; }
        private OspfDbdHeader LastOspfDBDSeen { get; set; }
        private object LastReceivedDBD { get; set; }

        private List<OspfLsaHeader> LSRetransmissionList { get; set; }
        private List<OspfLsaHeader> DBSummaryList { get; set; }
        private List<OspfLsaHeader> LSRequestList { get; set; }

        internal IPAddress Address { get; set; }
        internal IPAddress DR { get; set; }
        internal IPAddress BDR { get; set; }
        private OspfNeighborState _ospfNeighborState;

        internal OspfNeighborState OspfNeighborState
        {
            get
            {
                return _ospfNeighborState;
            }
            set
            {
                _ospfNeighborState = value;
                Log.Write("OSPF", "NEIGHBOR", "State change: " + value);
            }
        }

        internal IPAddress RouterID { get; set; }

        public byte Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Priority must be at least 0.");
                }
                _priority = value;
            }
        }

        public DateTime LastSeen { get; set; }

        //public uint RouterDeadInterval { get; set; }

        //public uint HelloInterval
        //{
        //    get
        //    {
        //        return _helloInterval;
        //    }
        //    set
        //    {
        //        _inactivityTimer.Interval = value;
        //        _helloInterval = value;
        //    }
        //}

        private Neighbor()
        {
        }

        private static Neighbor QuickCreate(IPAddress dr, IPAddress bdr)
        {
            return new Neighbor
            {
                DR = dr,
                BDR = bdr
            };
        }

        public static Neighbor Create(OspfModule module, RoutingController controller, Interface ospfInterface, IPAddress source, IPAddress rid, OspfOptions options, byte priority, IPAddress dr, IPAddress bdr)
        {
            var n = new Neighbor
                   {
                       _controller = controller,
                       Module = module,
                       Interface = ospfInterface,
                       RouterID = rid,
                       DiscoveredOptions = options,
                       //TODO: is THIS the source??
                       Address = source,
                       Priority = priority,
                       OspfNeighborState = OspfNeighborState.Down,
                       DR = dr,
                       BDR = bdr,
                       LastSeen = DateTime.Now
                   };
            n.Init();
            return n;
        }

        private void Init()
        {
            _sendTimer = new Timer(100);
            _inactivityTimer = new Timer();
            InitListData();
            _stateMachine = new StateMachine<OspfNeighborState, NeighborEventType>();
            ////++ NBMA
            ////_stateMachine.Add(OspfNeighborState.Down, NeighborEventType.Start, Start);
            ////_stateMachine.Add(OspfNeighborState.Attempt, NeighborEventType.HelloReceived, HelloReceived);
            _stateMachine.Add(OspfNeighborState.Down, NeighborEventType.HelloReceived, HelloReceived, nextState: OspfNeighborState.Init);
            _stateMachine.Add(OspfNeighborState.Init, NeighborEventType.HelloReceived, HelloReceived, StateEntryMode.AtLeast, nextStateMode: NextStateMode.NoAction);
            _stateMachine.Add(OspfNeighborState.Init, NeighborEventType.TwoWayReceived, TwoWayReceived);
            _stateMachine.Add(OspfNeighborState.ExStart, NeighborEventType.NegotiationDone, NegotiationDone);
            _stateMachine.Add(OspfNeighborState.Exchange, NeighborEventType.ExchangeDone, ExchangeDone);
            //_stateMachine.Add(OspfNeighborState.Loading, NeighborEventType.LoadingDone, LoadingDone);
            _stateMachine.Add(OspfNeighborState.TwoWay, NeighborEventType.AdjOK, AdjOK);
            _stateMachine.Add(OspfNeighborState.ExStart, NeighborEventType.AdjOK, AdjOK, StateEntryMode.AtLeast);
            _stateMachine.Add(OspfNeighborState.Exchange, NeighborEventType.SeqNumberMismatch, SeqNumberMismatch, StateEntryMode.AtLeast);
            //_stateMachine.Add(OspfNeighborState.Exchange, NeighborEventType.BadLSReq, BadLSReq, StateEntryMode.AtLeast);
            _stateMachine.Add(NeighborEventType.KillNbr, (s, e, o) =>
            {
                InitListData();
                _inactivityTimer.Stop();
            }, OspfNeighborState.Down);
            _stateMachine.Add(NeighborEventType.LLDown, (s, e, o) =>
            {
                InitListData();
                _inactivityTimer.Stop();
            }, OspfNeighborState.Down);
            _stateMachine.Add(NeighborEventType.InactivityTimer, (s, e, o) => InitListData(), OspfNeighborState.Down);
            _stateMachine.Add(OspfNeighborState.TwoWay, NeighborEventType.OneWayReceived, (s, e, o) => InitListData(), StateEntryMode.AtLeast, OspfNeighborState.Init);
            _stateMachine.Add(OspfNeighborState.TwoWay, NeighborEventType.TwoWayReceived, StateEntryMode.AtLeast, NextStateMode.NoAction);
            _stateMachine.Add(OspfNeighborState.Init, NeighborEventType.OneWayReceived, NextStateMode.NoAction);
            _sendTimer.Elapsed += (s, e) =>
            {
                SendDBDReady();
            };
            _sendTimer.Start();
        }

        private void InitListData()
        {
            LSRetransmissionList = new List<OspfLsaHeader>();
            DBSummaryList = new List<OspfLsaHeader>();
            LSRequestList = new List<OspfLsaHeader>();
            _dbdSendList = new Map<uint, Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv>>();
        }

        private void HelloReceived(OspfNeighborState state, NeighborEventType evt, object data)
        {
            _inactivityTimer = new Timer();
            _inactivityTimer.Interval = 1000;
            _inactivityTimer.Elapsed += (s, e) =>
                                        {
                                            if ((DateTime.Now - LastSeen).Seconds > Interface.RouterDeadInterval)
                                            {
                                                RaiseEvent(NeighborEventType.InactivityTimer);
                                            }
                                        };
            _inactivityTimer.Start();
            OspfNeighborState nextState;
            if (_stateMachine.GetNext(state, evt, out nextState))
            {
                OspfNeighborState = nextState;
            }
        }

        private void TwoWayReceived(OspfNeighborState state, NeighborEventType evt, object data)
        {
            var tupleData = data as Tuple<IPHeader, OspfHeader, Tuple<IPAddress, IPAddress>>;
            var dr = tupleData.Item3.Item1;
            var bdr = tupleData.Item3.Item2;
            var newTupleData = new Tuple<IPAddress, IPAddress, IPAddress>(dr, bdr, tupleData.Item1.SourceAddress);
            if (ShouldBeAdjacent(newTupleData))
            {
                ProcessAdjacency(newTupleData);
            }
            else
            {
                OspfNeighborState = OspfNeighborState.TwoWay;
            }
        }

        private void ProcessAdjacency(Tuple<IPAddress, IPAddress, IPAddress> tupleData)
        {
            OspfNeighborState = OspfNeighborState.ExStart;
            if (_hasAttemptedAdjacency)
            {
                DDSequenceNumber++;
            }
            else
            {
                //++ sending blank DBDs to start things off
                DDSequenceNumber = (uint)DateTime.Now.Ticks;
                LocalMSState = MasterSlaveState.Master;
                Action sendDBD = () =>
                                 {
                                     _dbdPendingAck = null;
                                     PrepareSendDBD(tupleData.Item3, GetDBDOptions(isFirstRun: true, hasMore: true));
                                 };
                //_dbdResendTimer = new Timer(1000);
                //_dbdResendTimer.Elapsed += (s, e) =>
                //                         {
                //                             //TODO: does this need a "stillWaitingOnFirst" check or something?
                //                             if ((DateTime.Now - _dbdSendTimerLastSend).Seconds == Interface.RxmtInterval)
                //                             {
                //                                 sendDBD();
                //                             }
                //                         };
                sendDBD();
                _dbdSendTimerLastSend = DateTime.Now;
                //_dbdResendTimer.Start();
                //TODO: should this ever be reset?
                _hasAttemptedAdjacency = true;
            }
        }

        private void NegotiationDone(OspfNeighborState state, NeighborEventType evt, object data)
        {
            var tupleData = data as Tuple<IPHeader, OspfHeader, object>;
            //+
            DBSummaryList.AddRange(Interface.Area.RouterLSAMap.Values.Where(p => p.CommonHeader.LSAge < Constant.MaxAge).Select(p => p.CommonHeader));
            LSRetransmissionList.AddRange(Interface.Area.RouterLSAMap.Values.Where(p => p.CommonHeader.LSAge >= Constant.MaxAge).Select(p => p.CommonHeader));
            //+
            DBSummaryList.AddRange(Interface.Area.NetworkLSAMap.Values.Where(p => p.CommonHeader.LSAge < Constant.MaxAge).Select(p => p.CommonHeader));
            LSRetransmissionList.AddRange(Interface.Area.NetworkLSAMap.Values.Where(p => p.CommonHeader.LSAge >= Constant.MaxAge).Select(p => p.CommonHeader));
            //+
            DBSummaryList.AddRange(Interface.Area.SummaryLSAMap.Values.Where(p => p.CommonHeader.LSAge < Constant.MaxAge).Select(p => p.CommonHeader));
            LSRetransmissionList.AddRange(Interface.Area.SummaryLSAMap.Values.Where(p => p.CommonHeader.LSAge >= Constant.MaxAge).Select(p => p.CommonHeader));
            //+
            if (Interface.OspfNetworkType != OspfNetworkType.VirtualLink && Interface.Area.ExternalRoutingCapability)
            {
                DBSummaryList.AddRange(Module.ASExternalLSAMap.Values.Where(p => p.CommonHeader.LSAge < Constant.MaxAge).Select(p => p.CommonHeader));
                LSRetransmissionList.AddRange(Module.ASExternalLSAMap.Values.Where(p => p.CommonHeader.LSAge < Constant.MaxAge).Select(p => p.CommonHeader));
            }
            OspfNeighborState = OspfNeighborState.Exchange;
        }

        private void AdjOK(OspfNeighborState state, NeighborEventType evt, object data)
        {
            var tupleData = data as Tuple<IPAddress, IPAddress, IPAddress>;
            if (OspfNeighborState == OspfNeighborState.TwoWay)
            {
                if (ShouldBeAdjacent(tupleData))
                {
                    OspfNeighborState = OspfNeighborState.ExStart;
                    ProcessAdjacency(tupleData);
                }
            }
            else if (OspfNeighborState == OspfNeighborState.ExStart)
            {
                if (!ShouldBeAdjacent(tupleData))
                {
                    InitListData();
                    OspfNeighborState = OspfNeighborState.TwoWay;
                }
            }
        }

        private void ExchangeDone(OspfNeighborState state, NeighborEventType evt, object data)
        {
            if (LSRequestList.Count == 0)
            {
                OspfNeighborState = OspfNeighborState.Full;
            }
            else
            {
                OspfNeighborState = OspfNeighborState.Loading;
                BeginLSRProcess();
            }
        }

        private void BeginLSRProcess()
        {
            var headerList = new List<IHeader>();
            foreach (var lsaHeader in LSRequestList)
            {
                var header = (OspfLsrHeader)_controller.GetSingletonHandler("OSPFDBD").Build(
                    Value.Create("OspfLSRHeader:Type", lsaHeader.LSType),
                    Value.Create("OspfLSRHeader:LSID", lsaHeader.LSID),
                    Value.Create("OspfLSRHeader:AdvertisingRouter", lsaHeader.AdvertisingRouter));
                headerList.Add(header);
            }
            //TODO: is this address right?
            //TODO: need a RxmtInterval timer here
            Module.ContinueSend(Interface.DeviceID, Address, headerList.ToArray(), Value.Raw("OspfHeader:Type", (byte)OspfPacketType.LinkStateRequest));
        }

        private void SeqNumberMismatch(OspfNeighborState state, NeighborEventType evt, object data)
        {
            var tupleData = data as Tuple<IPHeader, OspfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]>>;
            OspfNeighborState = OspfNeighborState.ExStart;
            InitListData();
            DDSequenceNumber++;
            LocalMSState = MasterSlaveState.Master;
            PrepareSendDBD(tupleData.Item1.SourceAddress, GetDBDOptions(isFirstRun: true, hasMore: true));
        }

        private DBDOptions GetDBDOptions(bool isFirstRun, bool hasMore)
        {
            var options = new DBDOptions();
            if (isFirstRun)
            {
                options |= DBDOptions.Initial;
            }
            if (LocalMSState == MasterSlaveState.Master)
            {
                options |= DBDOptions.Master;
            }
            if (hasMore)
            {
                options |= DBDOptions.More;
            }
            return options;
        }

        private void PrepareSendDBD(IPAddress destinationAddress, DBDOptions dbdOptions, Boolean sendLsaHeaderData = false)
        {
            lock (Lock)
            {
                //++ parsers build the header
                ushort mtu = Interface.OspfNetworkType == OspfNetworkType.VirtualLink ? (ushort)0 : OspfDbdHeader.EthernetAndP2PMtu;
                var ospfLlsDataBlockTlv = (OspfLlsDataBlockTlv)_controller.GetSingletonHandler("OSPFLLSDATABLOCKTLV").Build();
                var ospfLlsDataBlockHeader = (OspfLlsDataBlockHeader)_controller.GetSingletonHandler("OSPFLLSDATABLOCK").Build();
                List<OspfLsaHeader> headerList = null;
                if (sendLsaHeaderData)
                {
                    headerList = DBSummaryList;
                }
                var dbdHeader = (OspfDbdHeader)_controller.GetSingletonHandler("OSPFDBD").Build(
                    Value.Raw("OspfDbdHeader:Mtu", mtu),
                    Value.Raw("OspfDbdHeader:Options", (byte)Interface.Area.GetOptions(supportOBit: true)),
                    Value.Raw("OspfDbdHeader:Flags", (byte)dbdOptions),
                    Value.Raw("OspfDbdHeader:SequenceNumber", DDSequenceNumber));
                dbdHeader.LsaList = headerList;
                _dbdSendList.Add(DDSequenceNumber, new Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv>(destinationAddress, dbdHeader, ospfLlsDataBlockHeader, ospfLlsDataBlockTlv));
                SendDBDReady();
            }
        }

        private void SendDBDReady()
        {
            if (_dbdPendingAck != null && (DateTime.Now - _dbdSendTimerLastSend).Seconds >= Interface.RxmtInterval)
            {
                _dbdSendList.Add(_dbdPendingAck.Key, _dbdPendingAck.Value);
                _dbdPendingAck = null;
            }
            if (_dbdPendingAck == null && _dbdSendList.Count > 0)
            {
                var dbd = _dbdSendList.OrderBy(p => p.Key).FirstOrDefault();
                _dbdSendList.Remove(dbd.Key);
                if (dbd.Value != null)
                {
                    _dbdPendingAck = MapEntry<uint, Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv>>.Create(dbd.Key, dbd.Value);
                    _dbdSendTimerLastSend = DateTime.Now;
                    var count = dbd.Value.Item2.LsaList == null ? 0 : dbd.Value.Item2.LsaList.Count;
                    Log.Write("OSPF", "NEIGHBOR", string.Format("Sending DBD (to {0}). SN: {1}, LSA count: {2}, List count: {3}", dbd.Value.Item1, dbd.Value.Item2.SequenceNumber, count, _dbdSendList.Count));
                    SendDBD(dbd.Value);
                }
            }
        }

        private void SendDBD(Tuple<IPAddress, OspfDbdHeader, OspfLlsDataBlockHeader, OspfLlsDataBlockTlv> data)
        {
            var destinationAddress = data.Item1;
            var dbdHeader = data.Item2;
            var ospfLlsDataBlockHeader = data.Item3;
            var ospfLlsDataBlockTlv = data.Item4;
            Module.ContinueSend(Interface.DeviceID, destinationAddress, ospfLlsDataBlockTlv, ospfLlsDataBlockHeader, dbdHeader,
                Value.Raw("OspfHeader:Type", (byte)OspfPacketType.DatabaseDescriptor));
        }

        private bool ShouldBeAdjacent(Tuple<IPAddress, IPAddress, IPAddress> tupleData)
        {
            IPAddress dr = tupleData.Item1 == IPAddress.GetDefault() ? DR : tupleData.Item1;
            IPAddress bdr = tupleData.Item2 == IPAddress.GetDefault() ? BDR : tupleData.Item2;
            IPAddress saddr = tupleData.Item3;
            if (Interface.OspfNetworkType == OspfNetworkType.PointToPoint ||
                Interface.OspfNetworkType == OspfNetworkType.PointToMultiPoint ||
                Interface.OspfNetworkType == OspfNetworkType.VirtualLink)
            {
                return true;
            }
            else if (Interface.IsDR || Interface.IsDR)
            {
                return true;
            }
            else if (saddr == dr || saddr == bdr)
            {
                return true;
            }
            return false;
        }

        internal void OnHelloReceive(IPHeader ipHeader, OspfHeader ospfHeader, OspfHelloHeader ospfHelloHeader)
        {
            lock (Lock)
            {
                LastSeen = DateTime.Now;
                Log.Write("OSPF", "NEIGHBOR", string.Format("Hello received. From: {0}, State: {1}", ospfHeader.RouterID, OspfNeighborState));
                RaiseEvent(NeighborEventType.HelloReceived);
                if (ospfHelloHeader.Neighbor.Any(p => p == Module.RouterID))
                {
                    var tupleData = new Tuple<IPHeader, OspfHeader, Tuple<IPAddress, IPAddress>>(ipHeader, ospfHeader,
                                                                                                     new Tuple<IPAddress, IPAddress>(ospfHelloHeader.Dr, ospfHelloHeader.Bdr));
                    RaiseEvent(NeighborEventType.TwoWayReceived, tupleData);
                }
                else
                {
                    var tupleData = new Tuple<IPHeader, OspfHeader, object>(ipHeader, ospfHeader, ospfHelloHeader);
                    RaiseEvent(NeighborEventType.OneWayReceived, tupleData);
                }
            }
        }

        internal void OnDBDReceive(IPHeader ipHeader, OspfHeader ospfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]> dbdHeaderData)
        {
            lock (Lock)
            {
                Log.Write("OSPF", "NEIGHBOR", string.Format("DBD received. From: {0}, State: {1}", ospfHeader.RouterID, OspfNeighborState));
                var tupleData = new Tuple<IPHeader, OspfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]>>(ipHeader, ospfHeader, dbdHeaderData);
                var ospfDBDHeader = dbdHeaderData.Item1;
                var ospfLsaHeaderArray = (OspfLsaHeader[])dbdHeaderData.Item2;
                switch (OspfNeighborState)
                {
                    case OspfNeighborState.Down:
                    case OspfNeighborState.Attempt:
                        return;
                    case OspfNeighborState.Init:
                        RaiseEvent(NeighborEventType.TwoWayReceived, new Tuple<IPHeader, OspfHeader, Tuple<IPAddress, IPAddress>>(ipHeader, ospfHeader,
                                                                                                                                      new Tuple<IPAddress, IPAddress>(DR, BDR)));
                        return;
                    case OspfNeighborState.TwoWay:
                        return;
                    case OspfNeighborState.ExStart:
                        if (ospfDBDHeader.IsInitial &&
                            ospfDBDHeader.HasMore &&
                            ospfDBDHeader.IsMaster &&
                            (dbdHeaderData.Item2 == null || ospfLsaHeaderArray.Length == 0) &&
                            ospfHeader.RouterID > Module.RouterID)
                        {
                            Log.Write("OSPF", "NEIGHBOR---------EXSTART", "IS SLAVE TO " + ospfHeader.RouterID);
                            LocalMSState = MasterSlaveState.Slave;
                            _dbdPendingAck = null;
                            DDSequenceNumber = ospfDBDHeader.SequenceNumber;
                            LastOspfDBDSeen = ospfDBDHeader;
                            DiscoveredOptions = ospfDBDHeader.OspfOptions;
                            RaiseEvent(NeighborEventType.NegotiationDone);
                            ProcessDBD(tupleData);
                        }
                        else if (!ospfDBDHeader.IsInitial && !ospfDBDHeader.IsMaster &&
                                 DDSequenceNumber == dbdHeaderData.Item1.SequenceNumber &&
                                 ospfHeader.RouterID < Module.RouterID)
                        {
                            Log.Write("OSPF", "NEIGHBOR---------EXSTART", "IS MASTER OF" + ospfHeader.RouterID);
                            LocalMSState = MasterSlaveState.Master;
                            _dbdPendingAck = null;
                            LastOspfDBDSeen = ospfDBDHeader;
                            DiscoveredOptions = ospfDBDHeader.OspfOptions;
                            RaiseEvent(NeighborEventType.NegotiationDone);
                            ProcessDBD(tupleData);
                        }
                        return;
                    case OspfNeighborState.Exchange:
                        //TODO: duplicate check?
                        if (dbdHeaderData.Item1.IsMaster && LocalMSState == MasterSlaveState.Master)
                        {
                            RaiseEvent(NeighborEventType.SeqNumberMismatch,
                                       new Tuple<IPHeader, OspfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]>>(ipHeader, ospfHeader,
                                                                                                                    new Tuple<OspfDbdHeader, OspfLsaHeader[]>(ospfDBDHeader, null)));
                            return;
                        }
                        else if (!dbdHeaderData.Item1.IsMaster && LocalMSState == MasterSlaveState.Slave)
                        {
                            RaiseEvent(NeighborEventType.SeqNumberMismatch,
                                       new Tuple<IPHeader, OspfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]>>(ipHeader, ospfHeader,
                                                                                                                    new Tuple<OspfDbdHeader, OspfLsaHeader[]>(ospfDBDHeader, null)));
                            return;
                        }
                        else if (dbdHeaderData.Item1.IsInitial)
                        {
                            RaiseEvent(NeighborEventType.SeqNumberMismatch, tupleData);
                            return;
                        }
                        else if (dbdHeaderData.Item1.OspfOptions != DiscoveredOptions)
                        {
                            RaiseEvent(NeighborEventType.SeqNumberMismatch, tupleData);
                            return;
                        }
                        else if ((LocalMSState == MasterSlaveState.Master && dbdHeaderData.Item1.SequenceNumber == DDSequenceNumber) ||
                                 (LocalMSState == MasterSlaveState.Slave && dbdHeaderData.Item1.SequenceNumber == DDSequenceNumber + 1))
                        {
                            LastOspfDBDSeen = dbdHeaderData.Item1;
                            ProcessDBD(tupleData);
                            RaiseEvent(NeighborEventType.ExchangeDone);
                            return;
                        }
                        else
                        {
                            RaiseEvent(NeighborEventType.SeqNumberMismatch, tupleData);
                            return;
                        }
                    case OspfNeighborState.Loading:
                        break;
                    case OspfNeighborState.Full:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ProcessDBD(Tuple<IPHeader, OspfHeader, Tuple<OspfDbdHeader, OspfLsaHeader[]>> tupleData)
        {
            OspfDbdHeader ospfDbdHeader = tupleData.Item3.Item1;
            var lsaCount = Collection.IsNullOrEmpty(ospfDbdHeader.LsaList) ? 0 : ospfDbdHeader.LsaList.Count;
            Log.Write("OSPF", "NEIGHBOR", string.Format("Processing DBD (from {0}). SN: {1}, LSA count: {2}", tupleData.Item2.RouterID, tupleData.Item3.Item1.SequenceNumber, lsaCount));
            //if (ospfDbdHeader.seqnum == _dbdPendingAck.Value.Item2.seqnum)
            //{
            //    _dbdPendingAck = null;
            //}
            if (!Collection.IsNullOrEmpty(ospfDbdHeader.LsaList))
            {
                foreach (OspfLsaHeader item in ospfDbdHeader.LsaList)
                {
                    var lsaType = (int)item.OspfLsaType;
                    //TODO: consider type 7 for NSSA
                    if (lsaType < 1 || lsaType > 5)
                    {
                        RaiseEvent(NeighborEventType.SeqNumberMismatch, tupleData);
                        continue;
                    }
                    else if (lsaType == 5 && !Interface.Area.ExternalRoutingCapability)
                    {
                        RaiseEvent(NeighborEventType.SeqNumberMismatch, tupleData);
                        continue;
                    }
                    else
                    {
                        switch (item.OspfLsaType)
                        {
                            case OspfLsaType.Route:
                                if (Interface.Area.RouterLSAMap.ContainsKey(item.Key))
                                {
                                    LSRequestList.Add(item);
                                }
                                break;
                            case OspfLsaType.Network:
                                if (Interface.Area.NetworkLSAMap.ContainsKey(item.Key))
                                {
                                    LSRequestList.Add(item);
                                }
                                break;
                            case OspfLsaType.SummaryNetwork:
                            case OspfLsaType.SummaryAsbr:
                                if (Interface.Area.SummaryLSAMap.ContainsKey(item.Key))
                                {
                                    LSRequestList.Add(item);
                                }
                                break;
                            //TODO: do externals even do anything here?
                            case OspfLsaType.External:
                                break;
                            //case OspfLsaType.Nssa:
                            //    break;
                        }
                    }
                }
            }
            if (DDSequenceNumber == ospfDbdHeader.SequenceNumber)
            {
                if (LocalMSState == MasterSlaveState.Master)
                {
                    DDSequenceNumber++;
                    if (DBSummaryList.Count == 0 && !ospfDbdHeader.HasMore)
                    {
                        RaiseEvent(NeighborEventType.ExchangeDone);
                    }
                }
                else if (LocalMSState == MasterSlaveState.Slave)
                {
                    DDSequenceNumber = ospfDbdHeader.SequenceNumber;
                    //++ 1 means about to send last
                    if (DBSummaryList.Count == 1 && !ospfDbdHeader.HasMore)
                    {
                        //++ process will continue with last one
                        RaiseEvent(NeighborEventType.ExchangeDone);
                    }
                    else
                    {
                        //++ done; final confirmation
                        //_currentTxDDSequenceNumber = DDSequenceNumber;
                        PrepareSendDBD(tupleData.Item1.SourceAddress, GetDBDOptions(isFirstRun: false, hasMore: false), true);
                    }
                }
            }
        }

        internal void RaiseEvent(NeighborEventType eventType, object data = null)
        {
            lock (Lock)
            {
                Log.Write("OSPF", "NEIGHBOREVENT", string.Format("State: {0}, Event: {1}", OspfNeighborState, eventType));
                _stateMachine.RaiseEvent(OspfNeighborState, eventType, data);
            }
        }
    }
}
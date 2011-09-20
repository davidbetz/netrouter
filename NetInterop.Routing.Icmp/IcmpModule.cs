using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Icmp
{
    [Export(typeof(Module))]
    [ModuleMetadata("Icmp")]
    public class IcmpModule : Module
    {
        [ImportMany]
        private List<Handler> _parserList;

        private readonly Map<Tuple<int, int>, Timer> _sequenceTrackingMap = new Map<Tuple<int, int>, Timer>();

        protected override void Initialize()
        {
        }

        public void RequestReceived(object sender, ModuleEventArgs args)
        {
            //HeaderPackage package = args.HeaderPackage;
            //var imageRequestHeader = package.GetHeader<ImageRequestHeader>();
            //var ipHeader = package.GetHeader<IPHeader>();
            //HandleRequest(imageRequestHeader, ipHeader.saddr, args.DeviceID);
        }

        protected override void AcceptHandoff(string deviceID, string nextHandlerName, PacketData packetData, Value[] parameterArray)
        {
            var headerList = CatalogHeaderListInBranch(nextHandlerName, parameterArray);
            packetData = GeneratePacketData(packetData, headerList.ToArray());

            parameterArray = Controller.OverrideDefaultValueData(parameterArray,
                    Value.Raw("IPHeader:TypeOfService", (byte)0),
                    Value.Raw("IPHeader:TTL", (byte)128),
                    Value.Raw("IPHeader:SourceAddress", Controller.DeviceConfigurationMap[deviceID].PrimaryIPConfiguration.Address),
                    Value.Raw("IPHeader:Protocol", (byte)1));
            //++ send to system module for ip packaging
            var echoHeader = (IcmpEchoHeader)headerList.Single(p => p is IcmpEchoHeader);
            var timer = new Timer
            {
                Interval = 1000
            };
            var id = new Tuple<int, int>(echoHeader.Identifier, echoHeader.SequenceNumber);
            var then = DateTime.Now;
            timer.Elapsed += (s, e) =>
            {
                if ((DateTime.Now - then).Seconds > 4)
                {
                    lock (_sequenceTrackingMap)
                    {
                        timer.Stop();
                        _sequenceTrackingMap.Remove(id);
                        Log.Write("ICMP", "ECHOREQUESTTIMEOUT", echoHeader.Identifier + "," + echoHeader.SequenceNumber);
                    }
                }
            };
            lock (_sequenceTrackingMap)
            {
                _sequenceTrackingMap.Add(id, timer);
                Log.Write("ICMP", "ECHOREQUEST", echoHeader.Identifier + "," + echoHeader.SequenceNumber);
            }
            timer.Start();
            Handoff(deviceID, typeof(IPHeader), packetData, parameterArray);

        }

        public void ProcessEcho(object sender, ModuleEventArgs e)
        {
            var echoHeader = e.HeaderPackage.GetHeader<IcmpEchoHeader>();
            lock (_sequenceTrackingMap)
            {
                var id = new Tuple<int, int>(echoHeader.Identifier, echoHeader.SequenceNumber);
                if (_sequenceTrackingMap.ContainsKey(id))
                {
                    _sequenceTrackingMap[id].Stop();
                    _sequenceTrackingMap.Remove(id);
                    Log.Write("ICMP", "ECHOREPLY", echoHeader.Identifier + "," + echoHeader.SequenceNumber);
                }
            }
        }
    }
}
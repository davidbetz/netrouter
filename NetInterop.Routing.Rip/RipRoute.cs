using System;
using System.Timers;
using NetInterop.Routing.Rip.Configuration;

namespace NetInterop.Routing.Rip
{
    public class RipRoute : Route
    {
        private readonly Timer _checkupTimer = new Timer();
        private int _gcCounter;

        private int _holdDownTimerCounter;
        private int _invalidTimerCounter;

        private RipRoute()
        {
            _checkupTimer.Interval = 1000;
            _checkupTimer.Start();
            RipSection config = RipSection.GetConfigSection();
            //+ holddown
            _checkupTimer.Elapsed += (s, e) =>
                                     {
                                         if (IsLocal)
                                         {
                                             return;
                                         }
                                         _holdDownTimerCounter++;
                                         if (_holdDownTimerCounter == config.Holddown)
                                         {
                                             AcceptPotentialNewMetricOnNextSighting = true;
                                         }
                                     };
            //+ invalid
            _checkupTimer.Elapsed += (s, e) =>
                                     {
                                         if (IsLocal)
                                         {
                                             return;
                                         }
                                         _invalidTimerCounter++;
                                         if (_invalidTimerCounter == config.Invalid)
                                         {
                                             Metric = 16;
                                             MarkedForGC = true;
                                         }
                                     };
            //+ gc
            _checkupTimer.Elapsed += (s, e) =>
                                     {
                                         if (IsLocal)
                                         {
                                             return;
                                         }
                                         _gcCounter++;
                                         if (_gcCounter == config.Flush)
                                         {
                                             GCCollect(this, null);
                                         }
                                     };
        }

        private bool MarkedForGC { get; set; }

        internal string Interface { get; set; }
        internal ushort AddressFamily { get; set; }
        internal ushort RouteTag { get; set; }

        internal Boolean IsLocal { get; set; }
        internal uint PotentialNewMetric { get; set; }

        internal Boolean AcceptPotentialNewMetricOnNextSighting { get; set; }
        internal event ModuleEvent HoldDownHit = (s, e) => Log.Write("RIP", "HoldDownHit", (s as RipRoute).Network.StandardFormat);
        internal event ModuleEvent GCCollect = (s, e) => Log.Write("RIP", "GCCollect", (s as RipRoute).Network.StandardFormat);

        public RipDataHeader GetRipDataHeader()
        {
            return GetRipDataHeader(0);
        }

        public RipDataHeader GetRipDataHeader(uint metricIncrement)
        {
            return new RipDataHeader
                   {
                       AddressFamily = AddressFamily,
                       RouteTag = RouteTag,
                       Network = Network,
                       Mask = Mask,
                       NextHop = NextHop,
                       Metric = Metric + metricIncrement
                   };
        }

        private void ResetInvalidTimer()
        {
            _invalidTimerCounter = 0;
        }

        public static RipRoute Create(IPAddress source, RipDataHeader rip_data_header, string deviceID)
        {
            return Create(source, rip_data_header, deviceID, false);
        }

        public static RipRoute Create(IPAddress source, RipDataHeader rip_data_header, string deviceID, Boolean isLocal)
        {
            return new RipRoute
                   {
                       Via = source,
                       AddressFamily = rip_data_header.AddressFamily,
                       RouteTag = rip_data_header.RouteTag,
                       Network = rip_data_header.Network,
                       Mask = rip_data_header.Mask,
                       NextHop = rip_data_header.NextHop,
                       Metric = rip_data_header.Metric,
                       IsLocal = isLocal,
                       Interface = deviceID,
                       TimeUpdated = DateTime.Now
                   };
        }

        internal void RaiseEvent(string routingEvent)
        {
            if (routingEvent.Equals(RipEvent.Sighted))
            {
                Log.Write("RIP", "Sighted", Network.StandardFormat);
                _invalidTimerCounter = 0;
                _gcCounter = 0;
                MarkedForGC = false;
            }
        }

        internal void BeginHoldDownTime(RipDataHeader potentialReplacement)
        {
            AcceptPotentialNewMetricOnNextSighting = false;
            _holdDownTimerCounter = 0;
        }

        internal void AcceptPotentialNewMetric()
        {
            Metric = PotentialNewMetric;
        }

        internal void UpdateRipHeaderData(IPAddress source, RipDataHeader ripDataHeader)
        {
            Via = source;
            AddressFamily = ripDataHeader.AddressFamily;
            RouteTag = ripDataHeader.RouteTag;
            Network = ripDataHeader.Network;
            Mask = ripDataHeader.Mask;
            NextHop = ripDataHeader.NextHop;
            Metric = ripDataHeader.Metric;
        }
    }
}
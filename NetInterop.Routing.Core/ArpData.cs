using System;
using System.Timers;

namespace NetInterop.Routing.Core
{
    public class ArpData
    {
        private SystemModule _module;
        private Timer _timer;
        private Int32 _ttl;
        private String _deviceID;

        private ArpData(string deviceID, IPAddress ipAddress, MacAddress macAddress, SystemModule module)
        {
            _deviceID = deviceID;
            _ttl = 3000;
            IP = ipAddress;
            Mac = macAddress;
            _module = module;
        }

        public IPAddress IP { get; set; }
        public MacAddress Mac { get; set; }

        public void StartTimer()
        {
            _timer.Start();
        }

        public static ArpData Create(string deviceID, ArpHeader arpHeader, SystemModule module)
        {
            return Create(deviceID, arpHeader.SourceIPAddress, arpHeader.SourceMacAddress, module);
        }
        public static ArpData Create(string deviceID, IPAddress ipAddress, MacAddress macAddress, SystemModule module)
        {
            var arp = new ArpData(deviceID, ipAddress, macAddress, module);
            arp._timer = new Timer
                        {
                            Interval = 1000
                        };
            arp._timer.Elapsed += (s, e) =>
            {
                arp._ttl--;
                if (arp._ttl == 0)
                {
                    Log.Write("ArpData", "Timeout", string.Format("ip={0}, mac={1}", arp.IP.StandardFormat, arp.Mac.StandardFormat));
                    arp._timer.Stop();
                    module.KillArpData(arp);
                }
            };
            return arp;
        }
    }
}
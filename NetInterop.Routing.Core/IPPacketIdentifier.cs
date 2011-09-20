using System.Timers;

namespace NetInterop.Routing.Core
{
    public class IPPacketIdentifier
    {
        private SystemModule _module;
        private Timer _timer;
        private byte _ttl;

        private IPPacketIdentifier(ushort key, IPHeader ipHeader, SystemModule module)
        {
            Key = key;
            _ttl = ipHeader.TTL;
            Daddr = ipHeader.DestinationAddress;
            _module = module;
        }

        public ushort Key { get; private set; }
        public IPAddress Daddr { get; set; }

        public void StartTimer()
        {
            _timer.Start();
        }

        public static IPPacketIdentifier Create(ushort key, IPHeader ipHeader, SystemModule module)
        {
            var id = new IPPacketIdentifier(key, ipHeader, module);
            id._timer = new Timer
                        {
                            Interval = 1000
                        };
            id._timer.Elapsed += (s, e) =>
                                 {
                                     id._ttl--;
                                     if (id._ttl == 0)
                                     {
                                         Log.Write("IPPacketIdentifier", "Timeout", string.Format("addr={0}, id={1}", id.Daddr.StandardFormat, id.Key));
                                         id._timer.Stop();
                                         module.KillIPPacketIdentifier(id);
                                     }
                                 };
            return id;
        }
    }
}
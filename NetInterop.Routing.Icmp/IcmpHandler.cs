using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Nalarium;
using NetInterop.Routing.Core;

namespace NetInterop.Routing.Icmp
{
    [Export(typeof(Handler))]
    [HandlerMetadata("ICMP", "IPV4")]
    public class IcmpHandler : Handler
    {
        public static GlobalProperty IcmpHeaderProperty = GlobalProperty.Register("IcmpHeader", typeof(IcmpHeader),
                                                                                  typeof(IcmpHandler));

        protected override Boolean CheckForNext()
        {
            return ((IPHeader)GetValue(IPv4Handler.IPv4HeaderProperty)).Protocol == 1;
        }

        public override Handler Parse()
        {
            var header = new IcmpHeader();
            header.Type = LoadAndScroll<Byte>();
            header.Code = LoadAndScroll<Byte>();
            header.Crc = LoadUInt16ReversingEndian();

            SetValue(IcmpHeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Nalarium.Value[] parameterArray)
        {
            return CreateHeader<IcmpHeader>(parameterArray);
        }

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var icmpHeader = (IcmpHeader)header;
            var currentData = new List<byte>();
            currentData.AddRange(GetBytes(icmpHeader.Type));
            currentData.AddRange(GetBytes(icmpHeader.Code));
            currentData.AddRange(new byte[] { 0, 0 });
            currentData.AddRange(packetData.Data);
            byte[] crc = Checksum.GetCrc(currentData.ToArray());
            currentData[2] = crc[1];
            currentData[3] = crc[0];
            return packetData.UpdateData(currentData);
        }

        public override void Initialize(Module module)
        {
            //SeriesCompleted += IcmpReceived;
        }

        internal void IcmpReceived(object sender, ModuleEventArgs args)
        {
            //HeaderPackage package = args.HeaderPackage;
            //var icmpHeader = package.GetHeader<IcmpHeader>();
            //var ipHeader = package.GetHeader<IPHeader>();
            //IcmpTypeCode typeCode = IcmpTypeCode.FindByTypeAndCode(icmpHeader.type, icmpHeader.code);
            //if (typeCode == null)
            //{
            //    throw new InvalidOperationException("Invalid ICMP type/code.");
            //}
            //typeCode.Action(ipHeader, icmpHeader);
        }
    }
}
//using System;
//using System.ComponentModel.Composition;

//namespace NetInterop.Routing.Core
//{
//    [Export(typeof(Handler))]
//    [HandlerMetadata("EIGRP", "IPV4")]
//    public class EigrpHandler : Handler
//    {
//        public static GlobalProperty EigrpHeaderProperty = GlobalProperty.Register("EigrpHeader", typeof(EigrpHeader),
//                                                                                   typeof(EigrpHandler));

//        protected override bool CheckForNext()
//        {
//            return ((IPHeader)GetValue(IPv4Handler.IPv4HeaderProperty)).proto == 88;
//        }


//        public override Handler Parse()
//        {
//            var header = new EigrpHeader();
//            header.version = LoadAndScroll<Byte>();
//            header.op = LoadAndScroll<Byte>();
//            header.crc = LoadUInt16ReversingEndian();
//            header.flags = LoadUInt32ReversingEndian();
//            header.seq = LoadUInt32ReversingEndian();
//            header.ack = LoadUInt32ReversingEndian();
//            header.asn = LoadUInt32ReversingEndian();

//            header.param_type = LoadUInt16ReversingEndian();
//            header.param_size = LoadUInt16ReversingEndian();
//            header.param_k1 = LoadAndScroll<Byte>();
//            header.param_k2 = LoadAndScroll<Byte>();
//            header.param_k3 = LoadAndScroll<Byte>();
//            header.param_k4 = LoadAndScroll<Byte>();
//            header.param_k5 = LoadAndScroll<Byte>();
//            header.param_reserved = LoadAndScroll<Byte>();
//            header.param_holdtime = LoadUInt16ReversingEndian();

//            header.softver = LoadUInt16ReversingEndian();
//            header.softsize = LoadUInt16ReversingEndian();
//            header.iosver = LoadUInt16ReversingEndian();
//            header.eigrpver = LoadUInt16ReversingEndian();

//            SetValue(EigrpHeaderProperty, header);

//            return GetNextHandler();
//        }
//    }
//}
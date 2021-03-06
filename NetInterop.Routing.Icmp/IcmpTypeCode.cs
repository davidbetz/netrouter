//using System;
//using System.Collections.Generic;
//using System.Linq;
//using NetInterop.Routing.Core;

//namespace NetInterop.Routing.Icmp
//{
//    [DataTable]
//    public class IcmpTypeCode
//    {
//        private static readonly List<IcmpTypeCode> Instance;

//        static IcmpTypeCode()
//        {
//            Instance = new List<IcmpTypeCode>();
//            AddIcmpTypeCode(0, 0, "Echo Reply", true, false, (i, c) =>
//                                                             {
//                                                                 ushort size = c.body_size;
//                                                                 ushort id = BitConverter.ToUInt16(c.data, 0);
//                                                                 ushort sn = BitConverter.ToUInt16(c.data, 2);
//                                                             });
//            AddIcmpTypeCode(3, 0, "Network Unreachable", false, true);
//            AddIcmpTypeCode(3, 1, "Host Unreachable", false, true);
//            AddIcmpTypeCode(3, 2, "Protocol Unreachable", false, true);
//            AddIcmpTypeCode(3, 3, "Port Unreachable", false, true);
//            AddIcmpTypeCode(3, 4, "Fragmentation Needed But No Frag Bit Set", false, true);
//            AddIcmpTypeCode(3, 5, "Source Routing Failed", false, true);
//            AddIcmpTypeCode(3, 6, "Destination Network Unknown", false, true);
//            AddIcmpTypeCode(3, 7, "Destination Host Unknown", false, true);
//            AddIcmpTypeCode(3, 8, "Source Host Isolated (Obsolete)", false, true);
//            AddIcmpTypeCode(3, 9, "Destination Network Administratively Prohibited", false, true);
//            AddIcmpTypeCode(3, 10, "Destination Host Administratively Prohibited", false, true);
//            AddIcmpTypeCode(3, 11, "Network Unreachable For TOS", false, true);
//            AddIcmpTypeCode(3, 12, "Host Unreachable For TOS", false, true);
//            AddIcmpTypeCode(3, 13, "Communication Administratively Prohibited By Filtering", false, true);
//            AddIcmpTypeCode(3, 14, "Host Precedence Violation", false, true);
//            AddIcmpTypeCode(3, 15, "Precedence Cutoff In Effect", false, true);
//            AddIcmpTypeCode(4, 0, "Source Quench", false, false);
//            AddIcmpTypeCode(5, 0, "Redirect For Network", false, false);
//            AddIcmpTypeCode(5, 1, "Redirect For Host", false, false);
//            AddIcmpTypeCode(5, 2, "Redirect For TOS And Network", false, false);
//            AddIcmpTypeCode(5, 3, "Redirect For TOS And Host", false, false);
//            AddIcmpTypeCode(8, 0, "Echo Request", true, false);
//            AddIcmpTypeCode(9, 0, "Router Advertisement", false, false);
//            AddIcmpTypeCode(10, 0, "Route Solicitation", false, false);
//            AddIcmpTypeCode(11, 0, "TTL Equals0 During Transit", false, true);
//            AddIcmpTypeCode(11, 1, "TTL Equals0 During Reassembly", false, true);
//            AddIcmpTypeCode(12, 0, "IP Header Bad (Catchall Error)", false, true);
//            AddIcmpTypeCode(12, 1, "Required Options Missing", false, true);
//            AddIcmpTypeCode(13, 0, "Timestamp Request (Obsolete)", true, false);
//            AddIcmpTypeCode(14, 0, "Timestamp Reply (Obsolete)", true, false);
//            AddIcmpTypeCode(15, 0, "Information Request (Obsolete)", true, false);
//            AddIcmpTypeCode(16, 0, "Information Reply (Obsolete)", true, false);
//            AddIcmpTypeCode(17, 0, "Address Mask Request", true, false);
//        }

//        public int Type { get; set; }
//        public int Code { get; set; }
//        public String Description { get; set; }
//        public Boolean IsQuery { get; set; }
//        public Boolean IsError { get; set; }
//        public Action<IPHeader, IcmpHeader> Action { get; set; }

//        public static bool IsValid(Int32 type, int code)
//        {
//            return Instance.Any(p => p.Type == type && p.Code == code);
//        }

//        public static IcmpTypeCode FindByTypeAndCode(int type, int code)
//        {
//            return Instance.FirstOrDefault(p => p.Type == type && p.Code == code);
//        }

//        private static void AddIcmpTypeCode(Int32 type, int code, String description, Boolean isQuery, Boolean isError)
//        {
//            AddIcmpTypeCode(type, code, description, isQuery, isError, (i, c) =>
//                                                                       {
//                                                                       });
//        }

//        private static void AddIcmpTypeCode(Int32 type, int code, String description, Boolean isQuery, Boolean isError, Action<IPHeader, IcmpHeader> action)
//        {
//            Instance.Add(new IcmpTypeCode
//                         {
//                             Type = type,
//                             Code = code,
//                             Description = description,
//                             IsQuery = isQuery,
//                             IsError = isError,
//                             Action = action
//                         });
//        }
//    }
//}
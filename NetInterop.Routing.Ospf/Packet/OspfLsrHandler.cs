using System.ComponentModel.Composition;
using Nalarium;

namespace NetInterop.Routing.Ospf.Packet
{
    [Export(typeof(Handler))]
    [HandlerMetadata("OSPFLSR", "OSPF")]
    public class OspfLsrHandler : Handler
    {
        public static GlobalProperty LsrHeaderProperty = GlobalProperty.Register("LsrHeader", typeof(OspfLsrHeader),
                                                                                 typeof(OspfLsrHandler));

        protected override bool CheckForNext()
        {
            return GetValue<OspfHeader>(OspfHandler.OspfHeaderProperty).OspfPacketType == OspfPacketType.LinkStateRequest;
        }


        public override Handler Parse()
        {
            var header = LoadHeader<OspfLsrHeader>(OspfLsrHeader.Member.Type, 4,
                                                   OspfLsrHeader.Member.LSID, typeof(IPAddress),
                                                   OspfLsrHeader.Member.AdvertisingRouter, typeof(IPAddress));

            SetValue(LsrHeaderProperty, header);

            return GetNextHandler();
        }

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            return CreateHeader<OspfLsrHeader>(parameterArray);
        }
    }
}
using System;
using System.Linq;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [DoNotIncludeInDataTree]
    [HandlerMetadata("L2SELECTION", "ROOT")]
    public class L2SelectionHandler : Handler
    {
        public static GlobalProperty EthernetHeaderProperty = GlobalProperty.Register("EthernetHeader",
                                                                                      typeof(EthernetHeader),
                                                                                      typeof(L2SelectionHandler));

        //public static GlobalProperty LlcHeaderProperty = GlobalProperty.Register("LlcHeader", typeof(llc),
        //                                                                         typeof(L2SelectionHandler));

        public static GlobalProperty Ieee8023TypeProperty = GlobalProperty.Register("Ieee8023Type", typeof(string),
                                                                                    typeof(L2SelectionHandler),
                                                                                    new GlobalPropertyMetadata(
                                                                                        GlobalPropertyMetadataOptions.
                                                                                            InternalUseOnly));

        protected override Boolean CheckForNext()
        {
            return true;
        }

        internal static bool IsStp(Llc llch)
        {
            return llch.dsap == 66 && llch.ssap == 66;
        }

        internal static bool IsCisco(EthernetHeader eh)
        {
            return MacAddress.IsSameMacAddress(eh.Destination, new byte[]
                                                        {
                                                            1, 0, 12, 204, 204, 204
                                                        });
        }

        public override Handler Parse()
        {
            var eh = LoadAndScroll<EthernetHeader>();
            SetValue(EthernetHeaderProperty, eh);
            if (Controller.DeviceConfigurationMap.Any(p => p.Value.MacAddress.Equals(eh.Source)))
            {
                //++ don't process packets from us -- multicast would come back
                return CancelProcessing("Source address is this interface.");
            }
            return GetNextHandler();
        }
    }
}
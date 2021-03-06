using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("IEEE8023", "L2SELECTION")]
    public class IEEE8023Handler : Handler
    {
        public static GlobalProperty EthernetHeaderProperty = GlobalProperty.Register("EthernetHeader",
                                                                                      typeof(EthernetHeader),
                                                                                      typeof(IEEE8023Handler));

        public static GlobalProperty LlcHeaderProperty = GlobalProperty.Register("LlcHeader", typeof(Llc),
                                                                                 typeof(IEEE8023Handler));

        protected override Boolean CheckForNext()
        {
            return GetValue<EthernetHeader>(L2SelectionHandler.EthernetHeaderProperty).TypeOrLengthInt32 <= 1500;
        }

        public override Handler Parse()
        {
            MoveHeader(L2SelectionHandler.EthernetHeaderProperty, EthernetHeaderProperty);

            var llc = LoadAndScroll<Llc>();
            SetValue(LlcHeaderProperty, llc);

            return GetNextHandler();
        }
    }
}
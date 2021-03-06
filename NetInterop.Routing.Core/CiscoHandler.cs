using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Core
{
    [Export(typeof(Handler))]
    [HandlerMetadata("CISCO", "IEEE8023")]
    public class CiscoHandler : Handler
    {
        protected override Boolean CheckForNext()
        {
            var ethernetHeader = GetValue<EthernetHeader>(L2SelectionHandler.EthernetHeaderProperty);
            return L2SelectionHandler.IsCisco(ethernetHeader);
        }

        public override Handler Parse()
        {
            return GetNextHandler();
        }
    }
}
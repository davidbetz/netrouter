using NetInterop.Routing.Tcp.Configuration;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Tcp
{
    [Export(typeof(Module))]
    [ModuleMetadata("TCP")]
    public class TcpModule : Module
    {
        [ImportMany]
        private List<Handler> _parserList;

        protected override void SetupInterfaceData()
        {
            TcpSection config = TcpSection.GetConfigSection();
        }

        protected override void Initialize()
        {
            TcpSection config = TcpSection.GetConfigSection();
        }

    }
}
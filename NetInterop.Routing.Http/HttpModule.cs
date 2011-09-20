using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Nalarium;
using NetInterop.Routing.Core;
using NetInterop.Routing.Http.Configuration;

namespace NetInterop.Routing.Http
{
    [Export(typeof(Module))]
    [ModuleMetadata("TCP")]
    public class HttpModule : Module
    {
        [ImportMany]
        private List<Handler> _parserList;

        protected override void SetupInterfaceData()
        {
            HttpSection config = HttpSection.GetConfigSection();
        }

        protected override void Initialize()
        {
            HttpSection config = HttpSection.GetConfigSection();
        }

    }
}
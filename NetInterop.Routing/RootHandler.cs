using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NetInterop.Routing
{
    [Export(typeof(Handler))]
    [HandlerMetadata("ROOT", "NONE")]
    public class RootHandler : Handler
    {
        public static GlobalProperty SharedParentProperty = GlobalProperty.Register("SharedParent", typeof(bool?), typeof(RootHandler));

        public List<Handler> HandlerList { get; set; }

        protected internal override bool CheckForNext()
        {
            throw new InvalidOperationException("This should never be seen.");
        }

        public override Handler Parse()
        {
            return GetNextHandler();
        }
    }
}
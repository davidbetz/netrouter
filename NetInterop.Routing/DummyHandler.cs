using System;
using System.ComponentModel.Composition;

namespace NetInterop.Routing
{
    [Export(typeof(Handler))]
    [HandlerMetadata("DUMMY", "ROOT")]
    public class DummyHandler : Handler
    {
        protected internal override bool CheckForNext()
        {
            throw new InvalidOperationException("This should never be seen.");
        }

        public override Handler Parse()
        {
            return null;
        }
    }
}
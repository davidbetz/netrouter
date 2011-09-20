using System.ComponentModel.Composition;

namespace NetInterop.Routing
{
    [Export(typeof(Module))]
    [ModuleMetadata("INTERNAL")]
    public class InternalModule : Module
    {
        public override void Install()
        {
        }
    }
}
using System.ComponentModel.Composition;

namespace NetInterop.Routing.Mpls
{
    [Export(typeof(Module))]
    [ModuleMetadata("MPLS")]
    public class MplsModule : Module
    {
        public override void Install()
        {
        }
    }
}
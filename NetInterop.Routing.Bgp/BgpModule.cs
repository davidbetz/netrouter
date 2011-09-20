using System.ComponentModel.Composition;

namespace NetInterop.Routing.Bgp
{
    [Export(typeof(Module))]
    [ModuleMetadata("BGP")]
    public class BgpModule : Module
    {
        public override void Install()
        {
            //RegisterCallback<BgpHandler>((m, e) =>
            //{
            //    var ipHeader = e.HeaderPackage.GetHeader<IPHeader>();
            //    var bgpHeader = e.HeaderPackage.GetHeader<bgp_header>();
            //    Console.WriteLine("BGP call found from {0}.", ipHeader.saddr.StandardFormat);
            //});
        }
    }
}
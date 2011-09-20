namespace NetInterop.Routing.Ospf
{
    public interface IOspfLsaHeader
    {
        OspfLsaHeader CommonHeader { get; set; }
    }
}
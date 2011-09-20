namespace NetInterop.Routing.Ospf
{
    //public enum OspfLsaLinkType : byte
    //{
    //    PointToPointNumbered = 1,
    //    PointToPointUnnumbered = 2,
    //    Transit = 3,
    //    Stub = 4,
    //    VirtualLink = 5
    //}
    public enum OspfLsaLinkType : byte
    {
        PointToPoint = 1,
        Transit = 2,
        Stub = 3,
        VirtualLink = 4
    }
}
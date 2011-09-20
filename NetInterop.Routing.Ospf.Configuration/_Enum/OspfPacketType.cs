namespace NetInterop.Routing.Ospf
{
    public enum OspfPacketType : byte
    {
        Hello = 1,
        DatabaseDescriptor = 2,
        LinkStateRequest = 3,
        LinkStateUpdate = 4,
        LinkStateAcknowledgement = 5
    }
}
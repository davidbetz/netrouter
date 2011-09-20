namespace NetInterop.Routing.Ospf
{
    public enum NeighborEventType
    {
        HelloReceived,
        Start,
        TwoWayReceived,
        NegotiationDone,
        ExchangeDone,
        BadLSReq,
        LoadingDone,
        AdjOK,
        SeqNumberMismatch,
        OneWayReceived,
        KillNbr,
        InactivityTimer,
        LLDown
    }
}
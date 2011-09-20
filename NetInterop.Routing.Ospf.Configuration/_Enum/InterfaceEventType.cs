namespace NetInterop.Routing.Ospf
{
    public enum InterfaceEventType
    {
        /// Lower-level protocols have indicated that the network
        /// interface is operational.  This enables the interface to
        /// transition out of Down state.  On virtual links, the
        /// interface operational indication is actually a result of the
        /// shortest path calculation (see Section 16.7).
        InterfaceUp,

        /// The Wait Timer has fired, indicating the end of the waiting
        /// period that is required before electing a (Backup)
        /// Designated Router.
        WaitTimer,

        /// The router has detected the existence or non-existence of a
        /// Backup Designated Router for the network.  This is done in
        /// one of two ways.  First, an Hello Packet may be received
        /// from a neighbor claiming to be itself the Backup Designated
        /// Router.  Alternatively, an Hello Packet may be received from
        /// a neighbor claiming to be itself the Designated Router, and
        /// indicating that there is no Backup Designated Router.  In
        /// either case there must be bidirectional communication with
        /// the neighbor, i.e., the router must also appear in the
        /// neighbor's Hello Packet.  This event signals an end to the
        /// Waiting state.
        BackupSeen,

        /// There has been a change in the set of bidirectional
        /// neighbors associated with the interface.  The (Backup)
        /// Designated Router needs to be recalculated.  The following
        /// neighbor changes lead to the NeighborChange event.  For an
        /// explanation of neighbor states, see Section 10.1.
        /// oBidirectional communication has been established to a
        /// neighbor.  In other words, the state of the neighbor has
        /// transitioned to 2-Way or higher.
        /// oThere is no longer bidirectional communication with a
        /// neighbor.  In other words, the state of the neighbor has
        /// transitioned to Init or lower.
        /// oOne of the bidirectional neighbors is newly declaring
        /// itself as either Designated Router or Backup Designated
        /// Router.  This is detected through examination of that
        /// neighbor's Hello Packets.
        /// oOne of the bidirectional neighbors is no longer
        /// declaring itself as Designated Router, or is no longer
        /// declaring itself as Backup Designated Router.  This is
        /// again detected through examination of that neighbor's
        /// Hello Packets.
        /// oThe advertised Router Priority for a bidirectional
        /// neighbor has changed.  This is again detected through
        /// examination of that neighbor's Hello Packets.
        NeighborChange,

        /// An indication has been received that the interface is now
        /// looped back to itself.  This indication can be received
        /// either from network management or from the lower level
        /// protocols.
        LoopInd,

        //An indication has been received that the interface is no
        //longer looped back.  As with the LoopInd event, this
        //indication can be received either from network management or
        //from the lower level protocols.
        UnloopInd,

        //Lower-level protocols indicate that this interface is no
        //longer functional.  No matter what the current interface
        //state is, the new interface state will be Down.
        InterfaceDown
    }
}
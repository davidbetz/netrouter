namespace NetInterop.Routing.Ospf
{
    public enum InterfaceState
    {
        /// <summary>
        /// This is the initial interface state.  In this state, the
        /// lower-level protocols have indicated that the interface is
        /// unusable.  No protocol traffic at all will be sent or
        /// received on such a interface.  In this state, interface
        /// parameters should be set to their initial values.  All
        /// interface timers should be disabled, and there should be no
        /// adjacencies associated with the interface.
        /// </summary>
        Down = 1,

        /// <summary>
        /// In this state, the router's interface to the network is
        /// looped back.  The interface may be looped back in hardware
        /// or software.  The interface will be unavailable for regular
        /// data traffic.  However, it may still be desirable to gain
        /// information on the quality of this interface, either through
        /// sending ICMP pings to the interface or through something
        /// like a bit error test.  For this reason, IP packets may
        /// still be addressed to an interface in Loopback state.  To
        /// facilitate this, such interfaces are advertised in router-
        /// LSAs as single host routes, whose destination is the IP
        /// interface address.
        /// </summary>
        Loopback,

        /// In this state, the router is trying to determine the
        /// identity of the (Backup) Designated Router for the network.
        /// To do this, the router monitors the Hello Packets it
        /// receives.  The router is not allowed to elect a Backup
        /// Designated Router nor a Designated Router until it
        /// transitions out of Waiting state.  This prevents unnecessary
        /// changes of (Backup) Designated Router.
        Waiting,

        /// In this state, the interface is operational, and connects
        /// either to a physical point-to-point network or to a virtual
        /// link.  Upon entering this state, the router attempts to form
        /// an adjacency with the neighboring router.  Hello Packets are
        /// sent to the neighbor every HelloInterval seconds.
        PointToPoint,

        /// The interface is to a broadcast or NBMA network on which
        /// another router has been selected to be the Designated
        /// Router.  In this state, the router itself has not been
        /// selected Backup Designated Router either.  The router forms
        /// adjacencies to both the Designated Router and the Backup
        /// Designated Router (if they exist).
        DROther,

        /// In this state, the router itself is the Backup Designated
        /// Router on the attached network.  It will be promoted to
        /// Designated Router when the present Designated Router fails.
        /// The router establishes adjacencies to all other routers
        /// attached to the network.  The Backup Designated Router
        /// performs slightly different functions during the Flooding
        /// Procedure, as compared to the Designated Router (see Section
        /// 13.3).  See Section 7.4 for more details on the functions
        /// performed by the Backup Designated Router.
        Backup,

        /// In this state, this router itself is the Designated Router
        /// on the attached network.  Adjacencies are established to all
        /// other routers attached to the network.  The router must also
        /// originate a network-LSA for the network node.  The network-
        /// LSA will contain links to all routers (including the
        /// Designated Router itself) attached to the network.  See
        /// Section 7.3 for more details on the functions performed by
        /// the Designated Router.
        DR
    }
}
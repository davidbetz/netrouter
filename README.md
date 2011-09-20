# Control Plane Router in .NET (RIP/OSPF [mostly])

Built it while studying for [CCNP](http://www.cisco.com/web/learning/certifications/professional/ccnp/index.html). It was my obsession in Fall 2010.

It uses Winpcap to directly parse Ethernet packets. Mostly **C#**, but interop is all done in **C++** (the Bridge project). It's not just a sniffer/parser, though; it does talk back and works with a few protocols.

RIP, OSPF, and other functionality (e.g. ARP cache) are built in. TCP never interested me, but I stubbed out it and HTTP.  BGP and MPLS are also stubbed out and partially implemented (it would be fun to add BGP; since it's **NOT** a routing protocol, but a layer-7 routing application that runs on your router; however, there are a lot of prerequisites). MPLS was the next priority for me, but my priorities / interests in life changed. I don't even remember the state of IPV6. From the looks of it, it seems that I also started implemented STP (spanning-tree).

MEF was used to dynamically pickup modules to parse the level layer.

Everything is modular.

## Running

    RoutingController controller = RoutingController.Create();
    var monitor = new Thread(new ThreadStart(() =>
    {
        System.Console.CancelKeyPress += (s, e) =>
        {
            controller.Disable();
            e.Cancel = true;
        };
        RoutingController.VerboseOutput = ReportStatus;
        controller.Parsed += controller_Parsed;
        controller.Enable("not tcp port 3389");
    }));
    monitor.Start();

This setups up the initial logging (sniffing); RDP traffic is excluded from output

## Sample Config (general)
	
This loads the RIP, OSPF, Image (udp), and ICMP modules. The last two demonstrate more of the custom IP stack I wrote more than the control plane router.

It's bound to certain interfaces and... a bunch of other config (some might not be implemented).

<pre>
&lt;system&gt;
  &lt;routers&gt;
    &lt;add name="rip" /&gt;
    &lt;add name="ospf" /&gt;
    &lt;add name="image" /&gt;
    &lt;add name="icmp" /&gt;
  &lt;/routers&gt;
  &lt;interfaces&gt;
    &lt;!--h--&gt;
    &lt;add id="F063AD2D-214E-4CE2-AA42-3CFE31456FB4" disabled="false" /&gt;
    &lt;!--w-w--&gt;
    &lt;add id="12A8CADC-20F3-4586-BDE5-F5EF859C9991" disabled="false" ipMtu="1500" /&gt;
  &lt;/interfaces&gt;
  &lt;arpCache gratuitousOnStartup="false"&gt;
    &lt;add address="10.1.1.254" /&gt;
    &lt;add address="208.16.17.28" /&gt;
    &lt;add address="10.1.155.1" /&gt;
    &lt;add address="10.1.144.22" /&gt;
  &lt;/arpCache&gt;
  &lt;addresses&gt;
    &lt;add index="4" name="Realtek PCIe GBE Family Controller" id="F063AD2D-214E-4CE2-AA42-3CFE31456FB4" ip="10.1.1.1"
         mask="255.255.255.0" gateway="10.1.1.254" mac="48:5B:39:CA:B0:27"&gt;
      &lt;secondaries&gt;
        &lt;add address="10.1.1.1" mask="255.255.255.0" /&gt;
      &lt;/secondaries&gt;
      &lt;ipv6Addresses&gt;
        &lt;add address="38E4::1" mask="48" /&gt;
      &lt;/ipv6Addresses&gt;
    &lt;/add&gt;
  &lt;/addresses&gt;
  &lt;loopbacks&gt;
    &lt;add id="0" ip="10.91.1.1" mask="255.255.0.0"&gt;
      &lt;secondaries&gt;
        &lt;add address="10.191.1.1" mask="255.255.255.0" /&gt;
      &lt;/secondaries&gt;
      &lt;ipv6Addresses&gt;
        &lt;add address="38E4::1" mask="48" /&gt;
      &lt;/ipv6Addresses&gt;
    &lt;/add&gt;
    &lt;add id="1" ip="10.92.1.1" mask="255.255.0.0" /&gt;
    &lt;add id="2" ip="172.16.1.1" mask="255.255.255.0" v6ip="3EF0:1:" v6mask="48" /&gt;
    &lt;add id="3" ip="172.16.2.1" mask="255.255.255.0" v6ip="3EF0:2:" v6mask="48" /&gt;
    &lt;add id="4" ip="192.168.5.11" mask="255.255.255.0" /&gt;
  &lt;/loopbacks&gt;
  &lt;debug&gt;
    &lt;add category="ICMP" /&gt;
    &lt;add category="IMAGE" /&gt;
    &lt;add category="RIP" /&gt;
    &lt;add category="OSPF" /&gt;
    &lt;add category="SYSTEM" /&gt;
  &lt;/debug&gt;
&lt;/system&gt;
</pre>

## Sample Config (OSPF)

I studied the [OSPF RFC](https://www.ietf.org/rfc/rfc2328.txt) for a long time to get it to spec. In the end, I think I left it with a bug related to DR voting. However, it will detect your routers and do negotiation..

<pre>
&lt;ospf&gt;
  &lt;areas&gt;
    &lt;setup area="1" /&gt;
    &lt;setup area="2" /&gt;
  &lt;/areas&gt;
  &lt;interfaces passiveDefault="false"&gt;
    &lt;settings id="F063AD2D-214E-4CE2-AA42-3CFE31456FB4" type="Broadcast" hello="10" dead="40" priority="1" cost="100"
              delay="1" rxmt="5" authType="Null" passive="false" /&gt;
  &lt;/interfaces&gt;
  &lt;networks&gt;
    &lt;add area="0" network="10.1.1.0" mask="0.0.0.255" /&gt;
    &lt;add area="2" network="10.1.0.0" mask="0.0.255.255" /&gt;
    &lt;add area="1" network="10.1.1.0" mask="0.255.255.255" /&gt;
    
    &lt;add area="0" network="172.16.0.0" mask="0.0.255.255" /&gt;
  &lt;/networks&gt;
&lt;/ospf&gt;
</pre>

## Sample Config (RIP)

RIP is boring. Not a whole lot to see here.

<pre>
&lt;rip version="2" update="30" holddown="15" invalid="180" flush="60" autoSummary="false"&gt;
  &lt;networks&gt;
    &lt;add network="10.91.0.0" /&gt;
    &lt;add network="192.168.5.0" /&gt;
  &lt;/networks&gt;
&lt;/rip&gt;
</pre>

## Architecture

*(going from memory and quick reminders in code)*

Everything is module. If you want a new protocol, you write a new module (or set of modules). 

Whenever the system detects data, the system parses the data to decide if it's an Ethernet II or IEEE802.2 frame (layer 2 units; layer 1 units are electrical signals) and deal with ARP caching.

The system them asks a dynamically loaded list of children modules (children can have more than one parent; e.g. UDP can be used by both IPv4 and IPv6): "Who can respond to the next part of this header?" Could be OSPF... Could be UPD... Could be ARP.  That handler loads and parses it's part of the packet. The process just repeats: "which of of you children can handle this?"

You add functionality in modules. You have handlers in modules. Handlers will parse and build packets.

For example, here's RipHandler::Parse:

        public override Handler Parse()
        {
            var udpHeader = GetValue<UdpHeader>(UdpHandler.UdpHeaderProperty);

            var ripPreambleHeader = LoadHeader<RipPreambleHeader>("command", 1, "version", 1, "domain", 2);
            int bodySize = udpHeader.Len - UdpHandler.UdpHeaderProperty.Size - RipPreambleHeaderProperty.Size;
            int dataSectionCount = bodySize / RipDataHeaderProperty.Size;
            var list = new List<RipDataHeader>();
            for (int i = 0; i < dataSectionCount; i++)
            {
                list.Add(LoadHeader<RipDataHeader>("AddressFamily", 2,
                                                     "RouteTag", 2,
                                                     "Network", typeof(IPAddress),
                                                     "Mask", typeof(IPAddress),
                                                     "NextHop", typeof(IPAddress),
                                                     "Metric", 4));
            }

            SetValue(RipPreambleHeaderProperty, ripPreambleHeader);
            SetValue(RipDataHeaderProperty, list.ToArray());

            return GetNextHandler();
        }

Here's RipHandler::GetBytes:

        public override PacketData GetBytes(IHeader header, PacketData packetData)
        {
            var ripPreambleHeader = (RipPreambleHeader)header;
            var currentData = new List<byte>();
            currentData.Add(ripPreambleHeader.command);
            currentData.Add(ripPreambleHeader.Version);
            currentData.AddRange(GetBytes(ripPreambleHeader.Domain));
            foreach (var ripDataHeader in ripPreambleHeader.DataArray)
            {
                currentData.AddRange(GetBytes(ripDataHeader.AddressFamily));
                currentData.AddRange(GetBytes(ripDataHeader.RouteTag));
                currentData.AddRange(ripDataHeader.Network.GetBytes());
                currentData.AddRange(ripDataHeader.Mask.GetBytes());
                currentData.AddRange(ripDataHeader.NextHop.GetBytes());
                currentData.AddRange(GetBytes(ripDataHeader.Metric));
            }
            return packetData.UpdateData(currentData);
        }

Here's RipHandler::Build:

        protected override IHeader Build(Module module, params Value[] parameterArray)
        {
            var ripPreambleHeader = CreateHeader<RipPreambleHeader>(parameterArray);
            if (ripPreambleHeader.command == (byte)RipCommand.Response)
            {
                ripPreambleHeader.DataArray = (module as RipModule).RipDatabase.Select(p => p.Value.GetRipDataHeader()).ToArray();
            }
            else
            {
                ripPreambleHeader.DataArray = new[]
                                                 {
                                                     CreateHeader<RipDataHeader>(parameterArray)
                                                 };
            }
            return ripPreambleHeader;
        }

Module bind the handlers to the interfaces and deal with general algorithms. They provide the verbs where handlers provide the nouns. For example, RIP has a timer that floods packets. The module will handle the timing and the handler will handle the sending and receiving for the data.

## Complex Examples

While RIP is a complete joke, other protocols have some extensive mechanics.

OSPF, for example, virtualizes the entire internetworking structure into areas, each with different types and each with their own rules (e.g. all area must connect area 0). OSPF has to maintain multiple [state machines](http://www.cisco.com/c/en/us/support/docs/ip/open-shortest-path-first-ospf/13685-13.html) (one per area) to handle the various complex algorithm. This type of stuff is in a module with interface, area, and neighbor mechanics split out to their respective classes.

There are a lot of different packet types in OSPF and a lot of mechanics. Here's a sample rundown of types (Area, Interface, and Neighbor are part of the **Module** world):

	Area.cs
	Interface.cs
	Constant.cs
	Neighbor.cs
	OspfHandler.cs
	OspfModule.cs

	_Structure\OspfLsaExternalHeader.cs
	_Structure\OspfLlsDataBlockTlv.cs
	_Structure\OspfLsrHeader.cs
	_Structure\OspfLsaRouterHeader.cs
	_Structure\OspfLsaSummaryHeader.cs
	_Structure\OspfHeader.cs
	_Structure\OspfDbdHeader.cs
	_Structure\RouterLSAOptions.cs
	_Structure\OspfLsaRouterLinkHeader.cs
	_Structure\OspfLsuHeader.cs
	_Structure\OspfLlsDataBlockHeader.cs
	_Structure\IOspfLsaHeader.cs
	_Structure\OspfLsaNetworkHeader.cs
	_Structure\OspfLsaHeader.cs
	_Structure\OspfHelloHeader.cs

	Packet\OspfLsackHandler.cs
	Packet\OspfLlsDataBlockTlvHandler.cs
	Packet\OspfHelloHandler.cs
	Packet\OspfLlsDataBlockHandler.cs
	Packet\OspfLsrHandler.cs
	Packet\OspfDbdHandler.cs
	Packet\OspfLlsDataBlockTlvInterpreter.cs
	Packet\OspfLsuHandler.cs

	Lsa\OspfLsaCommonHandler.cs
	Lsa\OspfLsaRouteLinkHandler.cs
	Lsa\OspfLsaRouteHandler.cs
	Lsa\OfpsLsaExternalHandler.cs
	Lsa\OspfLsaNetworkHandler.cs
	Lsa\OspfLsaSummaryHandler.cs

Here's a sample of how declarative states in the OSPF state machine:
	
	_stateMachine = new StateMachine<OspfNeighborState, NeighborEventType>();
	////++ NBMA
	_stateMachine.Add(OspfNeighborState.Down, NeighborEventType.HelloReceived, HelloReceived, nextState: OspfNeighborState.Init);
	_stateMachine.Add(OspfNeighborState.Init, NeighborEventType.HelloReceived, HelloReceived, StateEntryMode.AtLeast, nextStateMode: NextStateMode.NoAction);
	_stateMachine.Add(OspfNeighborState.Init, NeighborEventType.TwoWayReceived, TwoWayReceived);
	_stateMachine.Add(OspfNeighborState.ExStart, NeighborEventType.NegotiationDone, NegotiationDone);
	_stateMachine.Add(OspfNeighborState.Exchange, NeighborEventType.ExchangeDone, ExchangeDone);
	_stateMachine.Add(OspfNeighborState.TwoWay, NeighborEventType.AdjOK, AdjOK);
	_stateMachine.Add(OspfNeighborState.ExStart, NeighborEventType.AdjOK, AdjOK, StateEntryMode.AtLeast);
	_stateMachine.Add(OspfNeighborState.Exchange, NeighborEventType.SeqNumberMismatch, SeqNumberMismatch, StateEntryMode.AtLeast);

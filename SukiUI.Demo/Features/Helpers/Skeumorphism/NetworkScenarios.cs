using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism;

/// <summary>A button of the network card: a protocol that can be played on the <see cref="NetworkMap"/>.</summary>
public sealed partial class NetworkProtocol : ObservableObject
{
    public NetworkProtocol(string name, string description, Func<NetworkScript> createScript, Action<NetworkProtocol> run)
    {
        Name = name;
        Description = description;
        CreateScript = createScript;
        RunCommand = new RelayCommand(() => run(this));
    }

    public string Name { get; }
    public string Description { get; }
    public Func<NetworkScript> CreateScript { get; }
    public ICommand RunCommand { get; }

    /// <summary>True while this protocol is playing: its button then stops it.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Label))] private bool _isActive;

    /// <summary>True while another protocol is playing: nothing new is accepted during an animation.</summary>
    [ObservableProperty] private bool _isBlocked;

    public string Label => IsActive ? "Stop" : Name;
}

/// <summary>
/// What each local-network protocol does, written as a list of steps. The <see cref="NetworkMap"/> only plays them:
/// the knowledge about the protocols lives here.
/// </summary>
internal static class NetworkScenarios
{
    private const NetNode A = NetNode.ComputerA, B = NetNode.ComputerB, C = NetNode.ComputerC;
    private const NetNode Router = NetNode.Router;

    public static IReadOnlyList<NetworkProtocol> Create(Action<NetworkProtocol> run) => new[]
    {
        new NetworkProtocol("ARP", "Address Resolution Protocol: find the MAC address that goes with an IP address", Arp, run),
        new NetworkProtocol("DHCP", "Dynamic Host Configuration Protocol: get an IP address when joining the network", Dhcp, run),
        new NetworkProtocol("ICMP", "Internet Control Message Protocol: the ping, is this machine reachable?", Icmp, run),
    };

    private static NetworkScript Arp() => new("ARP", new[]
    {
        new NetStep("PC A wants to talk to 192.168.1.12 but only knows its IP address, not its MAC address.",
            new NetPulse(A)),
        new NetStep("ARP request, sent as a broadcast to every device: «who has 192.168.1.12?»",
            new NetBroadcast(A, "WHO HAS .12?")),
        new NetStep("Everybody received it, but only PC B owns this address: the others ignore it.",
            new NetPulse(B)),
        new NetStep("PC B answers PC A directly (unicast) with its MAC address.",
            new NetSend(B, A, "I'M .12", PacketKind.Reply)),
        new NetStep("PC A keeps the IP ↔ MAC pair in its ARP cache: no need to ask again for a while.",
            new NetPulse(A)),
    });

    private static NetworkScript Dhcp() => new("DHCP", new[]
    {
        new NetStep("PC C starts: it has no IP address yet, so it cannot talk to anybody in particular.",
            new NetPower(C, true)),
        new NetStep("DISCOVER: PC C shouts «is there a DHCP server?» to the whole network.",
            new NetBroadcast(C, "DISCOVER")),
        new NetStep("The router, which is also the DHCP server, picks a free address in its range.",
            new NetPulse(Router)),
        new NetStep("OFFER: the router proposes 192.168.1.14 to PC C.",
            new NetSend(Router, C, "OFFER", PacketKind.Reply)),
        new NetStep("REQUEST: PC C accepts the offer. It is still a broadcast, so any other server knows it was declined.",
            new NetBroadcast(C, "REQUEST")),
        new NetStep("ACK: the router confirms the lease of 192.168.1.14.",
            new NetSend(Router, C, "ACK", PacketKind.Reply)),
        new NetStep("PC C is configured: IP address, subnet mask, gateway and DNS server.",
            new NetPulse(C)),
    }, new[] { C });

    private static NetworkScript Icmp() => new("ICMP", new[]
    {
        new NetStep("ping 192.168.1.12: PC A sends an ICMP echo request to PC B.",
            new NetSend(A, B, "ECHO")),
        new NetStep("PC B receives it and answers right away.",
            new NetPulse(B)),
        new NetStep("ICMP echo reply goes back to PC A.",
            new NetSend(B, A, "REPLY", PacketKind.Reply)),
        new NetStep("The round trip worked: PC B is reachable, and the time it took is the latency.",
            new NetPulse(A)),
    });
}

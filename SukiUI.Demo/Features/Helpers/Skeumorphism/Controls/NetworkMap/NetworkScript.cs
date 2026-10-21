using System.Collections.Generic;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>Devices of the <see cref="NetworkMap"/>. Every device is plugged into the switch.</summary>
public enum NetNode
{
    Router,
    Switch,
    ComputerA,
    ComputerB,
    ComputerC
}

/// <summary>How a packet looks: a request is a filled capsule, a reply is a hollow glass one.</summary>
public enum PacketKind
{
    Request,
    Reply
}

/// <summary>One thing that happens on the network. Actions of a step run one after the other.</summary>
public abstract record NetAction;

/// <summary>A packet travels from a device to another one, through the switch.</summary>
public sealed record NetSend(NetNode From, NetNode To, string? Label = null, PacketKind Kind = PacketKind.Request) : NetAction;

/// <summary>A packet goes from a device to the switch, which copies it to every other port.</summary>
public sealed record NetBroadcast(NetNode From, string? Label = null, PacketKind Kind = PacketKind.Request) : NetAction;

/// <summary>The switch copies a packet it already holds to every port but one.</summary>
public sealed record NetFlood(NetNode Except, string? Label = null, PacketKind Kind = PacketKind.Request) : NetAction;

/// <summary>A device is busy processing something: its halo lights up for a moment.</summary>
public sealed record NetPulse(NetNode Node) : NetAction;

/// <summary>A computer wakes up (screen on) or goes to sleep (screen off).</summary>
public sealed record NetPower(NetNode Node, bool On) : NetAction;

/// <summary>A group of actions with a sentence that explains what is going on.</summary>
public sealed record NetStep(string Caption, params NetAction[] Actions);

/// <summary>A scenario played by the <see cref="NetworkMap"/>: its steps, and the computers that start asleep.</summary>
public sealed record NetworkScript(string Name, IReadOnlyList<NetStep> Steps, IReadOnlyList<NetNode>? Asleep = null);

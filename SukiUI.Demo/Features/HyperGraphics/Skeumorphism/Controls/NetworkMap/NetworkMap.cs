using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

/// <summary>
/// Topology diagram animating packet journeys, broadcasts, port LEDs,
/// device halos, and screen wakeups driven by a <see cref="NetworkScript"/>.
/// </summary>
public partial class NetworkMap : HyperControl<NetworkMapState>
{
    public static readonly StyledProperty<NetworkScript?> ScriptProperty =
        AvaloniaProperty.Register<NetworkMap, NetworkScript?>(nameof(Script));

    public static readonly DirectProperty<NetworkMap, int> StepIndexProperty =
        AvaloniaProperty.RegisterDirect<NetworkMap, int>(
            nameof(StepIndex),
            o => o.StepIndex,
            defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly DirectProperty<NetworkMap, bool> IsPlayingProperty =
        AvaloniaProperty.RegisterDirect<NetworkMap, bool>(
            nameof(IsPlaying),
            o => o.IsPlaying,
            defaultBindingMode: BindingMode.OneWayToSource);

    private readonly NetCommandChannel _channel = new();
    private int _stepIndex;
    private bool _isPlaying;
    private CancellationTokenSource? _runCts;

    static NetworkMap()
    {
        ScriptProperty.Changed.AddClassHandler<NetworkMap>((m, e) => m.OnScriptChanged(e.NewValue as NetworkScript));
    }

    public NetworkMap()
    {
        Width = 288;
        Height = 222;
        IsHitTestVisible = false;
    }

    /// <summary>Script currently driving the network diagram.</summary>
    public NetworkScript? Script
    {
        get => GetValue(ScriptProperty);
        set => SetValue(ScriptProperty, value);
    }

    /// <summary>Zero-based index of the currently executing step.</summary>
    public int StepIndex
    {
        get => _stepIndex;
        private set => SetAndRaise(StepIndexProperty, ref _stepIndex, value);
    }

    /// <summary>True while a script is actively running.</summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetAndRaise(IsPlayingProperty, ref _isPlaying, value);
    }

    /// <summary>Replays the current <see cref="Script"/> from the beginning.</summary>
    public void Replay() => Run(Script);

    protected override HyperSource Source => HyperSource.Shared(Svg);

    protected override NetworkMapState CaptureState() =>
        new(SkeumoSkia.CapturePalette(this), _channel);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Script is { } script)
            Run(script);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelRun();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnScriptChanged(NetworkScript? script) => Run(script);

    private void Run(NetworkScript? script)
    {
        CancelRun();
        if (script is null || !IsAttachedToVisualTree()) return;

        var cts = new CancellationTokenSource();
        _runCts = cts;
        _ = RunAsync(script, cts);
    }

    private void CancelRun()
    {
        var cts = _runCts;
        _runCts = null;
        cts?.Cancel();
        cts?.Dispose();
        _channel.CancelAll();
        IsPlaying = false;
        Refresh();
    }

    private async Task RunAsync(NetworkScript script, CancellationTokenSource cts)
    {
        var token = cts.Token;
        StepIndex = 0;
        IsPlaying = true;
        _channel.Reset(script.Asleep ?? Array.Empty<NetNode>());
        Refresh();

        try
        {
            for (int i = 0; i < script.Steps.Count; i++)
            {
                if (token.IsCancellationRequested) return;
                StepIndex = i;
                foreach (var action in script.Steps[i].Actions)
                {
                    if (token.IsCancellationRequested) return;
                    await ExecuteActionAsync(action, token);
                }
                await Task.Delay(TimeSpan.FromSeconds(StepPause), token);
            }

            if (!token.IsCancellationRequested)
                StepIndex = script.Steps.Count;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_runCts, cts))
            {
                IsPlaying = false;
                cts.Dispose();
                _runCts = null;
            }
        }
    }

    private static readonly NetNode[] LeafNodes =
    [
        NetNode.Router,
        NetNode.ComputerA,
        NetNode.ComputerB,
        NetNode.ComputerC
    ];

    private async Task ExecuteActionAsync(NetAction action, CancellationToken token)
    {
        switch (action)
        {
            case NetSend s:
                await EnqueuePacketAsync(s.From, s.To, s.Label, s.Kind, 0, token);
                break;

            case NetBroadcast b:
                if (b.From != NetNode.Switch)
                    await EnqueuePacketAsync(b.From, NetNode.Switch, b.Label, b.Kind, 0, token);
                await FloodFromSwitchAsync(b.From, b.Label, b.Kind, token);
                break;

            case NetFlood f:
                await FloodFromSwitchAsync(f.Except, f.Label, f.Kind, token);
                break;

            case NetPulse p:
                _channel.Enqueue(new NetCmdPulse(p.Node));
                Refresh();
                await Task.Delay(TimeSpan.FromSeconds(PulseSeconds), token);
                break;

            case NetPower pw:
                _channel.Enqueue(new NetCmdPower(pw.Node, pw.On));
                Refresh();
                await Task.Delay(TimeSpan.FromSeconds(0.35), token);
                break;
        }
    }

    private async Task FloodFromSwitchAsync(
        NetNode except, string? label, PacketKind kind, CancellationToken token)
    {
        _channel.Enqueue(new NetCmdRipple(NetNode.Switch));
        Refresh();

        var tasks = new List<Task>(LeafNodes.Length);
        foreach (var node in LeafNodes)
        {
            if (node != except)
                tasks.Add(EnqueuePacketAsync(NetNode.Switch, node, label, kind, 0, token));
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    private Task EnqueuePacketAsync(
        NetNode from, NetNode to, string? label, PacketKind kind, double delay, CancellationToken token)
    {
        if (from == to) return Task.CompletedTask;
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var reg = token.Register(() => tcs.TrySetCanceled(token));
        _channel.Enqueue(new NetCmdPacket(from, to, label, kind, delay, tcs));
        Refresh();
        return tcs.Task;
    }

    private bool IsAttachedToVisualTree() => VisualRoot is not null;
}

public readonly record struct NetworkMapState(SkeumoPalette Palette, NetCommandChannel Channel);

public sealed class NetCommandChannel
{
    private readonly ConcurrentQueue<NetCmd> _queue = new();
    private readonly ConcurrentBag<TaskCompletionSource> _pendingTcs = new();

    public void Enqueue(NetCmd cmd)
    {
        if (cmd is NetCmdPacket p)
            _pendingTcs.Add(p.Completion);
        _queue.Enqueue(cmd);
    }

    public void Reset(IReadOnlyList<NetNode> asleep)
    {
        CancelAll();
        _queue.Enqueue(new NetCmdReset(asleep));
    }

    public void CancelAll()
    {
        while (_queue.TryDequeue(out _)) { }
        while (_pendingTcs.TryTake(out var tcs))
            Dispatcher.UIThread.Post(() => tcs.TrySetCanceled());
        _queue.Enqueue(new NetCmdClear());
    }

    public bool TryDequeue(out NetCmd? cmd) => _queue.TryDequeue(out cmd);
}

public abstract record NetCmd;
public sealed record NetCmdReset(IReadOnlyList<NetNode> Asleep) : NetCmd;
public sealed record NetCmdClear : NetCmd;
public sealed record NetCmdPacket(NetNode From, NetNode To, string? Label, PacketKind Kind, double Delay, TaskCompletionSource Completion) : NetCmd;
public sealed record NetCmdRipple(NetNode Center) : NetCmd;
public sealed record NetCmdPulse(NetNode Target) : NetCmd;
public sealed record NetCmdPower(NetNode Target, bool Awake) : NetCmd;

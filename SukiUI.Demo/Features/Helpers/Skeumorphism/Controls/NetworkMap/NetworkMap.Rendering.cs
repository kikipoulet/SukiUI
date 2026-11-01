using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the NetworkMap: cables, packets gliding along them, ripples of broadcasts, halos of busy devices,
// port LEDs of the switch, screens waking up. Plays the scripts; knows nothing about what a protocol is.
public partial class NetworkMap
{
    private const double PacketSpeed = 150;     // px per second along a cable
    private const double SwitchDwell = 0.16;    // seconds a packet waits inside the switch
    private const double StepPause = 0.6;       // reading time between two steps
    private const double PulseSeconds = 0.9;    // how long the halo of a busy device takes to fade
    private const double LedSeconds = 0.45;     // how long a port LED stays lit after a packet

    private static readonly NetNode[] AllNodes = Enum.GetValues<NetNode>();
    private static readonly NetNode[] Ports = { NetNode.Router, NetNode.ComputerA, NetNode.ComputerB, NetNode.ComputerC };

    // The cables run from the middle of each device (hidden under it) to the middle of the switch (144,112):
    // the same geometry draws the cable and is the route of the packets, so they can never disagree.
    private static readonly Dictionary<NetNode, string> CableData = new()
    {
        [NetNode.Router] = "M 144,47 L 144,112",
        [NetNode.ComputerA] = "M 40,84 C 80,84 100,112 144,112",
        [NetNode.ComputerB] = "M 66,174 C 100,174 120,140 144,112",
        [NetNode.ComputerC] = "M 222,174 C 188,174 168,140 144,112",
    };

    private static readonly Point SwitchCenter = new(144, 112);

    // Bodies of the devices (same coordinates as the XAML), for the halos
    private static readonly Dictionary<NetNode, Rect> Bodies = new()
    {
        [NetNode.Router] = new Rect(111, 26, 66, 30),
        [NetNode.Switch] = new Rect(100, 99, 88, 26),
        [NetNode.ComputerA] = new Rect(12, 66, 56, 46),
        [NetNode.ComputerB] = new Rect(38, 156, 56, 46),
        [NetNode.ComputerC] = new Rect(194, 156, 56, 46),
    };

    private readonly Random _rng = new();
    private readonly Dictionary<NetNode, Geometry> _cables = new();
    private readonly Dictionary<NetNode, Border> _halos = new();
    private readonly Dictionary<NetNode, Border> _portLeds = new();
    private readonly Dictionary<NetNode, Border> _deviceLeds = new();
    private readonly Dictionary<NetNode, Border> _screens = new();
    private readonly Dictionary<NetNode, TextBlock> _labels = new();
    private readonly Dictionary<NetNode, double> _pulse = AllNodes.ToDictionary(n => n, _ => 0d);
    private readonly Dictionary<NetNode, double> _led = AllNodes.ToDictionary(n => n, _ => 0d);
    private readonly Dictionary<NetNode, double> _screen = AllNodes.ToDictionary(n => n, _ => 1d);
    private readonly Dictionary<NetNode, bool> _powered = AllNodes.ToDictionary(n => n, _ => true);
    private readonly Dictionary<NetNode, bool> _labelOn = AllNodes.ToDictionary(n => n, _ => false);
    private readonly List<Packet> _packets = new();
    private readonly List<Wave> _ripples = new();

    private DispatcherTimer _timer = null!;
    private CancellationTokenSource? _cts;
    private Color _accent = Colors.DodgerBlue;
    private long _lastStamp;
    private bool _built, _attached;

    private void InitializeRendering()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree += (_, _) =>
        {
            _attached = true;
            Build();
            ApplyTheme();
            _lastStamp = Stopwatch.GetTimestamp();
            _timer.Start();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            _timer.Stop();
            _attached = false;
            CancelRun();
        };
        ActualThemeVariantChanged += (_, _) => ApplyTheme();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ScriptProperty) ApplyScript(change.GetNewValue<NetworkScript?>());
    }

    // ------------------------------------------------------------------ construction

    private void Build()
    {
        if (_built) return;
        _built = true;

        var halos = this.Get<Canvas>("HaloLayer");
        foreach (var (node, body) in Bodies)
        {
            var halo = new Border
            {
                Width = body.Width + 10, Height = body.Height + 10, CornerRadius = new CornerRadius(13),
                Opacity = 0, IsHitTestVisible = false
            };
            Canvas.SetLeft(halo, body.X - 5);
            Canvas.SetTop(halo, body.Y - 5);
            halos.Children.Add(halo);
            _halos[node] = halo;
        }

        var cables = this.Get<Canvas>("CableLayer");
        foreach (var (node, data) in CableData)
        {
            var geometry = Geometry.Parse(data);
            _cables[node] = geometry;
            cables.Children.Add(new Path
            {
                Data = geometry, Stroke = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0)), StrokeThickness = 6,
                StrokeLineCap = PenLineCap.Round, RenderTransform = new TranslateTransform(0, 2)
            });
            var jacket = new Path { Data = geometry, StrokeThickness = 4.2, StrokeLineCap = PenLineCap.Round };
            jacket.BindBrush(Shape.StrokeProperty, this, "NmCable");
            cables.Children.Add(jacket);
            cables.Children.Add(new Path
            {
                Data = geometry, Stroke = new SolidColorBrush(Color.FromArgb(0x4D, 255, 255, 255)), StrokeThickness = 1.2,
                StrokeLineCap = PenLineCap.Round, RenderTransform = new TranslateTransform(0, -1)
            });
        }

        _portLeds[NetNode.ComputerA] = this.Get<Border>("LedOnA");
        _portLeds[NetNode.ComputerB] = this.Get<Border>("LedOnB");
        _portLeds[NetNode.Router] = this.Get<Border>("LedOnR");
        _portLeds[NetNode.ComputerC] = this.Get<Border>("LedOnC");
        _deviceLeds[NetNode.Router] = this.Get<Border>("RouterLedOn");
        _screens[NetNode.ComputerA] = this.Get<Border>("ScreenA");
        _screens[NetNode.ComputerB] = this.Get<Border>("ScreenB");
        _screens[NetNode.ComputerC] = this.Get<Border>("ScreenC");
        _labels[NetNode.ComputerA] = this.Get<TextBlock>("LabelA");
        _labels[NetNode.ComputerB] = this.Get<TextBlock>("LabelB");
        _labels[NetNode.ComputerC] = this.Get<TextBlock>("LabelC");
    }

    // Everything light-emitting is the accent: glows cannot reference a resource, so they are rebuilt here
    private void ApplyTheme()
    {
        if (SkeumoMotion.TryAccent(this, out var a)) _accent = a;
        foreach (var halo in _halos.Values) halo.BoxShadow = SkeumoMotion.Glow(_accent, 16, 0xA0, 2);
        foreach (var led in _portLeds.Values.Concat(_deviceLeds.Values)) led.BoxShadow = SkeumoMotion.Glow(_accent, 5, 0xB0);
        this.Get<Border>("RouterPowerOn").BoxShadow = SkeumoMotion.Glow(_accent, 4, 0x80);
        foreach (var p in _packets) p.Pill.BoxShadow = SkeumoMotion.Glow(_accent, 7, 0x70);
    }

    // ------------------------------------------------------------------ scripts

    private void ApplyScript(NetworkScript? script)
    {
        CancelRun();
        if (script == null || !_attached) return;
        var cts = new CancellationTokenSource();
        _cts = cts;
        _ = RunAsync(script, cts);
    }

    private async Task RunAsync(NetworkScript script, CancellationTokenSource cts)
    {
        var ct = cts.Token;
        try
        {
            foreach (var n in AllNodes)
            {
                var awake = script.Asleep?.Contains(n) != true;
                _powered[n] = awake;
                _screen[n] = awake ? 1 : 0;
            }

            SetCurrentValue(IsPlayingProperty, true);
            for (var i = 0; i < script.Steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                SetCurrentValue(StepIndexProperty, i);
                foreach (var action in script.Steps[i].Actions) await RunActionAsync(action, ct);
                await Task.Delay(TimeSpan.FromSeconds(StepPause), ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            // A newer script (or a stop) has already taken over: leave the state alone
            if (ReferenceEquals(_cts, cts))
            {
                _cts = null;
                SetCurrentValue(IsPlayingProperty, false);
            }
        }
    }

    private async Task RunActionAsync(NetAction action, CancellationToken ct)
    {
        switch (action)
        {
            case NetSend s:
                await Transmit(s.From, s.To, s.Kind, s.Label);
                break;
            case NetBroadcast b:
                Ripple(Anchor(b.From));
                await Transmit(b.From, NetNode.Switch, b.Kind, b.Label);
                Ripple(SwitchCenter);
                await Flood(b.From, b.Kind, b.Label);
                break;
            case NetFlood f:
                Ripple(SwitchCenter);
                await Flood(f.Except, f.Kind, f.Label);
                break;
            case NetPulse p:
                Pulse(p.Node);
                await Task.Delay(TimeSpan.FromSeconds(0.75), ct);
                break;
            case NetPower w:
                _powered[w.Node] = w.On;
                Pulse(w.Node);
                await Task.Delay(TimeSpan.FromSeconds(0.9), ct);
                break;
        }
    }

    // Stops everything at once: packets vanish, the pending tasks are cancelled, the scene goes quiet
    private void CancelRun()
    {
        var cts = _cts;
        _cts = null;
        cts?.Cancel();

        var layer = this.Get<Canvas>("PacketLayer");
        foreach (var p in _packets)
        {
            layer.Children.Remove(p.Pill);
            p.Done.TrySetCanceled();
        }
        _packets.Clear();
        var fx = this.Get<Canvas>("FxLayer");
        foreach (var r in _ripples) fx.Children.Remove(r.E);
        _ripples.Clear();
        foreach (var n in AllNodes)
        {
            _pulse[n] = 0;
            _led[n] = 0;
        }

        SetCurrentValue(IsPlayingProperty, false);
        SetCurrentValue(StepIndexProperty, -1);
    }

    // ------------------------------------------------------------------ transmissions

    private static Point Anchor(NetNode node) => node == NetNode.Switch ? SwitchCenter : CableStart(node);

    private static Point CableStart(NetNode node)
    {
        var parsed = Geometry.Parse(CableData[node]);
        parsed.TryGetPointAtDistance(0, out var start);
        return start;
    }

    private Task Transmit(NetNode from, NetNode to, PacketKind kind, string? label)
    {
        var legs = new List<Leg>();
        if (from != NetNode.Switch) legs.Add(new Leg(_cables[from], false, from));
        if (to != NetNode.Switch) legs.Add(new Leg(_cables[to], true, to));
        return Launch(legs, kind, label);
    }

    // The switch copies the packet to every port but the one it came from
    private Task Flood(NetNode except, PacketKind kind, string? label) =>
        Task.WhenAll(Ports.Where(n => n != except).Select(n => Launch(new List<Leg> { new(_cables[n], true, n) }, kind, label)));

    private Task Launch(List<Leg> legs, PacketKind kind, string? label)
    {
        if (legs.Count == 0 || !_attached) return Task.CompletedTask;

        var hasLabel = !string.IsNullOrEmpty(label);
        var width = hasLabel ? 8 + label!.Length * 4.1 : 9;
        var height = hasLabel ? 11 : 9;
        var pill = new Border
        {
            Width = width, Height = height, CornerRadius = new CornerRadius(height / 2), Opacity = 0,
            BoxShadow = SkeumoMotion.Glow(_accent, 7, 0x70), IsHitTestVisible = false
        };
        var text = hasLabel
            ? new TextBlock
            {
                Text = label, FontFamily = new FontFamily("Consolas"), FontSize = 6.5, FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
            : null;
        if (kind == PacketKind.Request)
        {
            // a request is a filled capsule...
            pill.BindBrush(Border.BackgroundProperty, this, "SukiPrimaryColor");
            if (text != null) text.Foreground = Brushes.White;
        }
        else
        {
            // ...a reply is a glass one outlined by the accent
            pill.BindBrush(Border.BackgroundProperty, this, "SkTubeBg");
            pill.BindBrush(Border.BorderBrushProperty, this, "SukiPrimaryColor");
            pill.BorderThickness = new Thickness(1.2);
            text?.BindBrush(TextBlock.ForegroundProperty, this, "SukiPrimaryColor");
        }
        pill.Child = text;

        var packet = new Packet
        {
            Pill = pill, Legs = legs, Speed = PacketSpeed * (0.9 + 0.2 * _rng.NextDouble()),
            Done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        Place(packet);
        this.Get<Canvas>("PacketLayer").Children.Add(pill);
        _packets.Add(packet);
        return packet.Done.Task;
    }

    private void Place(Packet p, double? sOverride = null)
    {
        var leg = p.Legs[p.Leg];
        var s = sOverride ?? p.S;
        var d = Math.Clamp(leg.Reverse ? leg.Length - s : s, 0, leg.Length);
        if (!leg.Route.TryGetPointAtDistance(d, out var pt)) return;
        Canvas.SetLeft(p.Pill, pt.X - p.Pill.Width / 2);
        Canvas.SetTop(p.Pill, pt.Y - p.Pill.Height / 2);
        // fades in as it leaves the device, fades out as it reaches the last one
        var fadeOut = p.Leg == p.Legs.Count - 1 ? Math.Min(1, (leg.Length - s) / 10) : 1;
        p.Pill.Opacity = Math.Clamp(Math.Min(1, p.Age * 6) * fadeOut, 0, 1);
    }

    private void Ripple(Point center)
    {
        var fx = this.Get<Canvas>("FxLayer");
        for (var i = 0; i < 2; i++)
        {
            var e = new Ellipse { StrokeThickness = 1.6, Opacity = 0, IsHitTestVisible = false };
            e.BindBrush(Shape.StrokeProperty, this, "SukiPrimaryColor");
            fx.Children.Add(e);
            _ripples.Add(new Wave { E = e, Center = center, Delay = i * 0.22 });
        }
    }

    private void Pulse(NetNode node, double strength = 1) => _pulse[node] = Math.Max(_pulse[node], strength);

    private void Flash(NetNode node) => _led[node] = 1;

    // ------------------------------------------------------------------ frame

    private void Tick()
    {
        var now = Stopwatch.GetTimestamp();
        var dt = Math.Min(0.05, (now - _lastStamp) / (double)Stopwatch.Frequency);
        _lastStamp = now;

        TickPackets(dt);
        TickRipples(dt);

        foreach (var n in AllNodes)
        {
            // Halo of a busy device, and its label (SukiText while busy, SukiLowText otherwise)
            var pulse = Math.Max(0, _pulse[n] - dt / PulseSeconds);
            _pulse[n] = pulse;
            if (_halos.TryGetValue(n, out var halo)) halo.Opacity = Smooth(pulse);
            if (_labels.TryGetValue(n, out var label))
            {
                var on = pulse > 0.12;
                if (_labelOn[n] != on)
                {
                    _labelOn[n] = on;
                    label.Classes.Set("Active", on);
                }
            }

            // LEDs: a port LED of the switch flashes while a packet passes; router and printer also glow when busy
            var led = Math.Max(0, _led[n] - dt / LedSeconds);
            _led[n] = led;
            if (_portLeds.TryGetValue(n, out var port)) port.Opacity = led;
            if (_deviceLeds.TryGetValue(n, out var device)) device.Opacity = Math.Max(led, Smooth(pulse));

            // Screens warm up and cool down
            if (_screens.TryGetValue(n, out var screen))
            {
                var target = _powered[n] ? 1 : 0;
                var shown = _screen[n] + (target - _screen[n]) * Math.Min(1, dt * 5);
                if (Math.Abs(target - shown) < 0.003) shown = target;
                _screen[n] = shown;
                screen.Opacity = 0.9 * shown;
            }
        }
    }

    private void TickPackets(double dt)
    {
        var layer = this.Get<Canvas>("PacketLayer");
        for (var i = _packets.Count - 1; i >= 0; i--)
        {
            var p = _packets[i];
            p.Age += dt;
            if (p.Dwell > 0)
            {
                p.Dwell -= dt;
                continue;                                           // waiting inside the switch
            }

            var leg = p.Legs[p.Leg];
            if (!p.Started)
            {
                p.Started = true;
                if (leg.Reverse) Flash(leg.Node);                   // leaving the switch: the port lights up
            }

            p.S += p.Speed * (0.4 + 0.6 * Math.Min(1, p.S / 30)) * dt;   // quick start, no sudden jump
            if (p.S < leg.Length)
            {
                Place(p);
                continue;
            }

            Place(p, leg.Length);
            if (leg.Reverse) Pulse(leg.Node, 0.7);                  // delivered: the device notices
            else Flash(leg.Node);                                   // entering the switch: the port lights up

            p.Leg++;
            p.S = 0;
            p.Started = false;
            if (p.Leg >= p.Legs.Count)
            {
                layer.Children.Remove(p.Pill);
                _packets.RemoveAt(i);
                p.Done.TrySetResult();
            }
            else p.Dwell = SwitchDwell;
        }
    }

    private void TickRipples(double dt)
    {
        var fx = this.Get<Canvas>("FxLayer");
        for (var i = _ripples.Count - 1; i >= 0; i--)
        {
            var r = _ripples[i];
            r.Age += dt;
            if (r.Age < r.Delay) continue;
            var t = (r.Age - r.Delay) / 0.95;
            if (t >= 1)
            {
                fx.Children.Remove(r.E);
                _ripples.RemoveAt(i);
                continue;
            }

            var radius = 8 + 56 * SkeumoMotion.EaseOutCubic(t);
            r.E.Width = r.E.Height = radius * 2;
            r.E.Opacity = 0.5 * (1 - t);
            Canvas.SetLeft(r.E, r.Center.X - radius);
            Canvas.SetTop(r.E, r.Center.Y - radius);
        }
    }

    private static double Smooth(double t) => t * t * (3 - 2 * t);

    private sealed record Leg(Geometry Route, bool Reverse, NetNode Node)
    {
        public double Length { get; } = Route.ContourLength;
    }

    private sealed class Packet
    {
        public Border Pill = null!;
        public List<Leg> Legs = null!;
        public TaskCompletionSource Done = null!;
        public int Leg;
        public double S, Speed, Dwell, Age;
        public bool Started;
    }

    private sealed class Wave
    {
        public Ellipse E = null!;
        public Point Center;
        public double Age, Delay;
    }
}

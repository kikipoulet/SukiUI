using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Syringe: barrel, plunger drag preview, valve rotor, stream and suspended particles.
public partial class Syringe
{
    // Geometry (local frame of the Scene canvas, 300 x 290). The barrel is anchored on top, the plunger enters from below.
    private const double BarrelTop = 50, BarrelHeight = 200, BarrelLeft = 120, PlungerHeight = 12, HandleTop = 262;

    // In / Out manifold: horizontal tube fading at both ends, junction in the center, neck to the barrel
    private const double JunctionX = 150, ManifoldY = 22, TubeEndInX = 34, TubeEndOutX = 266, TubeLength = 116, NeckLength = 30;

    private readonly List<StreamBubble> _streamBubbles = new();
    private readonly List<Particle> _particles = new();
    private readonly List<TextBlock> _scaleLabels = new();
    private DispatcherTimer _timer = null!;
    private bool _ready, _scaleBuilt, _dragging;
    private double _shown;          // displayed position 0..1 (= Position, or the drag preview)
    private double _rotorAngle, _flowFade, _glintPhase, _bubbleTick, _tick;
    private int _rotorTween;

    private void InitializeRendering()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        InitStream();
        InitLiquidParticles();

        _shown = Position;
        RenderPlunger();
        RenderValve(animate: false);

        AttachedToVisualTree      += (_, _) => { BuildScale(); ApplyGlow(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => { _timer.Stop(); CancelDrag(); };
        ActualThemeVariantChanged += (_, _) => ApplyGlow();
        _ready = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_ready) return;                                     // still constructing

        if (change.Property == PositionProperty && !_dragging)
        {
            _shown = Position;
            RenderPlunger();
        }
        else if (change.Property == IsValveOutProperty) RenderValve(animate: true);
        else if (change.Property == FlowProperty) UpdateStreamDirection();
        else if (change.Property == VolumeProperty) UpdateScaleLabels();
    }

    // ------------------------------------------------------------------ input on the device itself

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var canvas = (Canvas)s!;
        var p = e.GetPosition(canvas);

        if (p.X >= 110 && p.X <= 190 && p.Y >= 50 && p.Y <= 275)
        {
            // Plunger: the piece follows the pointer (preview), the request leaves on release
            _dragging = true;
            PreviewFromY(p.Y);
            e.Pointer.Capture(canvas);
            e.Handled = true;
        }
        else if (p.X >= 60 && p.X <= 240 && p.Y >= 6 && p.Y <= 42)
        {
            ValveToggleRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private void Scene_OnMoved(object? s, PointerEventArgs e)
    {
        if (_dragging) PreviewFromY(e.GetPosition((Canvas)s!).Y);
    }

    private void Scene_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);

        var requested = _shown;
        PositionRequested?.Invoke(this, requested);
        // Honest state: if the host did not accept the request, the plunger returns to the confirmed position
        _shown = Position;
        RenderPlunger();
    }

    // Capture lost (alt-tab, popup...): no request, back to the confirmed position
    private void Scene_OnCaptureLost(object? s, PointerCaptureLostEventArgs e) => CancelDrag();

    private void CancelDrag()
    {
        if (!_dragging) return;
        _dragging = false;
        _shown = Position;
        RenderPlunger();
    }

    private void PreviewFromY(double y)
    {
        _shown = Math.Clamp((y - BarrelTop) / BarrelHeight, 0, 1);
        RenderPlunger();
    }

    // ------------------------------------------------------------------ rendering

    private void RenderPlunger()
    {
        var fill = _shown * BarrelHeight;

        // Liquid anchored on top: it grows downwards
        this.Get<Border>("Liquid").Height = fill;

        // Seal under the liquid, rod down to the handle
        var plungerTop = BarrelTop + fill;
        Canvas.SetTop(this.Get<Border>("Plunger"), plungerTop);
        var rod = this.Get<Border>("Rod");
        var rodTop = plungerTop + PlungerHeight;
        Canvas.SetTop(rod, rodTop);
        rod.Height = Math.Max(0, HandleTop - rodTop);

        // Glow follows the liquid height
        var aura = this.Get<Border>("Aura");
        aura.Height = Math.Max(0, fill);
        aura.Opacity = fill > 6 ? 0.9 : 0;
    }

    private void RenderValve(bool animate)
    {
        // The L bore of the rotor points to In (0) or Out (-90)
        var rotor = (RotateTransform)this.Get<Grid>("ValveRotor").RenderTransform!;
        var target = IsValveOut ? -90.0 : 0.0;
        var id = ++_rotorTween;
        if (animate)
            Tween(_rotorAngle, target, 260, angle => { if (id != _rotorTween) return; _rotorAngle = angle; rotor.Angle = angle; });
        else
        {
            _rotorAngle = target;
            rotor.Angle = target;
        }

        this.Get<Border>("ChanIn").Opacity = IsValveOut ? 0.1 : 0.7;
        this.Get<Border>("ChanOut").Opacity = IsValveOut ? 0.7 : 0.1;
        this.Get<TextBlock>("LabelIn").Classes.Set("Active", !IsValveOut);
        this.Get<TextBlock>("LabelOut").Classes.Set("Active", IsValveOut);
        UpdateStreamDirection();
    }

    // The tube liquid runs from the end of the active way to the junction
    private void UpdateStreamDirection()
    {
        var startX = IsValveOut ? TubeEndOutX : TubeEndInX;
        foreach (var (name, dy) in new[] { ("TubeBody", 0.0), ("TubeCore", 0.0), ("TubeGlint", -1.0) })
        {
            var line = this.Get<Line>(name);
            line.StartPoint = new Point(startX, ManifoldY + dy);
            line.EndPoint = new Point(JunctionX, ManifoldY + dy);
        }
    }

    private void ApplyGlow()
    {
        if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color accent) return;
        this.Get<Border>("Aura").BoxShadow = new BoxShadows(
            new BoxShadow { Blur = 18, Spread = 4, Color = Color.FromArgb(0xB0, accent.R, accent.G, accent.B) },
            new[] { new BoxShadow { Blur = 42, Spread = 10, Color = Color.FromArgb(0x55, accent.R, accent.G, accent.B) } });
    }

    // ------------------------------------------------------------------ volume scale

    // 50 ticks: a major tick (label in mL) every 10, a medium one every 5, a fine one in between.
    // Colours come from the ScaleTick / ScaleLabel styles, so they follow the theme.
    private void BuildScale()
    {
        if (_scaleBuilt) return;
        _scaleBuilt = true;

        var canvas = this.Get<Canvas>("Scene");
        for (var k = 0; k <= 50; k++)
        {
            var y = BarrelTop + BarrelHeight * k / 50.0;
            var major = k % 10 == 0;
            var medium = k % 5 == 0;
            var length = major ? 12 : medium ? 8 : 4;

            var tick = new Border
            {
                Width = length, Height = major ? 1.5 : 1, CornerRadius = new CornerRadius(0.5),
                Opacity = major ? 1 : 0.55, IsHitTestVisible = false
            };
            tick.Classes.Add("ScaleTick");
            Canvas.SetLeft(tick, BarrelLeft - 27 - length);
            Canvas.SetTop(tick, y);
            canvas.Children.Add(tick);

            if (!major) continue;

            var label = new TextBlock
            {
                FontSize = 10, FontWeight = FontWeight.Medium, TextAlignment = TextAlignment.Right,
                Width = 36, IsHitTestVisible = false
            };
            label.Classes.Add("ScaleLabel");
            Canvas.SetLeft(label, BarrelLeft - 79);
            Canvas.SetTop(label, y - 7);
            canvas.Children.Add(label);
            _scaleLabels.Add(label);
        }
        UpdateScaleLabels();
    }

    private void UpdateScaleLabels()
    {
        for (var i = 0; i < _scaleLabels.Count; i++)
            _scaleLabels[i].Text = $"{Volume * (i * 10) / 50.0:0.#}";
    }

    // ------------------------------------------------------------------ stream and particles

    private sealed class StreamBubble
    {
        public Ellipse Tube = null!;
        public Ellipse Neck = null!;
        public double S;   // curvilinear abscissa: 0..TubeLength in the tube, then the neck
    }

    private sealed class Particle
    {
        public Ellipse Dot = null!;
        public double X, Y, Vx, Vy, Phase;
    }

    private void InitStream()
    {
        var rng = new Random(11);
        var tube = this.Get<Canvas>("TubeBubbles");
        var neck = this.Get<Canvas>("NeckBubbles");
        var fill = new SolidColorBrush(Color.Parse("#E6FFFFFF"));

        for (var i = 0; i < 7; i++)
        {
            var size = 2.2 + rng.NextDouble();
            var bubble = new StreamBubble
            {
                Tube = new Ellipse { Width = size, Height = size, Fill = fill },
                Neck = new Ellipse { Width = size, Height = size, Fill = fill, IsVisible = false },
                S = i * 20 + rng.NextDouble() * 8
            };
            _streamBubbles.Add(bubble);
            tube.Children.Add(bubble.Tube);
            neck.Children.Add(bubble.Neck);
        }
    }

    private void InitLiquidParticles()
    {
        var rng = new Random(7);
        var host = this.Get<Canvas>("LiquidParticles");

        for (var i = 0; i < 10; i++)
        {
            var size = 2 + rng.Next(2); // 2 or 3 px
            var p = new Particle
            {
                Dot = new Ellipse
                {
                    Width = size,
                    Height = size,
                    // bright micro-bubbles + a few dark impurities
                    Fill = new SolidColorBrush(Color.Parse(i % 3 == 2 ? "#33000000" : "#66FFFFFF")),
                    Opacity = 0.6 + rng.NextDouble() * 0.3
                },
                X = 3 + rng.NextDouble() * (56 - 6 - size),
                Y = 4 + rng.NextDouble() * 186,
                Vx = (rng.NextDouble() - 0.5) * 0.24,
                Vy = (rng.NextDouble() - 0.5) * 0.18,
                Phase = rng.NextDouble() * 6.28
            };
            _particles.Add(p);
            host.Children.Add(p.Dot);
        }
    }

    private void Tick()
    {
        // The stream fades in and out with the flow
        _flowFade += ((Flow != 0 ? 1.0 : 0.0) - _flowFade) * 0.12;
        var opacity = _flowFade < 0.01 ? 0 : _flowFade;
        this.Get<Canvas>("StreamTube").Opacity = opacity;
        this.Get<Canvas>("StreamNeck").Opacity = opacity;
        if (opacity > 0) AdvanceStream();

        UpdateLiquidParticles();
    }

    private double _lastSpeed = 0.5, _lastSign = 1;

    private void AdvanceStream()
    {
        // Filling: towards the barrel. Emptying: the other way. The last direction is kept while the stream fades out.
        if (Flow != 0)
        {
            _lastSpeed = Math.Abs(Flow);
            _lastSign = Math.Sign(Flow);
        }
        var step = Math.Max(0.4, _lastSpeed * 100 / 60.0) * _lastSign;
        _glintPhase += step;
        _bubbleTick++;

        // StrokeDashOffset is in multiples of the stroke thickness; a decreasing offset moves the glints forward
        this.Get<Line>("TubeGlint").StrokeDashOffset = -(_glintPhase % 18) / 1.4;
        this.Get<Line>("NeckGlint").StrokeDashOffset = -(_glintPhase % 17) / 1.6;

        const double total = TubeLength + NeckLength;
        foreach (var b in _streamBubbles)
        {
            b.S = ((b.S + step) % total + total) % total;
            var r = b.Tube.Width / 2;
            if (b.S < TubeLength)
            {
                var x = IsValveOut ? TubeEndOutX - b.S : TubeEndInX + b.S;
                Canvas.SetLeft(b.Tube, x - r);
                Canvas.SetTop(b.Tube, ManifoldY + Math.Sin(b.S / 6 + _bubbleTick * 0.05) * 1.4 - r);
                b.Tube.IsVisible = true;
                b.Neck.IsVisible = false;
            }
            else
            {
                Canvas.SetLeft(b.Neck, JunctionX + Math.Sin(b.S / 4) * 1.6 - r);
                Canvas.SetTop(b.Neck, ManifoldY + (b.S - TubeLength) - r);
                b.Neck.IsVisible = true;
                b.Tube.IsVisible = false;
            }
        }
    }

    // Slow drift + bounces, visible only under the liquid surface
    private void UpdateLiquidParticles()
    {
        var h = this.Get<Border>("Liquid").Height;
        _tick++;

        foreach (var p in _particles)
        {
            p.X += p.Vx + 0.04 * Math.Sin(_tick * 0.02 + p.Phase);
            p.Y += p.Vy;

            if (p.X < 3) { p.X = 3; p.Vx = -p.Vx; }
            if (p.X > 56 - 3 - p.Dot.Width) { p.X = 56 - 3 - p.Dot.Width; p.Vx = -p.Vx; }
            if (p.Y < 4) { p.Y = 4; p.Vy = -p.Vy; }
            if (p.Y > 190) { p.Y = 190; p.Vy = -p.Vy; }

            p.Dot.IsVisible = h > 12 && p.Y < h - 4;
            if (!p.Dot.IsVisible) continue;
            Canvas.SetLeft(p.Dot, p.X);
            Canvas.SetTop(p.Dot, p.Y);
        }
    }

    private static void Tween(double from, double to, int ms, Action<double> set, Func<double, double>? ease = null)
    {
        ease ??= t => 1 - Math.Pow(1 - t, 3);
        var start = DateTime.UtcNow;
        DispatcherTimer? timer = null;
        timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
        {
            var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
            set(from + (to - from) * ease(t));
            if (t >= 1) timer!.Stop();
        });
        timer.Start();
    }
}

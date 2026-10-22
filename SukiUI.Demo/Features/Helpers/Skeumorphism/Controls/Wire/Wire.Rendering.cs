using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Wire: insulation, fading start, current pulses along the geometry.
public partial class Wire
{
    private const double FadeLength = 46;

    private readonly Random _rng = new();
    private readonly List<Pulse> _pulses = new();
    private readonly List<Glint> _glints = new();
    private DispatcherTimer _timer = null!;
    private Color _accent = Colors.DodgerBlue;
    private double _shown, _length;
    private bool _armed;
    private readonly List<Crackle> _crackles = new();
    private double _phase;

    private void InitializeRendering()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree      += (_, _) => { ApplyTheme(); Rebuild(); _timer.Start(); Dispatcher.UIThread.Post(() => _armed = true); };
        DetachedFromVisualTree    += (_, _) => { _timer.Stop(); _armed = false; };
        ActualThemeVariantChanged += (_, _) => ApplyTheme();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataProperty || change.Property == FadeStartProperty) Rebuild();
        else if (change.Property == InsulationProperty) ApplyInsulation();
        else if (change.Property == TriggerProperty && _armed) Emit(2.4);
    }

    private void Rebuild()
    {
        var data = Data;
        foreach (var name in new[] { "Shadow", "Jacket", "Gloss", "Flow" }) this.Get<Path>(name).Data = data;
        _length = data?.ContourLength ?? 0;
        ApplyInsulation();

        // Fading start: an absolute gradient from the first point along the initial direction
        var root = this.Get<Canvas>("Root");
        if (FadeStart && data != null && data.TryGetPointAndTangentAtDistance(0, out var p0, out var t0))
        {
            var len = Math.Sqrt(t0.X * t0.X + t0.Y * t0.Y);
            var dir = len > 0 ? new Point(t0.X / len, t0.Y / len) : new Point(1, 0);
            root.OpacityMask = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(p0.X, p0.Y, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(p0.X + dir.X * FadeLength, p0.Y + dir.Y * FadeLength, RelativeUnit.Absolute),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(0, 255, 255, 255), 0),
                    new GradientStop(Color.FromArgb(0x59, 255, 255, 255), 0.4),
                    new GradientStop(Colors.White, 1)
                }
            };
        }
        else root.OpacityMask = null;
    }

    private void ApplyInsulation()
    {
        var key = Insulation switch
        {
            WireInsulation.Red => "WrRed",
            WireInsulation.Black => "WrBlack",
            _ => "WrNeutral"
        };
        this.Get<Path>("Jacket").BindBrush(Shape.StrokeProperty, this, key);
    }

    private void ApplyTheme()
    {
        if (SkeumoMotion.TryAccent(this, out var a)) _accent = a;
    }

    private const double SparkLength = 13;
    private static readonly Color Hot = Color.FromRgb(0xFF, 0xE8, 0x7A);

    // A small electric arc: a short crackling bolt (warm core inside a cool glow) that sheds a few glints
    private void Emit(double speed)
    {
        if (Data == null || _length <= 0) return;
        var glow = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(0x38, 0x6E, 0xB8, 0xFF)), StrokeThickness = 4,
            StrokeJoin = PenLineJoin.Round, StrokeLineCap = PenLineCap.Round,
            Effect = new BlurEffect { Radius = 2.5 }
        };
        var core = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(0xA6, Hot.R, Hot.G, Hot.B)), StrokeThickness = 1.2,
            StrokeJoin = PenLineJoin.Miter, StrokeLineCap = PenLineCap.Round
        };
        var canvas = this.Get<Canvas>("Pulses");
        canvas.Children.Add(glow);
        canvas.Children.Add(core);
        _pulses.Add(new Pulse { Glow = glow, Core = core, S = 0, Speed = speed });
    }

    private void Tick()
    {
        _shown += (Current - _shown) * 0.1;
        if (Math.Abs(Current - _shown) < 0.002) _shown = Current;
        var data = Data;
        var canvas = this.Get<Canvas>("Pulses");
        if (data == null || _length <= 0) return;

        // Continuous flow: only a faint highlight drifting slowly, never a distraction
        var flow = this.Get<Path>("Flow");
        _phase += 0.03 + 0.05 * _shown;
        flow.StrokeDashOffset = -_phase;
        flow.Opacity = _shown * (0.26 + 0.08 * Math.Sin(_phase * 0.5));

        // Crackle: tiny short-lived sparks flickering here and there along the wire while current runs
        if (_shown > 0.02 && _rng.NextDouble() < 0.06 + 0.12 * _shown)
        {
            var line = new Polyline { Stroke = new SolidColorBrush(Color.FromArgb(0xB0, Hot.R, Hot.G, Hot.B)), StrokeThickness = 1, StrokeLineCap = PenLineCap.Round };
            canvas.Children.Add(line);
            _crackles.Add(new Crackle { E = line, S = _rng.NextDouble() * _length, Life = 4 + _rng.Next(4) });
        }
        for (var i = _crackles.Count - 1; i >= 0; i--)
        {
            var c = _crackles[i];
            if (--c.Life < 0)
            {
                canvas.Children.Remove(c.E);
                _crackles.RemoveAt(i);
                continue;
            }
            var pts = new List<Point>();
            for (var k = 0; k < 5; k++)
            {
                if (!data.TryGetPointAndTangentAtDistance(Math.Min(_length, c.S + k * 2.2), out var pt, out var tg)) continue;
                var tl = Math.Sqrt(tg.X * tg.X + tg.Y * tg.Y);
                if (tl <= 0) continue;
                var j = (k == 0 || k == 4 ? 0 : (_rng.NextDouble() - 0.5) * 3.4);
                pts.Add(new Point(pt.X - tg.Y / tl * j, pt.Y + tg.X / tl * j));
            }
            c.E.Points = pts;
            c.E.Opacity = Math.Min(1, c.Life / 3.0) * (0.5 + 0.5 * _shown);
        }

        for (var i = _pulses.Count - 1; i >= 0; i--)
        {
            var p = _pulses[i];
            p.S += p.Speed;
            p.Frame++;
            if (p.S - SparkLength >= _length)
            {
                canvas.Children.Remove(p.Glow);
                canvas.Children.Remove(p.Core);
                _pulses.RemoveAt(i);
                continue;
            }

            // The bolt crackles: a new zigzag every few frames, fixed in between
            if (p.Frame % 3 == 1)
                for (var k = 0; k < p.Jitter.Length; k++)
                    p.Jitter[k] = (k % 2 == 0 ? 1 : -1) * (0.4 + _rng.NextDouble() * 0.6) * (k == 0 || k == p.Jitter.Length - 1 ? 0 : 1.1);

            var points = new List<Point>();
            for (var k = 0; k < p.Jitter.Length; k++)
            {
                var d = Math.Clamp(p.S - SparkLength * (1 - k / (double)(p.Jitter.Length - 1)), 0, _length);
                if (!data.TryGetPointAndTangentAtDistance(d, out var pt, out var tg)) continue;
                var tl = Math.Sqrt(tg.X * tg.X + tg.Y * tg.Y);
                if (tl <= 0) continue;
                points.Add(new Point(pt.X - tg.Y / tl * p.Jitter[k], pt.Y + tg.X / tl * p.Jitter[k]));
            }
            p.Glow.Points = points;
            p.Core.Points = points;
            var fade = Math.Min(1, p.S / 30) * Math.Min(1, (_length - p.S + SparkLength) / 40);
            p.Glow.Opacity = Math.Clamp(fade, 0, 1) * (0.7 + 0.3 * _rng.NextDouble());   // slight flicker
            p.Core.Opacity = Math.Clamp(fade, 0, 1);

            // Glints dropped behind the head
            if (p.Frame % 4 == 0 && p.S < _length && data.TryGetPointAtDistance(p.S, out var head))
            {
                var g = new Ellipse { Width = 1.8, Height = 1.8, Fill = new SolidColorBrush(Color.FromArgb(0xCC, Hot.R, Hot.G, Hot.B)) };
                canvas.Children.Add(g);
                _glints.Add(new Glint
                {
                    E = g, X = head.X, Y = head.Y,
                    Vx = (_rng.NextDouble() - 0.5) * 0.9, Vy = (_rng.NextDouble() - 0.5) * 0.9 - 0.15,
                });
            }
        }

        for (var i = _glints.Count - 1; i >= 0; i--)
        {
            var g = _glints[i];
            g.Life -= 0.04;
            if (g.Life <= 0)
            {
                canvas.Children.Remove(g.E);
                _glints.RemoveAt(i);
                continue;
            }
            g.X += g.Vx;
            g.Y += g.Vy;
            g.E.Opacity = g.Life * 0.6;
            Canvas.SetLeft(g.E, g.X - 0.9);
            Canvas.SetTop(g.E, g.Y - 0.9);
        }
    }

    private sealed class Pulse
    {
        public Polyline Glow = null!, Core = null!;
        public double S, Speed;
        public int Frame;
        public readonly double[] Jitter = new double[6];
    }

    private sealed class Glint
    {
        public Ellipse E = null!;
        public double X, Y, Vx, Vy, Life = 1;
    }

    private sealed class Crackle
    {
        public Polyline E = null!;
        public double S;
        public int Life;
    }
}

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Fan: rotor with inertia, motion blur ghosts, motor glow and air particles.
public partial class Fan
{
    private const int BladeCount = 5;
    private const double MaxDegPerFrame = 26;
    private const int GhostCount = 6;
    private const double CenterX = 80, CenterY = 80;

    // One blade in the rotor frame (136 x 136, center 68,68), swept forward
    private const string BladeData = "M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z";

    private readonly Random _rng = new();
    private readonly List<AirSpeck> _air = new();
    private DispatcherTimer _timer = null!;
    private readonly List<(Canvas Canvas, RotateTransform Rotate)> _ghosts = new();
    private RotateTransform _rotor = null!;
    private BlurEffect _ghostBlur = null!;
    private double _shown, _angle;

    private void InitializeRendering()
    {
        _rotor = BuildBlades(this.Get<Canvas>("Rotor"));
        var host = this.Get<Canvas>("Ghosts");
        _ghostBlur = (BlurEffect)host.Effect!;
        for (var i = 0; i < GhostCount; i++)
        {
            var ghost = new Canvas
            {
                Width = 136, Height = 136, Opacity = 0,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new RotateTransform()
            };
            host.Children.Add(ghost);
            _ghosts.Add((ghost, BuildBlades(ghost)));
        }

        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree      += (_, _) => { ApplyGlow(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
        ActualThemeVariantChanged += (_, _) => ApplyGlow();
    }

    private RotateTransform BuildBlades(Canvas canvas)
    {
        for (var i = 0; i < BladeCount; i++)
        {
            var blade = new Path
            {
                Data = Geometry.Parse(BladeData),
                Width = 136, Height = 136,
                StrokeThickness = 0.7,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new RotateTransform(i * 360.0 / BladeCount)
            };
            blade.BindBrush(Shape.FillProperty, this, "FnBlade");
            blade.BindBrush(Shape.StrokeProperty, this, "SkEdge");
            canvas.Children.Add(blade);
        }
        return (RotateTransform)canvas.RenderTransform!;
    }

    private void ApplyGlow()
    {
        if (!SkeumoMotion.TryAccent(this, out var a)) return;
        this.Get<Border>("MotorGlow").BoxShadow = new BoxShadows(
            new BoxShadow { Blur = 22, Spread = 6, Color = Color.FromArgb(0xA0, a.R, a.G, a.B) },
            new[] { new BoxShadow { Blur = 48, Spread = 14, Color = Color.FromArgb(0x40, a.R, a.G, a.B) } });
    }

    private void Tick()
    {
        // Inertia: the rotor spins up and down progressively
        _shown += (Speed - _shown) * 0.035;
        if (Math.Abs(Speed - _shown) < 0.001) _shown = Speed;
        var s = _shown;

        var step = s * MaxDegPerFrame;
        _angle = (_angle + step) % 360;
        _rotor.Angle = _angle;

        // Motion blur: ghosts spread evenly over the sweep since the last frame (and a bit more), blurred more and
        // more; the sharp blades fade out; at the top of the range a translucent disc reads as a spinning rotor
        var sweep = Math.Max(step * 1.4, 1);
        for (var i = 0; i < _ghosts.Count; i++)
        {
            var (canvas, rotate) = _ghosts[i];
            rotate.Angle = _angle - sweep * (i + 1) / _ghosts.Count;
            canvas.Opacity = 0.3 * Math.Min(1, s * 1.6) * (1 - 0.5 * i / _ghosts.Count);
        }
        _ghostBlur.Radius = 4 * s * s;
        this.Get<Canvas>("Rotor").Opacity = 1 - 0.85 * Math.Pow(s, 1.4);
        this.Get<Ellipse>("BlurDisc").Opacity = 0.9 * Math.Pow(Math.Max(0, s - 0.35) / 0.65, 1.5);

        this.Get<Border>("MotorGlow").Opacity = 0.9 * s;
        this.Get<Ellipse>("HubRing").Opacity = 0.35 + 0.65 * s;

        UpdateAir(s);
    }

    // Air pushed outwards with a slight swirl, spawned at random so it never forms a pattern
    private void UpdateAir(double s)
    {
        var canvas = this.Get<Canvas>("Air");
        var expected = 0.8 * s;
        while (_rng.NextDouble() < expected)
        {
            var size = 1.2 + _rng.NextDouble() * 1.6;
            var speck = new AirSpeck
            {
                E = new Ellipse { Width = size, Height = size, Fill = new SolidColorBrush(Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF)) },
                R = 22 + _rng.NextDouble() * 8,
                Theta = _rng.NextDouble() * Math.PI * 2,
                Speed = 0.6 + _rng.NextDouble() * 0.9,
                Alpha = 0.35 + _rng.NextDouble() * 0.5,
            };
            _air.Add(speck);
            canvas.Children.Add(speck.E);
            expected -= 1;
        }

        for (var i = _air.Count - 1; i >= 0; i--)
        {
            var p = _air[i];
            p.R += p.Speed * (0.4 + 2.2 * s);
            p.Theta += 0.035 * s;
            if (p.R > 66)
            {
                canvas.Children.Remove(p.E);
                _air.RemoveAt(i);
                continue;
            }
            var fade = Math.Min(1, (p.R - 22) / 10) * Math.Min(1, (66 - p.R) / 14);
            p.E.Opacity = p.Alpha * fade * Math.Max(0.15, s);
            Canvas.SetLeft(p.E, CenterX + p.R * Math.Cos(p.Theta) - p.E.Width / 2);
            Canvas.SetTop(p.E, CenterY + p.R * Math.Sin(p.Theta) - p.E.Height / 2);
        }
    }

    private sealed class AirSpeck
    {
        public Ellipse E = null!;
        public double R, Theta, Speed, Alpha;
    }
}

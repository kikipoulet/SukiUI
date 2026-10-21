using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Stirrer: motor inertia, vortex surface, swirling particles, stir bar and display.
public partial class Stirrer
{
    private const double W = 92, H = 106, LiquidMaxH = 92, BarLength = 30, MaxRadPerFrame = 0.55;
    private const int ParticleCount = 18;

    private readonly Random _rng = new();
    private readonly List<Swirl> _swirls = new();
    private DispatcherTimer _timer = null!;
    private double _shown, _barAngle, _shownRpm = -1;
    private long _tick;
    private bool _ledOn;

    private void InitializeRendering()
    {
        SeedParticles();
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree      += (_, _) => { ApplyGlow(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
        ActualThemeVariantChanged += (_, _) => ApplyGlow();
    }

    private void Tick()
    {
        _tick++;
        // Inertia: the motor and the liquid take time to spin up and to calm down
        _shown += (Speed - _shown) * 0.03;
        if (Math.Abs(Speed - _shown) < 0.001) _shown = Speed;
        var s = _shown;

        var omega = s * MaxRadPerFrame;
        _barAngle += omega;

        // Vortex: the center digs, the liquid climbs along the walls, a light ripple keeps it alive
        var y0 = H - Level * LiquidMaxH;
        var depth = s * Math.Min(34, Level * LiquidMaxH * 0.55);
        var rise = depth * 0.3;
        var ripple = Math.Sin(_tick * 0.09) * (0.6 + 1.2 * s);
        var surfaceAt = BuildLiquid(y0, depth, rise, ripple);

        // Stir bar seen side-on: its apparent length follows the rotation
        var bar = this.Get<Border>("StirBar");
        var len = Math.Max(6, BarLength * Math.Abs(Math.Cos(_barAngle)));
        bar.Width = len;
        Canvas.SetLeft(bar, W / 2 - len / 2);

        UpdateParticles(omega, surfaceAt);

        this.Get<Border>("FieldGlow").Opacity = 0.9 * s;
        var running = s > 0.01;
        if (running != _ledOn) ApplyLed(running);
        this.Get<Ellipse>("Led").Opacity = running ? 0.6 + 0.4 * Math.Sin(_tick * 0.1) : 1;

        var rpm = Math.Round(s * MaxRpm / 10) * 10;
        if (rpm != _shownRpm)
        {
            _shownRpm = rpm;
            this.Get<TextBlock>("RpmText").Text = $"{rpm:0}";
        }
    }

    // Returns the surface height for a given x, used to keep the particles in the liquid
    private Func<double, double> BuildLiquid(double y0, double depth, double rise, double ripple)
    {
        var top = y0 - rise;
        var bottom = y0 + depth;
        var surface = new StreamGeometry();
        using (var ctx = surface.Open())
        {
            ctx.BeginFigure(new Point(0, top + ripple * 0.4), false);
            ctx.CubicBezierTo(new Point(W * 0.3, top + ripple), new Point(W * 0.38, bottom), new Point(W / 2, bottom));
            ctx.CubicBezierTo(new Point(W * 0.62, bottom), new Point(W * 0.7, top - ripple), new Point(W, top - ripple * 0.4));
            ctx.EndFigure(false);
        }
        var body = new StreamGeometry();
        using (var ctx = body.Open())
        {
            ctx.BeginFigure(new Point(0, top + ripple * 0.4), true);
            ctx.CubicBezierTo(new Point(W * 0.3, top + ripple), new Point(W * 0.38, bottom), new Point(W / 2, bottom));
            ctx.CubicBezierTo(new Point(W * 0.62, bottom), new Point(W * 0.7, top - ripple), new Point(W, top - ripple * 0.4));
            ctx.LineTo(new Point(W, H));
            ctx.LineTo(new Point(0, H));
            ctx.EndFigure(true);
        }
        this.Get<Path>("Liquid").Data = body;
        this.Get<Path>("LiquidShade").Data = body;
        this.Get<Path>("Surface").Data = surface;
        this.Get<Path>("Liquid").IsVisible = Level > 0.01;

        // Approximation of the surface: a smooth bowl between the walls and the center
        return x =>
        {
            var u = Math.Abs(x - W / 2) / (W / 2);
            return bottom + (top - bottom) * Math.Pow(u, 1.6);
        };
    }

    // Particles turning around the axis: depth gives size and brightness, inner ones turn faster
    private void UpdateParticles(double omega, Func<double, double> surfaceAt)
    {
        foreach (var p in _swirls)
        {
            p.Phi += omega * (0.6 + 0.8 * (1 - p.R / 42)) + 0.004;
            // Slow vertical drift, stirred harder when the bar spins
            p.Y += Math.Sin(_tick * 0.02 + p.Phase) * 0.08 + (_rng.NextDouble() - 0.5) * omega * 0.8;
            var x = W / 2 + p.R * Math.Cos(p.Phi);
            var z = Math.Sin(p.Phi);                              // -1 back .. 1 front
            var top = surfaceAt(x) + 4;
            if (p.Y < top) p.Y = top;
            if (p.Y > H - 6) p.Y = H - 6 - _rng.NextDouble() * 4;

            var size = p.Size * (0.75 + 0.35 * z);
            p.E.Width = p.E.Height = size;
            p.E.Opacity = p.Alpha * (0.45 + 0.55 * (z + 1) / 2) * (Level > 0.05 ? 1 : 0);
            Canvas.SetLeft(p.E, x - size / 2);
            Canvas.SetTop(p.E, p.Y - size / 2);
        }
    }

    private void SeedParticles()
    {
        var canvas = this.Get<Canvas>("Particles");
        for (var i = 0; i < ParticleCount; i++)
        {
            var dark = i % 5 == 0;
            var p = new Swirl
            {
                E = new Ellipse { Fill = new SolidColorBrush(dark ? Color.FromArgb(0x80, 0x05, 0x08, 0x0D) : Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)) },
                R = 6 + _rng.NextDouble() * 36,
                Phi = _rng.NextDouble() * Math.PI * 2,
                Y = H - 10 - _rng.NextDouble() * 50,
                Size = dark ? 1.4 + _rng.NextDouble() : 1.6 + _rng.NextDouble() * 1.8,
                Alpha = dark ? 0.7 : 0.4 + _rng.NextDouble() * 0.5,
                Phase = _rng.NextDouble() * Math.PI * 2,
            };
            _swirls.Add(p);
            canvas.Children.Add(p.E);
        }
    }

    private void ApplyGlow()
    {
        if (!SkeumoMotion.TryAccent(this, out var a)) return;
        this.Get<Border>("FieldGlow").BoxShadow = new BoxShadows(
            new BoxShadow { Blur = 18, Spread = 4, Color = Color.FromArgb(0xA0, a.R, a.G, a.B) },
            new[] { new BoxShadow { Blur = 36, Spread = 10, Color = Color.FromArgb(0x40, a.R, a.G, a.B) } });
        ApplyLed(_shown > 0.01);
    }

    private void ApplyLed(bool on)
    {
        _ledOn = on;
        var led = this.Get<Ellipse>("Led");
        if (on && SkeumoMotion.TryAccent(this, out var a)) led.Fill = new SolidColorBrush(a);
        else led.BindBrush(Shape.FillProperty, this, "SkLedOff");
    }

    private sealed class Swirl
    {
        public Ellipse E = null!;
        public double R, Phi, Y, Size, Alpha, Phase;
    }
}

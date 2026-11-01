using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Tank: liquid, suspended particles, glow and pipe flows.
public partial class Tank
{
    // Interior of the vessel (local frame of the clipped interior canvas, origin at scene (36,36)).
    private const double InteriorW = 96, InteriorH = 126, InteriorTop = 36, LiquidMaxH = 112, FallX = 48;
    private const int TankParticleCount = 28;

    private static readonly Route OutletRoute = new Route(new Point(96, 160))
        .LineTo(new Point(96, 180)).QuadTo(new Point(96, 192), new Point(108, 192)).LineTo(new Point(288, 192));

    private static readonly Route InletRoute = new Route(new Point(0, 16))
        .LineTo(new Point(72, 16)).QuadTo(new Point(84, 16), new Point(84, 28)).LineTo(new Point(84, 30));

    private readonly Random _rng = new();
    private readonly List<Speck> _tankSpecks = new();
    private readonly List<PipeSpeck> _outletSpecks = new();
    private readonly List<PipeSpeck> _inletSpecks = new();
    private readonly List<Speck> _fallSpecks = new();

    private DispatcherTimer _timer = null!;
    private Canvas _tankCanvas = null!, _outletCanvas = null!, _inletCanvas = null!;
    private IBrush _brightSpeck = Brushes.White, _darkSpeck = Brushes.Black;
    private double _shownIn, _shownOut;
    private long _tick;

    private void InitializeRendering()
    {
        _tankCanvas = this.Get<Canvas>("TankParticles");
        _outletCanvas = this.Get<Canvas>("OutletParticles");
        _inletCanvas = this.Get<Canvas>("InletParticles");
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());

        SeedTankParticles();

        AttachedToVisualTree      += (_, _) => { ApplyTheme(); RenderLevel(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
        ActualThemeVariantChanged += (_, _) => ApplyTheme();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LevelProperty) RenderLevel();
    }

    // ------------------------------------------------------------------ level

    private double LiquidHeight => Level * LiquidMaxH;
    private double SurfaceY => InteriorH - LiquidHeight;

    private void RenderLevel()
    {
        var h = LiquidHeight;
        var liquid = this.Get<Border>("Liquid");
        liquid.Height = h;
        Canvas.SetTop(liquid, InteriorH - h);

        var aura = this.Get<Border>("Aura");
        aura.Height = Math.Max(0, h - 4);
        Canvas.SetTop(aura, InteriorTop + InteriorH - aura.Height);
        aura.Opacity = h > 8 ? 0.9 : 0;
    }

    // ------------------------------------------------------------------ animation loop

    private void Tick()
    {
        _tick++;

        // The flows fade in and out, they never switch abruptly.
        _shownIn = Approach(_shownIn, Inflow);
        _shownOut = Approach(_shownOut, Outflow);

        this.Get<Path>("OutletFlow").Opacity = 0.85 * _shownOut;
        this.Get<Path>("InletFlow").Opacity = 0.85 * _shownIn;
        var fall = this.Get<Line>("FallStream");
        fall.Opacity = 0.8 * _shownIn;
        fall.StrokeThickness = 1.5 + 2.5 * _shownIn;
        fall.EndPoint = new Point(FallX, SurfaceY + 2);

        UpdatePipe(_outletSpecks, _outletCanvas, OutletRoute, _shownOut, onExit: null);
        UpdatePipe(_inletSpecks, _inletCanvas, InletRoute, _shownIn, onExit: SpawnFallSpeck);
        UpdateFall();
        UpdateTank();
    }

    private static double Approach(double shown, double target)
    {
        shown += (target - shown) * 0.12;
        return Math.Abs(target - shown) < 0.002 ? target : shown;
    }

    // Suspended matter: slow drift, bounce on the walls, always below the surface.
    private void UpdateTank()
    {
        var surface = SurfaceY;
        var visible = LiquidHeight > 8;
        var stir = 1 + 3 * (_shownIn + _shownOut);

        foreach (var p in _tankSpecks)
        {
            p.E.IsVisible = visible;
            if (!visible) continue;

            p.X += (p.Vx + 0.05 * Math.Sin(_tick * 0.02 + p.Phase)) * stir + (p.X < 60 ? 0.05 * _shownOut : 0);
            p.Y += (p.Vy + 0.04 * Math.Cos(_tick * 0.017 + p.Phase * 1.3)) * stir + 0.03 * _shownOut;

            if (p.X < 4) { p.X = 4; p.Vx = Math.Abs(p.Vx); }
            else if (p.X > InteriorW - 4) { p.X = InteriorW - 4; p.Vx = -Math.Abs(p.Vx); }

            var top = surface + 3;
            if (p.Y < top) { p.Y = top; p.Vy = Math.Abs(p.Vy); }
            else if (p.Y > InteriorH - 4) { p.Y = InteriorH - 4; p.Vy = -Math.Abs(p.Vy); }

            // Fade out right under the surface so particles never "touch" it.
            p.E.Opacity = p.Alpha * Math.Clamp((p.Y - surface) / 8, 0, 1);
            Canvas.SetLeft(p.E, p.X - p.E.Width / 2);
            Canvas.SetTop(p.E, p.Y - p.E.Height / 2);
        }
    }

    // Particles carried by a pipe flow: random spawn, speed, lateral offset and wobble, so they never form a queue.
    private void UpdatePipe(List<PipeSpeck> specks, Canvas canvas, Route route, double flow, Action? onExit)
    {
        if (flow > 0.02)
        {
            var expected = 0.9 * flow;                           // Poisson-like spawning
            while (_rng.NextDouble() < expected)
            {
                specks.Add(CreatePipeSpeck(canvas));
                expected -= 1;
            }
        }

        for (var i = specks.Count - 1; i >= 0; i--)
        {
            var p = specks[i];
            p.S += p.Speed * (0.25 + 3.2 * flow);
            p.Offset += (_rng.NextDouble() - 0.5) * 0.35;        // brownian drift across the pipe
            p.Offset = Math.Clamp(p.Offset, -2.6, 2.6);

            var exited = p.S >= route.Length;
            if (flow < 0.02) p.Fade -= 0.05;
            if (exited || p.Fade <= 0)
            {
                canvas.Children.Remove(p.E);
                specks.RemoveAt(i);
                if (exited) onExit?.Invoke();
                continue;
            }

            var (pos, normal) = route.At(p.S);
            var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
            p.E.Opacity = p.Alpha * Math.Min(1, p.S / 8) * p.Fade;
            Canvas.SetLeft(p.E, pos.X + normal.X * lateral - p.E.Width / 2);
            Canvas.SetTop(p.E, pos.Y + normal.Y * lateral - p.E.Height / 2);
        }
    }

    // Particles falling from the inlet into the liquid.
    private void UpdateFall()
    {
        var surface = SurfaceY;
        for (var i = _fallSpecks.Count - 1; i >= 0; i--)
        {
            var p = _fallSpecks[i];
            p.Vy += 0.12;
            p.Y += p.Vy;
            p.X += p.Vx;
            if (p.Y >= surface)
            {
                _tankCanvas.Children.Remove(p.E);
                _fallSpecks.RemoveAt(i);
                continue;
            }
            p.E.Opacity = p.Alpha;
            Canvas.SetLeft(p.E, p.X - p.E.Width / 2);
            Canvas.SetTop(p.E, p.Y - p.E.Height / 2);
        }
    }

    // ------------------------------------------------------------------ particles

    private void SeedTankParticles()
    {
        for (var i = 0; i < TankParticleCount; i++)
        {
            var dark = i % 5 == 0;                               // a few impurities among the bubbles
            var size = dark ? 1.2 + _rng.NextDouble() * 1.2 : 1.4 + _rng.NextDouble() * 2.2;
            var p = new Speck
            {
                E = new Ellipse { Width = size, Height = size },
                Dark = dark,
                X = 6 + _rng.NextDouble() * (InteriorW - 12),
                Y = InteriorH - _rng.NextDouble() * LiquidMaxH,
                Vx = (_rng.NextDouble() - 0.5) * 0.12,
                Vy = (_rng.NextDouble() - 0.5) * 0.08,
                Phase = _rng.NextDouble() * Math.PI * 2,
                Alpha = dark ? 0.7 : 0.35 + _rng.NextDouble() * 0.5,
            };
            p.E.Fill = dark ? _darkSpeck : _brightSpeck;
            _tankSpecks.Add(p);
            _tankCanvas.Children.Add(p.E);
        }
    }

    private PipeSpeck CreatePipeSpeck(Canvas canvas)
    {
        var dark = _rng.NextDouble() < 0.2;
        var size = 1.2 + _rng.NextDouble() * 1.6;
        var p = new PipeSpeck
        {
            E = new Ellipse { Width = size, Height = size, Opacity = 0 },
            Dark = dark,
            S = _rng.NextDouble() * 4,
            Speed = 0.55 + _rng.NextDouble() * 0.9,
            Offset = (_rng.NextDouble() * 2 - 1) * 2.4,
            Phase = _rng.NextDouble() * Math.PI * 2,
            WobbleLength = 4 + _rng.NextDouble() * 8,
            Alpha = dark ? 0.75 : 0.45 + _rng.NextDouble() * 0.5,
        };
        p.E.Fill = dark ? _darkSpeck : _brightSpeck;
        canvas.Children.Add(p.E);
        return p;
    }

    private void SpawnFallSpeck()
    {
        var dark = _rng.NextDouble() < 0.2;
        var size = 1.2 + _rng.NextDouble() * 1.4;
        var p = new Speck
        {
            E = new Ellipse { Width = size, Height = size },
            Dark = dark,
            X = FallX + (_rng.NextDouble() - 0.5) * 2.4,
            Y = 0,
            Vx = (_rng.NextDouble() - 0.5) * 0.15,
            Vy = 0.8 + _rng.NextDouble() * 0.8,
            Alpha = dark ? 0.75 : 0.5 + _rng.NextDouble() * 0.4,
        };
        p.E.Fill = dark ? _darkSpeck : _brightSpeck;
        _fallSpecks.Add(p);
        _tankCanvas.Children.Add(p.E);
    }

    // ------------------------------------------------------------------ theme

    private void ApplyTheme()
    {
        var isDark = ActualThemeVariant == ThemeVariant.Dark;
        _brightSpeck = new SolidColorBrush(Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF));
        _darkSpeck = new SolidColorBrush(isDark ? Color.FromArgb(0x99, 0x05, 0x08, 0x0D) : Color.FromArgb(0x80, 0x28, 0x32, 0x50));
        foreach (var p in _tankSpecks) p.E.Fill = p.Dark ? _darkSpeck : _brightSpeck;
        foreach (var p in _fallSpecks) p.E.Fill = p.Dark ? _darkSpeck : _brightSpeck;
        foreach (var p in _outletSpecks) p.E.Fill = p.Dark ? _darkSpeck : _brightSpeck;
        foreach (var p in _inletSpecks) p.E.Fill = p.Dark ? _darkSpeck : _brightSpeck;

        if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color a) return;
        this.Get<Border>("Aura").BoxShadow = new BoxShadows(
            new BoxShadow { Blur = 18, Spread = 4, Color = Color.FromArgb(0x90, a.R, a.G, a.B) },
            new[] { new BoxShadow { Blur = 42, Spread = 10, Color = Color.FromArgb(0x40, a.R, a.G, a.B) } });
    }

    // ------------------------------------------------------------------ helpers

    private sealed class Speck
    {
        public Ellipse E = null!;
        public bool Dark;
        public double X, Y, Vx, Vy, Phase, Alpha;
    }

    private sealed class PipeSpeck
    {
        public Ellipse E = null!;
        public bool Dark;
        public double S, Speed, Offset, Phase, WobbleLength, Alpha, Fade = 1;
    }

    /// Flattened pipe centerline (same geometry as the XAML paths), walkable by distance.
    private sealed class Route
    {
        private readonly List<Point> _pts = new();
        private readonly List<double> _cum = new() { 0 };

        public Route(Point start) => _pts.Add(start);

        public double Length => _cum[^1];

        public Route LineTo(Point p) => Add(p);

        public Route QuadTo(Point control, Point end)
        {
            var s = _pts[^1];
            for (var i = 1; i <= 12; i++)
            {
                var t = i / 12.0;
                var u = 1 - t;
                Add(new Point(u * u * s.X + 2 * u * t * control.X + t * t * end.X,
                              u * u * s.Y + 2 * u * t * control.Y + t * t * end.Y));
            }
            return this;
        }

        private Route Add(Point p)
        {
            var last = _pts[^1];
            _cum.Add(_cum[^1] + Math.Sqrt((p.X - last.X) * (p.X - last.X) + (p.Y - last.Y) * (p.Y - last.Y)));
            _pts.Add(p);
            return this;
        }

        /// Position and unit normal at a distance along the route.
        public (Point Pos, Vector Normal) At(double s)
        {
            var i = 1;
            while (i < _cum.Count - 1 && _cum[i] < s) i++;
            var a = _pts[i - 1];
            var b = _pts[i];
            var len = _cum[i] - _cum[i - 1];
            var t = len > 0 ? Math.Clamp((s - _cum[i - 1]) / len, 0, 1) : 0;
            var dir = len > 0 ? new Vector((b.X - a.X) / len, (b.Y - a.Y) / len) : new Vector(1, 0);
            return (new Point(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t), new Vector(-dir.Y, dir.X));
        }
    }
}

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Camera: beam, lens glow, chamber light, particles and data pulses.
public partial class Camera
{
    // Beam: from the lens (x = 166) to the cell face (x = 92), on the axis y = 57
    private const double BeamX0 = 166, BeamX1 = 92, AxisY = 57;

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly long _t0 = Environment.TickCount64;
    private bool _ready;

    private void InitializeRendering()
    {
        _timer.Tick += (_, _) => Tick();
        AttachedToVisualTree      += (_, _) => { ApplyAccentGlows(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
        ActualThemeVariantChanged += (_, _) => ApplyAccentGlows();

        _ready = true;
        Render();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_ready && (change.Property == ExposureProperty || change.Property == GainProperty))
            Render();
    }

    private void ApplyAccentGlows()
    {
        if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color accent) return;
        this.Get<Border>("FrontRing").BoxShadow =
            new BoxShadows(new BoxShadow { Blur = 6, Color = Color.FromArgb(0x99, accent.R, accent.G, accent.B) });
    }

    private void Render()
    {
        // Light: only the exposure drives it; the gain only acts on the particles (brightness + pixelation)
        var lum = Math.Clamp(0.5 + Math.Log2(Math.Max(Exposure, 1) / 600) * 0.16, 0.05, 1);
        var half = 6 + lum * 34;

        var beam = this.Get<Polygon>("Beam");
        beam.Points = new Points
        {
            new Point(BeamX0, AxisY - 9),
            new Point(BeamX1, AxisY - half),
            new Point(BeamX1, AxisY + half),
            new Point(BeamX0, AxisY + 9)
        };
        beam.Opacity = 0.15 + lum * 0.75;
        this.Get<Ellipse>("LensGlow").Opacity = 0.2 + lum * 0.8;
        this.Get<Border>("Liq").Opacity = 0.35 + lum * 0.65;
        this.Get<Border>("Inl").Opacity = 0.1 + lum * 0.9;

        var particles = this.Get<CellParticles>("Particles");
        particles.Light = lum;
        particles.Gain = Gain;
    }

    private void Tick()
    {
        // Data pulses: one every 20 frames
        var period = Math.Max(1000.0 / Math.Max(Fps, 0.1) * 20, 200);
        var p = (Environment.TickCount64 - _t0) % (long)period / period;
        var pt = CablePoint(p);
        var pulse = this.Get<Ellipse>("Pulse");
        Canvas.SetLeft(pulse, pt.X - 3.5);
        Canvas.SetTop(pulse, pt.Y - 3.5);
        pulse.Opacity = Math.Sin(p * Math.PI);

        this.Get<CellParticles>("Particles").Advance();
    }

    // Bezier curve of the cable (M156,112 C170,112 170,138 142,144 S100,146 68,146), camera frame
    private static Point CablePoint(double p) => p < 0.5
        ? Bezier(new Point(156, 112), new Point(170, 112), new Point(170, 138), new Point(142, 144), p * 2)
        : Bezier(new Point(142, 144), new Point(114, 150), new Point(100, 146), new Point(68, 146), (p - 0.5) * 2);

    private static Point Bezier(Point a, Point b, Point c, Point d, double t)
    {
        var u = 1 - t;
        return new Point(
            u * u * u * a.X + 3 * u * u * t * b.X + 3 * u * t * t * c.X + t * t * t * d.X,
            u * u * u * a.Y + 3 * u * u * t * b.Y + 3 * u * t * t * c.Y + t * t * t * d.Y);
    }
}

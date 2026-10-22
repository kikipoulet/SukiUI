using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Rendering piece of the <see cref="Camera"/>: particles suspended in the cell chamber.
/// Their brightness follows the light (exposure) multiplied by the gain; the gain also pixelates them
/// (low resolution render scaled up without smoothing).
/// </summary>
public class CellParticles : Control
{
    private const int Count = 38;
    private const double MaxPixel = 4.4;

    public static readonly StyledProperty<double> LightProperty =
        AvaloniaProperty.Register<CellParticles, double>(nameof(Light), 0.5);

    public static readonly StyledProperty<double> GainProperty =
        AvaloniaProperty.Register<CellParticles, double>(nameof(Gain), 1);

    /// <summary>Illumination 0..1 (from the exposure only).</summary>
    public double Light { get => GetValue(LightProperty); set => SetValue(LightProperty, value); }

    /// <summary>Gain 1..8: brightens and pixelates.</summary>
    public double Gain { get => GetValue(GainProperty); set => SetValue(GainProperty, value); }

    private struct Particle
    {
        public double X, Y, R, B, V;
    }

    private readonly Particle[] _p = new Particle[Count];
    private readonly Random _rng = new();
    private RenderTargetBitmap? _low;
    private bool _seeded;

    static CellParticles()
    {
        AffectsRender<CellParticles>(LightProperty, GainProperty);
    }

    public CellParticles()
    {
        IsHitTestVisible = false;
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
    }

    private void Seed(double w, double h)
    {
        for (var i = 0; i < Count; i++)
            _p[i] = Spawn(w, h, _rng.NextDouble() * h);
        _seeded = true;
    }

    private Particle Spawn(double w, double h, double y) => new()
    {
        X = 4 + _rng.NextDouble() * Math.Max(1, w - 8),
        Y = y,
        R = 0.7 + Math.Pow(_rng.NextDouble(), 2) * 2.0,
        B = 0.5 + _rng.NextDouble() * 0.5,
        V = 0.04 + _rng.NextDouble() * 0.1
    };

    /// <summary>Advances the simulation by one frame (called by the camera timer).</summary>
    public void Advance()
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;
        if (!_seeded) Seed(w, h);
        for (var i = 0; i < Count; i++)
        {
            ref var p = ref _p[i];
            p.Y += p.V;
            p.X = Math.Clamp(p.X + (_rng.NextDouble() - 0.5) * 0.08, 3, w - 3);
            if (p.Y > h + 3) p = Spawn(w, h, -3);
        }
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;
        if (!_seeded) Seed(w, h);

        var px = 0.2 + (Math.Clamp(Gain, 1, 8) - 1) / 7 * (MaxPixel - 0.2);
        if (px <= 0.4)
        {
            Draw(context);
            return;
        }

        var lw = Math.Max(2, (int)Math.Ceiling(w / px));
        var lh = Math.Max(2, (int)Math.Ceiling(h / px));
        _low = new RenderTargetBitmap(new PixelSize(lw, lh), new Vector(96, 96));
        using (var dc = _low.CreateDrawingContext())
        using (dc.PushTransform(Matrix.CreateScale(lw / w, lh / h)))
            Draw(dc);
        context.DrawImage(_low, new Rect(0, 0, lw, lh), new Rect(0, 0, w, h));
    }

    private void Draw(DrawingContext dc)
    {
        var basis = 0.06 + 0.5 * Math.Clamp(Light, 0, 1);
        var gainFactor = Math.Pow(Math.Max(Gain, 1), 0.6);
        foreach (var p in _p)
        {
            var a = Math.Min(1, basis * gainFactor * p.B * 1.3);
            var center = new Point(p.X, p.Y);
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(a * 36), 150, 185, 255)), null, center, p.R * 2.4, p.R * 2.4);
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(a * 230), 200, 222, 255)), null, center, p.R, p.R);
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb((byte)(a * 255), 255, 255, 255)), null,
                new Point(p.X - p.R * 0.2, p.Y - p.R * 0.2), p.R * 0.55, p.R * 0.55);
        }
    }
}

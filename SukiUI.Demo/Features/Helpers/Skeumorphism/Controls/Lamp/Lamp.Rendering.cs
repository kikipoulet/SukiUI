using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the Lamp: thermal inertia, filament glow, then bulb light and spread, in real incandescent colours.
public partial class Lamp
{
    private const double GlowEnd = 0.35, LightStart = 0.25;
    private static readonly Color FilamentGlow = Color.Parse("#FFFF9A40");

    private DispatcherTimer _timer = null!;
    private double _heat = -1;
    private long _tick;

    private void InitializeRendering()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        AttachedToVisualTree      += (_, _) => { ApplyGlow(); _timer.Start(); };
        DetachedFromVisualTree    += (_, _) => _timer.Stop();
        ActualThemeVariantChanged += (_, _) => ApplyGlow();
    }

    private void Tick()
    {
        _tick++;
        if (_heat < 0) _heat = Intensity;
        // Thermal inertia: the filament heats up a bit faster than it cools down
        var k = Intensity > _heat ? 0.07 : 0.045;
        _heat += (Intensity - _heat) * k;

        // 1. The filament glows first...
        var glow = Smooth(Math.Clamp(_heat / GlowEnd, 0, 1));
        // 2. ...then the bulb lights up and the light spreads
        var light = Smooth(Math.Clamp((_heat - LightStart) / (1 - LightStart), 0, 1));
        // A barely visible flicker keeps the light alive
        var flicker = 1 + 0.015 * Math.Sin(_tick * 0.7) * light;

        this.Get<Path>("FilamentHeat").Opacity = glow;
        this.Get<Path>("FilamentCold").Opacity = 1 - 0.7 * glow;
        this.Get<Path>("FilamentCore").Opacity = light;
        this.Get<Path>("BulbGlow").Opacity = 0.75 * light * flicker;
        this.Get<Ellipse>("Spread").Opacity = 0.55 * light * flicker;
    }

    private static double Smooth(double t) => t * t * (3 - 2 * t);

    // Real incandescent light: the filament glow is orange, not the accent
    private void ApplyGlow() =>
        this.Get<Path>("FilamentHeat").Effect = new DropShadowEffect { Color = FilamentGlow, BlurRadius = 8, OffsetX = 0, OffsetY = 0, Opacity = 1 };
}

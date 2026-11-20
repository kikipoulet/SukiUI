using System;
using Avalonia;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct LampState(double Intensity);

/// <summary>
/// Skeumo-Suki filament bulb on a screw socket. Display only: at low power the filament glows,
/// then the bulb lights up and the light spreads around it. The filament has thermal inertia.
/// The light uses real incandescent colours (a tolerated real-world colour, like a status LED), not the accent.
/// Drawing and animation live in <c>Lamp.Rendering.cs</c>.
/// </summary>
public partial class Lamp : HyperControl<LampState>
{
    /// <summary>Power reported by the dimmer: 0 = off .. 1 = full brightness.</summary>
    public static readonly StyledProperty<double> IntensityProperty =
        AvaloniaProperty.Register<Lamp, double>(nameof(Intensity), coerce: (_, v) => Math.Clamp(v, 0, 1));

    static Lamp() => AffectsRenderState(IntensityProperty);

    public Lamp() => IsHitTestVisible = false;

    public double Intensity
    {
        get => GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    protected override LampState CaptureState() => new(Intensity);
}

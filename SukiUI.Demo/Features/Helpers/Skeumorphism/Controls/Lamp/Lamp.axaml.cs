using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki filament bulb on a screw socket. Display only: at low power the filament glows,
/// then the bulb lights up and the light spreads around it. The filament has thermal inertia.
/// The light uses real incandescent colours (a tolerated real-world colour, like a status LED), not the accent.
/// Drawing and animation live in <c>Lamp.Rendering.cs</c>.
/// </summary>
public partial class Lamp : UserControl
{
    /// <summary>Power reported by the dimmer: 0 = off .. 1 = full brightness.</summary>
    public static readonly StyledProperty<double> IntensityProperty =
        AvaloniaProperty.Register<Lamp, double>(nameof(Intensity), coerce: (_, v) => Math.Clamp(v, 0, 1));

    public double Intensity
    {
        get => GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    public Lamp()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

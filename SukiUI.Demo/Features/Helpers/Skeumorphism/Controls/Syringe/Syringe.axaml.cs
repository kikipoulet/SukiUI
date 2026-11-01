using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki syringe pump: glass barrel, plunger and a two-way valve (In / Out) on top.
/// It displays the state reported by the device and reports the user's intent on its own parts:
/// dragging the plunger previews a position and requests it on release, clicking the valve requests a toggle.
/// The host decides whether to honour a request; a refused position snaps back to <see cref="Position"/>.
/// Drawing and animation live in <c>Syringe.Rendering.cs</c>.
/// </summary>
public partial class Syringe : UserControl
{
    /// <summary>Plunger position reported by the device: 0 = empty .. 1 = full.</summary>
    public static readonly StyledProperty<double> PositionProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Position), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>
    /// Liquid flowing through the valve: positive = filling (towards the barrel), negative = emptying,
    /// 0 = still. The magnitude (0..1) is the speed of the stream.
    /// </summary>
    public static readonly StyledProperty<double> FlowProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Flow), coerce: (_, v) => Math.Clamp(v, -1, 1));

    /// <summary>Valve position reported by the device: false = In, true = Out.</summary>
    public static readonly StyledProperty<bool> IsValveOutProperty =
        AvaloniaProperty.Register<Syringe, bool>(nameof(IsValveOut));

    /// <summary>Barrel volume in mL, used for the scale labels.</summary>
    public static readonly StyledProperty<double> VolumeProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Volume), 10);

    public double Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public double Flow
    {
        get => GetValue(FlowProperty);
        set => SetValue(FlowProperty, value);
    }

    public bool IsValveOut
    {
        get => GetValue(IsValveOutProperty);
        set => SetValue(IsValveOutProperty, value);
    }

    public double Volume
    {
        get => GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }

    /// <summary>Raised when the user releases the plunger: requested position (0..1).</summary>
    public event EventHandler<double>? PositionRequested;

    /// <summary>Raised when the user clicks the valve: the host may switch <see cref="IsValveOut"/>.</summary>
    public event EventHandler? ValveToggleRequested;

    public Syringe()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

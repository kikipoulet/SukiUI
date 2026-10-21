using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>Insulation colour of a <see cref="Wire"/>.</summary>
public enum WireInsulation
{
    Neutral,
    Black,
    Red
}

/// <summary>
/// Skeumo-Suki insulated wire linking two parts of a scene (a switch and the device it powers, for instance).
/// Display only: when current flows, a faint highlight drifts slowly along the wire (stronger with the current); a command is carried by a single impulse sent on each <see cref="Trigger"/> change. The start can melt into the card (a supply coming from outside).
/// The geometry is expressed in the coordinates of the host panel: size the control like that panel.
/// Drawing and animation live in <c>Wire.Rendering.cs</c>.
/// </summary>
public partial class Wire : UserControl
{
    /// <summary>Path of the wire, from where the current comes from to where it goes.</summary>
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<Wire, Geometry?>(nameof(Data));

    public static readonly StyledProperty<WireInsulation> InsulationProperty =
        AvaloniaProperty.Register<Wire, WireInsulation>(nameof(Insulation));

    /// <summary>Current reported by the circuit: 0 = none .. 1 = full load.</summary>
    public static readonly StyledProperty<double> CurrentProperty =
        AvaloniaProperty.Register<Wire, double>(nameof(Current), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>When true, the start of the wire fades into the card (a cable coming from outside the scene).</summary>
    public static readonly StyledProperty<bool> FadeStartProperty =
        AvaloniaProperty.Register<Wire, bool>(nameof(FadeStart));

    /// <summary>
    /// Event-driven signal: every time this value changes, one electric impulse travels along the wire.
    /// Bind it to the command the wire carries (a setting, a selected index); it never pulses on its own.
    /// </summary>
    public static readonly StyledProperty<double> TriggerProperty =
        AvaloniaProperty.Register<Wire, double>(nameof(Trigger));

    public double Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public WireInsulation Insulation
    {
        get => GetValue(InsulationProperty);
        set => SetValue(InsulationProperty, value);
    }

    public double Current
    {
        get => GetValue(CurrentProperty);
        set => SetValue(CurrentProperty, value);
    }

    public bool FadeStart
    {
        get => GetValue(FadeStartProperty);
        set => SetValue(FadeStartProperty, value);
    }

    public Wire()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

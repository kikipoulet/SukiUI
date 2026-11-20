using System;
using Avalonia;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct TankState(
    double Level,
    double Inflow,
    double Outflow,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki glass tank with an inlet on top and an outlet at the bottom.
/// Display only: it shows the level and the flows it receives. Drawing and animation live in <c>Tank.Rendering.cs</c>.
/// </summary>
public partial class Tank : HyperControl<TankState>
{
    /// <summary>Liquid level (0 = empty .. 1 = full).</summary>
    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Level), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow entering through the top inlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> InflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Inflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Flow leaving through the bottom outlet (0 = closed .. 1 = full flow).</summary>
    public static readonly StyledProperty<double> OutflowProperty =
        AvaloniaProperty.Register<Tank, double>(nameof(Outflow), coerce: (_, v) => Math.Clamp(v, 0, 1));

    static Tank() => AffectsRenderState(LevelProperty, InflowProperty, OutflowProperty);

    public Tank() => IsHitTestVisible = false;

    public double Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public double Inflow
    {
        get => GetValue(InflowProperty);
        set => SetValue(InflowProperty, value);
    }

    public double Outflow
    {
        get => GetValue(OutflowProperty);
        set => SetValue(OutflowProperty, value);
    }

    protected override TankState CaptureState() =>
        new(Level, Inflow, Outflow, SkeumoSkia.CapturePalette(this));
}

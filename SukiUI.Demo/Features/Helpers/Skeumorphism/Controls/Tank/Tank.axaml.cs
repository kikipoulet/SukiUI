using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki glass tank with an inlet on top and an outlet at the bottom.
/// Display only: it shows the level and the flows it receives. Drawing and animation live in <c>Tank.Rendering.cs</c>.
/// </summary>
public partial class Tank : UserControl
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

    public Tank()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

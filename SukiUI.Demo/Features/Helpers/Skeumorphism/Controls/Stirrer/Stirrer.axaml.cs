using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki magnetic stirrer with a glass beaker. Display only: the stir bar spins at the reported speed
/// (with inertia), the vortex digs deeper, particles swirl and the display shows the rpm.
/// Drawing and animation live in <c>Stirrer.Rendering.cs</c>.
/// </summary>
public partial class Stirrer : UserControl
{
    /// <summary>Stir speed reported by the device: 0 = stopped .. 1 = full speed.</summary>
    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<Stirrer, double>(nameof(Speed), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Liquid level in the beaker: 0 = empty .. 1 = full.</summary>
    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<Stirrer, double>(nameof(Level), 0.65, coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>Speed shown on the display at full speed, in rpm.</summary>
    public static readonly StyledProperty<double> MaxRpmProperty =
        AvaloniaProperty.Register<Stirrer, double>(nameof(MaxRpm), 1500);

    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public double Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public double MaxRpm
    {
        get => GetValue(MaxRpmProperty);
        set => SetValue(MaxRpmProperty, value);
    }

    public Stirrer()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

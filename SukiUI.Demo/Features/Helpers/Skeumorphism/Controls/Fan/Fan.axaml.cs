using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki axial fan seen from the front. Display only: the rotor spins at the reported speed,
/// with motion blur, a motor glow and air pushed outwards. Drawing and animation live in <c>Fan.Rendering.cs</c>.
/// </summary>
public partial class Fan : UserControl
{
    /// <summary>Rotor speed reported by the device: 0 = stopped .. 1 = full speed.</summary>
    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<Fan, double>(nameof(Speed), coerce: (_, v) => Math.Clamp(v, 0, 1));

    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public Fan()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

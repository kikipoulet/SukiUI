using System;
using Avalonia;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct FanState(double Speed, SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki axial fan seen from the front. Display only: the rotor spins at the reported speed,
/// with motion blur, a motor glow and air pushed outwards. Drawing and animation live in <c>Fan.Rendering.cs</c>.
/// </summary>
public partial class Fan : HyperControl<FanState>
{
    /// <summary>Rotor speed reported by the device: 0 = stopped .. 1 = full speed.</summary>
    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<Fan, double>(nameof(Speed), coerce: (_, v) => Math.Clamp(v, 0, 1));

    static Fan() => AffectsRenderState(SpeedProperty);

    public Fan() => IsHitTestVisible = false;

    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    protected override FanState CaptureState() => new(Speed, SkeumoSkia.CapturePalette(this));
}

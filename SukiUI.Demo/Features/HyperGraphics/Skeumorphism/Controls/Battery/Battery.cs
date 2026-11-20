using System;
using Avalonia;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct BatteryState(
    double Charge,
    bool IsCharging,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki battery pack: a case with two terminals (+ left, âˆ’ right) and a charge gauge behind a window.
/// Display only: four flat pill segments filled bottom to top by the charge
/// (green above 50 %, amber above 25 %, red below). The segment being charged breathes gently. A readout gives the
/// percentage and the bolt on the lid turns yellow while charging and green when full.
/// Drawing and animation live in <c>Battery.Rendering.cs</c>.
/// </summary>
public partial class Battery : HyperControl<BatteryState>
{
    /// <summary>Charge reported by the device: 0 = empty .. 1 = full.</summary>
    public static readonly StyledProperty<double> ChargeProperty =
        AvaloniaProperty.Register<Battery, double>(nameof(Charge), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>True while current flows into the battery.</summary>
    public static readonly StyledProperty<bool> IsChargingProperty =
        AvaloniaProperty.Register<Battery, bool>(nameof(IsCharging));

    static Battery() => AffectsRenderState(ChargeProperty, IsChargingProperty);

    public Battery() => IsHitTestVisible = false;

    public double Charge
    {
        get => GetValue(ChargeProperty);
        set => SetValue(ChargeProperty, value);
    }

    public bool IsCharging
    {
        get => GetValue(IsChargingProperty);
        set => SetValue(IsChargingProperty, value);
    }

    protected override BatteryState CaptureState() =>
        new(Charge, IsCharging, SkeumoSkia.CapturePalette(this));
}

using System;
using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki battery pack: a case with two terminals (+ left, − right) and a charge gauge behind a window.
/// Display only: four flat pill segments filled bottom to top by the charge
/// (green above 50 %, amber above 25 %, red below). The segment being charged breathes gently. A readout gives the
/// percentage and the bolt on the lid turns yellow while charging and green when full.
/// Drawing and animation live in <c>Battery.Rendering.cs</c>.
/// </summary>
public partial class Battery : UserControl
{
    /// <summary>Charge reported by the device: 0 = empty .. 1 = full.</summary>
    public static readonly StyledProperty<double> ChargeProperty =
        AvaloniaProperty.Register<Battery, double>(nameof(Charge), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>True while current flows into the battery.</summary>
    public static readonly StyledProperty<bool> IsChargingProperty =
        AvaloniaProperty.Register<Battery, bool>(nameof(IsCharging));

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

    public Battery()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

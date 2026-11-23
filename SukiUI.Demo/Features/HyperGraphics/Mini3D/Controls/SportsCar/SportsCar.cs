using System;
using Avalonia;
using Avalonia.Data;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Interactive stylized GT sports car backed by <see cref="SportsCarScene"/>.
/// Exposes bindable Avalonia properties for headlights, active rear aero wing, and wheel spin;
/// 3D scene definition lives in <c>SportsCar.Scene.cs</c>.
/// </summary>
public sealed class SportsCar : Model3DControl<SportsCarScene>
{
    public static readonly StyledProperty<bool> IsLightsOnProperty =
        AvaloniaProperty.Register<SportsCar, bool>(nameof(IsLightsOn), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsAeroDeployedProperty =
        AvaloniaProperty.Register<SportsCar, bool>(nameof(IsAeroDeployed), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> WheelTurnsProperty =
        AvaloniaProperty.Register<SportsCar, double>(nameof(WheelTurns), 0.0);

    public static readonly StyledProperty<bool> IsDrivingProperty =
        AvaloniaProperty.Register<SportsCar, bool>(nameof(IsDriving), true, defaultBindingMode: BindingMode.TwoWay);

    static SportsCar()
    {
        BindChannel<SportsCar>(IsLightsOnProperty, "lights");
        BindChannel<SportsCar>(IsAeroDeployedProperty, "aero");
        BindChannel<SportsCar>(WheelTurnsProperty, "wheel");
    }

    public bool IsLightsOn
    {
        get => GetValue(IsLightsOnProperty);
        set => SetValue(IsLightsOnProperty, value);
    }

    public bool IsAeroDeployed
    {
        get => GetValue(IsAeroDeployedProperty);
        set => SetValue(IsAeroDeployedProperty, value);
    }

    public double WheelTurns
    {
        get => GetValue(WheelTurnsProperty);
        set => SetValue(WheelTurnsProperty, value);
    }

    public bool IsDriving
    {
        get => GetValue(IsDrivingProperty);
        set => SetValue(IsDrivingProperty, value);
    }

    protected override void OnModelClicked(string id)
    {
        switch (id)
        {
            case "lights":
                SetCurrentValue(IsLightsOnProperty, !IsLightsOn);
                break;
            case "aero":
                SetCurrentValue(IsAeroDeployedProperty, !IsAeroDeployed);
                break;
            case "drive":
                SetCurrentValue(IsDrivingProperty, !IsDriving);
                break;
        }
        base.OnModelClicked(id);
    }
}

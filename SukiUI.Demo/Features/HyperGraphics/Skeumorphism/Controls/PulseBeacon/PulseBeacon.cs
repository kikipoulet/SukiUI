using Avalonia;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct PulseBeaconState(bool IsActive);

public partial class PulseBeacon : HyperControl<PulseBeaconState>
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<PulseBeacon, bool>(nameof(IsActive), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    static PulseBeacon() => AffectsRenderState(IsActiveProperty);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        SetCurrentValue(IsActiveProperty, !IsActive);
        e.Handled = true;
    }

    protected override PulseBeaconState CaptureState() => new(IsActive);
}

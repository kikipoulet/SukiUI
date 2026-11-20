using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct RockerSwitchState(bool IsOn, SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki wall rocker switch (a light switch): the paddle rocks to "I" or "O" and a neon pilot glows when on.
/// A click, Space or Enter flips it. Two terminals at the bottom take the wires. It only reports the chosen state.
/// Drawing and animation live in <c>RockerSwitch.Rendering.cs</c>.
/// </summary>
public partial class RockerSwitch : HyperControl<RockerSwitchState>
{
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<RockerSwitch, bool>(nameof(IsOn), defaultBindingMode: BindingMode.TwoWay);

    static RockerSwitch() => AffectsRenderState(IsOnProperty);

    public RockerSwitch()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        SetCurrentValue(IsOnProperty, !IsOn);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Space or Key.Enter)) return;
        SetCurrentValue(IsOnProperty, !IsOn);
        e.Handled = true;
    }

    protected override RockerSwitchState CaptureState() => new(IsOn, SkeumoSkia.CapturePalette(this));
}

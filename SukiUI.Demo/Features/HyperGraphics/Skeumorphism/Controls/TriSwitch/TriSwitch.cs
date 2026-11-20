using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct TriSwitchState(
    int SelectedIndex,
    string? LeftLabel,
    string? MiddleLabel,
    string? RightLabel,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki horizontal three-position selector (OFF / SLOW / FAST for instance): a grooved cap slides
/// in a slot and clicks into each detent. Click a position, drag, mouse wheel or arrow keys. A terminal takes the wire
/// to the device. It only reports the chosen position. Drawing and animation live in <c>TriSwitch.Rendering.cs</c>.
/// </summary>
public partial class TriSwitch : HyperControl<TriSwitchState>
{
    private static readonly double[] DetentX = { 20, 48, 76 };

    /// <summary>Selected position: 0 = left, 1 = middle, 2 = right.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<TriSwitch, int>(nameof(SelectedIndex),
            defaultBindingMode: BindingMode.TwoWay, coerce: (_, v) => Math.Clamp(v, 0, 2));

    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(LeftLabel), "OFF");

    public static readonly StyledProperty<string?> MiddleLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(MiddleLabel));

    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(RightLabel));

    static TriSwitch() => AffectsRenderState(SelectedIndexProperty, LeftLabelProperty, MiddleLabelProperty, RightLabelProperty);

    private bool _dragging;

    public TriSwitch()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string? LeftLabel
    {
        get => GetValue(LeftLabelProperty);
        set => SetValue(LeftLabelProperty, value);
    }

    public string? MiddleLabel
    {
        get => GetValue(MiddleLabelProperty);
        set => SetValue(MiddleLabelProperty, value);
    }

    public string? RightLabel
    {
        get => GetValue(RightLabelProperty);
        set => SetValue(RightLabelProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        _dragging = true;
        SelectNearest(e.GetPosition(this).X);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging) SelectNearest(e.GetPosition(this).X);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _dragging = false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        Select(SelectedIndex + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var dir = e.Key is Key.Right or Key.Up ? 1 : e.Key is Key.Left or Key.Down ? -1 : 0;
        if (dir == 0) return;
        Select(SelectedIndex + dir);
        e.Handled = true;
    }

    private void SelectNearest(double x)
    {
        var best = 0;
        for (var i = 1; i < DetentX.Length; i++)
            if (Math.Abs(x - DetentX[i]) < Math.Abs(x - DetentX[best])) best = i;
        Select(best);
    }

    private void Select(int index)
    {
        index = Math.Clamp(index, 0, 2);
        if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);
    }

    protected override TriSwitchState CaptureState() =>
        new(SelectedIndex, LeftLabel, MiddleLabel, RightLabel, SkeumoSkia.CapturePalette(this));
}

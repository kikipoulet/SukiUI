using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct NotchSwitchState(
    int SelectedIndex,
    string? TopLabel,
    string? MiddleLabel,
    string? BottomLabel,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki vertical switch with three notches: a knob slides in a dug Suki rail and the active label lights up.
/// Drag, click, mouse wheel and arrow keys change the notch. It only reports the chosen notch:
/// what it selects is up to the host. Drawing and animation live in <c>NotchSwitch.Rendering.cs</c>.
/// </summary>
public partial class NotchSwitch : HyperControl<NotchSwitchState>
{
    private const double KnobBottomY = 77, NotchStepY = 27;

    /// <summary>Selected notch: 0 = bottom, 1 = middle, 2 = top.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<NotchSwitch, int>(nameof(SelectedIndex),
            defaultBindingMode: BindingMode.TwoWay, coerce: (_, v) => Math.Clamp(v, 0, 2));

    /// <summary>Label of the top notch (index 2).</summary>
    public static readonly StyledProperty<string?> TopLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(TopLabel));

    /// <summary>Label of the middle notch (index 1).</summary>
    public static readonly StyledProperty<string?> MiddleLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(MiddleLabel));

    /// <summary>Label of the bottom notch (index 0).</summary>
    public static readonly StyledProperty<string?> BottomLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(BottomLabel));

    static NotchSwitch() => AffectsRenderState(SelectedIndexProperty, TopLabelProperty, MiddleLabelProperty, BottomLabelProperty);

    private bool _dragging;

    public NotchSwitch()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string? TopLabel
    {
        get => GetValue(TopLabelProperty);
        set => SetValue(TopLabelProperty, value);
    }

    public string? MiddleLabel
    {
        get => GetValue(MiddleLabelProperty);
        set => SetValue(MiddleLabelProperty, value);
    }

    public string? BottomLabel
    {
        get => GetValue(BottomLabelProperty);
        set => SetValue(BottomLabelProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        _dragging = true;
        SelectFromY(e.GetPosition(this).Y);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging) SelectFromY(e.GetPosition(this).Y);
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
        SelectFromUser(SelectedIndex + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var dir = e.Key is Key.Up or Key.Right ? 1 : e.Key is Key.Down or Key.Left ? -1 : 0;
        if (dir == 0) return;
        SelectFromUser(SelectedIndex + dir);
        e.Handled = true;
    }

    private void SelectFromY(double y) =>
        SelectFromUser((int)Math.Round((KnobBottomY - y) / NotchStepY));

    private void SelectFromUser(int index)
    {
        index = Math.Clamp(index, 0, 2);
        if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);
    }

    protected override NotchSwitchState CaptureState() =>
        new(SelectedIndex, TopLabel, MiddleLabel, BottomLabel, SkeumoSkia.CapturePalette(this));
}

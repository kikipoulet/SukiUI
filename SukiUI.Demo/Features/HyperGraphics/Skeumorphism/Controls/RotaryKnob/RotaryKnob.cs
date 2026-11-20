using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct RotaryKnobState(
    double TargetAngle,
    int Steps,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki rotary potentiometer with detents: a knurled knob on a round plate, 270 degrees of travel.
/// The knob turns with the hand (relative anchoring) and clicks into each notch with a small overshoot.
/// Drag, mouse wheel and arrow keys move one notch at a time. It only reports the chosen value.
/// Drawing and animation live in <c>RotaryKnob.Rendering.cs</c>.
/// </summary>
public partial class RotaryKnob : HyperControl<RotaryKnobState>
{
    private const double Cx = 60, Cy = 60, MinDeg = -135, MaxDeg = 135;

    /// <summary>Current value, always on a notch between <see cref="Minimum"/> and <see cref="Maximum"/>.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Maximum), 10);

    /// <summary>Number of intervals between the minimum and the maximum (notches = StepCount + 1).</summary>
    public static readonly StyledProperty<int> StepCountProperty =
        AvaloniaProperty.Register<RotaryKnob, int>(nameof(StepCount), 10);

    static RotaryKnob() => AffectsRenderState(ValueProperty, MinimumProperty, MaximumProperty, StepCountProperty);

    private readonly RotaryDrag _drag = new(() => new Point(Cx, Cy), RotaryDrag.FromTop);
    private bool _dragging;

    public RotaryKnob()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }

    private int Steps => Math.Max(1, StepCount);

    private int Index
    {
        get
        {
            var range = Maximum - Minimum;
            if (range <= 0) return 0;
            return (int)Math.Round(Math.Clamp((Value - Minimum) / range, 0, 1) * Steps);
        }
    }

    private double AngleOf(int index) => MinDeg + (MaxDeg - MinDeg) * index / Steps;
    private int IndexOf(double angle) => (int)Math.Round((angle - MinDeg) / (MaxDeg - MinDeg) * Steps);

    private void SelectFromUser(int index)
    {
        index = Math.Clamp(index, 0, Steps);
        if (index == Index) return;
        SetCurrentValue(ValueProperty, Minimum + (Maximum - Minimum) * index / Steps);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        _dragging = true;
        _drag.Begin(e.GetPosition(this), AngleOf(Index));
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        if (_drag.Move(e.GetPosition(this), MinDeg, MaxDeg))
            SelectFromUser(IndexOf(_drag.Angle));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        _drag.End();
        e.Pointer.Capture(null);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _dragging = false;
        _drag.End();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        SelectFromUser(Index + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var dir = e.Key is Key.Right or Key.Up ? 1 : e.Key is Key.Left or Key.Down ? -1 : 0;
        if (dir == 0) return;
        SelectFromUser(Index + dir);
        e.Handled = true;
    }

    protected override RotaryKnobState CaptureState() =>
        new(AngleOf(Index), Steps, SkeumoSkia.CapturePalette(this));
}

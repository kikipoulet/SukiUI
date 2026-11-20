using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct SpringLeverState(
    double Angle,
    bool Engaged,
    int ReleaseId,
    string? LeftLabel,
    string? RightLabel,
    double NormalizedValue,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki spring-loaded lever (jog). The user pushes it left or right; released, it springs back to the center.
/// It only reports the user's intent: what to do with it (response curve, anti-spam, safety) belongs to the host.
/// Drawing and animation live in <c>SpringLever.Rendering.cs</c>.
/// </summary>
public partial class SpringLever : HyperControl<SpringLeverState>
{
    private const double PivotX = 55, PivotY = 82, MaxDeg = 35, DeadDeg = 4, KeyStepDeg = 5;

    /// <summary>
    /// Lever position set by the user: -1 (fully left) .. 1 (fully right), 0 inside the center dead zone.
    /// Set back to 0 immediately on release, before the spring-back animation.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SpringLever, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Label shown above the left end of the travel.</summary>
    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(LeftLabel));

    /// <summary>Label shown above the right end of the travel.</summary>
    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(RightLabel));

    static SpringLever() => AffectsRenderState(ValueProperty, LeftLabelProperty, RightLabelProperty);

    private readonly RotaryDrag _drag = new(() => new Point(PivotX, PivotY), RotaryDrag.FromTop);
    private double _angle;
    private double _normalized;
    private bool _engaged;
    private int _releaseId;

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? LeftLabel
    {
        get => GetValue(LeftLabelProperty);
        set => SetValue(LeftLabelProperty, value);
    }

    public string? RightLabel
    {
        get => GetValue(RightLabelProperty);
        set => SetValue(RightLabelProperty, value);
    }

    /// <summary>Raised when the user lets go of the lever (pointer released, capture lost or key released).</summary>
    public event EventHandler? Released;

    public SpringLever()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        DetachedFromVisualTree += (_, _) => Release();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        _engaged = true;
        _drag.Begin(e.GetPosition(this), _angle);
        e.Pointer.Capture(this);
        e.Handled = true;
        Refresh();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_engaged) return;
        if (_drag.Move(e.GetPosition(this), -MaxDeg, MaxDeg))
            SetAngle(_drag.Angle);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_engaged) return;
        e.Pointer.Capture(null);
        Release();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        Release();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var step = e.Key switch
        {
            Key.Right or Key.Up => KeyStepDeg,
            Key.Left or Key.Down => -KeyStepDeg,
            _ => 0,
        };
        if (step == 0) return;
        _engaged = true;
        SetAngle(Math.Clamp(_angle + step, -MaxDeg, MaxDeg));
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key is not (Key.Right or Key.Up or Key.Left or Key.Down)) return;
        Release();
        e.Handled = true;
    }

    private void SetAngle(double deg)
    {
        _angle = deg;
        var abs = Math.Abs(deg);
        _normalized = abs <= DeadDeg ? 0 : Math.Sign(deg) * (abs - DeadDeg) / (MaxDeg - DeadDeg);
        SetCurrentValue(ValueProperty, _normalized);
        Refresh();
    }

    private void Release()
    {
        if (!_engaged) return;
        _engaged = false;
        _drag.End();
        _angle = 0;
        _normalized = 0;
        _releaseId++;
        SetCurrentValue(ValueProperty, 0d);
        Released?.Invoke(this, EventArgs.Empty);
        Refresh();
    }

    protected override SpringLeverState CaptureState() =>
        new(_angle, _engaged, _releaseId, LeftLabel, RightLabel, _normalized, SkeumoSkia.CapturePalette(this));
}

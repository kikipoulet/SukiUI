using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct HingeSliderState(
    double Value,
    IReadOnlyList<double> Steps,
    string? LeftCaption,
    string? RightCaption,
    double BoundsWidth,
    double BoundsHeight,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki hinge slider: a Suki rail with a pearl thumb, snapping on notches.
/// The notches are spread evenly along the rail and the thumb is interpolated logarithmically between them,
/// so a geometric series (exposure stops, gain steps) reads as an even scale.
/// It only reports the chosen value: what it controls is up to the host. Drawing lives in <c>HingeSlider.Rendering.cs</c>.
/// </summary>
public partial class HingeSlider : HyperControl<HingeSliderState>
{
    private const double TrackPadX = 4;

    /// <summary>Current value. Set by the user on a notch; can also be set by the host (any value inside the range).</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<HingeSlider, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Notch values, strictly increasing and positive.</summary>
    public static readonly StyledProperty<IReadOnlyList<double>> StepsProperty =
        AvaloniaProperty.Register<HingeSlider, IReadOnlyList<double>>(nameof(Steps), new double[] { 1, 2, 4, 8 });

    /// <summary>Caption under the left end: the consequence of a low value. Without any caption, the caption line is hidden.</summary>
    public static readonly StyledProperty<string?> LeftCaptionProperty =
        AvaloniaProperty.Register<HingeSlider, string?>(nameof(LeftCaption));

    /// <summary>Caption under the right end: the consequence of a high value.</summary>
    public static readonly StyledProperty<string?> RightCaptionProperty =
        AvaloniaProperty.Register<HingeSlider, string?>(nameof(RightCaption));

    static HingeSlider() => AffectsRenderState(ValueProperty, StepsProperty, LeftCaptionProperty, RightCaptionProperty, BoundsProperty);

    private bool _dragging;

    public HingeSlider()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<double> Steps
    {
        get => GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public string? LeftCaption
    {
        get => GetValue(LeftCaptionProperty);
        set => SetValue(LeftCaptionProperty, value);
    }

    public string? RightCaption
    {
        get => GetValue(RightCaptionProperty);
        set => SetValue(RightCaptionProperty, value);
    }

    private bool HasCaptions => !string.IsNullOrEmpty(LeftCaption) || !string.IsNullOrEmpty(RightCaption);

    protected override Size MeasureOverride(Size availableSize)
    {
        var w = double.IsInfinity(availableSize.Width) ? 268 : availableSize.Width;
        return new Size(w, HasCaptions ? 44 : 28);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _dragging = true;
        e.Pointer.Capture(this);
        Focus();
        Pick(e);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging) Pick(e);
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var dir = e.Key is Key.Right or Key.Up ? 1 : e.Key is Key.Left or Key.Down ? -1 : 0;
        if (dir == 0) return;
        SetFromUser(NextStep(dir));
        e.Handled = true;
    }

    private void Pick(PointerEventArgs e)
    {
        var trackW = Math.Max(1, Bounds.Width - TrackPadX * 2);
        if (Steps.Count == 0) return;
        var f = Math.Clamp((e.GetPosition(this).X - TrackPadX) / trackW, 0, 1);
        SetFromUser(Steps[(int)Math.Round(f * (Steps.Count - 1))]);
    }

    private void SetFromUser(double value)
    {
        if (Math.Abs(value - Value) < 1e-9) return;
        SetCurrentValue(ValueProperty, value);
    }

    private double NextStep(int dir)
    {
        var steps = Steps;
        if (dir > 0)
        {
            foreach (var s in steps)
                if (s > Value * 1.001) return s;
            return steps[^1];
        }
        for (var i = steps.Count - 1; i >= 0; i--)
            if (steps[i] < Value / 1.001) return steps[i];
        return steps[0];
    }

    protected override HingeSliderState CaptureState() =>
        new(Value, Steps, LeftCaption, RightCaption, Math.Max(1, Bounds.Width), Math.Max(1, Bounds.Height), SkeumoSkia.CapturePalette(this));
}

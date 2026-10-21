using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the HingeSlider: notches, fill, thumb, glows and pointer / keyboard input.
public partial class HingeSlider
{
    private bool _dragging;

    private void InitializeRendering()
    {
        AttachedToVisualTree      += (_, _) => { ApplyTheme(); Place(); };
        ActualThemeVariantChanged += (_, _) => ApplyTheme();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty) Place();
        else if (change.Property == StepsProperty) { BuildTicks(); Place(); }
        else if (change.Property == LeftCaptionProperty || change.Property == RightCaptionProperty)
            // No caption at all: the notches speak for themselves, no empty line under the rail
            this.Get<Grid>("Captions").IsVisible = !string.IsNullOrEmpty(LeftCaption) || !string.IsNullOrEmpty(RightCaption);
    }

    // ------------------------------------------------------------------ input

    private void Track_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var track = (Grid)s!;
        if (!e.GetCurrentPoint(track).Properties.IsLeftButtonPressed) return;
        _dragging = true;
        e.Pointer.Capture(track);
        track.Focus();
        Pick(e);
        e.Handled = true;
    }

    private void Track_OnMoved(object? s, PointerEventArgs e)
    {
        if (_dragging) Pick(e);
    }

    private void Track_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void Track_OnCaptureLost(object? s, PointerCaptureLostEventArgs e) => _dragging = false;

    private void Track_OnKeyDown(object? s, KeyEventArgs e)
    {
        var dir = e.Key is Key.Right or Key.Up ? 1 : e.Key is Key.Left or Key.Down ? -1 : 0;
        if (dir == 0) return;
        SetFromUser(NextStep(dir));
        e.Handled = true;
    }

    private void Track_OnSizeChanged(object? s, SizeChangedEventArgs e)
    {
        BuildTicks();
        Place();
    }

    private void Pick(PointerEventArgs e)
    {
        var track = this.Get<Grid>("Track");
        var width = track.Bounds.Width;
        if (width <= 0 || Steps.Count == 0) return;
        var f = Math.Clamp(e.GetPosition(track).X / width, 0, 1);
        SetFromUser(Steps[(int)Math.Round(f * (Steps.Count - 1))]);
    }

    private void SetFromUser(double value)
    {
        if (Math.Abs(value - Value) < 1e-9) return;
        SetCurrentValue(ValueProperty, value);                   // keeps the TwoWay binding alive
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

    // ------------------------------------------------------------------ rendering

    private void ApplyTheme()
    {
        // Lighter inner shadow of the rail on the light theme
        this.Get<Border>("Rail").BoxShadow = BoxShadows.Parse(ActualThemeVariant == ThemeVariant.Light
            ? "inset 0 1 2 0 #33000000"
            : "inset 0 1 3 0 #AA000000");

        if (!this.TryFindResource("SukiPrimaryColor", ActualThemeVariant, out var res) || res is not Color accent) return;
        BoxShadows Glow(double blur, byte alpha) =>
            new(new BoxShadow { Blur = blur, Color = Color.FromArgb(alpha, accent.R, accent.G, accent.B) });
        this.Get<Border>("Fill").BoxShadow = Glow(8, 0x66);
        this.Get<Border>("ThumbDot").BoxShadow = Glow(6, 0xFF);
    }

    private void BuildTicks()
    {
        var track = this.Get<Grid>("Track");
        var ticks = this.Get<Canvas>("Ticks");
        ticks.Children.Clear();
        var width = track.Bounds.Width;
        var count = Steps.Count;
        if (width <= 0 || count < 2) return;
        for (var i = 0; i < count; i++)
        {
            var x = i / (double)(count - 1) * width;
            ticks.Children.Add(new Rectangle
            {
                Width = 1, Height = 4, Fill = new SolidColorBrush(Color.FromArgb(0x66, 0x80, 0x90, 0xA8)),
                [Canvas.LeftProperty] = Math.Clamp(x, 0, width - 1),
                [Canvas.TopProperty] = 21d
            });
        }
    }

    private void Place()
    {
        var steps = Steps;
        if (steps.Count < 2) return;
        var fraction = IndexOf(Value) / (steps.Count - 1);
        var x = fraction * this.Get<Grid>("Track").Bounds.Width;
        this.Get<Border>("Fill").Width = Math.Max(0, x);
        this.Get<Border>("Thumb").Margin = new Thickness(x - 11, 0, 0, 0);
    }

    // Fractional notch index of a value, logarithmic between two notches
    private double IndexOf(double value)
    {
        var steps = Steps;
        value = Math.Clamp(value, steps[0], steps[^1]);
        for (var i = 0; i < steps.Count - 1; i++)
            if (value <= steps[i + 1])
                return i + Math.Log(value / steps[i]) / Math.Log(steps[i + 1] / steps[i]);
        return steps.Count - 1;
    }
}

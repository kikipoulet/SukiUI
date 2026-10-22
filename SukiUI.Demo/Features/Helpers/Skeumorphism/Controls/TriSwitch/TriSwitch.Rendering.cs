using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the TriSwitch: detent slide with a small overshoot, accent fill, active label and input.
public partial class TriSwitch
{
    private static readonly double[] DetentX = { 20, 48, 76 };
    private const double KnobHalf = 11, FillLeft = 14;

    private DispatcherTimer? _slide;
    private double _x = DetentX[0];
    private bool _ready, _dragging;

    private void InitializeRendering()
    {
        AttachedToVisualTree      += (_, _) => ApplyGlow();
        ActualThemeVariantChanged += (_, _) => ApplyGlow();
        _ready = true;
        _x = DetentX[SelectedIndex];
        Render();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_ready || change.Property != SelectedIndexProperty) return;
        // Click into the detent: ease with a small overshoot
        _slide?.Stop();
        _slide = SkeumoMotion.Tween(_x, DetentX[SelectedIndex], 220, x => { _x = x; Render(); }, SkeumoMotion.BackOut);
    }

    // ------------------------------------------------------------------ input

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var c = (Canvas)s!;
        c.Focus();
        _dragging = true;
        SelectNearest(e.GetPosition(c).X);
        e.Pointer.Capture(c);
        e.Handled = true;
    }

    private void Scene_OnMoved(object? s, PointerEventArgs e)
    {
        if (_dragging) SelectNearest(e.GetPosition((Canvas)s!).X);
    }

    private void Scene_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void Scene_OnCaptureLost(object? s, PointerCaptureLostEventArgs e) => _dragging = false;

    private void Scene_OnWheel(object? s, PointerWheelEventArgs e)
    {
        Select(SelectedIndex + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
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
        if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);   // keeps the TwoWay binding alive
    }

    // ------------------------------------------------------------------ rendering

    private void Render()
    {
        Canvas.SetLeft(this.Get<Border>("Knob"), _x - KnobHalf);
        this.Get<Border>("Fill").Width = Math.Max(0, _x - FillLeft - 3);
        this.Get<TextBlock>("LeftPositionText").Classes.Set("Active", SelectedIndex == 0);
        this.Get<TextBlock>("MiddlePositionText").Classes.Set("Active", SelectedIndex == 1);
        this.Get<TextBlock>("RightPositionText").Classes.Set("Active", SelectedIndex == 2);
    }

    private void ApplyGlow()
    {
        if (!SkeumoMotion.TryAccent(this, out var a)) return;
        this.Get<Border>("KnobIndex").BoxShadow = SkeumoMotion.Glow(a, 6, 0xFF);
        this.Get<Border>("Fill").BoxShadow = SkeumoMotion.Glow(a, 8, 0x66);
    }
}

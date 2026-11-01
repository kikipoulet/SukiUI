using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the NotchSwitch: knob slide (ease-out), accent fill, active label and input.
public partial class NotchSwitch
{
    private const double KnobBottomY = 77, NotchStepY = 27;
    private static readonly double[] KnobOffsetY = { 0, -27, -54 };   // knob offset from the bottom notch

    private TranslateTransform _move = null!;
    private double _knobY;
    private bool _ready, _dragging;
    private int _tweenId;

    private void InitializeRendering()
    {
        _move = (TranslateTransform)this.Get<Border>("Knob").RenderTransform!;
        _ready = true;
        Render(animate: false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_ready && change.Property == SelectedIndexProperty) Render(animate: true);
    }

    // ------------------------------------------------------------------ input

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var canvas = (Canvas)s!;
        canvas.Focus();
        _dragging = true;
        SelectFromY(e.GetPosition(canvas).Y);
        e.Pointer.Capture(canvas);
        e.Handled = true;
    }

    private void Scene_OnMoved(object? s, PointerEventArgs e)
    {
        if (_dragging) SelectFromY(e.GetPosition((Canvas)s!).Y);
    }

    private void Scene_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void Scene_OnCaptureLost(object? s, PointerCaptureLostEventArgs e) => _dragging = false;

    private void Scene_OnWheel(object? s, PointerWheelEventArgs e)
    {
        SelectFromUser(SelectedIndex + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
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
        if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);   // keeps the TwoWay binding alive
    }

    // ------------------------------------------------------------------ rendering

    private void Render(bool animate)
    {
        var idx = SelectedIndex;
        this.Get<TextBlock>("BottomNotchLabel").Classes.Set("Active", idx == 0);
        this.Get<TextBlock>("MiddleNotchLabel").Classes.Set("Active", idx == 1);
        this.Get<TextBlock>("TopNotchLabel").Classes.Set("Active", idx == 2);
        SlideKnob(KnobOffsetY[idx], animate);
    }

    // Slides the knob to the new notch: manual ease-out cubic over 250 ms (no Animation on RenderTransform)
    private void SlideKnob(double y, bool animate)
    {
        var from = _knobY;
        _knobY = y;
        var id = ++_tweenId;

        if (!animate)
        {
            Place(y);
            return;
        }

        var start = DateTime.UtcNow;
        var timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (s, _) =>
        {
            var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / 250);
            if (id == _tweenId) Place(from + (y - from) * (1 - Math.Pow(1 - t, 3)));
            if (t >= 1 || id != _tweenId) ((DispatcherTimer)s!).Stop();
        });
        timer.Start();
    }

    private void Place(double y)
    {
        _move.Y = y;
        // Accent fill: from the bottom of the rail up to the knob center (bottom center = 77)
        var fill = this.Get<Border>("Fill");
        Canvas.SetTop(fill, KnobBottomY + y);
        fill.Height = 7 - y;
    }
}

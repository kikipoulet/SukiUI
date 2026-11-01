using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the RotaryKnob: notches, accent arc, knurling, detent snap and input.
public partial class RotaryKnob
{
    private const double Cx = 60, Cy = 60, MinDeg = -135, MaxDeg = 135, ArcR = 47;

    private RotateTransform _rot = null!;
    private DispatcherTimer? _snap;
    private readonly RotaryDrag _drag = new(() => new Point(Cx, Cy), RotaryDrag.FromTop);
    private double _angle;
    private bool _ready, _dragging;

    private void InitializeRendering()
    {
        _rot = (RotateTransform)this.Get<Canvas>("Knob").RenderTransform!;
        BuildKnurling();
        this.Get<Path>("Track").Data = Arc(MinDeg, MaxDeg);

        AttachedToVisualTree      += (_, _) => { ApplyGlow(); BuildTicks(); };
        ActualThemeVariantChanged += (_, _) => ApplyGlow();

        _ready = true;
        _angle = AngleOf(Index);
        Render();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_ready) return;
        if (change.Property == ValueProperty && !_dragging) SnapTo(AngleOf(Index));
        else if (change.Property == StepCountProperty) { BuildTicks(); SnapTo(AngleOf(Index)); }
        else if (change.Property == MinimumProperty || change.Property == MaximumProperty) SnapTo(AngleOf(Index));
    }

    // ------------------------------------------------------------------ value <-> notch <-> angle

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
        SetCurrentValue(ValueProperty, Minimum + (Maximum - Minimum) * index / Steps);   // keeps the TwoWay binding alive
        SnapTo(AngleOf(index));
    }

    // ------------------------------------------------------------------ input

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var c = (Canvas)s!;
        c.Focus();
        _dragging = true;
        // Relative anchoring: the knob turns by the same rotation as the hand around the axis, accumulated move by move
        _drag.Begin(e.GetPosition(c), AngleOf(Index));
        e.Pointer.Capture(c);
        e.Handled = true;
    }

    private void Scene_OnMoved(object? s, PointerEventArgs e)
    {
        if (!_dragging) return;
        if (_drag.Move(e.GetPosition((Canvas)s!), MinDeg, MaxDeg))
            SelectFromUser(IndexOf(_drag.Angle));
    }

    private void Scene_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        _dragging = false;
        _drag.End();
        e.Pointer.Capture(null);
    }

    private void Scene_OnCaptureLost(object? s, PointerCaptureLostEventArgs e)
    {
        _dragging = false;
        _drag.End();
    }

    private void Scene_OnWheel(object? s, PointerWheelEventArgs e)
    {
        SelectFromUser(Index + (e.Delta.Y > 0 ? 1 : -1));
        e.Handled = true;
    }

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
        var dir = e.Key is Key.Right or Key.Up ? 1 : e.Key is Key.Left or Key.Down ? -1 : 0;
        if (dir == 0) return;
        SelectFromUser(Index + dir);
        e.Handled = true;
    }

    // ------------------------------------------------------------------ rendering

    // Detent: the knob clicks into the notch with a small overshoot
    private void SnapTo(double target)
    {
        _snap?.Stop();
        _snap = SkeumoMotion.Tween(_angle, target, 180, a => { _angle = a; Render(); }, SkeumoMotion.BackOut);
    }

    private void Render()
    {
        _rot.Angle = _angle;
        var arc = this.Get<Path>("ArcFill");
        if (_angle - MinDeg < 1) { arc.IsVisible = false; return; }
        arc.Data = Arc(MinDeg, Math.Min(_angle, MaxDeg));
        arc.IsVisible = true;
    }

    private static Geometry Arc(double fromDeg, double toDeg)
    {
        Point At(double d) => new(Cx + ArcR * Math.Sin(d * Math.PI / 180), Cy - ArcR * Math.Cos(d * Math.PI / 180));
        var g = new StreamGeometry();
        using var ctx = g.Open();
        ctx.BeginFigure(At(fromDeg), false);
        ctx.ArcTo(At(toDeg), new Size(ArcR, ArcR), 0, toDeg - fromDeg > 180, SweepDirection.Clockwise);
        ctx.EndFigure(false);
        return g;
    }

    private void BuildTicks()
    {
        var canvas = this.Get<Canvas>("Ticks");
        canvas.Children.Clear();
        for (var i = 0; i <= Steps; i++)
        {
            var d = AngleOf(i) * Math.PI / 180;
            var major = i == 0 || i == Steps || (Steps % 2 == 0 && i == Steps / 2);
            var r0 = major ? 51.5 : 52.5;
            var line = new Line
            {
                StartPoint = new Point(Cx + r0 * Math.Sin(d), Cy - r0 * Math.Cos(d)),
                EndPoint = new Point(Cx + 55.5 * Math.Sin(d), Cy - 55.5 * Math.Cos(d)),
                StrokeThickness = major ? 1.8 : 1.2,
                StrokeLineCap = PenLineCap.Round,
                Opacity = major ? 1 : 0.7
            };
            line.BindBrush(Shape.StrokeProperty, this, "SkTick");
            canvas.Children.Add(line);
        }
    }

    private void BuildKnurling()
    {
        var knob = this.Get<Canvas>("Knob");
        for (var i = 0; i < 36; i++)
        {
            var d = i * 10 * Math.PI / 180;
            var line = new Line
            {
                StartPoint = new Point(33 + 28.5 * Math.Sin(d), 33 - 28.5 * Math.Cos(d)),
                EndPoint = new Point(33 + 32 * Math.Sin(d), 33 - 32 * Math.Cos(d)),
                StrokeThickness = 1
            };
            line.BindBrush(Shape.StrokeProperty, this, "SkGroove");
            knob.Children.Insert(0, line);
        }
    }

    private void ApplyGlow()
    {
        if (!SkeumoMotion.TryAccent(this, out var a)) return;
        this.Get<Border>("Indicator").BoxShadow = SkeumoMotion.Glow(a, 6, 0xFF);
        this.Get<Path>("ArcFill").Effect = new DropShadowEffect { Color = a, BlurRadius = 6, OffsetX = 0, OffsetY = 0, Opacity = 0.7 };
    }
}

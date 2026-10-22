using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Path = Avalonia.Controls.Shapes.Path;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the SpringLever: relative anchoring, dead zone, glow, arc fill and damped spring-back.
public partial class SpringLever
{
    private const double PivotX = 55, PivotY = 82, ArcR = 60, KnobR = 57, MaxDeg = 35, DeadDeg = 4, KeyStepDeg = 5;

    private RotateTransform _rot = null!;
    private readonly RotaryDrag _drag = new(() => new Point(PivotX, PivotY), RotaryDrag.FromTop);
    private double _angle;
    private bool _engaged;
    private int _tweenId;

    private void InitializeRendering()
    {
        _rot = (RotateTransform)this.Get<Grid>("Lever").RenderTransform!;
        DetachedFromVisualTree += (_, _) => Release();
    }

    // ------------------------------------------------------------------ input

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        var c = (Canvas)s!;
        c.Focus();
        Engage();
        _drag.Begin(e.GetPosition(c), _angle);                   // relative anchoring: the lever never jumps to the pointer
        e.Pointer.Capture(c);
        e.Handled = true;
    }

    private void Scene_OnMoved(object? s, PointerEventArgs e)
    {
        if (!_engaged) return;
        // Accumulated move by move: never flips when the hand passes 180 degrees from where it started
        if (_drag.Move(e.GetPosition((Canvas)s!), -MaxDeg, MaxDeg)) SetAngle(_drag.Angle);
    }

    private void Scene_OnReleased(object? s, PointerReleasedEventArgs e)
    {
        if (!_engaged) return;
        e.Pointer.Capture(null);
        Release();
    }

    private void Scene_OnCaptureLost(object? s, PointerCaptureLostEventArgs e) => Release();

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
        var step = e.Key switch
        {
            Key.Right or Key.Up => KeyStepDeg,
            Key.Left or Key.Down => -KeyStepDeg,
            _ => 0,
        };
        if (step == 0) return;
        if (!_engaged) Engage();
        SetAngle(Math.Clamp(_angle + step, -MaxDeg, MaxDeg));
        e.Handled = true;
    }

    private void Scene_OnKeyUp(object? s, KeyEventArgs e)
    {
        if (e.Key is not (Key.Right or Key.Up or Key.Left or Key.Down)) return;
        Release();
        e.Handled = true;
    }

    // ------------------------------------------------------------------ mechanics

    private void Engage()
    {
        _engaged = true;
        _tweenId++;                                              // stops a spring-back in progress
    }

    private void Release()
    {
        if (!_engaged) return;
        _engaged = false;
        _drag.End();
        SetCurrentValue(ValueProperty, 0d);                      // stop first, animate after
        SetLabels(0);
        Released?.Invoke(this, EventArgs.Empty);

        var id = ++_tweenId;
        Tween(_angle, 0, 700, a => { if (id == _tweenId) SetAngle(a, drive: false); }, ElasticOut);
    }

    private void SetAngle(double deg, bool drive = true)
    {
        _angle = deg;
        _rot.Angle = deg;

        var abs = Math.Abs(deg);
        var n = abs <= DeadDeg ? 0 : Math.Sign(deg) * (abs - DeadDeg) / (MaxDeg - DeadDeg);
        var rad = deg * Math.PI / 180;

        var glow = this.Get<Ellipse>("KnobGlow");
        glow.IsVisible = n != 0;
        glow.Opacity = 0.25 + 0.6 * Math.Abs(n);
        Canvas.SetLeft(glow, PivotX + KnobR * Math.Sin(rad) - glow.Width / 2);
        Canvas.SetTop(glow, PivotY - KnobR * Math.Cos(rad) - glow.Height / 2);

        UpdateArc(deg);

        if (!drive) return;
        SetLabels(n);
        SetCurrentValue(ValueProperty, n);
    }

    private void SetLabels(double n)
    {
        this.Get<TextBlock>("LeftEndLabel").Classes.Set("Active", n < 0);
        this.Get<TextBlock>("RightEndLabel").Classes.Set("Active", n > 0);
    }

    private void UpdateArc(double deg)
    {
        var arc = this.Get<Path>("ArcFill");
        if (Math.Abs(deg) < 0.5) { arc.IsVisible = false; return; }
        Point At(double d) => new(PivotX + ArcR * Math.Sin(d * Math.PI / 180), PivotY - ArcR * Math.Cos(d * Math.PI / 180));
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(At(0), false);
            ctx.ArcTo(At(deg), new Size(ArcR, ArcR), 0, false,
                deg > 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise);
            ctx.EndFigure(false);
        }
        arc.Data = g;
        arc.IsVisible = true;
    }

    private static double ElasticOut(double t) => Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;

    private static void Tween(double from, double to, int ms, Action<double> set, Func<double, double>? ease = null)
    {
        ease ??= t => 1 - Math.Pow(1 - t, 3);
        var start = DateTime.UtcNow;
        DispatcherTimer? timer = null;
        timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
        {
            var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
            set(from + (to - from) * ease(t));
            if (t >= 1) timer!.Stop();
        });
        timer.Start();
    }
}

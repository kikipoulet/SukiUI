using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the GuardedToggle: cover flip, lever throw, LED and input.
public partial class GuardedToggle
{
    private const double PivotY = 92, RodLength = 36, TipRadius = 8, OpenCoverScale = -0.28;
    private static readonly TimeSpan CoverAutoClose = TimeSpan.FromSeconds(4);

    private ScaleTransform _rod = null!, _cover = null!;
    private DispatcherTimer? _leverTween, _coverTween;
    private DispatcherTimer _autoClose = null!;
    private double _lever = 1, _coverScale = 1;    // lever: 1 = down (off), -1 = up (on)
    private bool _ready, _coverOpen;

    private void InitializeRendering()
    {
        _rod = (ScaleTransform)this.Get<Border>("Rod").RenderTransform!;
        _cover = (ScaleTransform)this.Get<Border>("Cover").RenderTransform!;
        _autoClose = new DispatcherTimer { Interval = CoverAutoClose };
        _autoClose.Tick += (_, _) => { _autoClose.Stop(); SetCover(false); };

        AttachedToVisualTree      += (_, _) => RenderLed();
        DetachedFromVisualTree    += (_, _) => _autoClose.Stop();
        ActualThemeVariantChanged += (_, _) => RenderLed();

        _ready = true;
        _lever = IsOn ? -1 : 1;
        RenderLever();
        RenderLabels();
        RenderCoverVisibility();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_ready) return;
        if (change.Property == IsOnProperty)
        {
            // Throw: ease-out, the rod shortens while it points at the viewer
            _leverTween?.Stop();
            _leverTween = SkeumoMotion.Tween(_lever, IsOn ? -1 : 1, 220, v => { _lever = v; RenderLever(); });
            RenderLabels();
            RenderLed();
        }
        else if (change.Property == RequiresCoverProperty) RenderCoverVisibility();
    }

    // ------------------------------------------------------------------ input

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        ((Control)s!).Focus();
        Act();
        e.Handled = true;
    }

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
        if (e.Key is not (Key.Space or Key.Enter)) return;
        Act();
        e.Handled = true;
    }

    private void Act()
    {
        if (RequiresCover && !_coverOpen)
        {
            SetCover(true);                                      // first gesture: lift the cover
            return;
        }
        SetCurrentValue(IsOnProperty, !IsOn);                    // keeps the TwoWay binding alive
        RestartAutoClose();
    }

    private void RestartAutoClose()
    {
        _autoClose.Stop();
        if (RequiresCover && _coverOpen) _autoClose.Start();
    }

    // ------------------------------------------------------------------ rendering

    private void SetCover(bool open)
    {
        _coverOpen = open;
        _coverTween?.Stop();
        _coverTween = SkeumoMotion.Tween(_coverScale, open ? OpenCoverScale : 1, 260, v => { _coverScale = v; _cover.ScaleY = v; });
        RestartAutoClose();
    }

    private void RenderCoverVisibility()
    {
        this.Get<Border>("Cover").IsVisible = RequiresCover;
        this.Get<Border>("CoverHinge").IsVisible = RequiresCover;
        if (!RequiresCover) _autoClose.Stop();
    }

    private void RenderLever()
    {
        _rod.ScaleY = _lever;
        var tipY = PivotY + RodLength * _lever;
        Canvas.SetTop(this.Get<Ellipse>("Tip"), tipY - TipRadius);
        Canvas.SetTop(this.Get<Ellipse>("TipShine"), tipY - TipRadius + 2);
    }

    private void RenderLabels()
    {
        this.Get<TextBlock>("OnStateText").Classes.Set("Active", IsOn);
        this.Get<TextBlock>("OffStateText").Classes.Set("Active", !IsOn);
    }

    private void RenderLed()
    {
        var led = this.Get<Ellipse>("Led");
        var halo = this.Get<Ellipse>("LedHalo");
        if (!IsOn || !SkeumoMotion.TryAccent(this, out var a))
        {
            led.BindBrush(Shape.FillProperty, this, "SkLedOff");
            halo.Opacity = 0;
            return;
        }
        led.Fill = new SolidColorBrush(a);
        halo.Fill = new SolidColorBrush(Color.FromArgb(0x55, a.R, a.G, a.B));
        halo.Opacity = 1;
    }
}

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

// Visual mechanics of the RockerSwitch: paddle shading crossfade (it rocks), marks, neon pilot and input.
public partial class RockerSwitch
{
    private static readonly Color Neon = Color.Parse("#FFFFB347");

    private DispatcherTimer? _rock;
    private double _on;
    private bool _ready;

    private void InitializeRendering()
    {
        _ready = true;
        _on = IsOn ? 1 : 0;
        Render();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_ready || change.Property != IsOnProperty) return;
        // The paddle rocks quickly: a short crossfade between the two shadings
        _rock?.Stop();
        _rock = SkeumoMotion.Tween(_on, IsOn ? 1 : 0, 120, v => { _on = v; Render(); });
    }

    private void Scene_OnPressed(object? s, PointerPressedEventArgs e)
    {
        ((Control)s!).Focus();
        SetCurrentValue(IsOnProperty, !IsOn);                    // keeps the TwoWay binding alive
        e.Handled = true;
    }

    private void Scene_OnKeyDown(object? s, KeyEventArgs e)
    {
        if (e.Key is not (Key.Space or Key.Enter)) return;
        SetCurrentValue(IsOnProperty, !IsOn);
        e.Handled = true;
    }

    private void Render()
    {
        this.Get<Border>("PaddleOn").Opacity = _on;
        this.Get<Border>("PaddleOff").Opacity = 1 - _on * 0.999;
        this.Get<TextBlock>("OnMark").Classes.Set("Active", IsOn);
        this.Get<TextBlock>("OffMark").Classes.Set("Active", !IsOn);

        var pilot = this.Get<Ellipse>("Pilot");
        this.Get<Ellipse>("PilotHalo").Opacity = _on;
        if (_on > 0.5) pilot.Fill = new SolidColorBrush(Neon);
        else pilot.BindBrush(Shape.FillProperty, this, "SkLedOff");
    }
}

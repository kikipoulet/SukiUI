using System;
using Avalonia;
using Avalonia.Data;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Interactive 3D synthesizer deck backed by <see cref="SynthConsoleScene"/>.
/// Exposes bindable Avalonia properties for power, auto-pulse, lid, dial, and spectrum level;
/// 3D scene definition lives in <c>SynthConsole.Scene.cs</c>.
/// </summary>
public sealed class SynthConsole : Model3DControl<SynthConsoleScene>
{
    public static readonly StyledProperty<bool> IsPoweredProperty =
        AvaloniaProperty.Register<SynthConsole, bool>(nameof(IsPowered), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsAutoRunningProperty =
        AvaloniaProperty.Register<SynthConsole, bool>(nameof(IsAutoRunning), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsLidOpenProperty =
        AvaloniaProperty.Register<SynthConsole, bool>(nameof(IsLidOpen), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> DialTurnsProperty =
        AvaloniaProperty.Register<SynthConsole, double>(nameof(DialTurns), 0.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> LevelProperty =
        AvaloniaProperty.Register<SynthConsole, double>(nameof(Level), 0.65,
            coerce: (_, v) => Math.Clamp(v, 0, 1));

    static SynthConsole()
    {
        BindChannel<SynthConsole>(IsPoweredProperty, "power");
        BindChannel<SynthConsole>(IsAutoRunningProperty, "auto");
        BindChannel<SynthConsole>(IsLidOpenProperty, "lid");
        BindChannel<SynthConsole>(DialTurnsProperty, "dial");
        BindChannel<SynthConsole>(LevelProperty, "level");
    }

    public bool IsPowered
    {
        get => GetValue(IsPoweredProperty);
        set => SetValue(IsPoweredProperty, value);
    }

    public bool IsAutoRunning
    {
        get => GetValue(IsAutoRunningProperty);
        set => SetValue(IsAutoRunningProperty, value);
    }

    public bool IsLidOpen
    {
        get => GetValue(IsLidOpenProperty);
        set => SetValue(IsLidOpenProperty, value);
    }

    public double DialTurns
    {
        get => GetValue(DialTurnsProperty);
        set => SetValue(DialTurnsProperty, value);
    }

    public double Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    protected override void OnModelClicked(string id)
    {
        switch (id)
        {
            case "power":
                SetCurrentValue(IsPoweredProperty, !IsPowered);
                break;
            case "auto":
                SetCurrentValue(IsAutoRunningProperty, !IsAutoRunning);
                break;
            case "lid":
                SetCurrentValue(IsLidOpenProperty, !IsLidOpen);
                break;
            case "dial":
                SetCurrentValue(DialTurnsProperty, (DialTurns + 0.35) % 1.0);
                break;
        }
        base.OnModelClicked(id);
    }
}

using System;
using Avalonia;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct SyringeState(
    double ShownPosition,
    double Flow,
    bool IsValveOut,
    double Volume,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki syringe pump: glass barrel, plunger and a two-way valve (In / Out) on top.
/// It displays the state reported by the device and reports the user's intent on its own parts:
/// dragging the plunger previews a position and requests it on release, clicking the valve requests a toggle.
/// The host decides whether to honour a request; a refused position snaps back to <see cref="Position"/>.
/// Drawing and animation live in <c>Syringe.Rendering.cs</c>.
/// </summary>
public partial class Syringe : HyperControl<SyringeState>
{
    private const double BarrelTop = 50, BarrelHeight = 200;

    /// <summary>Plunger position reported by the device: 0 = empty .. 1 = full.</summary>
    public static readonly StyledProperty<double> PositionProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Position), coerce: (_, v) => Math.Clamp(v, 0, 1));

    /// <summary>
    /// Liquid flowing through the valve: positive = filling (towards the barrel), negative = emptying,
    /// 0 = still. The magnitude (0..1) is the speed of the stream.
    /// </summary>
    public static readonly StyledProperty<double> FlowProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Flow), coerce: (_, v) => Math.Clamp(v, -1, 1));

    /// <summary>Valve position reported by the device: false = In, true = Out.</summary>
    public static readonly StyledProperty<bool> IsValveOutProperty =
        AvaloniaProperty.Register<Syringe, bool>(nameof(IsValveOut));

    /// <summary>Barrel volume in mL, used for the scale labels.</summary>
    public static readonly StyledProperty<double> VolumeProperty =
        AvaloniaProperty.Register<Syringe, double>(nameof(Volume), 10);

    static Syringe() => AffectsRenderState(PositionProperty, FlowProperty, IsValveOutProperty, VolumeProperty);

    private bool _dragging;
    private double _dragPreview;

    public Syringe() => DetachedFromVisualTree += (_, _) => CancelDrag();

    public double Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public double Flow
    {
        get => GetValue(FlowProperty);
        set => SetValue(FlowProperty, value);
    }

    public bool IsValveOut
    {
        get => GetValue(IsValveOutProperty);
        set => SetValue(IsValveOutProperty, value);
    }

    public double Volume
    {
        get => GetValue(VolumeProperty);
        set => SetValue(VolumeProperty, value);
    }

    /// <summary>Raised when the user releases the plunger: requested position (0..1).</summary>
    public event EventHandler<double>? PositionRequested;

    /// <summary>Raised when the user clicks the valve: the host may switch <see cref="IsValveOut"/>.</summary>
    public event EventHandler? ValveToggleRequested;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var p = e.GetPosition(this);

        if (p.X >= 110 && p.X <= 190 && p.Y >= 50 && p.Y <= 275)
        {
            _dragging = true;
            _dragPreview = Math.Clamp((p.Y - BarrelTop) / BarrelHeight, 0, 1);
            e.Pointer.Capture(this);
            e.Handled = true;
            Refresh();
        }
        else if (p.X >= 60 && p.X <= 240 && p.Y >= 6 && p.Y <= 42)
        {
            ValveToggleRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging) return;
        _dragPreview = Math.Clamp((e.GetPosition(this).Y - BarrelTop) / BarrelHeight, 0, 1);
        Refresh();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        var requested = _dragPreview;
        PositionRequested?.Invoke(this, requested);
        Refresh();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        CancelDrag();
    }

    private void CancelDrag()
    {
        if (!_dragging) return;
        _dragging = false;
        Refresh();
    }

    protected override SyringeState CaptureState() =>
        new(_dragging ? _dragPreview : Position, Flow, IsValveOut, Volume, SkeumoSkia.CapturePalette(this));
}

using System.Diagnostics;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public readonly record struct GuardedToggleState(
    bool IsOn,
    bool RequiresCover,
    bool CoverOpen,
    long CoverOpenedStamp,
    string? OnLabel,
    string? OffLabel,
    SkeumoPalette Palette);

/// <summary>
/// Skeumo-Suki industrial toggle switch with a safety cover. With <see cref="RequiresCover"/>, the first click lifts
/// the cover and only the next one throws the lever; the cover drops back by itself after a few idle seconds.
/// A click or Space acts; it only reports the chosen state. Drawing and animation live in <c>GuardedToggle.Rendering.cs</c>.
/// </summary>
public partial class GuardedToggle : HyperControl<GuardedToggleState>
{
    private const double AutoCloseSeconds = 4.0;

    /// <summary>Switch state: lever up = on, lever down = off.</summary>
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<GuardedToggle, bool>(nameof(IsOn), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>When true, the safety cover must be lifted before the lever can be thrown.</summary>
    public static readonly StyledProperty<bool> RequiresCoverProperty =
        AvaloniaProperty.Register<GuardedToggle, bool>(nameof(RequiresCover), true);

    public static readonly StyledProperty<string?> OnLabelProperty =
        AvaloniaProperty.Register<GuardedToggle, string?>(nameof(OnLabel), "ON");

    public static readonly StyledProperty<string?> OffLabelProperty =
        AvaloniaProperty.Register<GuardedToggle, string?>(nameof(OffLabel), "OFF");

    static GuardedToggle() => AffectsRenderState(IsOnProperty, RequiresCoverProperty, OnLabelProperty, OffLabelProperty);

    private bool _coverOpen;
    private long _coverOpenedStamp;

    public GuardedToggle()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public bool RequiresCover
    {
        get => GetValue(RequiresCoverProperty);
        set => SetValue(RequiresCoverProperty, value);
    }

    public string? OnLabel
    {
        get => GetValue(OnLabelProperty);
        set => SetValue(OnLabelProperty, value);
    }

    public string? OffLabel
    {
        get => GetValue(OffLabelProperty);
        set => SetValue(OffLabelProperty, value);
    }

    private bool IsCoverCurrentlyOpen()
    {
        if (!_coverOpen) return false;
        var elapsed = (Stopwatch.GetTimestamp() - _coverOpenedStamp) / (double)Stopwatch.Frequency;
        if (elapsed >= AutoCloseSeconds)
        {
            _coverOpen = false;
            return false;
        }
        return true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        Act();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Space or Key.Enter)) return;
        Act();
        e.Handled = true;
    }

    private void Act()
    {
        if (RequiresCover && !IsCoverCurrentlyOpen())
        {
            _coverOpen = true;
            _coverOpenedStamp = Stopwatch.GetTimestamp();
            Refresh();
            return;
        }
        SetCurrentValue(IsOnProperty, !IsOn);
        if (RequiresCover)
        {
            _coverOpen = true;
            _coverOpenedStamp = Stopwatch.GetTimestamp();
            Refresh();
        }
    }

    protected override GuardedToggleState CaptureState() =>
        new(IsOn, RequiresCover, IsCoverCurrentlyOpen(), _coverOpenedStamp, OnLabel, OffLabel, SkeumoSkia.CapturePalette(this));
}

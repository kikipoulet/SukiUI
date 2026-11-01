using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace SukiUI.Theme;

/// <summary>
/// Caps a menu popup's scrollable area to the work area of the screen the owning window is actually
/// on, so long menus scroll instead of running off the display.
/// </summary>
/// <remarks>
/// <para>
/// Nothing in the menu templates bounds their height, so whether a long menu scrolls depends entirely
/// on <see cref="PopupRoot"/> clamping its measure to <c>PlatformImpl.MaxAutoSizeHint</c>. On Win32
/// that hint is cached in <c>PopupImpl</c> and invalidated only on <c>WM_DISPLAYCHANGE</c> -- not on
/// <c>WM_DPICHANGED</c> and not when the popup moves to a monitor with a different scaling factor, so
/// a stale value leaves the scroll viewer unbounded and the popup clipped by the screen instead.
/// </para>
/// <para>
/// Reading the screen ourselves sidesteps that path entirely. <c>Screen.Scaling</c> is
/// per-monitor, so the result is correct on mixed-DPI setups rather than merely correct on one.
/// </para>
/// </remarks>
public static class MenuPopupSizing
{
    /// <summary>
    /// Set on the scrollable element inside a menu popup. Applied on attach to the visual tree, which
    /// Avalonia raises once per open: <c>Popup.Open()</c> parents the child and <c>Close()</c> unparents
    /// it, so the cap is recomputed every time the menu is shown and follows the window between monitors.
    /// </summary>
    public static readonly AttachedProperty<bool> ConstrainToScreenProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "ConstrainToScreen",
            typeof(MenuPopupSizing));

    /// <summary>
    /// Height allowance, in DIPs, for the chrome wrapping the scrollable element -- the shadow border
    /// margins the menu templates apply, and the offset of the popup from the top of the work area.
    /// This is a flat allowance rather than a measurement because the cap is applied before the popup's
    /// first layout pass, when no ancestor bounds exist yet.
    /// </summary>
    public static readonly AttachedProperty<double> ScreenInsetProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "ScreenInset",
            typeof(MenuPopupSizing),
            48d);

    static MenuPopupSizing()
    {
        ConstrainToScreenProperty.Changed.AddClassHandler<Control>(OnConstrainToScreenChanged);
    }

    public static bool GetConstrainToScreen(Control control) =>
        control.GetValue(ConstrainToScreenProperty);

    public static void SetConstrainToScreen(Control control, bool value) =>
        control.SetValue(ConstrainToScreenProperty, value);

    public static double GetScreenInset(Control control) =>
        control.GetValue(ScreenInsetProperty);

    public static void SetScreenInset(Control control, double value) =>
        control.SetValue(ScreenInsetProperty, value);

    private static void OnConstrainToScreenChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        control.AttachedToVisualTree -= OnAttachedToVisualTree;

        if (e.NewValue is true)
        {
            control.AttachedToVisualTree += OnAttachedToVisualTree;

            if (TopLevel.GetTopLevel(control) is not null)
            {
                Apply(control);
            }
        }
        else
        {
            control.ClearValue(Layoutable.MaxHeightProperty);
        }
    }

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            Apply(control);
        }
    }

    private static void Apply(Control control)
    {
        var available = GetAvailableHeight(control);

        if (double.IsFinite(available) && available > 0)
        {
            control.SetCurrentValue(Layoutable.MaxHeightProperty, available);
        }
        else
        {
            control.ClearValue(Layoutable.MaxHeightProperty);
        }
    }

    private static double GetAvailableHeight(Control control)
    {
        var root = TopLevel.GetTopLevel(control);

        // From inside popup content this returns the PopupRoot, not the window that owns it. The owner
        // is what has a real on-screen position to resolve a monitor from.
        var owner = root is PopupRoot popupRoot
            ? popupRoot.ParentTopLevel
            : root;

        if (owner?.Screens?.ScreenFromVisual(owner) is not { } screen || screen.Scaling <= 0)
        {
            return double.PositiveInfinity;
        }

        var available = (screen.WorkingArea.Height / screen.Scaling) - GetScreenInset(control);

        // MaxHeight applies in the content's own coordinate space, which sits underneath the scale
        // Popup.InheritsTransform hands the host (see PopupBehaviorStyles.axaml). Without dividing it
        // out, a scaled window would let the menu grow past the screen again.
        if (root is PopupRoot { Transform: { Value.M22: var scaleY and > 0 } })
        {
            available /= scaleY;
        }

        return available;
    }
}

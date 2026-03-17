using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using SukiUI.Controls;
using SukiUI.Helpers;

namespace SukiUI.Animations;

public static class LoadingBehavior
{
    private const int AnimationDurationMs = 200;

    private static readonly Dictionary<Control, PopupData> _popups = new();
    private static readonly Dictionary<Control, double> _originalOpacities = new();

    public static readonly AttachedProperty<bool> IsBusyProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsBusy",
            typeof(LoadingBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<double> DimOpacityProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "DimOpacity",
            typeof(LoadingBehavior),
            defaultValue: 0.3);

    static LoadingBehavior()
    {
        IsBusyProperty.Changed.AddClassHandler<Control>(OnIsBusyChanged);
    }

    public static void SetIsBusy(Control element, bool value)
        => element.SetValue(IsBusyProperty, value);

    public static bool GetIsBusy(Control element)
        => element.GetValue(IsBusyProperty);

    public static void SetDimOpacity(Control element, double value)
        => element.SetValue(DimOpacityProperty, value);

    public static double GetDimOpacity(Control element)
        => element.GetValue(DimOpacityProperty);

    private static void OnIsBusyChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isBusy = (bool)e.NewValue!;

        if (!control.IsAttachedToVisualTree())
        {
            if (isBusy)
            {
                _originalOpacities[control] = control.Opacity;
                control.Animate(Visual.OpacityProperty)
                    .From(control.Opacity)
                    .To(GetDimOpacity(control))
                    .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                    .Start();
            }
            else if (_originalOpacities.TryGetValue(control, out var originalOpacity))
            {
                control.Animate(Visual.OpacityProperty)
                    .From(control.Opacity)
                    .To(originalOpacity)
                    .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                    .Start();
                _originalOpacities.Remove(control);
            }
            return;
        }

        if (isBusy)
        {
            ShowBusy(control);
        }
        else
        {
            HideBusy(control);
        }
    }

    private static void ShowBusy(Control control)
    {
        var originalOpacity = control.Opacity;
        var dimOpacity = GetDimOpacity(control);

        _originalOpacities[control] = originalOpacity;

        control.Animate(Visual.OpacityProperty)
            .From(originalOpacity)
            .To(dimOpacity)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .Start();

        control.IsHitTestVisible = false;

        var loading = new Loading
        {
            Width = 40,
            Height = 40,
            Opacity = 0
        };

        var popup = new Popup
        {
            PlacementTarget = control,
            Placement = PlacementMode.AnchorAndGravity,
            IsLightDismissEnabled = false,
            Child = loading
        };

        ((ISetLogicalParent)popup).SetParent(control);

        popup.Opened += async (_, _) =>
        {
            loading.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            await loading.Animate(Visual.OpacityProperty)
                .From(0)
                .To(1)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .RunAsync();
        };

        var scrollViewer = control.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        _popups[control] = new PopupData(popup, scrollViewer);

        control.AttachedToVisualTree += OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;

        popup.IsOpen = true;
    }

    private static async void HideBusy(Control control)
    {
        if (!_originalOpacities.TryGetValue(control, out var originalOpacity))
            return;

        var dimOpacity = GetDimOpacity(control);

        await control.Animate(Visual.OpacityProperty)
            .From(dimOpacity)
            .To(originalOpacity)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .RunAsync();

        control.Opacity = originalOpacity;
        _originalOpacities.Remove(control);
        control.IsHitTestVisible = true;

        if (_popups.TryGetValue(control, out var popupData) && popupData.Popup.Child is Loading loading)
        {
            await loading.Animate(Visual.OpacityProperty)
                .From(1)
                .To(0)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .RunAsync();

            popupData.Popup.IsOpen = false;
            popupData.Popup.Child = null;
            ((ISetLogicalParent)popupData.Popup).SetParent(null);

            if (popupData.ScrollViewer != null)
            {
                popupData.ScrollViewer.ScrollChanged -= OnScrollChanged;
            }

            _popups.Remove(control);
        }

        control.AttachedToVisualTree -= OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
    }

    private static void OnControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;
        if (!GetIsBusy(control)) return;

        if (_popups.TryGetValue(control, out var popupData) && !popupData.Popup.IsOpen)
        {
            popupData.Popup.IsOpen = true;
        }
    }

    private static void OnControlDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;

        if (_popups.TryGetValue(control, out var popupData))
        {
            popupData.Popup.IsOpen = false;
        }
    }

    private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        foreach (var kvp in _popups)
        {
            if (kvp.Value.ScrollViewer == scrollViewer && kvp.Value.Popup.IsOpen)
            {
                var popup = kvp.Value.Popup;
                var placement = popup.Placement;
                popup.Placement = PlacementMode.Center;
                popup.Placement = placement;
            }
        }
    }

    private record PopupData(Popup Popup, ScrollViewer? ScrollViewer);
}

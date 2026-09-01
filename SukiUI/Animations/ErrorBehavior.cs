using ShapePath = Avalonia.Controls.Shapes.Path;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.Content;
using SukiUI.Helpers;
using Avalonia.Animation.Easings;

namespace SukiUI.Animations;

public static class ErrorBehavior
{
    private const int AnimationDurationMs = 600;
    private const double ErrorOpacityTarget = 0.1;

    private static readonly Dictionary<Control, PopupData> _popups = new();

    /// <summary>
    /// Popups grouped by the <see cref="ScrollViewer"/> they scroll with, so a scroll event only
    /// touches the popups that need repositioning and only subscribes once per host.
    /// </summary>
    private static readonly Dictionary<ScrollViewer, List<PopupData>> _scrollHosts = new();

    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsActive",
            typeof(ErrorBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<Color> ColorProperty =
        AvaloniaProperty.RegisterAttached<Control, Color>(
            "Color",
            typeof(ErrorBehavior),
            defaultValue: Colors.Red);

    public static readonly AttachedProperty<double> ThicknessProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Thickness",
            typeof(ErrorBehavior),
            defaultValue: 2);

    public static readonly AttachedProperty<int> SpeedProperty =
        AvaloniaProperty.RegisterAttached<Control, int>(
            "Speed",
            typeof(ErrorBehavior),
            defaultValue: 2000);

    public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<Control, CornerRadius>(
            "CornerRadius",
            typeof(ErrorBehavior),
            defaultValue: new CornerRadius(13));

    public static readonly AttachedProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.RegisterAttached<Control, string?>(
            "ErrorMessage",
            typeof(ErrorBehavior),
            defaultValue: null!);

    public static readonly AttachedProperty<double> IconSizeProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "IconSize",
            typeof(ErrorBehavior),
            defaultValue: 28);

    public static readonly AttachedProperty<double> FontSizeProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "FontSize",
            typeof(ErrorBehavior),
            defaultValue: 14);

    static ErrorBehavior()
    {
        IsActiveProperty.Changed.AddClassHandler<Control>(OnIsActiveChanged);
    }

    public static void SetIsActive(Control element, bool value)
        => element.SetValue(IsActiveProperty, value);

    public static bool GetIsActive(Control element)
        => element.GetValue(IsActiveProperty);

    public static void SetColor(Control element, Color value)
        => element.SetValue(ColorProperty, value);

    public static Color GetColor(Control element)
        => element.GetValue(ColorProperty);

    public static void SetThickness(Control element, double value)
        => element.SetValue(ThicknessProperty, value);

    public static double GetThickness(Control element)
        => element.GetValue(ThicknessProperty);

    public static void SetSpeed(Control element, int value)
        => element.SetValue(SpeedProperty, value);

    public static int GetSpeed(Control element)
        => element.GetValue(SpeedProperty);

    public static void SetCornerRadius(Control element, CornerRadius value)
        => element.SetValue(CornerRadiusProperty, value);

    public static CornerRadius GetCornerRadius(Control element)
        => element.GetValue(CornerRadiusProperty);

    public static void SetErrorMessage(Control element, string? value)
        => element.SetValue(ErrorMessageProperty, value);

    public static string? GetErrorMessage(Control element)
        => element.GetValue(ErrorMessageProperty);

    public static void SetIconSize(Control element, double value)
        => element.SetValue(IconSizeProperty, value);

    public static double GetIconSize(Control element)
        => element.GetValue(IconSizeProperty);

    public static void SetFontSize(Control element, double value)
        => element.SetValue(FontSizeProperty, value);

    public static double GetFontSize(Control element)
        => element.GetValue(FontSizeProperty);

    private static void OnIsActiveChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isActive = (bool)e.NewValue!;

        if (isActive)
        {
            SubscribeToLifecycle(control);
            TryShowError(control);
        }
        else
        {
            control.LayoutUpdated -= DeferShowErrorOnLayout;
            HideError(control);
        }
    }

    private static void TryShowError(Control control)
    {
        if (!GetIsActive(control) || !control.IsAttachedToVisualTree())
            return;

        if (control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
        {
            control.LayoutUpdated -= DeferShowErrorOnLayout;
            control.LayoutUpdated += DeferShowErrorOnLayout;
            return;
        }

        control.LayoutUpdated -= DeferShowErrorOnLayout;
        ShowError(control);
    }

    private static void DeferShowErrorOnLayout(object? sender, EventArgs e)
    {
        if (sender is not Control control) return;
        TryShowError(control);
    }

    private static void ShowError(Control control)
    {
        if (_popups.TryGetValue(control, out var existingPopup))
        {
            if (!existingPopup.IsHiding)
                return;

            ReleasePopup(control, existingPopup, restoreOpacity: true);
        }

        var originalOpacity = control.Opacity;

        var width = control.Bounds.Width + 3;
        var height = control.Bounds.Height + 3;
        var color = GetColor(control);
        var thickness = GetThickness(control);
        var cornerRadius = GetCornerRadius(control).TopLeft;
        var speed = GetSpeed(control);
        var errorMessage = GetErrorMessage(control) ?? string.Empty;
        var iconSize = GetIconSize(control);
        var fontSize = GetFontSize(control);

        var gradientStops = new GradientStops
        {
            new GradientStop { Color = Colors.Transparent, Offset = 0 },
            new GradientStop { Color = new Color((byte)(color.A / 4), color.R, color.G, color.B), Offset = 0.6 },
            new GradientStop { Color = new Color((byte)(color.A / 1.2), color.R, color.G, color.B), Offset = 1 }
        };

        var lineGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = gradientStops
        };

        var errorIcon = new ShapePath
        {
            Data = Icons.Error,
            Fill = new SolidColorBrush(color),
            Width = iconSize,
            Height = iconSize,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var messageText = new TextBlock
        {
            Text = errorMessage,
            Foreground = new SolidColorBrush(color),
            FontWeight = FontWeight.DemiBold, TextWrapping = TextWrapping.Wrap,
            FontSize = fontSize,
            Width = width - 30, TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };

        var contentStack = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { errorIcon, messageText }
        };

        var backgroundRect = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb((byte)(color.A * 0.1), color.R, color.G, color.B)),
            CornerRadius = new CornerRadius(cornerRadius)
        };

        var innerBorder = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = new SolidColorBrush(Color.FromArgb((byte)(color.A * 0.5), color.R, color.G, color.B)),
            BorderThickness = new Thickness(thickness),
            CornerRadius = new CornerRadius(cornerRadius),
            Child = backgroundRect
        };

        var outerBorder = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = lineGradient,
            BorderThickness = new Thickness(thickness),
            CornerRadius = new CornerRadius(cornerRadius),
            Background = Brushes.Transparent,
            Opacity = 0,
            Child = innerBorder
        };

        var popupContent = new Grid
        {
            Children = { outerBorder, contentStack }
        };

        var popup = new Popup
        {
            PlacementTarget = control,
            Placement = PlacementMode.Center,
            IsLightDismissEnabled = false,
            Child = popupContent
        };

        ((ISetLogicalParent)popup).SetParent(control);

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        var currentAngle = 0.0;
        var angleStep = 360.0 / (speed / 16.0);

        var scrollViewer = control.FindAncestorOfType<ScrollViewer>();
        var popupData = new PopupData(popup, timer, scrollViewer, originalOpacity);

        timer.Tick += (_, _) =>
        {
            if (!_popups.TryGetValue(control, out var current) ||
                !ReferenceEquals(current, popupData) || !popup.IsOpen)
            {
                timer.Stop();
                return;
            }

            currentAngle = (currentAngle + angleStep) % 360;
            lineGradient.EndPoint = new RelativePoint(
                (float)(0.5 + 0.5 * Math.Cos(currentAngle * Math.PI / 180)),
                (float)(0.5 + 0.5 * Math.Sin(currentAngle * Math.PI / 180)),
                RelativeUnit.Relative);
        };

        popup.Opened += (_, _) =>
        {
            if (!_popups.TryGetValue(control, out var current) ||
                !ReferenceEquals(current, popupData) || !popup.IsOpen)
                return;

            popupContent.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            _ = AnimateOpacityAsync(outerBorder, 0, 1, popupData.AnimationToken);

            timer.Start();
        };

        RegisterScrollHost(popupData);

        _popups[control] = popupData;

        _ = AnimateOpacityAsync(control, originalOpacity, ErrorOpacityTarget, popupData.AnimationToken);

        popup.IsOpen = true;
    }

    private static async void HideError(Control control)
    {
        if (!_popups.TryGetValue(control, out var popupData))
        {
            UnsubscribeFromLifecycle(control);
            return;
        }

        popupData.IsHiding = true;
        popupData.Timer.Stop();

        // Read the live opacities before cancelling: cancelling an in-flight fade reverts the
        // animated value to its pre-animation base, which would make these fades no-ops.
        var outerBorder = popupData.Popup.Child is Grid popupGrid
            ? popupGrid.Children[0] as Border
            : null;
        var borderFadeFrom = outerBorder?.Opacity ?? 0;
        var controlFadeFrom = control.Opacity;

        var cancellationToken = popupData.BeginAnimation();

        if (outerBorder != null)
        {
            await AnimateOpacityAsync(outerBorder, borderFadeFrom, 0, cancellationToken);
        }

        if (!IsCurrentInactivePopup(control, popupData))
            return;

        popupData.Popup.IsOpen = false;
        await AnimateOpacityAsync(control, controlFadeFrom, popupData.OriginalOpacity, cancellationToken);

        if (!IsCurrentInactivePopup(control, popupData))
            return;

        control.SetCurrentValue(Visual.OpacityProperty, popupData.OriginalOpacity);
        ReleasePopup(control, popupData, restoreOpacity: false);
        UnsubscribeFromLifecycle(control);
    }

    private static void OnControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
            TryShowError(control);
    }

    private static void OnControlDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;

        if (_popups.TryGetValue(control, out var popupData))
        {
            ReleasePopup(control, popupData, restoreOpacity: true);
        }

        control.LayoutUpdated -= DeferShowErrorOnLayout;
        if (!GetIsActive(control))
            UnsubscribeFromLifecycle(control);
    }

    private static bool IsCurrentInactivePopup(Control control, PopupData popupData) =>
        !GetIsActive(control) && _popups.TryGetValue(control, out var current) &&
        ReferenceEquals(current, popupData);

    private static void SubscribeToLifecycle(Control control)
    {
        control.AttachedToVisualTree -= OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        control.AttachedToVisualTree += OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
    }

    private static void UnsubscribeFromLifecycle(Control control)
    {
        control.AttachedToVisualTree -= OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
    }

    private static void ReleasePopup(Control control, PopupData popupData, bool restoreOpacity)
    {
        popupData.CancelAnimations();
        popupData.Timer.Stop();
        popupData.Popup.IsOpen = false;
        popupData.Popup.Child = null;
        ((ISetLogicalParent)popupData.Popup).SetParent(null);

        UnregisterScrollHost(popupData);

        if (restoreOpacity)
            control.SetCurrentValue(Visual.OpacityProperty, popupData.OriginalOpacity);

        if (_popups.TryGetValue(control, out var current) && ReferenceEquals(current, popupData))
            _popups.Remove(control);
    }

    private static void RegisterScrollHost(PopupData popupData)
    {
        if (popupData.ScrollViewer is not { } scrollViewer) return;

        if (!_scrollHosts.TryGetValue(scrollViewer, out var hosted))
        {
            hosted = new List<PopupData>();
            _scrollHosts[scrollViewer] = hosted;
            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        hosted.Add(popupData);
    }

    private static void UnregisterScrollHost(PopupData popupData)
    {
        if (popupData.ScrollViewer is not { } scrollViewer) return;
        if (!_scrollHosts.TryGetValue(scrollViewer, out var hosted)) return;

        hosted.Remove(popupData);

        if (hosted.Count != 0) return;

        _scrollHosts.Remove(scrollViewer);
        scrollViewer.ScrollChanged -= OnScrollChanged;
    }

    private static async Task AnimateOpacityAsync(Visual visual, double from, double to,
        CancellationToken cancellationToken)
    {
        try
        {
            await visual.Animate(Visual.OpacityProperty)
                .From(from)
                .To(to)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .WithEasing(new CubicEaseInOut())
                .WithCancellationToken(cancellationToken)
                .RunAsync();
        }
        catch (OperationCanceledException)
        {
            // Defensive: Avalonia signals cancellation by completing the task rather than
            // throwing, so a detach or fast reactivation normally just returns early here.
        }
    }

    private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        if (!_scrollHosts.TryGetValue(scrollViewer, out var hosted)) return;

        // Reverse index walk: re-placing a popup can close it, which unregisters it from this list.
        for (var i = hosted.Count - 1; i >= 0; i--)
        {
            if (i >= hosted.Count) continue;

            var popup = hosted[i].Popup;
            if (!popup.IsOpen) continue;

            var placement = popup.Placement;
            popup.Placement = PlacementMode.AnchorAndGravity;
            popup.Placement = placement;
        }
    }

    private sealed class PopupData(Popup popup, DispatcherTimer timer, ScrollViewer? scrollViewer,
        double originalOpacity)
    {
        private CancellationTokenSource? _animationCancellation = new();

        public Popup Popup { get; } = popup;
        public DispatcherTimer Timer { get; } = timer;
        public ScrollViewer? ScrollViewer { get; } = scrollViewer;
        public double OriginalOpacity { get; } = originalOpacity;
        public bool IsHiding { get; set; }

        /// <summary>
        /// Token for the current animation batch, or an already-cancelled token once released.
        /// Never touches a disposed source, so callers cannot trip an <see cref="ObjectDisposedException"/>.
        /// </summary>
        public CancellationToken AnimationToken =>
            _animationCancellation?.Token ?? new CancellationToken(true);

        public CancellationToken BeginAnimation()
        {
            CancelAnimations();
            _animationCancellation = new CancellationTokenSource();
            return _animationCancellation.Token;
        }

        public void CancelAnimations()
        {
            var cancellation = Interlocked.Exchange(ref _animationCancellation, null);
            if (cancellation is null) return;

            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}

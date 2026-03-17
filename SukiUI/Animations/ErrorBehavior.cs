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
    private static readonly Dictionary<Control, double> _originalOpacities = new();

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
            ShowError(control);
        }
        else
        {
            HideError(control);
        }
    }

    private static void ShowError(Control control)
    {
        ((Visual)control).Animate(Visual.OpacityProperty)
            .From(control.Opacity)
            .To(ErrorOpacityTarget)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();

        _originalOpacities[control] = control.Opacity;

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

        timer.Tick += (_, _) =>
        {
            if (!_popups.ContainsKey(control) || !popup.IsOpen)
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
            popupContent.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            outerBorder.Animate(Visual.OpacityProperty)
                .From(0)
                .To(1)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .Start();

            timer.Start();
        };

        var scrollViewer = control.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        _popups[control] = new PopupData(popup, timer, scrollViewer);

        control.AttachedToVisualTree += OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;

        popup.IsOpen = true;
    }

    private static async void HideError(Control control)
    {
        if (!_popups.TryGetValue(control, out var popupData))
            return;

        var popup = popupData.Popup;
        var timer = popupData.Timer;

        timer?.Stop();

        if (popup.Child is Grid popupGrid && popupGrid.Children[0] is Border outerBorder)
        {
            await outerBorder.Animate(Visual.OpacityProperty)
                .From(1)
                .To(0)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .RunAsync();

            popup.IsOpen = false;
            popup.Child = null;
            ((ISetLogicalParent)popup).SetParent(null);
            _popups.Remove(control);
        }

        if (popupData.ScrollViewer != null)
        {
            popupData.ScrollViewer.ScrollChanged -= OnScrollChanged;
        }

        if (_originalOpacities.TryGetValue(control, out var originalOpacity))
        {
            ((Visual)control).Animate(Visual.OpacityProperty)
                .From(control.Opacity)
                .To(originalOpacity)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .WithEasing(new CubicEaseInOut())
                .Start();

            _originalOpacities.Remove(control);
        }

        control.AttachedToVisualTree -= OnControlAttachedToVisualTree;
        control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
    }

    private static void OnControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;
        if (!GetIsActive(control)) return;

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
            popupData.Timer?.Stop();
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
                popup.Placement = PlacementMode.AnchorAndGravity;
                popup.Placement = placement;
            }
        }
    }

    private record PopupData(Popup Popup, DispatcherTimer Timer, ScrollViewer? ScrollViewer);
}

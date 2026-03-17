using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.Helpers;

namespace SukiUI.Animations;

public static class GlowBehavior
{
    private const int AnimationDurationMs = 200;

    private static readonly Dictionary<Control, PopupData> _popups = new();

    public static readonly AttachedProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsActive",
            typeof(GlowBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<Color> ColorProperty =
        AvaloniaProperty.RegisterAttached<Control, Color>(
            "Color",
            typeof(GlowBehavior),
            defaultValue: Colors.DodgerBlue);

    public static readonly AttachedProperty<double> ThicknessProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Thickness",
            typeof(GlowBehavior),
            defaultValue: 2);

    public static readonly AttachedProperty<int> SpeedProperty =
        AvaloniaProperty.RegisterAttached<Control, int>(
            "Speed",
            typeof(GlowBehavior),
            defaultValue: 2000);

    public static readonly AttachedProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.RegisterAttached<Control, CornerRadius>(
            "CornerRadius",
            typeof(GlowBehavior),
            defaultValue: new CornerRadius(13));

    static GlowBehavior()
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

    private static void OnIsActiveChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isActive = (bool)e.NewValue!;

        if (isActive)
        {
            ShowContour(control);
        }
        else
        {
            HideContour(control);
        }
    }

    private static void ShowContour(Control control)
    {
        var width = control.Bounds.Width + 3;
        var height = control.Bounds.Height + 3;
        var color = GetColor(control);
        var thickness = GetThickness(control);
        var cornerRadius = GetCornerRadius(control).TopLeft;
        var speed = GetSpeed(control);

        
        var gradientStops = new GradientStops
        {
            new GradientStop { Color = Colors.Transparent, Offset = 0 },
            
            new GradientStop { Color = new Color((byte)(color.A / 4), color.R, color.G, color.B), Offset = 0.6 },
            new GradientStop { Color = new Color((byte)(color.A / 1.2), color.R, color.G, color.B), Offset = 1 },
            
            
        };

        var lineGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = gradientStops
        };

        var border = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = lineGradient,
            BorderThickness = new Thickness(thickness),
            CornerRadius = new CornerRadius(cornerRadius),
            Background = Brushes.Transparent,
            Opacity = 0
        };

        var popup = new Popup
        {
            PlacementTarget = control,
            Placement = PlacementMode.Center,
            IsLightDismissEnabled = false,
            Child = border
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
            border.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));
            
            border.Animate(Visual.OpacityProperty)
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

    private static async void HideContour(Control control)
    {
        if (!_popups.TryGetValue(control, out var popupData))
            return;

        var popup = popupData.Popup;
        var timer = popupData.Timer;

        timer?.Stop();

        if (popup.Child is Border border)
        {
            await border.Animate(Visual.OpacityProperty)
                .From(1)
                .To(0)
                .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
                .RunAsync();

            popup.IsOpen = false;
            popup.Child = null;
            ((ISetLogicalParent)popup).SetParent(null);

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

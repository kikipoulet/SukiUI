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
            SubscribeToLifecycle(control);
            TryShowContour(control);
        }
        else
        {
            control.LayoutUpdated -= DeferShowContourOnLayout;
            HideContour(control);
        }
    }

    private static void TryShowContour(Control control)
    {
        if (!GetIsActive(control) || !control.IsAttachedToVisualTree())
            return;

        if (control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
        {
            control.LayoutUpdated -= DeferShowContourOnLayout;
            control.LayoutUpdated += DeferShowContourOnLayout;
            return;
        }

        control.LayoutUpdated -= DeferShowContourOnLayout;
        ShowContour(control);
    }

    private static void DeferShowContourOnLayout(object? sender, EventArgs e)
    {
        if (sender is not Control control) return;
        TryShowContour(control);
    }

    private static void ShowContour(Control control)
    {
        if (_popups.TryGetValue(control, out var existingPopup))
        {
            if (!existingPopup.IsHiding)
                return;

            ReleasePopup(control, existingPopup);
        }

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
            Height = height, IsHitTestVisible = false,
            BorderBrush = lineGradient,
            BorderThickness = new Thickness(thickness),
            CornerRadius = new CornerRadius(cornerRadius),
            Background = Brushes.Transparent,
            Opacity = 0
        };

        var popup = new Popup
        {
            PlacementTarget = control, IsHitTestVisible = false,
            Placement = PlacementMode.Center, ShouldUseOverlayLayer = true,
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

        var scrollViewer = control.FindAncestorOfType<ScrollViewer>();
        var popupData = new PopupData(popup, timer, scrollViewer);

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

            border.Measure(new Avalonia.Size(double.PositiveInfinity, double.PositiveInfinity));

            _ = AnimateOpacityAsync(border, 0, 1, popupData.AnimationToken);

            timer.Start();
        };

        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        _popups[control] = popupData;

        popup.IsOpen = true;
    }

    private static async void HideContour(Control control)
    {
        if (!_popups.TryGetValue(control, out var popupData))
        {
            UnsubscribeFromLifecycle(control);
            return;
        }

        popupData.IsHiding = true;
        popupData.Timer.Stop();
        var cancellationToken = popupData.BeginAnimation();

        if (popupData.Popup.Child is Border border)
        {
            await AnimateOpacityAsync(border, border.Opacity, 0, cancellationToken);
        }

        if (!_popups.TryGetValue(control, out var current) ||
            !ReferenceEquals(current, popupData) || GetIsActive(control))
            return;

        ReleasePopup(control, popupData);
        UnsubscribeFromLifecycle(control);
    }

    private static void OnControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
            TryShowContour(control);
    }

    private static void OnControlDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control) return;

        if (_popups.TryGetValue(control, out var popupData))
        {
            ReleasePopup(control, popupData);
        }

        control.LayoutUpdated -= DeferShowContourOnLayout;
        if (!GetIsActive(control))
            UnsubscribeFromLifecycle(control);
    }

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

    private static void ReleasePopup(Control control, PopupData popupData)
    {
        popupData.CancelAnimations();
        popupData.Timer.Stop();
        popupData.Popup.IsOpen = false;
        popupData.Popup.Child = null;
        ((ISetLogicalParent)popupData.Popup).SetParent(null);

        if (popupData.ScrollViewer != null)
            popupData.ScrollViewer.ScrollChanged -= OnScrollChanged;

        if (_popups.TryGetValue(control, out var current) && ReferenceEquals(current, popupData))
            _popups.Remove(control);
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
                .WithCancellationToken(cancellationToken)
                .RunAsync();
        }
        catch (OperationCanceledException)
        {
            // A detach or fast reactivation superseded this animation.
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

    private sealed class PopupData(Popup popup, DispatcherTimer timer, ScrollViewer? scrollViewer)
    {
        private CancellationTokenSource _animationCancellation = new();

        public Popup Popup { get; } = popup;
        public DispatcherTimer Timer { get; } = timer;
        public ScrollViewer? ScrollViewer { get; } = scrollViewer;
        public bool IsHiding { get; set; }
        public CancellationToken AnimationToken => _animationCancellation.Token;

        public CancellationToken BeginAnimation()
        {
            CancelAnimations();
            _animationCancellation = new CancellationTokenSource();
            return _animationCancellation.Token;
        }

        public void CancelAnimations()
        {
            _animationCancellation.Cancel();
            _animationCancellation.Dispose();
        }
    }
}

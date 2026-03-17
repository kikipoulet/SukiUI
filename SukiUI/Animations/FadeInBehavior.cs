using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class FadeInBehavior
{
    private const int DefaultDurationMs = 600;

    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "Enable",
            typeof(FadeInBehavior),
            defaultValue: false);

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>(
            "Duration",
            typeof(FadeInBehavior),
            defaultValue: TimeSpan.FromMilliseconds(DefaultDurationMs));

    public static readonly AttachedProperty<double> ScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "Scale",
            typeof(FadeInBehavior),
            defaultValue: 0.6);

    static FadeInBehavior()
    {
        EnableProperty.Changed.AddClassHandler<Control>(OnEnableChanged);
    }

    public static void SetEnable(Control element, bool value)
        => element.SetValue(EnableProperty, value);

    public static bool GetEnable(Control element)
        => element.GetValue(EnableProperty);

    public static void SetDuration(Control element, TimeSpan value)
        => element.SetValue(DurationProperty, value);

    public static TimeSpan GetDuration(Control element)
        => element.GetValue(DurationProperty);

    public static void SetScale(Control element, double value)
        => element.SetValue(ScaleProperty, value);

    public static double GetScale(Control element)
        => element.GetValue(ScaleProperty);

    private static void OnEnableChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true)
            return;

        if (control.IsAttachedToVisualTree())
        {
            RunFadeIn(control);
        }
        else
        {
            control.AttachedToVisualTree += OnAttachedToVisualTree;
        }
    }

    private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not Control control)
            return;

        control.AttachedToVisualTree -= OnAttachedToVisualTree;

        if (control.GetValue(EnableProperty))
        {
            RunFadeIn(control);
        }
    }

    private static void RunFadeIn(Control control)
    {
        var duration = GetDuration(control);
        var startScale = GetScale(control);

        if (startScale < 0) 
            startScale = 0;
        if (startScale > 1) 
            startScale = 1;

        control.Opacity = 0;

        var transform = new ScaleTransform(startScale, startScale);
        control.RenderTransform = transform;
        control.RenderTransformOrigin = RelativePoint.Center;

        var animation = new Animation
        {
            Duration = duration,
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, startScale),
                        new Setter(ScaleTransform.ScaleYProperty, startScale)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    }
                }
            }
        };

        animation.RunAsync(control).ContinueWith(_ =>
        {
            if (!control.GetValue(EnableProperty))
            {
                control.IsVisible = false;
            }
        });
    }
}

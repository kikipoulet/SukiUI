using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Animation.Easings;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class VisibilityBehavior
{
    private const int AnimationDurationMs = 600;
    
    private static readonly Dictionary<Control, (double Width, double Height)> _lastDimensions 
        = new();
    
    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsVisible",
            typeof(VisibilityBehavior),
            defaultValue: true);

    public static readonly AttachedProperty<double> HiddenScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "HiddenScale",
            typeof(VisibilityBehavior),
            defaultValue: 0.6);

    public static readonly AttachedProperty<bool> AnimateDimensionsProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "AnimateDimensions",
            typeof(VisibilityBehavior),
            defaultValue: true);

    static VisibilityBehavior()
    {
        IsVisibleProperty.Changed.AddClassHandler<Control>(OnIsVisibleChanged);
    }

    public static void SetIsVisible(Control element, bool value)
        => element.SetValue(IsVisibleProperty, value);

    public static bool GetIsVisible(Control element)
        => element.GetValue(IsVisibleProperty);

    public static void SetHiddenScale(Control element, double value)
        => element.SetValue(HiddenScaleProperty, value);

    public static double GetHiddenScale(Control element)
        => element.GetValue(HiddenScaleProperty);

    public static void SetAnimateDimensions(Control element, bool value)
        => element.SetValue(AnimateDimensionsProperty, value);

    public static bool GetAnimateDimensions(Control element)
        => element.GetValue(AnimateDimensionsProperty);

    private static void OnIsVisibleChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool newValue) return;
        var hiddenScale = GetHiddenScale(control);
        var animateDimensions = GetAnimateDimensions(control);

        if (!control.IsAttachedToVisualTree())
        {
            control.IsVisible = newValue;
            control.Opacity = newValue ? 1 : 0;
            SetScale(control, newValue ? 1.0 : hiddenScale);
            if (animateDimensions)
            {
                if (newValue && _lastDimensions.TryGetValue(control, out var dims))
                {
                    control.Width = dims.Width;
                    control.Height = dims.Height;
                }
                else
                {
                    control.Width = 0;
                    control.Height = 0;
                }
            }
            return;
        }

        if (newValue)
        {
            control.IsVisible = true;
            
            double? targetWidth = null;
            double? targetHeight = null;
            
            if (animateDimensions)
            {
                if (_lastDimensions.TryGetValue(control, out var dims))
                {
                    targetWidth = dims.Width;
                    targetHeight = dims.Height;
                }
                
                control.Width = 0;
                control.Height = 0;
            }
            
            RunAnimation(control, 0, 1, hiddenScale, 1.0, animateDimensions, 
                targetWidth, targetHeight, () =>
                {
                    if (!control.GetValue(IsVisibleProperty))
                    {
                        control.IsVisible = false;
                        if (animateDimensions)
                        {
                            control.Width = 0;
                            control.Height = 0;
                        }
                    }
                    else
                    {
                        control.Width = double.NaN;
                        control.Height = double.NaN;
                    }
                });
        }
        else
        {
            double currentWidth = 0;
            double currentHeight = 0;
            
            if (animateDimensions)
            {
                currentWidth = control.Bounds.Width;
                currentHeight = control.Bounds.Height;
                _lastDimensions[control] = (currentWidth, currentHeight);
            }

            RunAnimation(control, 1, 0, 1.0, hiddenScale, animateDimensions, 
                currentWidth, currentHeight, () =>
                {
                    if (!control.GetValue(IsVisibleProperty))
                    {
                        control.IsVisible = false;
                        if (animateDimensions)
                        {
                            control.Width = 0;
                            control.Height = 0;
                        }
                    }
                });
        }
    }

    private static void SetScale(Control control, double scale)
    {
        var transform = control.RenderTransform as ScaleTransform;
        if (transform == null)
        {
            transform = new ScaleTransform(scale, scale);
            control.RenderTransform = transform;
            control.RenderTransformOrigin = RelativePoint.Center;
        }
        else
        {
            transform.ScaleX = scale;
            transform.ScaleY = scale;
        }
    }

    private static void RunAnimation(Control control, double opacityFrom, double opacityTo, 
        double scaleFrom, double scaleTo, bool animateDimensions, double? targetWidth, 
        double? targetHeight, Action? onComplete = null)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, opacityFrom),
                        new Setter(ScaleTransform.ScaleXProperty, scaleFrom),
                        new Setter(ScaleTransform.ScaleYProperty, scaleFrom)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, opacityTo),
                        new Setter(ScaleTransform.ScaleXProperty, scaleTo),
                        new Setter(ScaleTransform.ScaleYProperty, scaleTo)
                    }
                }
            }
        };

        if (animateDimensions && opacityFrom > opacityTo)
        {
            animation.Children[0].Setters.Add(new Setter(Control.WidthProperty, targetWidth ?? 0d));
            animation.Children[0].Setters.Add(new Setter(Control.HeightProperty, targetHeight ?? 0d));
            animation.Children[1].Setters.Add(new Setter(Control.WidthProperty, 0d));
            animation.Children[1].Setters.Add(new Setter(Control.HeightProperty, 0d));
        }
        else if (animateDimensions && opacityFrom < opacityTo && targetWidth.HasValue && targetHeight.HasValue)
        {
            animation.Children[0].Setters.Add(new Setter(Control.WidthProperty, 0d));
            animation.Children[0].Setters.Add(new Setter(Control.HeightProperty, 0d));
            animation.Children[1].Setters.Add(new Setter(Control.WidthProperty, targetWidth.Value));
            animation.Children[1].Setters.Add(new Setter(Control.HeightProperty, targetHeight.Value));
        }

        animation.RunAsync(control).ContinueWith(_ =>
        {
            if (onComplete != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(onComplete);
            }
        });
    }
}

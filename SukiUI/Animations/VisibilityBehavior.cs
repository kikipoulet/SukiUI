using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.Animations;

public static class VisibilityBehavior
{
    private const int AnimationDurationMs = 600;

    private static readonly Dictionary<Control, (double Width, double Height)> _lastDimensions = new();
    private static readonly Dictionary<Control, (double? Width, double? Height)> _originalDimensions = new();

    public static readonly AttachedProperty<bool> IsVisibleProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsVisible", typeof(VisibilityBehavior), defaultValue: true);

    public static readonly AttachedProperty<double> HiddenScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double>(
            "HiddenScale", typeof(VisibilityBehavior), defaultValue: 0.6);

    public static readonly AttachedProperty<bool> AnimateDimensionsProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "AnimateDimensions", typeof(VisibilityBehavior), defaultValue: true);

    static VisibilityBehavior()
    {
        IsVisibleProperty.Changed.AddClassHandler<Control>(OnIsVisibleChanged);
    }

    public static void SetIsVisible(Control element, bool value) => element.SetValue(IsVisibleProperty, value);
    public static bool GetIsVisible(Control element) => element.GetValue(IsVisibleProperty);
    public static void SetHiddenScale(Control element, double value) => element.SetValue(HiddenScaleProperty, value);
    public static double GetHiddenScale(Control element) => element.GetValue(HiddenScaleProperty);
    public static void SetAnimateDimensions(Control element, bool value) => element.SetValue(AnimateDimensionsProperty, value);
    public static bool GetAnimateDimensions(Control element) => element.GetValue(AnimateDimensionsProperty);

    private static void OnIsVisibleChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool isVisible) 
            return;

        var hiddenScale = GetHiddenScale(control);
        var animateDims = GetAnimateDimensions(control);

        if (!control.IsAttachedToVisualTree())
        {
            ApplyInstantState(control, isVisible, hiddenScale, animateDims);
            return;
        }

        if (isVisible)
            AnimateShow(control, hiddenScale, animateDims);
        else
            AnimateHide(control, hiddenScale, animateDims);
    }

    private static void ApplyInstantState(Control control, bool isVisible, double hiddenScale, bool animateDims)
    {
        control.IsVisible = isVisible;
        control.Opacity = isVisible ? 1 : 0;
        SetScale(control, isVisible ? 1.0 : hiddenScale);

        if (animateDims)
        {
            SaveOriginalDimensions(control);

            if (isVisible)
            {
                if (!double.IsNaN(control.Width) && !double.IsNaN(control.Height))
                {
                     /* Keep explicit dimensions */
                }
                else if (_lastDimensions.TryGetValue(control, out var dims))
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
            else
            {
                control.Width = 0;
                control.Height = 0;
            }
        }

        control.DetachedFromVisualTree += (_, _) =>
        {
            _originalDimensions.Remove(control);
            _lastDimensions.Remove(control);
        };
    }

    private static void AnimateShow(Control control, double hiddenScale, bool animateDims)
    {
        control.IsVisible = true;

        if (!animateDims)
        {
            RunAnimation(control, 0, 1, hiddenScale, 1.0, false, null, null,
                () => OnShowComplete(control, animateDims));
            return;
        }

        SaveOriginalDimensions(control);
        var (targetWidth, targetHeight) = ResolveTargetDimensions(control);

        if (!targetWidth.HasValue || !targetHeight.HasValue)
        {
            RunAnimation(control, 0, 1, hiddenScale, 1.0, false, null, null, () =>
            {
                if (!control.GetValue(IsVisibleProperty))
                {
                    CollapseControl(control, animateDims);
                }
                else
                {
                    if (control.Bounds.Width > 0 && control.Bounds.Height > 0)
                        _lastDimensions[control] = (control.Bounds.Width, control.Bounds.Height);
                    control.Width = double.NaN;
                    control.Height = double.NaN;
                }
            });
            return;
        }

        control.Width = 0;
        control.Height = 0;

        RunAnimation(control, 0, 1, hiddenScale, 1.0, true, targetWidth, targetHeight,
            () => OnShowComplete(control, animateDims));
    }

    private static void AnimateHide(Control control, double hiddenScale, bool animateDims)
    {
        double currentWidth = 0, currentHeight = 0;

        if (animateDims)
        {
            SaveOriginalDimensions(control);
            currentWidth = control.Bounds.Width;
            currentHeight = control.Bounds.Height;
            _lastDimensions[control] = (currentWidth, currentHeight);
        }

        RunAnimation(control, 1, 0, 1.0, hiddenScale, animateDims, currentWidth, currentHeight,
            () => CollapseControl(control, animateDims));
    }

    private static void OnShowComplete(Control control, bool animateDims)
    {
        if (!control.GetValue(IsVisibleProperty))
        {
            CollapseControl(control, animateDims);
            return;
        }

        if (animateDims && _originalDimensions.TryGetValue(control, out var orig))
        {
            control.Width = orig.Width ?? double.NaN;
            control.Height = orig.Height ?? double.NaN;
        }
        else
        {
            control.Width = double.NaN;
            control.Height = double.NaN;
        }
    }

    private static void CollapseControl(Control control, bool animateDims)
    {
        control.IsVisible = false;
        if (animateDims)
        {
            control.Width = 0;
            control.Height = 0;
        }
    }

    private static (double? Width, double? Height) ResolveTargetDimensions(Control control)
    {
        if (_originalDimensions.TryGetValue(control, out var orig)
            && orig.Width.HasValue && orig.Height.HasValue
            && !double.IsNaN(orig.Width.Value) && !double.IsNaN(orig.Height.Value))
            return (orig.Width.Value, orig.Height.Value);

        if (_lastDimensions.TryGetValue(control, out var last))
            return (last.Width, last.Height);
        
        control.Width = double.NaN;
        control.Height = double.NaN;
        control.InvalidateMeasure();
        control.Measure(Size.Infinity);

        var w = control.DesiredSize.Width;
        var h = control.DesiredSize.Height;

        if (w == 0 && h == 0 && control.Bounds.Width > 0)
        {
            w = control.Bounds.Width;
            h = control.Bounds.Height;
        }

        if (w > 0 && h > 0)
        {
            _lastDimensions[control] = (w, h);
            return (w, h);
        }

        return (null, null);
    }

    private static void SaveOriginalDimensions(Control control)
    {
        if (!_originalDimensions.ContainsKey(control))
            _originalDimensions[control] = (control.Width, control.Height);
    }

    private static void SetScale(Control control, double scale)
    {
        if (control.RenderTransform is ScaleTransform transform)
        {
            transform.ScaleX = scale;
            transform.ScaleY = scale;
        }
        else
        {
            control.RenderTransform = new ScaleTransform(scale, scale);
            control.RenderTransformOrigin = RelativePoint.Center;
        }
    }

    private static void RunAnimation(Control control, double opacityFrom, double opacityTo,
        double scaleFrom, double scaleTo, bool animateDims, double? targetWidth,
        double? targetHeight, Action? onComplete = null)
    {
        var startFrame = new KeyFrame
        {
            Cue = new Cue(0.0),
            Setters =
            {
                new Setter(Visual.OpacityProperty, opacityFrom),
                new Setter(ScaleTransform.ScaleXProperty, scaleFrom),
                new Setter(ScaleTransform.ScaleYProperty, scaleFrom)
            }
        };

        var endFrame = new KeyFrame
        {
            Cue = new Cue(1.0),
            Setters =
            {
                new Setter(Visual.OpacityProperty, opacityTo),
                new Setter(ScaleTransform.ScaleXProperty, scaleTo),
                new Setter(ScaleTransform.ScaleYProperty, scaleTo)
            }
        };

        if (animateDims)
        {
            bool isShowing = opacityFrom < opacityTo;
            double fromW = isShowing ? 0d : (targetWidth ?? 0d);
            double fromH = isShowing ? 0d : (targetHeight ?? 0d);
            double toW = isShowing ? (targetWidth ?? 0d) : 0d;
            double toH = isShowing ? (targetHeight ?? 0d) : 0d;

            startFrame.Setters.Add(new Setter(Control.WidthProperty, fromW));
            startFrame.Setters.Add(new Setter(Control.HeightProperty, fromH));
            endFrame.Setters.Add(new Setter(Control.WidthProperty, toW));
            endFrame.Setters.Add(new Setter(Control.HeightProperty, toH));
        }

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            Children = { startFrame, endFrame }
        };

        animation.RunAsync(control).ContinueWith(_ =>
        {
            if (onComplete != null)
                Dispatcher.UIThread.Post(onComplete);
        });
    }
}

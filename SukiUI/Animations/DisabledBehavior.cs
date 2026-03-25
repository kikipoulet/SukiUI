using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Helpers;
using Avalonia.Animation.Easings;

namespace SukiUI.Animations;

public static class DisabledBehavior
{
    private const int AnimationDurationMs = 400;
    
    public static readonly AttachedProperty<double?> OpacityProperty =
        AvaloniaProperty.RegisterAttached<Control, double?>(
            "Opacity",
            typeof(DisabledBehavior),
            defaultValue: null);
    
    public static readonly AttachedProperty<double?> ScaleProperty =
        AvaloniaProperty.RegisterAttached<Control, double?>(
            "Scale",
            typeof(DisabledBehavior),
            defaultValue: null);

    static DisabledBehavior()
    {
        Control.IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
    }

    public static void SetOpacity(Control element, double? value)
        => element.SetValue(OpacityProperty, value);

    public static double? GetOpacity(Control element)
        => element.GetValue(OpacityProperty);
    
    public static void SetScale(Control element, double? value)
        => element.SetValue(ScaleProperty, value);

    public static double? GetScale(Control element)
        => element.GetValue(ScaleProperty);

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isEnabled = (bool)e.NewValue!;
        
        if (!isEnabled)
        {
            ApplyCustomOpacity(control);
            ApplyCustomScale(control);
        }
        else
        {
            RestoreOpacity(control);
            RestoreScale(control);
        }
    }

    private static void ApplyCustomOpacity(Control control)
    {
        var customOpacity = GetOpacity(control);
        
        if (!customOpacity.HasValue || customOpacity.Value == 1.0)
            return;
        
        ((Visual)control).Animate(Visual.OpacityProperty)
            .From(control.Opacity)
            .To(customOpacity.Value)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
    }

    private static void RestoreOpacity(Control control)
    {
        var customOpacity = GetOpacity(control);
        
        if (!customOpacity.HasValue || customOpacity.Value == 1.0)
            return;
        
        ((Visual)control).Animate(Visual.OpacityProperty)
            .From(control.Opacity)
            .To(1.0)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
    }
    
    private static void ApplyCustomScale(Control control)
    {
        var customScale = GetScale(control);
        
        if (!customScale.HasValue || customScale.Value == 1.0)
            return;
        
        if (!EnsureScaleTransform(control, out var scaleTransform))
            return;
        
        ((Visual)control).Animate(ScaleTransform.ScaleXProperty)
            .From(scaleTransform.ScaleX)
            .To(customScale.Value)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
        
        ((Visual)control).Animate(ScaleTransform.ScaleYProperty)
            .From(scaleTransform.ScaleY)
            .To(customScale.Value)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
    }
    
    private static void RestoreScale(Control control)
    {
        var customScale = GetScale(control);
        
        if (!customScale.HasValue || customScale.Value == 1.0)
            return;
        
        if (!EnsureScaleTransform(control, out var scaleTransform))
            return;
        
        ((Visual)control).Animate(ScaleTransform.ScaleXProperty)
            .From(scaleTransform.ScaleX)
            .To(1.0)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
        
        ((Visual)control).Animate(ScaleTransform.ScaleYProperty)
            .From(scaleTransform.ScaleY)
            .To(1.0)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
    }
    
    private static bool EnsureScaleTransform(Control control, out ScaleTransform scaleTransform)
    {
        if (control.RenderTransform is ScaleTransform existing)
        {
            scaleTransform = existing;
            return true;
        }
        
        if (control.RenderTransform == null)
        {
            scaleTransform = new ScaleTransform(1, 1);
            control.RenderTransform = scaleTransform;
            return true;
        }
        
        scaleTransform = null!;
        return false;
    }
}

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
    
    private static readonly Dictionary<Control, double> _originalOpacities = new();
    private static readonly Dictionary<Control, double> _originalScales = new();
    
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
        
        _originalOpacities[control] = control.Opacity;
        
        ((Visual)control).Animate(Visual.OpacityProperty)
            .From(control.Opacity)
            .To(customOpacity.Value)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
    }

    private static void RestoreOpacity(Control control)
    {
        if (!_originalOpacities.TryGetValue(control, out var originalOpacity))
            return;
        
        ((Visual)control).Animate(Visual.OpacityProperty)
            .From(control.Opacity)
            .To(originalOpacity)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
        
        _originalOpacities.Remove(control);
    }
    
    private static void ApplyCustomScale(Control control)
    {
        var customScale = GetScale(control);
        
        if (!customScale.HasValue || customScale.Value == 1.0)
            return;
        
        if (!EnsureScaleTransform(control, out var scaleTransform))
            return;
        
        // Sauvegarder la valeur actuelle de ScaleX (avant animation)
        _originalScales[control] = scaleTransform.ScaleX;
        
        // Animer ScaleX et ScaleY DIRECTEMENT sur le CONTROL, pas sur la ScaleTransform
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
        if (!_originalScales.TryGetValue(control, out var originalScale))
            return;
        
        if (!EnsureScaleTransform(control, out var scaleTransform))
            return;
        
        // Animer ScaleX et ScaleY DIRECTEMENT sur le CONTROL
        ((Visual)control).Animate(ScaleTransform.ScaleXProperty)
            .From(scaleTransform.ScaleX)
            .To(originalScale)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
        
        ((Visual)control).Animate(ScaleTransform.ScaleYProperty)
            .From(scaleTransform.ScaleY)
            .To(originalScale)
            .WithDuration(TimeSpan.FromMilliseconds(AnimationDurationMs))
            .WithEasing(new CubicEaseInOut())
            .Start();
        
        _originalScales.Remove(control);
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
        
        // Le contrôle a déjà un RenderTransform d'un autre type → on ne peut pas appliquer
        scaleTransform = null!;
        return false;
    }
}

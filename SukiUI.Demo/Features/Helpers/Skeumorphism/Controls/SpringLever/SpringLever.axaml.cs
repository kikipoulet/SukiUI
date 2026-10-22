using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki spring-loaded lever (jog). The user pushes it left or right; released, it springs back to the center.
/// It only reports the user's intent: what to do with it (response curve, anti-spam, safety) belongs to the host.
/// Drawing and animation live in <c>SpringLever.Rendering.cs</c>.
/// </summary>
public partial class SpringLever : UserControl
{
    /// <summary>
    /// Lever position set by the user: -1 (fully left) .. 1 (fully right), 0 inside the center dead zone.
    /// Set back to 0 immediately on release, before the spring-back animation.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SpringLever, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Label shown above the left end of the travel.</summary>
    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(LeftLabel));

    /// <summary>Label shown above the right end of the travel.</summary>
    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<SpringLever, string?>(nameof(RightLabel));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? LeftLabel
    {
        get => GetValue(LeftLabelProperty);
        set => SetValue(LeftLabelProperty, value);
    }

    public string? RightLabel
    {
        get => GetValue(RightLabelProperty);
        set => SetValue(RightLabelProperty, value);
    }

    /// <summary>Raised when the user lets go of the lever (pointer released, capture lost or key released).</summary>
    public event EventHandler? Released;

    public SpringLever()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

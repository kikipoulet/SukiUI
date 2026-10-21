using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki vertical switch with three notches: a knob slides in a dug Suki rail and the active label lights up.
/// Drag, click, mouse wheel and arrow keys change the notch. It only reports the chosen notch:
/// what it selects is up to the host. Drawing and animation live in <c>NotchSwitch.Rendering.cs</c>.
/// </summary>
public partial class NotchSwitch : UserControl
{
    /// <summary>Selected notch: 0 = bottom, 1 = middle, 2 = top.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<NotchSwitch, int>(nameof(SelectedIndex),
            defaultBindingMode: BindingMode.TwoWay, coerce: (_, v) => Math.Clamp(v, 0, 2));

    /// <summary>Label of the top notch (index 2).</summary>
    public static readonly StyledProperty<string?> TopLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(TopLabel));

    /// <summary>Label of the middle notch (index 1).</summary>
    public static readonly StyledProperty<string?> MiddleLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(MiddleLabel));

    /// <summary>Label of the bottom notch (index 0).</summary>
    public static readonly StyledProperty<string?> BottomLabelProperty =
        AvaloniaProperty.Register<NotchSwitch, string?>(nameof(BottomLabel));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string? TopLabel
    {
        get => GetValue(TopLabelProperty);
        set => SetValue(TopLabelProperty, value);
    }

    public string? MiddleLabel
    {
        get => GetValue(MiddleLabelProperty);
        set => SetValue(MiddleLabelProperty, value);
    }

    public string? BottomLabel
    {
        get => GetValue(BottomLabelProperty);
        set => SetValue(BottomLabelProperty, value);
    }

    public NotchSwitch()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

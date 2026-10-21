using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki horizontal three-position selector (OFF / SLOW / FAST for instance): a grooved cap slides
/// in a slot and clicks into each detent. Click a position, drag, mouse wheel or arrow keys. A terminal takes the wire
/// to the device. It only reports the chosen position. Drawing and animation live in <c>TriSwitch.Rendering.cs</c>.
/// </summary>
public partial class TriSwitch : UserControl
{
    /// <summary>Selected position: 0 = left, 1 = middle, 2 = right.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<TriSwitch, int>(nameof(SelectedIndex),
            defaultBindingMode: BindingMode.TwoWay, coerce: (_, v) => Math.Clamp(v, 0, 2));

    public static readonly StyledProperty<string?> LeftLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(LeftLabel), "OFF");

    public static readonly StyledProperty<string?> MiddleLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(MiddleLabel));

    public static readonly StyledProperty<string?> RightLabelProperty =
        AvaloniaProperty.Register<TriSwitch, string?>(nameof(RightLabel));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string? LeftLabel
    {
        get => GetValue(LeftLabelProperty);
        set => SetValue(LeftLabelProperty, value);
    }

    public string? MiddleLabel
    {
        get => GetValue(MiddleLabelProperty);
        set => SetValue(MiddleLabelProperty, value);
    }

    public string? RightLabel
    {
        get => GetValue(RightLabelProperty);
        set => SetValue(RightLabelProperty, value);
    }

    public TriSwitch()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki hinge slider: a Suki rail with a pearl thumb, snapping on notches.
/// The notches are spread evenly along the rail and the thumb is interpolated logarithmically between them,
/// so a geometric series (exposure stops, gain steps) reads as an even scale.
/// It only reports the chosen value: what it controls is up to the host. Drawing lives in <c>HingeSlider.Rendering.cs</c>.
/// </summary>
public partial class HingeSlider : UserControl
{
    /// <summary>Current value. Set by the user on a notch; can also be set by the host (any value inside the range).</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<HingeSlider, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Notch values, strictly increasing and positive.</summary>
    public static readonly StyledProperty<IReadOnlyList<double>> StepsProperty =
        AvaloniaProperty.Register<HingeSlider, IReadOnlyList<double>>(nameof(Steps), new double[] { 1, 2, 4, 8 });

    /// <summary>Caption under the left end: the consequence of a low value. Without any caption, the caption line is hidden.</summary>
    public static readonly StyledProperty<string?> LeftCaptionProperty =
        AvaloniaProperty.Register<HingeSlider, string?>(nameof(LeftCaption));

    /// <summary>Caption under the right end: the consequence of a high value.</summary>
    public static readonly StyledProperty<string?> RightCaptionProperty =
        AvaloniaProperty.Register<HingeSlider, string?>(nameof(RightCaption));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<double> Steps
    {
        get => GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public string? LeftCaption
    {
        get => GetValue(LeftCaptionProperty);
        set => SetValue(LeftCaptionProperty, value);
    }

    public string? RightCaption
    {
        get => GetValue(RightCaptionProperty);
        set => SetValue(RightCaptionProperty, value);
    }

    public HingeSlider()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

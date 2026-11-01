using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Skeumo-Suki rotary potentiometer with detents: a knurled knob on a round plate, 270 degrees of travel.
/// The knob turns with the hand (relative anchoring) and clicks into each notch with a small overshoot.
/// Drag, mouse wheel and arrow keys move one notch at a time. It only reports the chosen value.
/// Drawing and animation live in <c>RotaryKnob.Rendering.cs</c>.
/// </summary>
public partial class RotaryKnob : UserControl
{
    /// <summary>Current value, always on a notch between <see cref="Minimum"/> and <see cref="Maximum"/>.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RotaryKnob, double>(nameof(Maximum), 10);

    /// <summary>Number of intervals between the minimum and the maximum (notches = StepCount + 1).</summary>
    public static readonly StyledProperty<int> StepCountProperty =
        AvaloniaProperty.Register<RotaryKnob, int>(nameof(StepCount), 10);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int StepCount
    {
        get => GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }

    public RotaryKnob()
    {
        InitializeComponent();
        InitializeRendering();
    }
}

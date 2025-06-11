using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Theme;

public class NumericUpDownExtensions
{
    public static readonly AttachedProperty<object?> UnitProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, object?>("Unit", typeof(NumericUpDown));

    public static object? GetUnit(NumericUpDown textBox)
    {
        return textBox.GetValue(UnitProperty);
    }

    public static void SetUnit(NumericUpDown textBox, object? value)
    {
        textBox.SetValue(UnitProperty, value);
    }

    public static readonly AttachedProperty<bool> UseFloatingWatermarkProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, bool>("UseFloatingWatermark", typeof(NumericUpDown));

    public static bool GetUseFloatingWatermark(NumericUpDown textBox)
    {
        return textBox.GetValue(UseFloatingWatermarkProperty);
    }

    public static void SetUseFloatingWatermark(NumericUpDown textBox, bool value)
    {
        textBox.SetValue(UseFloatingWatermarkProperty, value);
    }
}
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
}
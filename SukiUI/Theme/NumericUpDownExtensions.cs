using Avalonia;
using Avalonia.Controls;

namespace SukiUI.Theme;

public class NumericUpDownExtensions
{
    /// <summary>
    /// Identifies the Prefix attached property, which specifies content to display before the text in a TextBox
    /// control.
    /// </summary>
    /// <remarks>This property can be used to add a prefix element, such as an icon or label, before the
    /// editable text area of a TextBox. The value can be any object, including strings, controls, or data templates.
    /// This property is typically set in XAML using attached property syntax.</remarks>
    public static readonly AttachedProperty<object?> PrefixProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, object?>("Prefix", typeof(NumericUpDown));

    public static object? GetPrefix(NumericUpDown control)
    {
        return control.GetValue(PrefixProperty);
    }

    public static void SetPrefix(NumericUpDown control, object? value)
    {
        control.SetValue(PrefixProperty, value);
    }

    /// <summary>
    /// Identifies the ResetText attached property, which stores custom reset text for a TextBox control.
    /// </summary>
    /// <remarks>This attached property can be used to associate additional rese information with a TextBox.
    /// The value can be retrieved or set using the standard Avalonia GetValue and SetValue methods for attached
    /// properties.</remarks>
    public static readonly AttachedProperty<object?> ResetTextProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, object?>("ResetText", typeof(NumericUpDown));

    public static object? GetResetText(NumericUpDown control)
    {
        return control.GetValue(ResetTextProperty);
    }

    public static void SetResetText(NumericUpDown control, object? value)
    {
        control.SetValue(ResetTextProperty, value);
    }

    public static readonly AttachedProperty<object?> UnitProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, object?>("Unit", typeof(NumericUpDown));

    public static object? GetUnit(NumericUpDown control)
    {
        return control.GetValue(UnitProperty);
    }

    public static void SetUnit(NumericUpDown control, object? value)
    {
        control.SetValue(UnitProperty, value);
    }

    public static readonly AttachedProperty<bool> UseFloatingWatermarkProperty =
        AvaloniaProperty.RegisterAttached<NumericUpDown, bool>("UseFloatingWatermark", typeof(NumericUpDown));

    public static bool GetUseFloatingWatermark(NumericUpDown control)
    {
        return control.GetValue(UseFloatingWatermarkProperty);
    }

    public static void SetUseFloatingWatermark(NumericUpDown control, bool value)
    {
        control.SetValue(UseFloatingWatermarkProperty, value);
    }
}
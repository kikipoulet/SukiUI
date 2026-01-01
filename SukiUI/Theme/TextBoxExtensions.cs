using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using System.Globalization;

namespace SukiUI.Theme;

public static class TextBoxExtensions
{
    /// <summary>
    /// Identifies the Prefix attached property, which specifies content to display before the text in a TextBox
    /// control.
    /// </summary>
    /// <remarks>This property can be used to add a prefix element, such as an icon or label, before the
    /// editable text area of a TextBox. The value can be any object, including strings, controls, or data templates.
    /// This property is typically set in XAML using attached property syntax.</remarks>
    public static readonly AttachedProperty<object?> PrefixProperty =
        AvaloniaProperty.RegisterAttached<TextBox, object?>("Prefix", typeof(TextBox));

    public static object? GetPrefix(TextBox control)
    {
        return control.GetValue(PrefixProperty);
    }

    public static void SetPrefix(TextBox control, object? value)
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
        AvaloniaProperty.RegisterAttached<TextBox, object?>("ResetText", typeof(TextBox));

    public static object? GetResetText(TextBox control)
    {
        return control.GetValue(ResetTextProperty);
    }

    public static void SetResetText(TextBox control, object? value)
    {
        control.SetValue(ResetTextProperty, value);
    }

    /// <summary>
    /// Identifies the Unit attached property for a TextBox, which allows associating a unit value with the control.
    /// </summary>
    /// <remarks>This attached property can be used to specify a unit (such as a measurement or label) that is
    /// associated with the value displayed in a TextBox. The property is typically used in scenarios where the input
    /// value requires contextual information, such as currency, length, or other units. The value can be any object,
    /// including a string representing the unit or a more complex type, depending on application
    /// requirements.</remarks>
    public static readonly AttachedProperty<object?> UnitProperty =
        AvaloniaProperty.RegisterAttached<TextBox, object?>("Unit", typeof(TextBox));

    public static object? GetUnit(TextBox control)
    {
        return control.GetValue(UnitProperty);
    }

    public static void SetUnit(TextBox control, object? value)
    {
        control.SetValue(UnitProperty, value);
    }

    /// <summary>
    /// Identifies an attached property that determines whether a delete button is displayed in a TextBox.
    /// </summary>
    /// <remarks>Set this property to <see langword="true"/> to display a delete button within the TextBox
    /// control, allowing users to clear its contents. The default value is <see langword="false"/>.</remarks>
    public static readonly AttachedProperty<bool> AddDeleteButtonProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("AddDeleteButton", typeof(TextBox), defaultValue: false);

    public static bool GetAddDeleteButton(TextBox control)
    {
        return control.GetValue(AddDeleteButtonProperty);
    }

    public static void SetAddDeleteButton(TextBox control, bool value)
    {
        control.SetValue(AddDeleteButtonProperty, value);
    }

    public static void Error(this TextBox control, string message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            control.BorderBrush = Brushes.IndianRed;

            control.ContextFlyout = new Flyout()
            {
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                Content = new TextBlock() { Text = message, FontWeight = FontWeight.Thin, Foreground = Brushes.IndianRed }
            };
            control.ContextFlyout.ShowAt(control);
            control.Vibrate(TimeSpan.FromMilliseconds(400));
        });
    }
}

public static class ButtonExtensions
{
    public static readonly AttachedProperty<bool> ShowProgressProperty =
        AvaloniaProperty.RegisterAttached<Button, bool>("ShowProgress", typeof(Button), defaultValue: false);

    public static bool GetShowProgress(Button control)
    {
        return control.GetValue(ShowProgressProperty);
    }

    public static void SetShowProgress(Button control, bool value)
    {
        control.SetValue(ShowProgressProperty, value);
    }

    public static void ShowProgress(this Button control)
    {
        control.SetValue(ShowProgressProperty, true);
    }

    public static void HideProgress(this Button control)
    {
        control.SetValue(ShowProgressProperty, false);
    }

    public static readonly AttachedProperty<object?> IconProperty =
        AvaloniaProperty.RegisterAttached<Button, object?>("Icon", typeof(Button));

    public static object? GetIcon(Button control)
    {
        return control.GetValue(IconProperty);
    }

    public static void SetIcon(Button control, object? value)
    {
        control.SetValue(IconProperty, value);
    }
}

public static class SplitButtonExtensions
{
    public static readonly AttachedProperty<bool> ShowProgressProperty =
        AvaloniaProperty.RegisterAttached<SplitButton, bool>("ShowProgress", typeof(SplitButton), defaultValue: false);

    public static bool GetShowProgress(SplitButton control)
    {
        return control.GetValue(ShowProgressProperty);
    }

    public static void SetShowProgress(SplitButton control, bool value)
    {
        control.SetValue(ShowProgressProperty, value);
    }

    public static void ShowProgress(this SplitButton control)
    {
        control.SetValue(ShowProgressProperty, true);
    }

    public static void HideProgress(this SplitButton control)
    {
        control.SetValue(ShowProgressProperty, false);
    }

    public static readonly AttachedProperty<object?> IconProperty =
        AvaloniaProperty.RegisterAttached<SplitButton, object?>("Icon", typeof(SplitButton));

    public static object? GetIcon(SplitButton control)
    {
        return control.GetValue(IconProperty);
    }

    public static void SetIcon(SplitButton control, object? value)
    {
        control.SetValue(IconProperty, value);
    }
}

public class StringToDoubleConverter : IValueConverter
{
    public static readonly StringToDoubleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return ((string)value).Length > 0 ? 1 : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToWidthProgressConverter : IValueConverter
{
    public static readonly BoolToWidthProgressConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return ((bool)value) ? 40 : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
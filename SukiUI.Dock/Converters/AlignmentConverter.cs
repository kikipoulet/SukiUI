using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Alignment"/> enum to avalonia <see cref="Dock"/> enum.
/// </summary>
public class AlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="AlignmentConverter"/> instance.
    /// </summary>
    public static readonly AlignmentConverter Instance = new AlignmentConverter();

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            Alignment alignment => alignment switch
            {
                Alignment.Unset => AvaloniaProperty.UnsetValue,
                Alignment.Left => global::Avalonia.Controls.Dock.Left,
                Alignment.Bottom => global::Avalonia.Controls.Dock.Bottom,
                Alignment.Right => global::Avalonia.Controls.Dock.Right,
                Alignment.Top => global::Avalonia.Controls.Dock.Top,
                _ => throw new NotSupportedException($"Provided dock is not supported in Avalonia.")
            },
            _ => value
        };
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            global::Avalonia.Controls.Dock dock => dock switch
            {
                global::Avalonia.Controls.Dock.Left => Alignment.Left,
                global::Avalonia.Controls.Dock.Bottom => Alignment.Bottom,
                global::Avalonia.Controls.Dock.Right => Alignment.Right,
                global::Avalonia.Controls.Dock.Top => Alignment.Top,
                _ => Alignment.Unset
            },
            _ => value
        };
    }
}

public class TransparentToTrueConverter : IValueConverter
{
    public static readonly TransparentToTrueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, 
        CultureInfo culture)
    {
        if (value == null)
            return false;
        
        var b = (ImmutableSolidColorBrush)value;
        return b.Opacity != 0;
    }

    public object ConvertBack(object? value, Type targetType, 
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

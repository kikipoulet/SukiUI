using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Model.Core.Orientation"/> enum to avalonia <see cref="Orientation"/> enum.
/// </summary>
public class OrientationConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="OrientationConverter"/> instance.
    /// </summary>
    public static readonly OrientationConverter Instance = new OrientationConverter();

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
            Model.Core.Orientation orientation => orientation switch
            {
                Model.Core.Orientation.Horizontal => Orientation.Horizontal,
                Model.Core.Orientation.Vertical => Orientation.Vertical,
                _ => throw new NotSupportedException($"Provided orientation is not supported in Avalonia.")
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
            Orientation orientation => orientation switch
            {
                Orientation.Horizontal => Model.Core.Orientation.Horizontal,
                Orientation.Vertical => Model.Core.Orientation.Vertical,
                _ => throw new NotSupportedException($"Provided orientation is not supported in Model.")
            },
            _ => value
        };
    }
}

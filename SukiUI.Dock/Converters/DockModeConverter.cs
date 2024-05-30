using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;
using AC = Avalonia.Controls;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Model.Core.DockMode"/> enum to avalonia <see cref="AC.Dock"/> enum.
/// </summary>
public class DockModeConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="DockModeConverter"/> instance.
    /// </summary>
    public static readonly DockModeConverter Instance = new DockModeConverter();

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
            DockMode dock => dock switch
            {
                DockMode.Left => AC.Dock.Left,
                DockMode.Bottom => AC.Dock.Bottom,
                DockMode.Right => AC.Dock.Right,
                DockMode.Top => AC.Dock.Top,
                _ => AvaloniaProperty.UnsetValue
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
            AC.Dock dock => dock switch
            {
                AC.Dock.Left => DockMode.Left,
                AC.Dock.Bottom => DockMode.Bottom,
                AC.Dock.Right => DockMode.Right,
                AC.Dock.Top => DockMode.Top,
                _ => DockMode.Center
            },
            _ => value
        };
    }
}

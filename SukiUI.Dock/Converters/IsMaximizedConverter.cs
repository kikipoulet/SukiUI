using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts WindowState to bool indicating if the window is maximized.
/// </summary>
public class IsMaximizedConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="IsMaximizedConverter"/> instance.
    /// </summary>
    public static IsMaximizedConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowState windowState)
        {
            return windowState == WindowState.Maximized;
        }

        return false;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

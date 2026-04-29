using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SukiUI.Theme;

public class LeftMarginConverter : IValueConverter
{
    private const double IndentWidth = 16d;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int level && level > 0)
        {
            return new Thickness(level * IndentWidth, 0, 0, 0);
        }

        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

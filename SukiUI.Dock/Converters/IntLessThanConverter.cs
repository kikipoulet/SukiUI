using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

internal class IntLessThanConverter : IValueConverter
{
    public int TrueIfLessThan { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue < TrueIfLessThan;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

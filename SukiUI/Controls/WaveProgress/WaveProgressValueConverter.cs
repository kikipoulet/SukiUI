using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SukiUI.Controls;

public class WaveProgressValueConverter : IValueConverter
{
    public static readonly WaveProgressValueConverter Instance = new();

    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        try
        {
            return (double) (155 - (((int)value) * (2.1)));
        }
        catch
        {
            
        }
        
        return 0;
    }

    public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}

public class WaveProgressValueColorConverter : IValueConverter
{
    public static readonly WaveProgressValueColorConverter Instance = new();

    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        try
        {
            if (((int)value) > 50)
                return Brushes.White;
            else
                return Brushes.Black;
        }
        catch
        {
            
        }
        
        return Brushes.Black;
    }

    public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}

public class WaveProgressValueTextConverter : IValueConverter
{
    public static readonly WaveProgressValueTextConverter Instance = new();

    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        try
        {
            return value.ToString() + "%";
        }
        catch
        {
            
        }
        
        return "0 %";
    }

    public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}
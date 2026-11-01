using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;

namespace SukiUI.Dialogs
{
    public class DialogContentMaxWidthValueConverter: IValueConverter
    {

            public static readonly DialogContentMaxWidthValueConverter Instance = new();

            public object? Convert(object? value, Type targetType, object? parameter, 
                CultureInfo culture)
            {
                if (value is string)
                    return 450;

                return 2000;
            }

            public object ConvertBack(object? value, Type targetType, 
                object? parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        
    }
}
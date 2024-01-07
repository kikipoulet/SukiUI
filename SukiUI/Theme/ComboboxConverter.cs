using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SukiUI.Theme
{
 
        public class PlusNineConverter : IValueConverter
        {
            public static readonly PlusNineConverter Instance = new();

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                double x = (double)value;
                if (x == 0)
                    return 0;
                
                x += 9;
                return x;
            }

            public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

}
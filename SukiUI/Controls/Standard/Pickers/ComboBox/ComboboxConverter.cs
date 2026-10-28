using Avalonia.Data.Converters;
using System.Globalization;

namespace SukiUI.Theme
{
    public class PlusNineConverter : IValueConverter
    {
        public static readonly PlusNineConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double x) {
                if (x == 0)
                    return 0;

                x += 9;
                return x;
            }
            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SukiUI.Enums;

namespace SukiUI.Converters
{
    public class SukiDialogBackgroundStyleToBoolConverter : IValueConverter
    {
        public static readonly SukiDialogBackgroundStyleToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SukiDialogBackgroundStyle style)
                return style == SukiDialogBackgroundStyle.Blur;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

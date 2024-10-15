using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using SukiUI.Controls;

namespace SukiUI.Converters
{
    public class ProgressToContentCOnverter : IValueConverter
    {
        public static readonly ProgressToContentCOnverter Instance = new ProgressToContentCOnverter();
        
        
        public object? Convert(object? value, Type targetType, object? parameter, 
            CultureInfo culture)
        {
            if ((bool)value)
                return new Loading() { LoadingStyle = LoadingStyle.Simple };

            return new Panel();
        }
        
        public object ConvertBack(object? value, Type targetType, 
            object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
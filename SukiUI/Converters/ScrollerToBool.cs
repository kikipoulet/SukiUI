using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SukiUI.Converters
{
    public class ScrollerToBool : IMultiValueConverter
    {
        public static ScrollerToBool Up { get; } = new((x,y) => x > y);

        public static ScrollerToBool Down { get; } = new((x, y) => x < y);

        private readonly Func<double, double, bool> _converter;
        
        public ScrollerToBool(Func<double, double, bool> converter)
        {
            _converter = converter;
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is not double firstVal) return null;
            if (values[1] is not double secondVal) return null;
            return _converter(firstVal,secondVal);
        }
    }
    
    // <!-- <LinearGradientBrush StartPoint="50%,0%" EndPoint="50%,95%"> -->
    // <!--     <GradientStop Offset="0.9" Color="Black" /> -->
    // <!--     <GradientStop Offset="1" Color="Transparent" /> -->
    // <!-- </LinearGradientBrush> -->

    public class ScrollerToOpacityMask : IMultiValueConverter
    {
        private readonly Func<double, double, IBrush?> _func;
        
        public static ScrollerToOpacityMask Top { get; } = new((x,y) => x > y ? TopBrush : Brushes.White);
        public static ScrollerToOpacityMask Bottom { get; } = new((x,y) => x < y ? BottomBrush : Brushes.White);

        private static readonly LinearGradientBrush BottomBrush = new()
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 0.95, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new(Colors.Black, 0.9),
                new(Colors.Transparent,1 )
            }
        };
        
        private static readonly LinearGradientBrush TopBrush = new()
        {
            StartPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 0.05, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new(Colors.Black, 0.9),
                new(Colors.Transparent,1 )
            }
        };
        
        
        public ScrollerToOpacityMask(Func<double, double, IBrush?> func)
        {
            _func = func;
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 2) return null;
            if (values[0] is not double valOne) return null;
            if (values[1] is not double valTwo) return null;
            return _func(valOne, valTwo);
        }
    }
}
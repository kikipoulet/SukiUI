using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace SukiUI.Converters
{
    public sealed class StartPointLiquidConverter : IMultiValueConverter
    {

        public static StartPointLiquidConverter Instance = new StartPointLiquidConverter();
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            
            if (values[0] is double w && values[1] is double h)
            {
                var min = Math.Min(h,w);
                var max = Math.Max(h,w);
                var factor = Math.Abs( min/max);

                var y = -( (1/ ( 1.75* factor)) )  ;

                return new RelativePoint(0.2, y , RelativeUnit.Relative);
            }

            return AvaloniaProperty.UnsetValue;
        }
    }
    
    public sealed class EndPointLiquidConverter : IMultiValueConverter
    {

        public static EndPointLiquidConverter Instance = new EndPointLiquidConverter();
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            
            if (values[0] is double w && values[1] is double h)
            {
                var min = Math.Min(h,w);
                var max = Math.Max(h,w);
                var factor = Math.Abs( min/max);

                var y = ( (1/ ( 1.75* factor)) )  ;

                return new RelativePoint(0.8, 1+ y , RelativeUnit.Relative);
            }

            return AvaloniaProperty.UnsetValue;
        }
    }
}
using System;
using System.Globalization;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class SliverMobilePage : UserControl
    {
        public SliverMobilePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<CircleProgressBar, string>(nameof(Header), defaultValue: "Header");

        public string Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value ); }
        }
    }
    
    public class OffsetToHeightConverter : IValueConverter
    {
        public static readonly OffsetToHeightConverter Instance = new OffsetToHeightConverter();

        public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
        {

            double Offset =  ((Vector)value).Y * 0.7;

      
            double Height = 170 - (Offset);

            if (Height < 100)
                return 100;
            else
                return Height;

        }

        public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    
    public class OffsetToMarginConverter : IValueConverter
    {
        public static readonly OffsetToMarginConverter Instance = new OffsetToMarginConverter();

        public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            double offset = ((Vector)value).Y * 0.7;

           

            if (offset > 70)
                offset = 70;

            return new Thickness(25 + (offset * 1.35), 70 - offset, 0 , -10 + (offset / 7));

        }

        public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }

    public class OffsetToMarginScrollConverter : IValueConverter
    {
        public static readonly OffsetToMarginScrollConverter Instance = new OffsetToMarginScrollConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double offset = ((Vector)value).Y * 0.7;

            if (offset > 70)
                offset = 70;

            return new Thickness(0, 170, 0, 0);

        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class OffsetToFontSizeConverter : IValueConverter
    {
        public static readonly OffsetToFontSizeConverter Instance = new OffsetToFontSizeConverter();

        public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            double Offset =  ((Vector)value).Y * 0.7;

         

            double fontsize = 45 - (Offset / 4.5);

            if (fontsize < 30)
                return 30;

            return fontsize;
        }

        public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}
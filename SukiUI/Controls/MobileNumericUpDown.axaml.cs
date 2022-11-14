using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class MobileNumericUpDown : UserControl
{
    public MobileNumericUpDown()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<MobileNumericUpDown, int>(nameof(Value), defaultValue: 0);

    public int Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value ); }
    }

    private void ButtonPlus(object sender, RoutedEventArgs e)
    {
        Value++;
    }
    
    private void ButtonMinus(object sender, RoutedEventArgs e)
    {
        Value--;
    }
}

public class IntToStringConverter : IValueConverter
{
    public static readonly IntToStringConverter Instance = new();

    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        return value.ToString();
    }

    public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Dialogs;

namespace SukiUI.Controls.Touch.MobileNumberPicker;


public partial class MobileNumberPicker : UserControl
{
    public MobileNumberPicker()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    
    public ISukiDialogManager DialogManager
    {
        get => GetValue(DialogManagerProperty);
        set => SetValue(DialogManagerProperty, value);
    }
    public static readonly StyledProperty<ISukiDialogManager> DialogManagerProperty =
        AvaloniaProperty.Register<MobileNumberPicker, ISukiDialogManager>(
            nameof(DialogManager));
    
    
    private object _content;

    public object Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }
    
    public static readonly DirectProperty<MobileNumberPicker, object> ContentProperty =
        AvaloniaProperty.RegisterDirect<MobileNumberPicker, object>(
            nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v,
            defaultBindingMode: BindingMode.OneWay,
            enableDataValidation: true);
    
    private int _value;

    public int Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value );
    }

    public static readonly DirectProperty<MobileNumberPicker, int> ValueProperty =
        AvaloniaProperty.RegisterDirect<MobileNumberPicker, int>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public int Minimum
    {
        get { return GetValue(MinimumProperty); }
        set { SetValue(MinimumProperty, value ); }
    }

    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<MobileNumberPicker, int>(nameof(Minimum), defaultValue: 0);

    public int Maximum
    {
        get { return GetValue(MaximumProperty); }
        set { SetValue(MaximumProperty, value ); }
    }

    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<MobileNumberPicker, int>(nameof(Maximum), defaultValue: 100);
    
    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        var control = new MobileNumberPickerPopup(this, DialogManager);

        DialogManager.CreateDialog().WithContent(control).Dismiss().ByClickingBackground().TryShow();
        
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        OpenPopup(null,null);
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

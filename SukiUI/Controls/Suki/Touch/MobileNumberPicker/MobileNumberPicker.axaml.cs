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
    
    
    private object? _displayContent;

    public object? DisplayContent
    {
        get => _displayContent;
        set => SetAndRaise(DisplayContentProperty, ref _displayContent, value);
    }
    
    public static readonly DirectProperty<MobileNumberPicker, object?> DisplayContentProperty =
        AvaloniaProperty.RegisterDirect<MobileNumberPicker, object?>(
            nameof(DisplayContent),
            o => o.DisplayContent,
            (o, v) => o.DisplayContent = v,
            defaultBindingMode: BindingMode.OneWay,
            enableDataValidation: true);
    
    private int _value;

    public int Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, Math.Clamp(value, Minimum, Maximum));
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
        if (DialogManager is null)
            return;
        var control = new MobileNumberPickerPopup(this, DialogManager);

        DialogManager.CreateDialog().WithContent(control).Dismiss().ByClickingBackground().TryShow();
        
    }

    private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        OpenPopup(sender, e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MinimumProperty && Minimum > Maximum)
            SetCurrentValue(MaximumProperty, Minimum);
        else if (change.Property == MaximumProperty && Maximum < Minimum)
            SetCurrentValue(MinimumProperty, Maximum);

        if (change.Property == MinimumProperty || change.Property == MaximumProperty)
            Value = Math.Clamp(Value, Minimum, Maximum);
    }
}


public class IntToStringConverter : IValueConverter
{
    public static readonly IntToStringConverter Instance = new();

    public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture )
    {
        throw new NotSupportedException();
    }
}

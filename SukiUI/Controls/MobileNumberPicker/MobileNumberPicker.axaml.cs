using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls.MobilePicker;

namespace SukiUI.Controls.MobileNumberPicker;

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

    
    private int _value;
    
    /// <summary>
    /// Gets the current value.
    /// </summary>
    public int Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value );
    }
    
    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly DirectProperty<MobileNumberPicker, int> ValueProperty =
        AvaloniaProperty.RegisterDirect<MobileNumberPicker, int>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<MobileNumberPicker, int>(nameof(Minimum), defaultValue: 0);

    public int Minimum
    {
        get { return GetValue(MinimumProperty); }
        set { SetValue(MinimumProperty, value ); }
    }
    
    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<MobileNumberPicker, int>(nameof(Maximum), defaultValue: 100);

    public int Maximum
    {
        get { return GetValue(MaximumProperty); }
        set { SetValue(MaximumProperty, value ); }
    }
    
    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        var control = new MobileNumberPickerPopup(this);
    

        
        MobileMenuPage.ShowDialogS(control , true);
    }
}
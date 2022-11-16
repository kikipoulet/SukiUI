using System;
using Avalonia;
using Avalonia.Controls;
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

    
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<MobileNumberPicker, int>(nameof(Value), defaultValue: 5);

    public int Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value ); }
    }
    
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
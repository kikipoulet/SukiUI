using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.TouchInput.TouchNumericPad;

public partial class TouchNumericPad : UserControl
{
    public TouchNumericPad()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        var dialog = new NumericPadPopUp()
        {
            rootControl = this
        };
        
        MobileMenuPage.ShowDialogS(dialog,true);
    }
    
    private double _value;
    
    public static readonly DirectProperty<TouchNumericPad, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<TouchNumericPad, double>(nameof(Value), numpicker => numpicker.Value,
            (numpicker, v) => numpicker.Value = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public double Value
    {
        get { return _value; }
        set
        {
            SetAndRaise(ValueProperty, ref _value, value);
            this.FindControl<TextBlock>("textValue").Text = Value.ToString();
        }
    }
    
    
}
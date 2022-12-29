using Avalonia;
using Avalonia.Controls;
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
    
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<TouchNumericPad, double>(nameof(Value), defaultValue: 100);

    public double Value
    {
        get { return GetValue(ValueProperty); }
        set
        {
            SetValue(ValueProperty, value );
            this.FindControl<TextBlock>("textValue").Text = Value.ToString();
        }
    }
}
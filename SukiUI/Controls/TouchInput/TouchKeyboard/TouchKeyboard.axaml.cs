using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls.TouchInput.TouchKeyboard;

public partial class TouchKeyboard : UserControl
{
    public TouchKeyboard()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OpenPopup(object sender, RoutedEventArgs e)
    {
        var dialog = new TouchKeyboardPopUp(this, Text)
        {
            
        };
        
        MobileMenuPage.ShowDialogS(dialog,true);
    }

    private string _text;

    public static readonly DirectProperty<TouchKeyboard, string> TextProperty =
        AvaloniaProperty.RegisterDirect<TouchKeyboard, string>(nameof(Text), numpicker => numpicker.Text,
            (numpicker, v) => numpicker.Text = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public string Text
    {
        get { return _text; }
        set
        {
            SetAndRaise(TextProperty, ref _text, value);
            this.FindControl<TextBlock>("textValue").Text = Text.ToString();
        }
    }

   
}
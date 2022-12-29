using Avalonia;
using Avalonia.Controls;
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
    
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TouchKeyboard, string>(nameof(Text), defaultValue: "");

    public string Text
    {
        get { return GetValue(TextProperty); }
        set
        {
            SetValue(TextProperty, value );
            this.FindControl<TextBlock>("textValue").Text = Text.ToString();
        }
    }
}
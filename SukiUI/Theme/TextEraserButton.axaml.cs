using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SukiUI.Theme;

public partial class TextEraserButton : UserControl
{
    public TextEraserButton()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<TextBox> TextProperty = AvaloniaProperty.Register<TextEraserButton, TextBox>(nameof(Text));

    public TextBox Text
    {
        get { return GetValue(TextProperty); }
        set
        {
            SetValue(TextProperty, value);
        }
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Text!.Text = string.Empty;
    }
}
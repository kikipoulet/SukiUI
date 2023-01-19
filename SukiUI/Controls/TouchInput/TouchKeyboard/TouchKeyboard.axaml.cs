using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

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

        dialog.RenderTransform = PopupScale;
        dialog.Height = PopupHeight;
        dialog.Width = PopupWidth;
        
        InteractiveContainer.ShowDialog(dialog,true);
    }
    
    
    public static readonly StyledProperty<ScaleTransform> PopupScaleProperty = AvaloniaProperty.Register<TouchKeyboard, ScaleTransform>(nameof(TouchKeyboard), defaultValue: new ScaleTransform());

    public ScaleTransform PopupScale
    {
        get { return GetValue(PopupScaleProperty); }
        set
        {
            
            SetValue(PopupScaleProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupHeightProperty = AvaloniaProperty.Register<TouchKeyboard, int>(nameof(TouchKeyboard), defaultValue: 400);

    public int PopupHeight
    {
        get { return GetValue(PopupHeightProperty); }
        set
        {
            
            SetValue(PopupHeightProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupWidthProperty = AvaloniaProperty.Register<TouchKeyboard, int>(nameof(TouchKeyboard), defaultValue: 900);

    public int PopupWidth
    {
        get { return GetValue(PopupWidthProperty); }
        set
        {
            
            SetValue(PopupWidthProperty, value );
        }
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
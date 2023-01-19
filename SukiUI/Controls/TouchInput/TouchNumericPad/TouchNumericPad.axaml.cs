using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

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

        dialog.RenderTransform = PopupScale;
        dialog.Height = PopupHeight;
        dialog.Width = PopupWidth;
        
       InteractiveContainer.ShowDialog(dialog,true);
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
    
    
        
    public static readonly StyledProperty<ScaleTransform> PopupScaleProperty = AvaloniaProperty.Register<TouchNumericPad, ScaleTransform>(nameof(TouchNumericPad), defaultValue: new ScaleTransform());

    public ScaleTransform PopupScale
    {
        get { return GetValue(PopupScaleProperty); }
        set
        {
            
            SetValue(PopupScaleProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupHeightProperty = AvaloniaProperty.Register<TouchNumericPad, int>(nameof(TouchNumericPad), defaultValue: 400);

    public int PopupHeight
    {
        get { return GetValue(PopupHeightProperty); }
        set
        {
            
            SetValue(PopupHeightProperty, value );
        }
    }
    
    public static readonly StyledProperty<int> PopupWidthProperty = AvaloniaProperty.Register<TouchNumericPad, int>(nameof(TouchNumericPad), defaultValue: 300);

    public int PopupWidth
    {
        get { return GetValue(PopupWidthProperty); }
        set
        {
            
            SetValue(PopupWidthProperty, value );
        }
    }
    
}
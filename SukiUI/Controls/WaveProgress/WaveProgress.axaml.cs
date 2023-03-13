using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class WaveProgress : UserControl
{
    public WaveProgress()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private int _value = 50;
    public int Value
    {
        get => _value;
        set
        {
            if (value is < 0 or > 100)
                return;

            SetAndRaise(ValueProperty, ref _value, value);
        }
    }

    public static readonly DirectProperty<WaveProgress, int> ValueProperty =
        AvaloniaProperty.RegisterDirect<WaveProgress, int>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);
    
    public static readonly StyledProperty<bool> IsTextVisibleProperty = AvaloniaProperty.Register<WaveProgress, bool>(nameof(IsTextVisible), defaultValue: true);

    public bool IsTextVisible
    {
        get { return GetValue(IsTextVisibleProperty); }
        set
        {
            
            SetValue(IsTextVisibleProperty, value );
        }
    }
}
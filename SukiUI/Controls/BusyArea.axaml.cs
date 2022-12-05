using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class BusyArea : UserControl
{
    public BusyArea()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<BusyArea, bool>(nameof(IsBusy), defaultValue: false);

    public bool IsBusy
    {
        get { return GetValue(IsBusyProperty); }
        set
        {
           
            LoadingOpacity = value ? 1 : 0;
            SetValue(IsBusyProperty, value );
        }
    }
    
    public static readonly StyledProperty<double> LoadingOpacityProperty = AvaloniaProperty.Register<BusyArea, double>(nameof(LoadingOpacity), defaultValue: 0);

    public double LoadingOpacity
    {
        get { return GetValue(LoadingOpacityProperty); }
        set
        {
            
            SetValue(LoadingOpacityProperty, value );
        }
    }
}
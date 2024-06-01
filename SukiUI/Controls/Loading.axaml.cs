using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SukiUI.Controls;

public partial class Loading : UserControl
{
    public Loading()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
        
    public new static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<Loading, IBrush>(nameof(Foreground), defaultValue: Brushes.Aqua);

    public new IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
}
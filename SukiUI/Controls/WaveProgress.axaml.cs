using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls;

public partial class WaveProgress : UserControl
{
    public WaveProgress()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var theme = SukiTheme.GetInstance();
        theme.OnBaseThemeChanged -= OnThemeChanged;
        theme.OnColorThemeChanged -= OnColorThemeChanged;
        theme.OnBaseThemeChanged += OnThemeChanged;
        theme.OnColorThemeChanged += OnColorThemeChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var theme = SukiTheme.GetInstance();
        theme.OnBaseThemeChanged -= OnThemeChanged;
        theme.OnColorThemeChanged -= OnColorThemeChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnThemeChanged(Avalonia.Styling.ThemeVariant _) => RefreshValueBindings();

    private void OnColorThemeChanged(Models.SukiColorTheme _) => RefreshValueBindings();

    private void RefreshValueBindings()
    {
        var value = Value;
        // double.Epsilon (~5e-324) is below the ULP of any normal double, so adding it
        // produces no change. Use a nudge that is always representable as a distinct double.
        SetCurrentValue(ValueProperty, value == 100 ? value - 1e-9 : value + 1e-9);
        SetCurrentValue(ValueProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<WaveProgress, double>(nameof(Value), defaultValue: 50);

    public double Value
    {
        get => GetValue(ValueProperty);
        set
        {
            if (value is >= 0 and <= 100)
                SetValue(ValueProperty, value);
        }
    }
    
    public static readonly StyledProperty<bool> IsTextVisibleProperty = AvaloniaProperty.Register<WaveProgress, bool>(nameof(IsTextVisible), defaultValue: true);

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons.Avalonia;

namespace SukiUI.Controls;

public partial class PercentProgressBar : UserControl
{
    public PercentProgressBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<int> ValueProperty = AvaloniaProperty.Register<Stepper, int>(nameof(Value), defaultValue: 50);

    public int Value
    {
        get { return GetValue(ValueProperty); }
        set
        {
            if (value is < 0 or > 100)
                return;
            
            SetValue(ValueProperty, value );

            this.FindControl<ProgressBar>("barPercent").Value = value;
            
            if (value != 100)
            {
                this.FindControl<TextBlock>("textPercent").Text = value.ToString() + " %";
            }
            else
            {
                this.FindControl<TextBlock>("textPercent").Opacity = 0;
                this.FindControl<MaterialIcon>("iconPercent").Opacity = 1;
                this.FindControl<ProgressBar>("barPercent").Foreground = Brushes.ForestGreen;
            }
        }
    }
}
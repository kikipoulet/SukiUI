using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI;


public enum SukiColorTheme
{
    Blue, Green, Red
}
public partial class SukiTheme : Styles
{

    public static readonly StyledProperty<SukiColorTheme> ColorThemeProperty =
        AvaloniaProperty.Register<SukiTheme, SukiColorTheme>(nameof(ColorTheme), defaultBindingMode: BindingMode.TwoWay, defaultValue: SukiColorTheme.Blue);

    public SukiColorTheme ColorTheme
    {
        get => GetValue(ColorThemeProperty);
        set { SetValue(ColorThemeProperty, value); SetColorThemeResources(); }
    }

    public void SetColorThemeResources()
    {
        switch (ColorTheme)
        {
            case SukiColorTheme.Red:
                Application.Current.Resources["SukiPrimaryColor"] = Colors.IndianRed;
                Application.Current.Resources["SukiIntBorderBrush"] = Color.Parse("#15cc8888");
                break;
            
            case SukiColorTheme.Green:
                Application.Current.Resources["SukiPrimaryColor"] = Colors.ForestGreen;
                Application.Current.Resources["SukiIntBorderBrush"] = Color.Parse("#1588cc88");
                break;
            
            default:
                Application.Current.Resources["SukiPrimaryColor"] = Color.Parse("#0A59F7");
                Application.Current.Resources["SukiIntBorderBrush"] = Color.Parse("#158888ff");
                break;
        }
    }
}
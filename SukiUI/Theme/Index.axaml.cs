using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Enums;

namespace SukiUI;

public partial class SukiTheme : Styles
{
    public static readonly StyledProperty<SukiColorTheme> ColorThemeProperty =
        AvaloniaProperty.Register<SukiTheme, SukiColorTheme>(nameof(ColorTheme), defaultBindingMode: BindingMode.TwoWay, defaultValue: SukiColorTheme.Blue);

    public SukiColorTheme ColorTheme
    {
        get => GetValue(ColorThemeProperty);
        set { SetValue(ColorThemeProperty, value); SetColorThemeResources(); }
    }
    
    /// <summary>
    /// Called whenever the application's <see cref="SukiColorTheme"/> is changed.
    /// Useful where controls cannot use "DynamicResource"
    /// </summary>
    public static Action<SukiColorTheme>? OnThemeChanged { get; set; }

    private static SukiTheme? _instance;

    public void SetColorThemeResources()
    {
        _instance ??= this;
        if (Application.Current is null) return;
        switch (ColorTheme)
        {
            case SukiColorTheme.Orange:
                Application.Current.Resources["SukiPrimaryColor"] = Color.Parse("#ED8E12");
                Application.Current.Resources["SukiIntBorderBrush"] = Color.Parse("#151271ED");
                break;
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

    /// <summary>
    /// Attempts to change the theme to the given value.
    /// </summary>
    /// <param name="theme">The <see cref="SukiColorTheme"/> to change to.</param>
    public static void TryChangeTheme(SukiColorTheme theme)
    {
        if (_instance is null) return;
        _instance.ColorTheme = theme;
        OnThemeChanged?.Invoke(theme);
    }

    /// <summary>
    /// Gets the current <see cref="SukiColorTheme"/>
    /// </summary>
    /// <returns>The current <see cref="SukiColorTheme"/> or null if the style hasn't been initialized.</returns>
    public static SukiColorTheme? GetCurrentTheme()
    {
        return _instance?.ColorTheme;
    }
}
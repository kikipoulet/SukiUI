using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using SukiUI.Enums;

namespace SukiUI;

public partial class SukiTheme : Styles
{
    public static readonly StyledProperty<SukiColorTheme> ColorThemeProperty =
        AvaloniaProperty.Register<SukiTheme, SukiColorTheme>(nameof(ColorTheme), defaultBindingMode: BindingMode.TwoWay,
            defaultValue: SukiColorTheme.Blue);

    public SukiColorTheme ColorTheme
    {
        get => GetValue(ColorThemeProperty);
        set
        {
            SetValue(ColorThemeProperty, value);
            SetColorThemeResources();
        }
    }

    /// <summary>
    /// Called whenever the application's <see cref="SukiColorTheme"/> is changed.
    /// Useful where controls cannot use "DynamicResource"
    /// </summary>
    public static Action<SukiColorTheme>? OnColorThemeChanged { get; set; }

    private static SukiTheme? _instance;

    public void SetColorThemeResources()
    {
        _instance ??= this;
        if (Application.Current is null) return;
        if (!Swatches.TryGetValue(ColorTheme, out var swatch))
            throw new Exception($"{ColorTheme} has no defined swatch.");
        Application.Current.Resources["SukiPrimaryColor"] = swatch.Primary;
        Application.Current.Resources["SukiIntBorderBrush"] = swatch.IntBorder;
    }

    public static readonly IReadOnlyDictionary<SukiColorTheme, (Color Primary, Color IntBorder)> Swatches =
        new Dictionary<SukiColorTheme, (Color Primary, Color IntBorder)>
        {
            { SukiColorTheme.Orange, (Color.Parse("#ED8E12"), Color.Parse("#151271ED")) },
            { SukiColorTheme.Red, (Colors.IndianRed, Color.Parse("#15cc8888")) },
            { SukiColorTheme.Green, (Colors.ForestGreen, Color.Parse("#1588cc88")) },
            { SukiColorTheme.Blue, (Color.Parse("#0A59F7"), Color.Parse("#158888ff")) }
        };

    /// <summary>
    /// Attempts to change the theme to the given value.
    /// </summary>
    /// <param name="theme">The <see cref="SukiColorTheme"/> to change to.</param>
    public static void TryChangeTheme(SukiColorTheme theme)
    {
        if (_instance is null) return;
        _instance.ColorTheme = theme;
        OnColorThemeChanged?.Invoke(theme);
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
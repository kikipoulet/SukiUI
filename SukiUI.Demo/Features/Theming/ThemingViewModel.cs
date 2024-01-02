using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Models;

namespace SukiUI.Demo.Features.Theming;

public partial class ThemingViewModel : DemoPageBase
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailableColors { get; }
    
    [ObservableProperty] private bool _isDark;
    [ObservableProperty] private SukiColorTheme _activeTheme;
    [ObservableProperty] private bool _animationsEnabled;
    
    private readonly SukiTheme _theme;
    
    public ThemingViewModel() : base("Theming", MaterialIconKind.PaletteOutline, -200)
    {
        _theme = SukiTheme.GetInstance();
        AvailableColors = _theme.ColorThemes;
        IsDark = _theme.ActiveBaseTheme == ThemeVariant.Dark;
        _activeTheme = _theme.ActiveColorTheme;
        _theme.OnBaseThemeChanged += variant =>
        { 
            IsDark = variant == ThemeVariant.Dark;
        };
        _theme.OnColorThemeChanged += theme =>
        {
            _activeTheme = theme;
        };
    }

    partial void OnIsDarkChanged(bool value)
    {
        _theme.ChangeBaseTheme(value ? ThemeVariant.Dark : ThemeVariant.Light);
    }

    partial void OnActiveThemeChanged(SukiColorTheme value)
    {
        _theme.ChangeColorTheme(value);
    }
}
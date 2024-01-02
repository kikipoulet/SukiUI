using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo.Features.Theming;

public partial class ThemingViewModel : DemoPageBase
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailableColors { get; }

    private readonly SukiTheme _theme = SukiTheme.GetInstance();
    
    [ObservableProperty] private bool _isBackgroundAnimated;
    [ObservableProperty] private bool _isLightTheme;

    // TODO: This is very bad.
    private SukiWindow Window =>
        ((SukiWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow!);
    
    public ThemingViewModel() : base("Theming", MaterialIconKind.PaletteOutline, -200)
    {
        AvailableColors = _theme.ColorThemes;
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        _theme.OnBaseThemeChanged += variant =>
        {
            IsLightTheme = variant == ThemeVariant.Light;
        };
        _theme.OnColorThemeChanged += theme =>
        {
            // TODO: Implement a way to make the correct, might need to wrap the thing in a VM, this isn't ideal.
        };
    }

    partial void OnIsLightThemeChanged(bool value)
    {
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    partial void OnIsBackgroundAnimatedChanged(bool value)
    {
        Window.BackgroundAnimationEnabled = value;
    }

    public void SwitchToColorTheme(SukiColorTheme colorTheme) => 
        _theme.ChangeColorTheme(colorTheme);
}
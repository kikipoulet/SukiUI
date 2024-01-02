using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo.Features.Theming;

public partial class ThemingViewModel() : DemoPageBase("Theming", MaterialIconKind.PaletteOutline, -200)
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailablesColors => SukiTheme.GetInstance().ColorThemes;

    [RelayCommand]
    public void SwitchToLightTheme() => SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);

    [RelayCommand]
    public void SwitchToDarkTheme() => SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);

    public bool IsLightTheme
    {
        get { return SukiTheme.GetInstance().ActiveBaseTheme == ThemeVariant.Light; }
    }

    [RelayCommand]
    public void SwitchToColorTheme(SukiColorTheme colorTheme) => SukiTheme.GetInstance().ChangeColorTheme(colorTheme);

    [ObservableProperty] private bool _isBackgroundAnimated;

    [RelayCommand]
    public void ChangeAnimated()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (lifetime?.MainWindow is not SukiWindow window)
        {
            return;
        }

        window.BackgroundAnimationEnabled = IsBackgroundAnimated;
    }
}
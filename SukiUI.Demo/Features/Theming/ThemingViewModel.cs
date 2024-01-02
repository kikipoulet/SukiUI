using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo.Features.Theming;

public partial class ThemingViewModel() : DemoPageBase("Theming", MaterialIconKind.PaletteOutline, -200)
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailablesColors { get; } = SukiTheme.GetInstance().ColorThemes;

    public void SwitchToLightTheme() => SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
    public void SwitchToDarkTheme() => SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
   
    public bool IsLightTheme
    {
        get { return SukiTheme.GetInstance().ActiveBaseTheme == ThemeVariant.Light; }
    }


    public void SwitchToColorTheme(SukiColorTheme colorTheme) => SukiTheme.GetInstance().ChangeColorTheme(colorTheme);
    
    [ObservableProperty] private bool _isBackgroundAnimated ;

    public void ChangeAnimated()
    {
        ((SukiWindow)((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow)
            .BackgroundAnimationEnabled = _isBackgroundAnimated;
    }


}
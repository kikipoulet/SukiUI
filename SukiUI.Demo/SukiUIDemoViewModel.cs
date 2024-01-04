using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;
using SukiUI.Demo.Features.CustomTheme;
using SukiUI.Demo.Services;
using SukiUI.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo;

public partial class SukiUIDemoViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<DemoPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private DemoPageBase _activePage;

    private readonly SukiTheme _theme;

    public SukiUIDemoViewModel(IEnumerable<DemoPageBase> demoPages, PageNavigationService nav)
    {
        DemoPages = new AvaloniaList<DemoPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _theme = SukiTheme.GetInstance();
        nav.NavigationRequested += t =>
        {
            var page = DemoPages.FirstOrDefault(x => x.GetType() == t);
            if (page is null || ActivePage.GetType() == t) return;
            ActivePage = page;
        };
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        _theme.OnBaseThemeChanged += variant =>
        {
            BaseTheme = variant;
            SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {variant}");
        };
        _theme.OnColorThemeChanged += theme =>
            SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {theme.DisplayName}.");
        _theme.OnBackgroundAnimationChanged +=
            value => AnimationsEnabled = value;
    }

    [RelayCommand]
    public void ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? "Animation Enabled" : "Animation Disabled";
        var content = AnimationsEnabled
            ? "Background animations are now enabled."
            : "Background animations are now disabled.";
        SukiHost.ShowToast(title, content);
    }

    [RelayCommand]
    public void ToggleBaseTheme() =>
        _theme.SwitchBaseTheme();

    public void ChangeTheme(SukiColorTheme theme) =>
        _theme.ChangeColorTheme(theme);

    [RelayCommand]
    public void CreateCustomTheme()
    {
        SukiHost.ShowDialog(new CustomThemeDialogViewModel(_theme), allowBackgroundClose: true);
    }

    [RelayCommand]
    public void OpenURL(string url) => UrlUtilities.OpenURL(url);
}
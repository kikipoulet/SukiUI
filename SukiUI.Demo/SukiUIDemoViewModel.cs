using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Demo.Features;
using SukiUI.Demo.Features.CustomTheme;
using SukiUI.Demo.Services;
using SukiUI.Demo.Utilities;
using SukiUI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SukiUI.Demo;

public partial class SukiUIDemoViewModel : ObservableObject
{
    public IAvaloniaReadOnlyList<DemoPageBase> DemoPages { get; }

    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;
    [ObservableProperty] private DemoPageBase? _activePage;
    [ObservableProperty] private bool _windowLocked;
    [ObservableProperty] private bool _titleBarVisible = true;

    private readonly SukiTheme _theme;

    public SukiUIDemoViewModel(IEnumerable<DemoPageBase> demoPages, PageNavigationService pageNavigationService)
    {
        DemoPages = new AvaloniaList<DemoPageBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _theme = SukiTheme.GetInstance();
        
        // Subscribe to the navigation service (when a page navigation is requested)
        pageNavigationService.NavigationRequested += pageType =>
        {
            var page = DemoPages.FirstOrDefault(x => x.GetType() == pageType);
            if (page is null || ActivePage?.GetType() == pageType) return;
            ActivePage = page;
        };
        
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        
        // Subscribe to the base theme changed events
        _theme.OnBaseThemeChanged += variant =>
        {
            BaseTheme = variant;
            SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {variant}");
        };
        
        // Subscribe to the color theme changed events
        _theme.OnColorThemeChanged += theme =>
            SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {theme.DisplayName}.");
        
        // Subscribe to the background animation changed events
        _theme.OnBackgroundAnimationChanged +=
            value => AnimationsEnabled = value;
    }

    [RelayCommand]
    private Task ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? "Animation Enabled" : "Animation Disabled";
        var content = AnimationsEnabled
            ? "Background animations are now enabled."
            : "Background animations are now disabled.";
        return SukiHost.ShowToast(title, content);
    }

    [RelayCommand]
    private void ToggleBaseTheme() =>
        _theme.SwitchBaseTheme();

    public void ChangeTheme(SukiColorTheme theme) =>
        _theme.ChangeColorTheme(theme);

    [RelayCommand]
    private void CreateCustomTheme() => 
        SukiHost.ShowDialog(new CustomThemeDialogViewModel(_theme), allowBackgroundClose: true);

    [RelayCommand]
    private void ToggleWindowLock()
    {
        WindowLocked = !WindowLocked;
        SukiHost.ShowToast(
            $"Window {(WindowLocked ? "Locked" : "Unlocked")}", 
            $"Window has been {(WindowLocked ? "locked" : "unlocked")}.");
    }

    [RelayCommand]
    private void ToggleTitleBar()
    {
        TitleBarVisible = !TitleBarVisible;
        SukiHost.ShowToast(
            $"Title Bar {(TitleBarVisible ? "Visible" : "Hidden")}", 
            $"Window title bar has been {(TitleBarVisible ? "shown" : "hidden")}.");
    }
    
    [RelayCommand]
    private static void OpenUrl(string url) => UrlUtilities.OpenUrl(url);
}
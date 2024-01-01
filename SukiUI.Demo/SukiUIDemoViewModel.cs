using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;
using SukiUI.Demo.Common;
using SukiUI.Demo.Features;
using SukiUI.Demo.Features.CustomTheme;
using SukiUI.Models;

namespace SukiUI.Demo;

public partial class SukiUIDemoViewModel : ViewAwareObservableObject
{
    public AvaloniaList<DemoPageBase> Features { get; } = [];
    
    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

    [ObservableProperty] private ThemeVariant _baseTheme;
    [ObservableProperty] private bool _animationsEnabled;

    private readonly SukiTheme _theme;
    
    public SukiUIDemoViewModel(IEnumerable<DemoPageBase> features)
    {
        Features.AddRange(features.OrderBy(x => x.Index));
        _theme = SukiTheme.GetInstance();
        Themes = _theme.ColorThemes;
        BaseTheme = _theme.ActiveBaseTheme;
        _theme.OnBaseThemeChanged += variant =>
        {
            BaseTheme = variant;
            SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {variant}");
        };
        _theme.OnColorThemeChanged += theme =>
        {
            SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {theme.DisplayName}.");
        };
    }

    public void ToggleAnimations()
    {
        AnimationsEnabled = !AnimationsEnabled;
        var title = AnimationsEnabled ? "Animation Enabled" : "Animation Disabled";
        var content = AnimationsEnabled
            ? "Background animations are now enabled."
            : "Background animations are now disabled.";
        SukiHost.ShowToast(title, content);
    }

    public void ToggleBaseTheme() => 
        _theme.SwitchBaseTheme();

    public void ChangeTheme(SukiColorTheme theme) =>
        _theme.ChangeColorTheme(theme);

    public void CreateCustomTheme()
    {
        SukiHost.ShowDialog(new CustomThemeDialogViewModel(_theme), allowBackgroundClose: true);
    }
}
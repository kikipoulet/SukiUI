using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo.Features.CustomTheme;

public partial class CustomThemeDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private Color _primaryColor;
    [ObservableProperty] private Color _accentColor;

    private readonly SukiTheme _theme;

    public CustomThemeDialogViewModel(SukiTheme theme)
    {
        _theme = theme;
        _displayName = "Pink";
        _primaryColor = Colors.DeepPink;
        _accentColor = Colors.Pink;
    }

    [RelayCommand]
    public void TryCreateTheme()
    {
        if (string.IsNullOrEmpty(DisplayName)) return;
        var theme = new SukiColorTheme(DisplayName, PrimaryColor, AccentColor);
        _theme.AddColorTheme(theme);
        _theme.ChangeColorTheme(theme);
        SukiHost.CloseDialog();
    }
}
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiTest;

public partial class MainWindow : SukiWindow
{
    private SukiTheme? _theme;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _theme = SukiTheme.GetInstance();
        _theme.OnBaseThemeChanged += async variant =>
        {
            await SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {variant}",
                onClicked: () => { SukiHost.ShowToast("Success!", "You Closed A Toast By Clicking On It!"); });
        };
        _theme.OnColorThemeChanged += async theme =>
        {
            await SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {theme.DisplayName}.");
        };

        _theme.AddColorTheme(new SukiColorTheme("Neon Pink", Colors.DeepPink, Colors.GreenYellow));
    }

    private void ChangeTheme(object? sender, RoutedEventArgs e)
    {
        _theme?.SwitchBaseTheme();
    }

    private void ChangeColor(object? sender, RoutedEventArgs e)
    {
        _theme?.SwitchColorTheme();
    }

    private void ChangeAnimationState(object? sender, RoutedEventArgs e)
    {
        BackgroundAnimationEnabled = !BackgroundAnimationEnabled;
        var title = BackgroundAnimationEnabled ? "Animation Enabled" : "Animation Disabled";
        var content = BackgroundAnimationEnabled
            ? "Background animations are now enabled."
            : "Background animations are now disabled.";
        SukiHost.ShowToast(title, content);
    }
}
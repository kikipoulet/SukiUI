using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace SukiUI.Controls;

/// <summary>
/// Simple initial implementation that replaces the standard Avalonia title bar.
/// Allows for window interaction to actually take place across the full height of the title bar.
/// </summary>
public class WindowTitleBar : Border
{
    private static Window? _window;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _window = desktop.MainWindow;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_window is null) return;
        if (e.ClickCount >= 2)
        {
            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
            _window.BeginMoveDrag(e);
    }
}
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace SukiUI.Extensions;

public static class WindowExtensions
{
    /// <summary>
    /// Gets the screen that contains the largest area of the window.
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static Screen? GetHostScreen(this Window window)
    {
        if (window.Screens.ScreenCount == 0) return null;

        return window.Screens.ScreenFromWindow(window) ??
               window.Screens.Primary ??
               window.Screens.All[0];
    }

    /// <summary>
    /// Centers the window on the screen that contains the largest area of the window.
    /// </summary>
    /// <param name="window"></param>
    public static void CenterOnScreen(this Window window)
    {
        window.CenterOnScreen(window.GetHostScreen());
    }

    /// <summary>
    /// Centers the window on the specified screen.
    /// </summary>
    /// <param name="window"></param>
    /// <param name="screen"></param>
    public static void CenterOnScreen(this Window window, Screen? screen)
    {
        if (screen is null || window.WindowState != WindowState.Normal) return;

        window.Position = new PixelPoint((int)(screen.Bounds.X + screen.WorkingArea.Width / 2.0 - window.Bounds.Width / (2.0 / window.RenderScaling)),
                                        (int)(screen.Bounds.Y + screen.WorkingArea.Height / 2.0 - window.Bounds.Height / (2.0 / window.RenderScaling)));
    }
}
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

    /// <summary>
    /// Constrain the maximum size of the window to a ratio of the screen size.
    /// </summary>
    /// <param name="window"></param>
    /// <param name="maxWidthScreenRatio">The max width ratio from [0.0 to 1.0].</param>
    /// <param name="maxHeightScreenRatio">The max height ratio from [0.0 to 1.0].</param>
    /// <remarks><p>A ratio &lt;= 0 or a window state of [<see cref="WindowState.FullScreen"/> / <see cref="WindowState.Maximized"/>] will remove the max limit.</p>
    /// <p>A ratio of <see cref="double.NaN"/> will cause that setter size to be ignored.</p>
    /// <p>The resulting size can never be smaller than the MinWidth/MinHeight properties.</p></remarks>
    public static void ConstrainMaxSizeToScreenRatio(this Window window, double maxWidthScreenRatio, double maxHeightScreenRatio)
    {
        Screen? screen = null;
        WindowState? windowState = null;

        if (!double.IsNaN(maxWidthScreenRatio))
        {
            windowState = window.WindowState;
            if (maxWidthScreenRatio <= 0 || windowState is WindowState.FullScreen or WindowState.Maximized)
            {
                window.MaxWidth = double.PositiveInfinity;
            }
            else
            {
                screen = window.GetHostScreen();
                if (screen is null) return;

                var desiredMaxWidth = screen.WorkingArea.Width / window.RenderScaling * maxWidthScreenRatio;
                window.MaxWidth = Math.Max(window.MinWidth, desiredMaxWidth);
            }
        }

        if (!double.IsNaN(maxHeightScreenRatio))
        {
            windowState ??= window.WindowState;
            if (maxHeightScreenRatio <= 0 || windowState is WindowState.FullScreen or WindowState.Maximized)
            {
                window.MaxHeight = double.PositiveInfinity;
            }
            else
            {
                screen ??= window.GetHostScreen();
                if (screen is null) return;

                var desiredMaxHeight = screen.WorkingArea.Height / window.RenderScaling * maxHeightScreenRatio;
                window.MaxHeight = Math.Max(window.MinHeight, desiredMaxHeight);
            }
        }
    }
}
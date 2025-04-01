using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.Controls;

namespace SukiUI.MessageBox;

/// <summary>
/// Message box factory.
/// </summary>
public static class SukiMessageBox
{
    #region Create MessageBox
    /// <summary>
    /// Creates a base message box window.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static SukiWindow CreateMessageBoxWindow(SukiMessageBoxOptions? options = null)
    {
        options ??= new SukiMessageBoxOptions();
        return new SukiWindow
        {
            CanResize = options.CanResize,
            CanMinimize = options.CanMinimize,
            CanMaximize = options.CanMaximize,
            CanFullScreen = false,
            IsTitleBarVisible = options.IsTitleBarVisible,
            Title = options.Title,
            LogoContent = options.LogoContent,
            ShowInTaskbar = options.ShowInTaskbar,
            WindowStartupLocation = options.WindowStartupLocation,
            SizeToContent = options.SizeToContent,
            Width = options.Width,
            Height = options.Height,
            MinWidth = options.MinWidth,
            MinHeight = options.MinHeight,
            MaxWidthScreenRatio = options.MaxWidthScreenRatio,
            MaxHeightScreenRatio = options.MaxHeightScreenRatio,
        };
    }
    #endregion

    #region ShowDialog
    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="owner">Parent window to own this message box.</param>
    /// <param name="messageBox">The message box.</param>
    /// <param name="windowOptions">The window options.</param>
    /// <returns>
    /// <see cref="SukiMessageBoxResult"/> when a preset button was clicked.<br/>
    /// <see cref="Button"/> when a custom button was clicked.<br/>
    /// <c>null</c> when the window was closed without clicking a button.
    /// </returns>
    public static Task<object?> ShowDialog(Window owner, SukiMessageBoxHost messageBox, SukiMessageBoxOptions? windowOptions = null)
    {
        var window = CreateMessageBoxWindow(windowOptions);
        window.Icon ??= owner.Icon;
        if (string.IsNullOrWhiteSpace(window.Title)) window.Title = $"{owner.Title} Message";
        window.Tag = messageBox;
        window.Closed += WindowOnClosed;

        if (messageBox.Header is string headerText)
        {
            messageBox.Header = new SelectableTextBlock
            {
                Text = headerText,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };
        }

        if (messageBox.Content is string contentText)
        {
            messageBox.Content = new SelectableTextBlock
            {
                Text = contentText,
                TextWrapping = TextWrapping.Wrap
            };
        }

        var actionButtons = messageBox.ActionButtonsSource;
        if (actionButtons is not null)
        {
            var buttonArray = actionButtons as Button[] ?? actionButtons.ToArray();
            foreach (var button in buttonArray)
            {
                button.Tag = (button.Tag, window);
                button.Click += ActionButtonOnClick;
            }

            if (buttonArray.Length <= 1)
            {
                window.KeyUp += WindowOnKeyUp;
            }

            if (buttonArray.Length == 1)
            {
                buttonArray[0].IsCancel = true;
            }
        }
        else
        {
            window.KeyUp += WindowOnKeyUp;
        }

        window.Content = messageBox;
        return window.ShowDialog<object?>(owner);
    }

    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="messageBox">The message box.</param>
    /// <param name="windowOptions">The window options.</param>
    /// <returns>
    /// <see cref="SukiMessageBoxResult"/> when a preset button was clicked.<br/>
    /// <see cref="Button"/> when a custom button was clicked.<br/>
    /// <c>null</c> when the window was closed without clicking a button.
    /// </returns>
    /// <exception cref="InvalidOperationException">The application does not contain a main window.</exception>
    public static Task<object?> ShowDialog(SukiMessageBoxHost messageBox, SukiMessageBoxOptions? windowOptions = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is not null)
            {
                return ShowDialog(desktop.MainWindow, messageBox, windowOptions);
            }

            throw new InvalidOperationException("The application does not contain a main window.");
        }

        throw new InvalidOperationException("The application is not an instance of IClassicDesktopStyleApplicationLifetime.");
    }
    #endregion

    #region Events
    private static void WindowOnKeyUp(object sender, KeyEventArgs e)
    {
        if (sender is not Window window) return;
        if (e.Key == Key.Escape)
        {
            window.Close(SukiMessageBoxResult.Close);
        }
    }

    /// <summary>
    /// Handles the closed event of the window to dispose events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void WindowOnClosed(object sender, EventArgs e)
    {
        if (sender is not Window window) return;
        if (window.Tag is not SukiMessageBoxHost host) return;
        window.KeyUp -= WindowOnKeyUp;
        window.Closed -= WindowOnClosed;

        var actionButtons = host.ActionButtonsSource;
        if (actionButtons is not null)
        {
            foreach (var button in actionButtons)
            {
                button.Click -= ActionButtonOnClick;
            }
        }
    }

    /// <summary>
    /// Handles the click event of the action button to close the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ActionButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is SukiWindow window)
        {
            window.Close();
            return;
        }
        if (button.Tag is not ValueTuple<object?, SukiWindow> tuple) return;

        if (tuple.Item1 is SukiMessageBoxResult result)
        {
            tuple.Item2.Close(result);
        }
        else
        {
            tuple.Item2.Close(sender);
        }
    }
    #endregion
}
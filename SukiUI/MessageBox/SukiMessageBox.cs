using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.Controls;
using SukiUI.Extensions;

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
    public static Window CreateMessageBoxWindow(SukiMessageBoxOptions? options = null)
    {
        options ??= new SukiMessageBoxOptions();

        Window window;

        if (options.UseNativeWindow)
        {
            window = new Window
            {
                SystemDecorations = options.IsTitleBarVisible ? SystemDecorations.Full : SystemDecorations.BorderOnly,
            };
            window.ConstrainMaxSizeToScreenRatio(options.MaxWidthScreenRatio, options.MaxHeightScreenRatio);
        }
        else
        {
            var sukiWindow = new SukiWindow
            {
                CanMinimize = options.CanMinimize,
                CanMaximize = options.CanMaximize,
                CanFullScreen = false,
                IsTitleBarVisible = options.IsTitleBarVisible,
                MaxWidthScreenRatio = options.MaxWidthScreenRatio,
                MaxHeightScreenRatio = options.MaxHeightScreenRatio,
            };

            if (options.BackgroundAnimationEnabled is not null) sukiWindow.BackgroundAnimationEnabled = options.BackgroundAnimationEnabled.Value;
            if (options.BackgroundForceSoftwareRendering is not null) sukiWindow.BackgroundForceSoftwareRendering = options.BackgroundForceSoftwareRendering.Value;
            if (options.BackgroundShaderFile is not null) sukiWindow.BackgroundShaderFile = options.BackgroundShaderFile;
            if (options.BackgroundShaderCode is not null) sukiWindow.BackgroundShaderCode = options.BackgroundShaderCode;
            if (options.BackgroundStyle is not null) sukiWindow.BackgroundStyle = options.BackgroundStyle.Value;
            if (options.BackgroundTransitionTime is not null) sukiWindow.BackgroundTransitionTime = options.BackgroundTransitionTime.Value;
            if (options.BackgroundTransitionsEnabled is not null) sukiWindow.BackgroundTransitionsEnabled = options.BackgroundTransitionsEnabled.Value;
            if (options.LogoContent is not null) sukiWindow.LogoContent = options.LogoContent;

            window = sukiWindow;
        }

        // Shared properties
        if (options.Icon is not null) window.Icon = options.Icon;
        window.Topmost = options.Topmost;
        window.CanResize = options.CanResize;
        window.Title = options.Title;
        window.ShowInTaskbar = options.ShowInTaskbar;
        window.WindowStartupLocation = options.WindowStartupLocation;
        window.SizeToContent = options.SizeToContent;
        window.Width = options.Width;
        window.Height = options.Height;
        window.MinWidth = options.MinWidth;
        window.MinHeight = options.MinHeight;

        return window;
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
    public static async Task<object?> ShowDialog(Window owner, SukiMessageBoxHost messageBox, SukiMessageBoxOptions? windowOptions = null)
    {
        windowOptions ??= new SukiMessageBoxOptions();

        if (owner is SukiWindow sukiWindow)
        {
            if (windowOptions.BackgroundAnimationEnabled is null) windowOptions = windowOptions with { BackgroundAnimationEnabled = sukiWindow.BackgroundAnimationEnabled };
            if (windowOptions.BackgroundForceSoftwareRendering is null) windowOptions = windowOptions with { BackgroundForceSoftwareRendering = sukiWindow.BackgroundForceSoftwareRendering };
            if (windowOptions.BackgroundShaderCode is null) windowOptions = windowOptions with { BackgroundShaderCode = sukiWindow.BackgroundShaderCode };
            if (windowOptions.BackgroundShaderFile is null) windowOptions = windowOptions with { BackgroundShaderFile = sukiWindow.BackgroundShaderFile };
            if (windowOptions.BackgroundStyle is null) windowOptions = windowOptions with { BackgroundStyle = sukiWindow.BackgroundStyle };
            if (windowOptions.BackgroundTransitionTime is null) windowOptions = windowOptions with { BackgroundTransitionTime = sukiWindow.BackgroundTransitionTime };
            if (windowOptions.BackgroundTransitionsEnabled is null) windowOptions = windowOptions with { BackgroundTransitionsEnabled = sukiWindow.BackgroundTransitionsEnabled };
        }

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
                Classes = { "h5" },
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
        // Subscribe events
        if (actionButtons is not null)
        {
            foreach (var button in actionButtons)
            {
                var tag = (button.Tag as SukiMessageBoxButtonTag) ?? new SukiMessageBoxButtonTag()
                {
                    Tag = button.Tag
                };
                tag.Owner = window;
                button.Tag = tag;
                button.Click += ActionButtonOnClick;
            }

            if (actionButtons.Count <= 1)
            {
                window.KeyUp += WindowOnKeyUp;
            }

            if (actionButtons.Count == 1)
            {
                actionButtons[0].IsCancel = true;
            }
        }
        else
        {
            window.KeyUp += WindowOnKeyUp;
        }

        if (windowOptions.UseNativeWindow)
        {
            var sukiHost = new SukiMainHost
            {
                Content = messageBox,
            };

            if (windowOptions.BackgroundAnimationEnabled is not null) sukiHost.BackgroundAnimationEnabled = windowOptions.BackgroundAnimationEnabled.Value;
            if (windowOptions.BackgroundForceSoftwareRendering is not null) sukiHost.BackgroundForceSoftwareRendering = windowOptions.BackgroundForceSoftwareRendering.Value;
            if (windowOptions.BackgroundShaderFile is not null) sukiHost.BackgroundShaderFile = windowOptions.BackgroundShaderFile;
            if (windowOptions.BackgroundShaderCode is not null) sukiHost.BackgroundShaderCode = windowOptions.BackgroundShaderCode;
            if (windowOptions.BackgroundStyle is not null) sukiHost.BackgroundStyle = windowOptions.BackgroundStyle.Value;
            if (windowOptions.BackgroundTransitionTime is not null) sukiHost.BackgroundTransitionTime = windowOptions.BackgroundTransitionTime.Value;
            if (windowOptions.BackgroundTransitionsEnabled is not null) sukiHost.BackgroundTransitionsEnabled = windowOptions.BackgroundTransitionsEnabled.Value;

            window.Content = sukiHost;
        }
        else
        {
            window.Content = messageBox;
        }

        var result = await window.ShowDialog<object?>(owner);

        // Unsubscribe events
        if (actionButtons is not null)
        {
            foreach (var button in actionButtons)
            {
                button.Click -= ActionButtonOnClick;
            }

            if (actionButtons.Count <= 1)
            {
                window.KeyUp -= WindowOnKeyUp;
            }
        }
        else
        {
            window.KeyUp -= WindowOnKeyUp;
        }

        // Dispose window if needed
        if (window is IDisposable disposable) disposable.Dispose();

        return result;
    }

    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="owner">Parent window to own this message box.</param>
    /// <param name="messageBox">The message box.</param>
    /// <param name="windowOptions">The window options.</param>
    /// <returns>
    /// <see cref="SukiMessageBoxResult"/> when a preset button was clicked.<br/>
    /// If the window was closed without clicking a button, returns <see cref="SukiMessageBoxResult.Close"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the dialog result is not a standard SukiMessageBoxResult button, such as when a custom button is used.</exception>
    public static async Task<SukiMessageBoxResult> ShowDialogResult(Window owner, SukiMessageBoxHost messageBox, SukiMessageBoxOptions? windowOptions = null)
    {
        var result = await ShowDialog(owner, messageBox, windowOptions);
        if (result is null) return SukiMessageBoxResult.Close; // Closed without clicking a button
        if (result is SukiMessageBoxResult mbResult) return mbResult;
        throw new InvalidOperationException("Unknown button type or custom button was used. This method is only compatible with SukiMessageBoxResult buttons.");
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

    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="messageBox">The message box.</param>
    /// <param name="windowOptions">The window options.</param>
    /// <returns>
    /// <see cref="SukiMessageBoxResult"/> when a preset button was clicked.<br/>
    /// If the window was closed without clicking a button, returns <see cref="SukiMessageBoxResult.Close"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the dialog result is not a standard SukiMessageBoxResult button, such as when a custom button is used.</exception>
    /// <exception cref="InvalidOperationException">The application does not contain a main window.</exception>
    /// <exception cref="InvalidOperationException">The application is not an instance of IClassicDesktopStyleApplicationLifetime.</exception>
    public static Task<SukiMessageBoxResult> ShowDialogResult(SukiMessageBoxHost messageBox, SukiMessageBoxOptions? windowOptions = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is not null)
            {
                return ShowDialogResult(desktop.MainWindow, messageBox, windowOptions);
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

        // Unload the content to free resources and allow reuse of the host control
        window.Content = null;
    }

    /// <summary>
    /// Handles the click event of the action button to close the window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static async void ActionButtonOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is Window window)
        {
            window.Close();
            return;
        }
        if (button.Tag is not SukiMessageBoxButtonTag buttonTag) return;
        if (buttonTag.Owner is null) return;

        buttonTag.Owner.IsEnabled = false; // Prevent multiple clicks and interactions
        if (buttonTag.Link is not null) await buttonTag.OpenLink();

        if (buttonTag.Result.HasValue)
        {
            buttonTag.Owner.Close(buttonTag.Result.Value);
        }
        else
        {
            buttonTag.Owner.Close(sender);
        }
    }
    #endregion
}
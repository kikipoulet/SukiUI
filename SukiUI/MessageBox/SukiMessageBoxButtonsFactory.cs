using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.MessageBox;

/// <summary>
/// Factory for creating buttons for the message box.
/// </summary>
public static class SukiMessageBoxButtonsFactory
{
    /// <summary>
    /// Creates a new Button configured for use as a message box action.
    /// </summary>
    /// <remarks>The returned button always includes the 'MessageBoxAction' class in addition to any classes
    /// provided. The link parameter is stored in the button's tag and does not affect the button's visual appearance
    /// directly.</remarks>
    /// <param name="content">The content to display within the button. This can be any object, such as a string or a UI element. If null, the
    /// button will have no content.</param>
    /// <param name="classes">A space-separated list of CSS class names to apply to the button. If null or empty, only the default
    /// 'MessageBoxAction' class is applied.</param>
    /// <param name="link">An optional link associated with the button. If specified, the link is stored in the button's tag for later use.</param>
    /// <returns>A Button instance configured with the specified content, classes, and link information.</returns>
    public static Button CreateButton(object? content = null, string? classes = null, string? link = null)
    {
        var button = new Button
        {
            Content = content,
            Tag = new SukiMessageBoxButtonTag
            {
                Link = link
            }
        };

        if (!string.IsNullOrWhiteSpace(classes))
        {
            var split = classes!.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            button.Classes.AddRange(split);
        }
        button.Classes.Add("MessageBoxAction");
        return button;
    }

    /// <summary>
    /// Creates a new button configured for a message box with the specified content, result value, optional
    /// classes, and optional link.
    /// </summary>
    /// <remarks>The returned button's <c>Tag</c> property is set to a <see cref="SukiMessageBoxButtonTag"/>
    /// containing the specified result and link. This method is typically used to create buttons for custom message
    /// boxes where each button represents a distinct user choice.</remarks>
    /// <param name="content">The content to display within the button. This can be a string, UI element, or any object representing the
    /// button's label or visual content. Can be null for an empty button.</param>
    /// <param name="result">The result value associated with the button, indicating the action or response when the button is clicked.</param>
    /// <param name="classes">Optional CSS class names to apply to the button for custom styling. Can be null or empty if no additional
    /// styling is required.</param>
    /// <param name="link">An optional link to associate with the button. If specified, clicking the button will navigate to this
    /// link. Can be null if no link is needed.</param>
    /// <returns>A <see cref="Button"/> instance configured with the specified content, result value, CSS classes, and hyperlink.</returns>
    public static Button CreateButton(object? content, SukiMessageBoxResult result, string? classes = null, string? link = null)
    {
        var button = CreateButton(content, classes);
        button.Tag = new SukiMessageBoxButtonTag(result)
        {
            Link = link
        };
        return button;
    }


    /// <summary>
    /// Create and builds a button for the specified result.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static Button CreateButton(SukiMessageBoxResult result)
    {
        var button = CreateButton(null, result, "Flat");

        var pathIcon = new PathIcon
        {
            Width = 12,
            Height = 12,
            Foreground = Brushes.White,
            Margin = new Thickness(0,0,5,0)
        };

        switch (result)
        {
            case SukiMessageBoxResult.OK:
                button.IsDefault = true;
                pathIcon.Data = Icons.Check;
                break;
            case SukiMessageBoxResult.Yes:
                button.IsDefault = true;
                pathIcon.Data = Icons.Check;
                break;
            case SukiMessageBoxResult.No:
                button.IsCancel = true;
                pathIcon.Data = Icons.Cross;
                break;
            case SukiMessageBoxResult.Cancel:
                button.IsCancel = true;
                pathIcon.Data = Icons.Cancel;
                break;
            case SukiMessageBoxResult.Apply:
                button.IsDefault = true;
                pathIcon.Data = Icons.Check;
                break;
            case SukiMessageBoxResult.Ignore:
                pathIcon.Data = Icons.DebugStepOver;
                break;
            case SukiMessageBoxResult.Retry:
                button.IsDefault = true;
                pathIcon.Data = Icons.Refresh;
                break;
            case SukiMessageBoxResult.Abort:
                button.IsCancel = true;
                pathIcon.Data = Icons.Cancel;
                break;
            case SukiMessageBoxResult.Continue:
                pathIcon.Data = Icons.ArrowRight;
                break;
            case SukiMessageBoxResult.Close:
                button.IsCancel = true;
                pathIcon.Data = Icons.Logout;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        Application.Current!.TryFindResource($"STRING_PROMPT_{result.ToString().ToUpperInvariant()}", out var buttonLocale);
        if (buttonLocale is not string buttonLocaleText)
        {
            buttonLocaleText = result.ToString();
        }

        button.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                pathIcon,
                new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = buttonLocaleText,
                }
            }
        };


        return button;
    }
}
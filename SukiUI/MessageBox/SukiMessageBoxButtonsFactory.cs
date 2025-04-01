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
    /// Create a custom button with the specified content and classes.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="classes"></param>
    /// <returns></returns>
    public static Button CreateButton(object? content = null, string? classes = null)
    {
        var button = new Button
        {
            Content = content
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
    /// Create a custom button with the specified content, result and classes.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="result"></param>
    /// <param name="classes"></param>
    /// <returns></returns>
    public static Button CreateButton(object? content, SukiMessageBoxResult result, string? classes = null)
    {
        var button = CreateButton(content, classes);
        button.Tag = result;
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
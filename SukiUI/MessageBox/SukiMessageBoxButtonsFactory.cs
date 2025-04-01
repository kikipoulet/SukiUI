using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.MessageBox;

public static class SukiMessageBoxButtonsFactory
{
    public static Button CreateButton(object? content = null, params IEnumerable<string> classes)
    {
        var button = new Button
        {
            Padding = new Thickness(15, 10),
            Content = content
        };

        button.Classes.AddRange(classes);
        return button;
    }

    public static Button CreateButton(SukiMessageBoxResult result)
    {
        var button = CreateButton(null, "Flat");
        button.Tag = result;

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
            case SukiMessageBoxResult.Cancel:
                button.IsCancel = true;
                pathIcon.Data = Icons.Cancel;
                break;
            case SukiMessageBoxResult.Yes:
                button.IsDefault = true;
                pathIcon.Data = Icons.Check;
                break;
            case SukiMessageBoxResult.No:
                button.IsCancel = true;
                pathIcon.Data = Icons.Cross;
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
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.MessageBox;

/// <summary>
/// Factory for creating icons for the message box.
/// </summary>
public static class SukiMessageBoxIconsFactory
{
    public static PathIcon CreateIcon(double size = 24)
    {
        return new PathIcon
        {
            Width = size,
            Height = size,
        };
    }

    public static PathIcon CreateIcon(SukiMessageBoxIcons icon, double size = 24)
    {
        var pathIcon = CreateIcon(size);
        pathIcon.Tag = icon;

        switch (icon)
        {
            case SukiMessageBoxIcons.Question:
                pathIcon.Data = Icons.CircleHelp;
                pathIcon.Foreground = Brushes.CornflowerBlue;
                break;
            case SukiMessageBoxIcons.Information:
                pathIcon.Data = Icons.CircleInformation;
                pathIcon.Foreground = Brushes.CornflowerBlue;
                break;
            case SukiMessageBoxIcons.Exclamation:
                pathIcon.Data = Icons.CircleWarning;
                pathIcon.Foreground = Brushes.Gold;
                break;
            case SukiMessageBoxIcons.Warning:
                pathIcon.Data = Icons.TriangleAlert;
                pathIcon.Foreground = Brushes.Gold;
                break;
            case SukiMessageBoxIcons.Error:
                pathIcon.Data = Icons.CircleClose;
                pathIcon.Foreground = Brushes.DarkRed;
                pathIcon.Foreground = Brushes.Firebrick;
                break;
            case SukiMessageBoxIcons.Success:
                pathIcon.Data = Icons.CircleCheck;
                pathIcon.Foreground = Brushes.ForestGreen;
                break;
            case SukiMessageBoxIcons.Star:
                pathIcon.Data = Icons.Star;
                pathIcon.Foreground = Brushes.Gold;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
        }

        return pathIcon;
    }
}
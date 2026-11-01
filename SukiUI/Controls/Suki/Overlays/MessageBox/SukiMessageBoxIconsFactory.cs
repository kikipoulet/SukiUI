using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Content;

namespace SukiUI.MessageBox;

/// <summary>
/// Factory for creating icons for the message box.
/// </summary>
public static class SukiMessageBoxIconsFactory
{
    public static PathIcon CreateIcon(double size = double.NaN)
    {
        return new PathIcon
        {
            Width = size,
            Height = size,
        };
    }

    public static PathIcon CreateIcon(SukiMessageBoxIcons icon, double size = double.NaN)
    {
        var pathIcon = CreateIcon(size);
        pathIcon.Tag = icon;

        switch (icon)
        {
            case SukiMessageBoxIcons.Question:
            {
                pathIcon.Data = Icons.CircleHelp;
                /*if (Application.Current!.TryGetResource("SukiInformationColor", Application.Current.ActualThemeVariant, out var value))
                {
                    pathIcon.Foreground = value is Color color ? new ImmutableSolidColorBrush(color) : Brushes.CornflowerBlue;
                }
                else
                {
                    pathIcon.Foreground = Brushes.CornflowerBlue;
                }*/
                pathIcon.Foreground = Brushes.CornflowerBlue;
                break;
            }
            case SukiMessageBoxIcons.Information:
                pathIcon.Data = Icons.CircleInformation;
                pathIcon.Foreground = Brushes.CornflowerBlue;
                break;
            case SukiMessageBoxIcons.Exclamation:
                pathIcon.Data = Icons.CircleWarning;
                pathIcon.Foreground = Brushes.Gold;
                break;
            case SukiMessageBoxIcons.Warning:
            {
                pathIcon.Data = Icons.TriangleAlert;
                pathIcon.Foreground = Brushes.Gold;
                /*if (Application.Current!.TryGetResource("SukiWarningColor", Application.Current.ActualThemeVariant, out var value))
                {
                    pathIcon.Foreground = value is Color color ? new ImmutableSolidColorBrush(color) : Brushes.Gold;
                }
                else
                {
                    pathIcon.Foreground = Brushes.Gold;
                }*/
                break;
            }
            case SukiMessageBoxIcons.Error:
                pathIcon.Data = Icons.CircleClose;
                pathIcon.Foreground = Brushes.Firebrick;
                break;
            case SukiMessageBoxIcons.Success:
            {
                pathIcon.Data = Icons.CircleCheck;
                pathIcon.Foreground = Brushes.ForestGreen;
                break;
            }
            case SukiMessageBoxIcons.Star:
            {
                pathIcon.Data = Icons.Star;
                pathIcon.Foreground = Brushes.Gold;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
        }

        return pathIcon;
    }
}
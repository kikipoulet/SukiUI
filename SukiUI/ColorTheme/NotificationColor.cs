using Avalonia.Media;

namespace SukiUI.ColorTheme;

public static class NotificationColor
{
    public static readonly SolidColorBrush Info = new(Color.FromRgb(89,126,255));
    public static readonly SolidColorBrush Success = new(Color.FromRgb(35,143,35));
    public static readonly SolidColorBrush Warning = new(Color.FromRgb(177,113,20));
    public static readonly SolidColorBrush Error = new(Color.FromRgb(216,63,48));
}
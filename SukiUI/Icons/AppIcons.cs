using Avalonia.Media;

namespace SukiUI.Icons;

public static class AppIcons
{
    public static readonly StreamGeometry WindowMinimize = Parse("M20,14H4V10H20");

    public static readonly StreamGeometry WindowRestore = Parse("M4,8H8V4H20V16H16V20H4V8M16,8V14H18V6H10V8H16M6,12V18H14V12H6Z");

    public static readonly StreamGeometry WindowMaximize = Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z");
    
    private static StreamGeometry Parse(string path) => StreamGeometry.Parse(path);
}
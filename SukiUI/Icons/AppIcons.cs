using Avalonia.Media;

namespace SukiUI.Icons;

/// <summary>
/// AppIcons provided by:
/// :: Material Icons under Apache V2 - https://github.com/google/material-design-icons/blob/master/LICENSE
/// </summary>
public static class AppIcons
{
    // Material Icons
    public static readonly StreamGeometry WindowMinimize = Parse("M20,14H4V10H20");

    // Material Icons
    public static readonly StreamGeometry WindowRestore = Parse("M4,8H8V4H20V16H16V20H4V8M16,8V14H18V6H10V8H16M6,12V18H14V12H6Z");

    // Material Icons
    public static readonly StreamGeometry WindowMaximize = Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z");

    // Material Icons
    public static readonly StreamGeometry WindowClose = Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z");
    
    private static StreamGeometry Parse(string path) => StreamGeometry.Parse(path);
}
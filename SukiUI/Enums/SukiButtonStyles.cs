namespace SukiUI.Enums;

/// <summary>
/// Represents the styles available on the Suki theme for the buttons.
/// </summary>
[Flags]
public enum SukiButtonStyles
{
    Standard  = (1 << 0),
    Basic     = (1 << 1),
    Flat      = (1 << 2),
    Accent    = (1 << 3),
    Success   = (1 << 4),
    Danger    = (1 << 5),

    Rounded   = (1 << 6),
    Outlined  = (1 << 7),
    Card      = (1 << 8),

    Icon      = (1 << 9),
    Large     = (1 << 10),

    WindowControlsButton = (1 << 11),
}
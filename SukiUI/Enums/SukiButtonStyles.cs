namespace SukiUI.Enums;

/// <summary>
/// Represents the styles available on the Suki theme for the buttons.
/// </summary>
[Flags]
public enum SukiButtonStyles
{
    Standard     = (1 << 0),
    Basic        = (1 << 1),
    Flat         = (1 << 2),
    Accent       = (1 << 3),
    Success      = (1 << 4),
    Information  = (1 << 5),
    Warning      = (1 << 6),
    Danger       = (1 << 7),

    Rounded      = (1 << 8),
    Outlined     = (1 << 9),
    Card         = (1 << 10),

    Icon         = (1 << 11),
    Large        = (1 << 12),

    NoPressedAnimation = (1 << 13),
    WindowControlsButton = (1 << 14),
}
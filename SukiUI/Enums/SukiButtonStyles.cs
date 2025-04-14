namespace SukiUI.Enums;

/// <summary>
/// Represents the styles available on the Suki theme for the buttons.
/// </summary>
[Flags]
public enum SukiButtonStyles
{
    Standard     = (1 << 0),
    Basic        = (1 << 1),
    Discrete     = (1 << 2),
    Flat         = (1 << 3),
    Accent       = (1 << 4),
    Success      = (1 << 5),
    Information  = (1 << 6),
    Warning      = (1 << 7),
    Danger       = (1 << 8),

    Rounded      = (1 << 9),
    Outlined     = (1 << 10),
    Card         = (1 << 11),

    Icon         = (1 << 12),
    Large        = (1 << 13),

    NoPressedAnimation = (1 << 14),
    WindowControlsButton = (1 << 15),
}
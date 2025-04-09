namespace SukiUI.Toasts;

public enum SukiToastDismissSource
{
    /// <summary>
    /// The toast was dismissed by code and not interacting with the UI.
    /// </summary>
    Code,

    /// <summary>
    /// The toast was dismissed by the user clicking on it.
    /// </summary>
    Click,

    /// <summary>
    /// The toast was dismissed by the user clicking on an action button.
    /// </summary>
    ActionButton,

    /// <summary>
    /// The toast was dismissed by a timeout.
    /// </summary>
    Timeout,
}
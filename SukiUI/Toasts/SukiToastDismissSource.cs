namespace SukiUI.Toasts;

public enum SukiToastDismissSource
{
    /// <summary>
    /// The toast was dismissed by the user clicking on it.
    /// </summary>
    Click,

    /// <summary>
    /// The toast was dismissed by a timeout.
    /// </summary>
    Timeout,
}
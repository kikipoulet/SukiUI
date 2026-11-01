namespace SukiUI.Toasts;

public interface ISukiToastManager
{
    /// <summary>
    /// Raised whenever a toast is queued.
    /// </summary>
    event SukiToastQueuedEventHandler OnToastQueued;

    /// <summary>
    /// Raised whenever a toast is dismissed.
    /// </summary>
    event SukiToastDismissedEventHandler OnToastDismissed;

    /// <summary>
    /// Raised whenever all toasts are dismissed at once.
    /// </summary>
    event EventHandler OnAllToastsDismissed;

    /// <summary>
    /// Queues a given toast for display.
    /// </summary>
    void Queue(ISukiToast toast);

    /// <summary>
    /// Dismisses a given toast from the stack, if it is still present.
    /// </summary>
    void Dismiss(ISukiToast toast, SukiToastDismissSource dismissSource = SukiToastDismissSource.Code);

    /// <summary>
    /// Dismisses a given toast from the stack, if it is still present.
    /// </summary>
    public void Dismiss(int index, SukiToastDismissSource dismissSource = SukiToastDismissSource.Code);

    /// <summary>
    /// Dismisses a specific range of toasts from the stack.
    /// </summary>
    void DismissRange(int startIndex, int count);

    /// <summary>
    /// Ensures that the toast stack doesn't exceed the specified maximum.
    /// If it does, it will dismiss the oldest toasts down to the maximum.
    /// </summary>
    /// <param name="maxAllowed"></param>
    void EnsureMaximum(int maxAllowed);

    /// <summary>
    /// Dismisses all toasts from the stack immediately.
    /// </summary>
    void DismissAll();

    /// <summary>
    /// Checks to see if a <see cref="ISukiToast"/> has already been dismissed from the stack.
    /// </summary>
    bool IsDismissed(ISukiToast toast);

    /// <summary>
    /// Sets the polling interval for the dismiss timer.
    /// </summary>
    /// <param name="milliseconds"></param>
    public void SetDismissTimerPollingInterval(int milliseconds);

    /// <summary>
    /// Sets the polling interval for the dismiss timer.
    /// </summary>
    /// <param name="timeSpan"></param>
    public void SetDismissTimerPollingInterval(TimeSpan timeSpan);
}
using System.Diagnostics;
using Avalonia.Threading;

namespace SukiUI.Toasts;

// It's important that events are raised BEFORE removing them from the manager so that the animation only plays once.
public class SukiToastManager : ISukiToastManager, IDisposable
{
    public event SukiToastQueuedEventHandler? OnToastQueued;
    public event SukiToastDismissedEventHandler? OnToastDismissed;
    public event EventHandler? OnAllToastsDismissed;

    private bool _isDisposed;

    private readonly List<ISukiToast> _toasts = new();

    private readonly DispatcherTimer _dismissPollingTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(50)
    };

    public SukiToastManager()
    {
        _dismissPollingTimer.Tick += DismissPollingTimerOnTick;
    }

    public void Queue(ISukiToast toast)
    {
        _toasts.Add(toast);
        OnToastQueued?.Invoke(this, new SukiToastQueuedEventArgs(toast));
        if (toast.CanDismissByTime && !_dismissPollingTimer.IsEnabled) _dismissPollingTimer.Start();
    }

    public void Dismiss(ISukiToast toast, SukiToastDismissSource dismissSource = SukiToastDismissSource.Code)
    {
        Dismiss(_toasts.IndexOf(toast), dismissSource);
    }

    public void Dismiss(int index, SukiToastDismissSource dismissSource = SukiToastDismissSource.Code)
    {
        if (index < 0 || index >= _toasts.Count) return;
        var toast = _toasts[index];
        OnToastDismissed?.Invoke(toast, new SukiToastDismissedEventArgs(toast, dismissSource));
        toast.OnDismissed?.Invoke(toast, dismissSource);
        _toasts.RemoveAt(index);
        SafelyStopDismissTimer();
    }

    public void DismissRange(int startIndex, int count)
    {
        if (count == 0
            || _toasts.Count == 0
            || startIndex < 0
            || startIndex >= _toasts.Count) return;
        if (count > _toasts.Count) count = _toasts.Count;

        var lastIndex = Math.Min(startIndex + count - 1, _toasts.Count - 1);

        for (var i = lastIndex; i >= startIndex; i--)
        {
            var removed = _toasts[i];
            OnToastDismissed?.Invoke(this, new SukiToastDismissedEventArgs(removed, SukiToastDismissSource.Code));
            removed.OnDismissed?.Invoke(removed, SukiToastDismissSource.Code);
            _toasts.RemoveAt(i);
        }

        SafelyStopDismissTimer();
    }

    public void EnsureMaximum(int maxAllowed)
    {
        if (_toasts.Count <= maxAllowed) return;
        DismissRange(0, _toasts.Count - maxAllowed);
    }

    public void DismissAll()
    {
        if (_toasts.Count == 0) return;
        _dismissPollingTimer.Stop();
        OnAllToastsDismissed?.Invoke(this, EventArgs.Empty);
        _toasts.Clear();
    }

    public bool IsDismissed(ISukiToast toast) => !_toasts.Contains(toast);

    private void SafelyStopDismissTimer()
    {
        if (_dismissPollingTimer.IsEnabled && !_toasts.Any(toast => toast.CanDismissByTime))
        {
            _dismissPollingTimer.Stop();
        }
    }

    public void SetDismissTimerPollingInterval(int milliseconds)
    {
        _dismissPollingTimer.Interval = TimeSpan.FromMilliseconds(milliseconds);
    }

    public void SetDismissTimerPollingInterval(TimeSpan timeSpan)
    {
        _dismissPollingTimer.Interval = timeSpan;
    }

    private void DismissPollingTimerOnTick(object sender, EventArgs e)
    {
        var timestampNow = Stopwatch.GetTimestamp() * 1000d / Stopwatch.Frequency;
        for (var i = _toasts.Count - 1; i >= 0; i--)
        {
            var toast = _toasts[i];
            if (!toast.CanDismissByTime || toast.DismissStartTimestamp <= 0) continue;
            var elapsedMilliseconds = timestampNow - toast.DismissStartTimestamp;
            if (elapsedMilliseconds >= toast.DismissTimeout.TotalMilliseconds)
            {
                toast.DismissProgressValue = 0;
                Dismiss(i, SukiToastDismissSource.Timeout);
                toast.OnDismissed?.Invoke(toast, SukiToastDismissSource.Timeout);
            }
            else
            {
                toast.DismissProgressValue =
                    Math.Min(Math.Max(1 - elapsedMilliseconds / toast.DismissTimeout.TotalMilliseconds, 0.0), 1.0);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        DismissAll();
        _dismissPollingTimer.Tick -= DismissPollingTimerOnTick;

        // Unsubscribe event handlers
        OnToastQueued = null;
        OnToastDismissed = null;
        OnAllToastsDismissed = null;
    }
}
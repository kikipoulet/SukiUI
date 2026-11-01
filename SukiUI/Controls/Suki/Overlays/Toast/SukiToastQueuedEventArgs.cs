namespace SukiUI.Toasts;

public class SukiToastQueuedEventArgs : EventArgs
{
    public ISukiToast Toast { get; set; }

    public SukiToastQueuedEventArgs(ISukiToast toast)
    {
        Toast = toast;
    }
}

public delegate void SukiToastQueuedEventHandler(object sender, SukiToastQueuedEventArgs args);
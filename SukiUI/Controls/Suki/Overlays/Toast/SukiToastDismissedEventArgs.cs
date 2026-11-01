using System;

namespace SukiUI.Toasts
{
    public class SukiToastDismissedEventArgs : EventArgs
    {
        public ISukiToast Toast { get; init; }

        public SukiToastDismissSource DismissSource { get; init; }

        public SukiToastDismissedEventArgs(ISukiToast toast, SukiToastDismissSource dismissSource)
        {
            Toast = toast;
            DismissSource = dismissSource;
        }
    }

    public delegate void SukiToastDismissedEventHandler(object sender, SukiToastDismissedEventArgs args);
}
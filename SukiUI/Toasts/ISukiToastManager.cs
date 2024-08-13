using System;

namespace SukiUI.Toasts
{
    // TODO: DOCS
    public interface ISukiToastManager
    {
        event SukiToastManagerEventHandler OnToastQueued;
        event SukiToastManagerEventHandler OnToastDismissed;
        event EventHandler OnAllToastsDismissed;
        void Queue(ISukiToast toast);
        void Dismiss(ISukiToast toast);
        void DismissAll();
    }
}
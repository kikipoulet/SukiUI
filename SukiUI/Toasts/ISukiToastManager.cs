using System;

namespace SukiUI.Toasts
{
    public interface ISukiToastManager
    {
        /// <summary>
        /// Raised whenever a toast is queued.
        /// </summary>
        event SukiToastManagerEventHandler OnToastQueued;
        
        /// <summary>
        /// Raised whenever a toast is dismissed.
        /// </summary>
        event SukiToastManagerEventHandler OnToastDismissed;
        
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
        void Dismiss(ISukiToast toast);
        
        /// <summary>
        /// Dismisses all toasts from the stack immediately.
        /// </summary>
        void DismissAll();
    }
}
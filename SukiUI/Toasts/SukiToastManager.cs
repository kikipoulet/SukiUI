using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SukiUI.Toasts
{
    public class SukiToastManager : ISukiToastManager
    {
        public event SukiToastManagerEventHandler? OnToastQueued;
        public event SukiToastManagerEventHandler? OnToastDismissed;
        public event EventHandler? OnAllToastsDismissed;
        
        private readonly HashSet<ISukiToast> _toasts = new();

        public void Queue(ISukiToast toast)
        {
            if (!_toasts.Add(toast)) return;
            OnToastQueued?.Invoke(this, new SukiToastManagerEventArgs(toast));
        }

        public void Dismiss(ISukiToast toast)
        {
            if(!_toasts.Remove(toast)) return;
            OnToastDismissed?.Invoke(this, new SukiToastManagerEventArgs(toast));
        }

        public void DismissAll()
        {
            if (!_toasts.Any()) return;
            _toasts.Clear();
            OnAllToastsDismissed?.Invoke(this, EventArgs.Empty);
        }
    }
}
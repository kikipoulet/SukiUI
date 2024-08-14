using System;
using System.Collections.Generic;
using System.Linq;

namespace SukiUI.Toasts
{
    public class SukiToastManager : ISukiToastManager
    {
        public event SukiToastManagerEventHandler? OnToastQueued;
        public event SukiToastManagerEventHandler? OnToastDismissed;
        public event EventHandler? OnAllToastsDismissed;
        
        private readonly List<ISukiToast> _toasts = new();

        public void Queue(ISukiToast toast)
        {
            _toasts.Add(toast);
            OnToastQueued?.Invoke(this, new SukiToastManagerEventArgs(toast));
        }

        public void Dismiss(ISukiToast toast)
        {
            if(!_toasts.Remove(toast)) return;
            OnToastDismissed?.Invoke(this, new SukiToastManagerEventArgs(toast));
        }

        public void Dismiss(int count)
        {
            if (!_toasts.Any()) return;
            if(count > _toasts.Count) count = _toasts.Count;
            for (var i = 0; i < count; i++)
            {
                var removed = _toasts[i];
                _toasts.RemoveAt(i);
                OnToastDismissed?.Invoke(this, new SukiToastManagerEventArgs(removed));
            }
        }

        public void EnsureMaximum(int maxAllowed)
        {
            Console.WriteLine($"{maxAllowed} - {_toasts.Count}");
            if (_toasts.Count <= maxAllowed) return;
            Dismiss(_toasts.Count - maxAllowed);
        }

        public void DismissAll()
        {
            if (!_toasts.Any()) return;
            _toasts.Clear();
            OnAllToastsDismissed?.Invoke(this, EventArgs.Empty);
        }
    }
}
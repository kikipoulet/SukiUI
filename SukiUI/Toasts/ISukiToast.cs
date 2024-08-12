using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;

namespace SukiUI.Toasts
{
    public interface ISukiToast
    {
        ISukiToastManager Manager { get; set; }
        
        public string Title { get; set; }
        public object? Content { get; set; }
        public object? Icon { get; set; }
        
        public IBrush? Foreground { get; set; }
        
        public bool CanDismissByClicking { get; set; }
        
        public Action<ISukiToast>? OnDismissed { get; set; }
        Action<ISukiToast> OnClicked { get; set; }
        
        public ObservableCollection<object> ActionButtons { get; }
        
        public void AnimateShow();
        public void AnimateDismiss();
    }
}
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;

namespace SukiUI.Toasts
{
    public interface ISukiToast
    {
        ISukiToastManager? Manager { get; set; }
        string Title { get; set; }
        object? Content { get; set; }
        object? Icon { get; set; }
        IBrush? Foreground { get; set; }
        bool CanDismissByClicking { get; set; }
        Action<ISukiToast>? OnDismissed { get; set; }
        Action<ISukiToast> OnClicked { get; set; }
        ObservableCollection<object> ActionButtons { get; }
        void AnimateShow();
        void AnimateDismiss();
        ISukiToast ResetToDefault();
    }
}
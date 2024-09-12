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
        bool LoadingState { get; set; }
        Action<ISukiToast>? OnDismissed { get; set; }
        Action<ISukiToast> OnClicked { get; set; }
        ObservableCollection<object> ActionButtons { get; }
        void AnimateShow();
        void AnimateDismiss();
        ISukiToast ResetToDefault();
        /// <summary>
        /// This is what's called when a delay based dismiss is used.
        /// This is tracked so that it can be disposed of when the toast is dismissed by other means.
        /// </summary>
        Action<ISukiToast>? DelayDismissAction { get; set; }
    }
}
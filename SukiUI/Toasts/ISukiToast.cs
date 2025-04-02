using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Toasts;

public interface ISukiToast
{
    ISukiToastManager? Manager { get; set; }
    string Title { get; set; }
    object? Content { get; set; }
    object? Icon { get; set; }
    IBrush? Foreground { get; set; }
    bool CanDismissByClicking { get; set; }
    bool CanDismissByTime { get; set; }
    bool InterruptDismissTimerWhileHover { get; set; }
    TimeSpan DismissTimeout { get; set; }
    bool LoadingState { get; set; }
    Action<ISukiToast, SukiToastDismissSource>? OnDismissed { get; set; }
    Action<ISukiToast> OnClicked { get; set; }
    ObservableCollection<object> ActionButtons { get; }
    void AnimateShow();
    void AnimateDismiss();
    ISukiToast ResetToDefault();

}
using System;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace SukiUI.Dialogs
{
    public interface ISukiDialog
    {
        ISukiDialogManager? Manager { get; set; }
        object? ViewModel { get; set; }
        string? Title { get; set; }
        object? Content { get; set; }
        object? Icon { get; set; }
        bool IsViewModelOnly { get; set; }
        IBrush? IconColor { get; set; }
        ObservableCollection<object> ActionButtons { get; }
        Action<ISukiDialog>? OnDismissed { get; set; }
        bool CanDismissWithBackgroundClick { get; set; }
        bool ShowCardBackground { get; set; }
        void Dismiss();
        void ResetToDefault();
    }
}
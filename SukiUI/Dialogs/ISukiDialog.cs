using System;
using System.Collections.ObjectModel;

namespace SukiUI.Dialogs
{
    public interface ISukiDialog
    {
        public object? ViewModel { get; set; }
        public string? Title { get; set; }
        public object? Content { get; set; }
        public ObservableCollection<object> ActionButtons { get; }
        public Action<ISukiDialog>? OnDismissed { get; set; }
        public bool CanDismissWithBackgroundClick { get; set; }
    }
}
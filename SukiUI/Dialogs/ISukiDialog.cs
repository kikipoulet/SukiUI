using System;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace SukiUI.Dialogs
{
    public interface ISukiDialog
    {
        public ISukiDialogManager? Manager { get; set; }
        public object? ViewModel { get; set; }
        public string? Title { get; set; }
        public object? Content { get; set; }
        internal object? Icon { get; set; }
        internal IBrush? IconColor { get; set; }
        public ObservableCollection<object> ActionButtons { get; }
        public Action<ISukiDialog>? OnDismissed { get; set; }
        public bool CanDismissWithBackgroundClick { get; set; }
        void Dismiss();
    }
}
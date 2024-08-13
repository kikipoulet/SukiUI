using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using SukiUI.Dialogs;

namespace SukiUI.Controls
{
    public class SukiDialog : TemplatedControl, ISukiDialog
    {
        public static readonly StyledProperty<object?> ViewModelProperty = AvaloniaProperty.Register<SukiDialog, object?>(nameof(ViewModel));

        public object? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<SukiDialog, string?>(nameof(Title));

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<SukiDialog, object?>(nameof(Content));

        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public ObservableCollection<object> ActionButtons { get; } = new();
        
        public Action<ISukiDialog>? OnDismissed { get; set; }

        public bool CanDismissWithBackgroundClick { get; set; }
    }
}
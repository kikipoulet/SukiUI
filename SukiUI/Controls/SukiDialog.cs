using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
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

        public static readonly StyledProperty<bool> IsViewModelOnlyProperty = AvaloniaProperty.Register<SukiDialog, bool>(nameof(IsViewModelOnly));

        public bool IsViewModelOnly
        {
            get => GetValue(IsViewModelOnlyProperty);
            set => SetValue(IsViewModelOnlyProperty, value);
        }

        public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<SukiDialog, object?>(nameof(Icon));

        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly StyledProperty<bool> ShowCardBackgroundProperty = AvaloniaProperty.Register<SukiDialog, bool>(nameof(ShowCardBackground), defaultValue: true);

        public bool ShowCardBackground
        {
            get => GetValue(ShowCardBackgroundProperty);
            set => SetValue(ShowCardBackgroundProperty, value);
        }
        
        public static readonly StyledProperty<IBrush?> IconColorProperty = AvaloniaProperty.Register<SukiDialog, IBrush?>(nameof(IconColor));

        public IBrush? IconColor
        {
            get => GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public static readonly StyledProperty<ObservableCollection<object>> ActionButtonsProperty = AvaloniaProperty.Register<SukiDialog, ObservableCollection<object>>(nameof(ActionButtons));

        public ObservableCollection<object> ActionButtons
        {
            get => GetValue(ActionButtonsProperty);
            set => SetValue(ActionButtonsProperty, value);
        }
        
        public ISukiDialogManager? Manager { get; set; }
        
        public Action<ISukiDialog>? OnDismissed { get; set; }

        public bool CanDismissWithBackgroundClick { get; set; }
        
        public SukiDialog()
        {
            ActionButtons = [];
        }
        
        public void Dismiss() => Manager?.TryDismissDialog(this);

        public void ResetToDefault()
        {
            ActionButtons.Clear();
            ViewModel = null;
            Title = null;
            Content = null;
            IsViewModelOnly = false;
        }
    }
}
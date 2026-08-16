
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering.Composition;
using Avalonia.LogicalTree;
using SukiUI.Dialogs;
using SukiUI.Helpers;

namespace SukiUI.Controls
{
    public class SukiDialogHost : TemplatedControl
    {
        private Border? _dialogBackground;
        private ISukiDialogManager? _attachedManager;
        private bool _isAttachedToLogicalTree;
        private CancellationTokenSource? _dismissCts;
        public static readonly StyledProperty<ISukiDialogManager> ManagerProperty = AvaloniaProperty.Register<SukiDialogHost, ISukiDialogManager>(nameof(Manager));

        public ISukiDialogManager Manager
        {
            get => GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }
        
        public static readonly StyledProperty<object?> DialogProperty = AvaloniaProperty.Register<SukiDialogHost, object?>(nameof(Dialog));

        internal object? Dialog
        {
            get => GetValue(DialogProperty);
            set => SetValue(DialogProperty, value);
        }

        public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<SukiDialogHost, bool>(nameof(IsDialogOpen));

        internal bool IsDialogOpen
        {
            get => GetValue(IsDialogOpenProperty);
            set => SetValue(IsDialogOpenProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            DetachTemplateEvents();
            if (e.NameScope.Find<Border>("PART_DialogBackground") is { } dialogBackground)
            {
                _dialogBackground = dialogBackground;
                dialogBackground.PointerPressed += DialogBackgroundOnPointerPressed;
                dialogBackground.Loaded += DialogBackgroundOnLoaded;
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            _isAttachedToLogicalTree = true;
            AttachManagerEvents(Manager);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _isAttachedToLogicalTree = false;
            DetachTemplateEvents();
            DetachManagerEvents();
            base.OnDetachedFromLogicalTree(e);
        }

        private void DialogBackgroundOnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e) =>
            BackgroundRequestClose();

        private void DialogBackgroundOnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dialogBackground is null)
                return;
            var visual = ElementComposition.GetElementVisual(_dialogBackground);
            if (visual is not null)
                CompositionAnimationHelper.MakeOpacityAnimated(visual, 400);
        }

        private void DetachTemplateEvents()
        {
            if (_dialogBackground is null)
                return;
            _dialogBackground.PointerPressed -= DialogBackgroundOnPointerPressed;
            _dialogBackground.Loaded -= DialogBackgroundOnLoaded;
            _dialogBackground = null;
        }

        private void BackgroundRequestClose()
        {
            if (Dialog is not ISukiDialog { CanDismissWithBackgroundClick: true } sukiDialog) return;
            if (!sukiDialog.CanDismissWithBackgroundClick) return;
            Manager.TryDismissDialog(sukiDialog);
        }

        private static void OnManagerPropertyChanged(AvaloniaObject sender,
            AvaloniaPropertyChangedEventArgs propChanged)
        {
            if (sender is not SukiDialogHost host)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(SukiDialogHost));
            host.DetachManagerEvents();
            if (!host._isAttachedToLogicalTree)
                return;
            if (propChanged.NewValue is ISukiDialogManager manager)
                host.AttachManagerEvents(manager);
        }

        private void AttachManagerEvents(ISukiDialogManager newManager)
        {
            if (ReferenceEquals(_attachedManager, newManager))
                return;
            DetachManagerEvents();
            _attachedManager = newManager;
            newManager.OnDialogShown += ManagerOnDialogShown;
            newManager.OnDialogDismissed += ManagerOnDialogDismissed;
        }
        
        private void DetachManagerEvents()
        {
            if (_attachedManager is null)
                return;
            _attachedManager.OnDialogShown -= ManagerOnDialogShown;
            _attachedManager.OnDialogDismissed -= ManagerOnDialogDismissed;
            _attachedManager = null;
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = null;
        }

        private void ManagerOnDialogShown(object sender, SukiDialogManagerEventArgs args)
        {
            // Cancel any pending clear from a prior dismissal so a reused pooled instance
            // is not nulled out by the old timer.
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = null;
            Dialog = args.Dialog;
            IsDialogOpen = true;
        }
        
        private void ManagerOnDialogDismissed(object sender, SukiDialogManagerEventArgs args)
        {
            IsDialogOpen = false;
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            var cts = new CancellationTokenSource();
            _dismissCts = cts;
            Task.Delay(500, cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                if (Dialog != args.Dialog) return;
                Dialog = null;
                _dismissCts?.Dispose();
                _dismissCts = null;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        static SukiDialogHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<ISukiDialogManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}

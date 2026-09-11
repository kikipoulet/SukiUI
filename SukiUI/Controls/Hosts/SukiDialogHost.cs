
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media.Transformation;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using SukiUI.Animations;
using SukiUI.ControlsAnimation;
using SukiUI.Dialogs;
using SukiUI.Helpers;

namespace SukiUI.Controls
{
    /// <summary>
    /// Host presenting the current <see cref="ISukiDialog"/> from a
    /// <see cref="ISukiDialogManager"/>: owns the backdrop, pointer tracking (so a dialog
    /// can emerge from the invoking click) and the dismiss/pool timings. All choreography
    /// (open / close / pinned-shake) lives in <see cref="SukiDialogMotion"/>, under
    /// <c>ControlsAnimation/DialogAnimation</c>, alongside the press and popup motions.
    /// </summary>
    public class SukiDialogHost : TemplatedControl
    {
        private Border? _dialogBackground;
        private ContentControl? _dialogContent;

        // All animation state, trajectories and the shake spring live over there.
        private readonly SukiDialogMotion _anim = new(() => SukiAnimationTheme.Current.Dialog[SukiDialogPreset.Default]);

        private ISukiDialogManager? _attachedManager;
        private bool _isAttachedToLogicalTree;
        private CancellationTokenSource? _dismissCts;

        // Last known pointer position (top-level coords), tracked so a dialog opening can
        // emerge from where the invoking click happened.
        private Point? _lastPointerPosition;
        private TopLevel? _pointerTrackingTopLevel;
        private EventHandler<PointerEventArgs>? _topLevelPointerMoved;

        public static readonly StyledProperty<ISukiDialogManager> ManagerProperty =
            AvaloniaProperty.Register<SukiDialogHost, ISukiDialogManager>(nameof(Manager));

        public ISukiDialogManager Manager
        {
            get => GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }

        public static readonly StyledProperty<object?> DialogProperty =
            AvaloniaProperty.Register<SukiDialogHost, object?>(nameof(Dialog));

        internal object? Dialog
        {
            get => GetValue(DialogProperty);
            set => SetValue(DialogProperty, value);
        }

        public static readonly StyledProperty<bool> IsDialogOpenProperty =
            AvaloniaProperty.Register<SukiDialogHost, bool>(nameof(IsDialogOpen));

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
            if (e.NameScope.Find<ContentControl>("PART_DialogContent") is { } dialogContent)
            {
                _dialogContent = dialogContent;
                // Rest pose before any open (no transitions on a fresh template, so this
                // lands instantly and invisibly).
                dialogContent.RenderTransform = TransformOperations.Parse("translate(0px, 0px) scale(0.72)");
                dialogContent.Opacity = 0.0;
            }
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            _isAttachedToLogicalTree = true;
            if (Manager is { } manager)
                AttachManagerEvents(manager);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _isAttachedToLogicalTree = false;
            _anim.Stop();
            DetachTemplateEvents();
            DetachManagerEvents();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            // The TopLevel is only reliably reachable once the VISUAL tree is attached —
            // at logical-attach time GetTopLevel may still return null.
            WirePointerTracking();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            UnwirePointerTracking();
        }

        private void WirePointerTracking()
        {
            if (_pointerTrackingTopLevel != null || TopLevel.GetTopLevel(this) is not { } topLevel)
                return;
            _pointerTrackingTopLevel = topLevel;
            // Track every move in the window (handledEventsToo: presses swallowed by
            // controls still update the position). A click is always preceded by a move to
            // where it happens, so this is where dialogs emerge from.
            _topLevelPointerMoved = (_, e) => _lastPointerPosition = e.GetPosition(topLevel);
            topLevel.AddHandler(InputElement.PointerMovedEvent, _topLevelPointerMoved,
                Avalonia.Interactivity.RoutingStrategies.Bubble, handledEventsToo: true);
        }

        private void UnwirePointerTracking()
        {
            if (_pointerTrackingTopLevel is { } topLevel && _topLevelPointerMoved != null)
                topLevel.RemoveHandler(InputElement.PointerMovedEvent, _topLevelPointerMoved);
            _pointerTrackingTopLevel = null;
            _topLevelPointerMoved = null;
        }

        private void DialogBackgroundOnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (Dialog is ISukiDialog { CanDismissWithBackgroundClick: false })
            {
                // The dialog is pinned: physically push back — a vertical spring impulse
                // away from the press (a press below the center bumps it upward), decaying
                // to rest through a few small, soft oscillations.
                if (_dialogContent is { } content)
                {
                    var click = e.GetPosition(this);
                    double direction = click.Y - Bounds.Height / 2.0 >= 0.0 ? -1.0 : 1.0;
                    _anim.StartShake(content, direction * SukiAnimationTheme.Current.Dialog[SukiDialogPreset.Default].ShakeImpulse);
                }
                return;
            }
            BackgroundRequestClose();
        }

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
            _dialogContent = null;
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
            WirePointerTracking(); // last-chance, idempotent: needed for the emergence offset
            // One dispatcher pass later the content has been laid out: its measured size
            // calibrates the spring, and the pointer is still at the invoking click.
            Dispatcher.UIThread.Post(PlayOpenAnimation, DispatcherPriority.Loaded);
        }

        private void ManagerOnDialogDismissed(object sender, SukiDialogManagerEventArgs args)
        {
            IsDialogOpen = false;
            if (_dialogContent is { } content)
                _anim.PlayClose(content);
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

        private void PlayOpenAnimation()
        {
            if (_dialogContent is not { } content)
                return;
            double width = content.Bounds.Width > 0 ? content.Bounds.Width : content.DesiredSize.Width;
            double height = content.Bounds.Height > 0 ? content.Bounds.Height : content.DesiredSize.Height;
            _anim.PlayOpen(content, width, height, EmergenceOffset());
        }

        // Only the horizontal direction of the gesture survives; the vertical part is
        // always a fixed rise from below (see SukiDialogProfile.EmergenceVertical).
        private (double Dx, double Dy) EmergenceOffset()
        {
            var profile = SukiAnimationTheme.Current.Dialog[SukiDialogPreset.Default];
            double dx = 0.0;
            if (GetPointerPositionInHost() is { } click)
                dx = Math.Clamp(click.X - Bounds.Width / 2.0, -profile.EmergenceHorizontalMax, profile.EmergenceHorizontalMax);
            return (dx, profile.EmergenceVertical);
        }

        private Point? GetPointerPositionInHost()
        {
            if (_lastPointerPosition is not { } position || _pointerTrackingTopLevel is not { } topLevel)
                return null;
            return topLevel.TranslatePoint(position, this);
        }

        static SukiDialogHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<ISukiDialogManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}

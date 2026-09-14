using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using SukiUI.ControlsAnimation;
using SukiUI.Enums;
using SukiUI.Helpers;
using SukiUI.Toasts;

namespace SukiUI.Controls
{
    public class SukiToastHost : ItemsControl
    {
        private ISukiToastManager? _attachedManager;
        private bool _isAttachedToLogicalTree;
        private readonly SukiToastMotion _toastMotion;

        public SukiToastHost() => _toastMotion = new SukiToastMotion(this);
        public static readonly StyledProperty<ISukiToastManager> ManagerProperty =
            AvaloniaProperty.Register<SukiToastHost, ISukiToastManager>(nameof(Manager));

        public ISukiToastManager Manager
        {
            get => GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }

        public static readonly StyledProperty<byte> MaxToastsProperty = AvaloniaProperty.Register<SukiToastHost, byte>(nameof(MaxToasts), defaultValue: 5);

        public byte MaxToasts
        {
            get => GetValue(MaxToastsProperty);
            set => SetValue(MaxToastsProperty, value);
        }

        public static readonly StyledProperty<ToastLocation> PositionProperty = AvaloniaProperty.Register<SukiToastHost, ToastLocation>(nameof(Position), defaultValue: ToastLocation.BottomRight);

        public ToastLocation Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            OnPositionChanged(Position);
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
            DetachManagerEvents();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == PositionProperty && change.NewValue is ToastLocation loc)
                OnPositionChanged(loc);
        }

        private void OnPositionChanged(ToastLocation newLoc)
        {
            HorizontalAlignment = newLoc switch
            {
                ToastLocation.BottomRight => HorizontalAlignment.Right,
                ToastLocation.BottomLeft => HorizontalAlignment.Left,
                ToastLocation.TopRight => HorizontalAlignment.Right,
                ToastLocation.TopLeft => HorizontalAlignment.Left,
                _ => throw new ArgumentOutOfRangeException()
            };
            VerticalAlignment = newLoc switch
            {
                ToastLocation.BottomRight => VerticalAlignment.Bottom,
                ToastLocation.BottomLeft => VerticalAlignment.Bottom,
                ToastLocation.TopRight => VerticalAlignment.Top,
                ToastLocation.TopLeft => VerticalAlignment.Top,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static void OnManagerPropertyChanged(AvaloniaObject sender,
            AvaloniaPropertyChangedEventArgs propChanged)
        {
            if (sender is not SukiToastHost host)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(SukiToastHost));
            host.DetachManagerEvents();
            if (!host._isAttachedToLogicalTree)
                return;
            if (propChanged.NewValue is ISukiToastManager manager)
                host.AttachManagerEvents(manager);
        }

        private void AttachManagerEvents(ISukiToastManager newManager)
        {
            if (ReferenceEquals(_attachedManager, newManager))
                return;
            DetachManagerEvents();
            _attachedManager = newManager;
            newManager.OnToastQueued += ManagerOnToastQueued;
            newManager.OnToastDismissed += ManagerOnToastDismissed;
            newManager.OnAllToastsDismissed += ManagerOnAllToastsDismissed;
        }

        private void DetachManagerEvents()
        {
            if (_attachedManager is null)
                return;
            _attachedManager.OnToastQueued -= ManagerOnToastQueued;
            _attachedManager.OnToastDismissed -= ManagerOnToastDismissed;
            _attachedManager.OnAllToastsDismissed -= ManagerOnAllToastsDismissed;
            _attachedManager = null;
        }

        private void ManagerOnToastDismissed(object sender, SukiToastDismissedEventArgs args) =>
            ClearToast(args.Toast);

        private void ManagerOnAllToastsDismissed(object sender, EventArgs e)
        {
            foreach(var toast in Items)
                ClearToast((ISukiToast)toast!);
        }

        private void ManagerOnToastQueued(object sender, SukiToastQueuedEventArgs args)
        {
            if (MaxToasts <= 0) return;
            Items.Add(args.Toast);
            Manager.EnsureMaximum(MaxToasts);
            if (args.Toast is SukiToast toast)
                _toastMotion.PlayShow(toast);
        }

        private void ClearToast(ISukiToast toast)
        {
            if (Manager.IsDismissed(toast)) return;
            if (toast is SukiToast suki)
            {
                _toastMotion.PlayDismiss(suki, () =>
                {
                    Items.Remove(toast);
                    ToastPool.Return(suki);
                });
            }
            else
            {
                Items.Remove(toast); // a custom ISukiToast: no motion, removed immediately
            }
        }

        static SukiToastHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<ISukiToastManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}

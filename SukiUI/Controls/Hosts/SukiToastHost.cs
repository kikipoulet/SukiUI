using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using SukiUI.Enums;
using SukiUI.Helpers;
using SukiUI.Toasts;

namespace SukiUI.Controls
{
    public class SukiToastHost : ItemsControl
    {
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

        private IDisposable? _subscriptions;
        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _subscriptions = this.GetObservable(PositionProperty)
                .Do(OnPositionChanged)
                .Select(_ => Unit.Default).ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe();
        }

        private void OnPositionChanged(ToastLocation obj)
        {
            HorizontalAlignment = Position switch
            {
                ToastLocation.BottomRight => HorizontalAlignment.Right,
                ToastLocation.BottomLeft => HorizontalAlignment.Left,
                ToastLocation.TopRight => HorizontalAlignment.Right,
                ToastLocation.TopLeft => HorizontalAlignment.Left,
                _ => throw new ArgumentOutOfRangeException()
            };
            VerticalAlignment = Position switch
            {
                ToastLocation.BottomRight => VerticalAlignment.Bottom,
                ToastLocation.BottomLeft => VerticalAlignment.Bottom,
                ToastLocation.TopRight => VerticalAlignment.Top,
                ToastLocation.TopLeft => VerticalAlignment.Top,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _subscriptions?.Dispose();
        }
        
        private static void OnManagerPropertyChanged(AvaloniaObject sender,
            AvaloniaPropertyChangedEventArgs propChanged)
        {
            if (sender is not SukiToastHost host)
                throw new NullReferenceException("Dependency object is not of valid type " + nameof(SukiToastHost));
            if (propChanged.OldValue is ISukiToastManager oldManager)
                host.DetachManagerEvents(oldManager);
            if (propChanged.NewValue is ISukiToastManager newManager)
                host.AttachManagerEvents(newManager);
        }

        private void AttachManagerEvents(ISukiToastManager newManager)
        {
            newManager.OnToastQueued += ManagerOnToastQueued;
            newManager.OnToastDismissed += ManagerOnToastDismissed;
            newManager.OnAllToastsDismissed += ManagerOnAllToastsDismissed;
        }

        private void DetachManagerEvents(ISukiToastManager oldManager)
        {
            oldManager.OnToastQueued -= ManagerOnToastQueued;
            oldManager.OnToastDismissed -= ManagerOnToastDismissed;
            oldManager.OnAllToastsDismissed -= ManagerOnAllToastsDismissed;
        }

        private void ManagerOnToastDismissed(object sender, SukiToastManagerEventArgs args) => 
            ClearToast(args.Toast);

        private void ManagerOnAllToastsDismissed(object sender, EventArgs e) => 
            Items.Clear();

        private void ManagerOnToastQueued(object sender, SukiToastManagerEventArgs args)
        {
            if (MaxToasts <= 0) return;
            var toast = args.Toast;
            if (Items.Count >= MaxToasts)
                ClearToast((ISukiToast)Items.First()!);
            Items.Add(args.Toast);
            toast.AnimateShow();
        }
        
        private void ClearToast(ISukiToast toast)
        {
            if (!Items.Contains(toast)) return;
            toast.AnimateDismiss();
            Task.Delay(300).ContinueWith(_ =>
            {
                Items.Remove(toast);
                ToastPool.Return((SukiToast)toast);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        static SukiToastHost()
        {
            ManagerProperty.Changed.Subscribe(
                new Avalonia.Reactive.AnonymousObserver<AvaloniaPropertyChangedEventArgs<ISukiToastManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}
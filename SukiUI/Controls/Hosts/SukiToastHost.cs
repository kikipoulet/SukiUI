using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Threading;
using SukiUI.Helpers;
using SukiUI.Toasts;

namespace SukiUI.Controls
{
    public class SukiToastHost : ItemsControl
    {
        public static readonly StyledProperty<SukiToastManager> ManagerProperty =
            AvaloniaProperty.Register<SukiToastHost, SukiToastManager>(nameof(Manager));

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
            Task.Delay(300).ContinueWith(_ => { Items.Remove(toast); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        static SukiToastHost()
        {
            ManagerProperty.Changed.Subscribe(
                new AnonymousObserver<AvaloniaPropertyChangedEventArgs<SukiToastManager>>(x =>
                    OnManagerPropertyChanged(x.Sender, x)));
        }
    }
}
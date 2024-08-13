using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using SukiUI.ColorTheme;
using SukiUI.Content;
using SukiUI.Helpers;

namespace SukiUI.Dialogs
{
    public class SukiDialogBuilder
    {
        public ISukiDialogManager Manager { get; }
        public ISukiDialog Dialog { get; }

        public SukiDialogBuilder(ISukiDialogManager manager)
        {
            Manager = manager;
            Dialog = DialogPool.Get();
            Dialog.Manager = Manager;
        }
        
        public bool TryShow() => Manager.TryShowDialog(Dialog);

        public void SetTitle(string title) => Dialog.Title = title;

        public void SetContent(object content) => Dialog.Content = content;

        public void SetViewModel(Func<ISukiDialog,object> viewModel)
        {
            Dialog.ViewModel = viewModel(Dialog);
        }
        
        public void SetType(NotificationType notificationType)
        {
            Dialog.Icon = notificationType switch
            {
                NotificationType.Information => Icons.InformationOutline,
                NotificationType.Success => Icons.Check,
                NotificationType.Warning => Icons.AlertOutline,
                NotificationType.Error => Icons.AlertOutline,
                _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
            };
            Dialog.IconColor = notificationType switch
            {
                NotificationType.Information => NotificationColor.InfoIconForeground,
                NotificationType.Success => NotificationColor.SuccessIconForeground,
                NotificationType.Warning => NotificationColor.WarningIconForeground,
                NotificationType.Error => NotificationColor.ErrorIconForeground,
                _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
            };
        }

        public void SetCanDismissWithBackgroundClick(bool canDismissWithBackgroundClick) =>
            Dialog.CanDismissWithBackgroundClick = canDismissWithBackgroundClick;
        
        public void SetOnDismissed(Action<ISukiDialog> onDismissed) => Dialog.OnDismissed = onDismissed;
        
        public void AddActionButton(object? buttonContent, Action<ISukiDialog> onClicked, bool dismissOnClick)
        {
            var btn = new Button()
            {
                Content = buttonContent,
                Classes = { "Flat" }
            };
            btn.Click += (_,_) =>
            {
                onClicked(Dialog);
                if (!dismissOnClick) return;
                Manager.TryDismissDialog(Dialog);
            };
            Dialog.ActionButtons.Add(btn);
        }
         

        public class DismissDialog
        {
            public SukiDialogBuilder Builder { get; }

            public DismissDialog(SukiDialogBuilder builder)
            {
                Builder = builder;
            }
        }
    }
}
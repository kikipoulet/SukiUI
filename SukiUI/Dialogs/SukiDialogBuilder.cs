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

        public TaskCompletionSource<bool>? Completion { get; set; }

        public SukiDialogBuilder(ISukiDialogManager manager)
        {
            Manager = manager;
            Dialog = DialogPool.Get();
            Dialog.Manager = Manager;
        }

        public bool TryShow()
            => Manager.TryShowDialog(Dialog);

        /// <summary>
        /// Tries to open a dialog and 'await's/ blocks until its being closed.
        /// </summary>
        /// <returns>'True' or 'False' as DialogResult</returns>
        /// <exception cref="InvalidOperationException">Will throw if there was an already open dialog, or if the builder wasnt configured to support waiting for it being closed</exception>
        public async Task<bool> TryShowAsync(CancellationToken cancellationToken = default)
        {
            var completion = Completion;
            if (completion is null)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException($"{nameof(SukiDialogBuilder)} is not configured corretly. Its missing a valid value for {nameof(Completion)}.");
            }

            cancellationToken.Register(CancellationRequested);
            Dialog.OnDismissed += DialogCancellationRequested;

            var result = Manager.TryShowDialog(Dialog);
            if (!result)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif

                Dialog.OnDismissed -= DialogCancellationRequested;
                throw new InvalidOperationException("Opening a new dialog failed. Looks like there is already one open.");
            }

            return await completion.Task;

            void CancellationRequested()
            {
                Manager.TryDismissDialog(Dialog);
                completion.TrySetResult(false);
            }

            void DialogCancellationRequested(ISukiDialog dialog)
            {
                dialog.OnDismissed -= DialogCancellationRequested;
                completion.TrySetResult(false);
            }
        }

        public void SetTitle(string title)
            => Dialog.Title = title;

        public void SetShowCardBackground(bool show)
            => Dialog.ShowCardBackground = show;

        public void SetContent(object content)
            => Dialog.Content = content;

        public void SetViewModel(Func<ISukiDialog, object> viewModel, bool isViewModelOnly)
        {
            if (isViewModelOnly)
            {
                Dialog.ViewModel = viewModel(Dialog);
                Dialog.IsViewModelOnly = true;
            }
            else
            {
                Dialog.Content = viewModel(Dialog);
                Dialog.IsViewModelOnly = false;
            }
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

        public void SetCanDismissWithBackgroundClick(bool canDismissWithBackgroundClick)
            => Dialog.CanDismissWithBackgroundClick = canDismissWithBackgroundClick;

        public void SetOnDismissed(Action<ISukiDialog> onDismissed)
            => Dialog.OnDismissed = onDismissed;

        public void AddActionButton(object? buttonContent, Action<ISukiDialog> onClicked, bool dismissOnClick, string[] classes)
        {
            if (classes.Length == 0)
                classes = ["Flat"];

            var btn = new Button { Content = buttonContent };
            foreach (var @class in classes)
                btn.Classes.Add(@class);

            btn.Click += (_, _) =>
            {
                onClicked(Dialog);

                if (!dismissOnClick)
                    return;

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
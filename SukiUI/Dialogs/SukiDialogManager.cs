using SukiUI.Helpers;
using SukiUI.Controls;
using Avalonia.Threading;
using System.Collections.Generic;

namespace SukiUI.Dialogs
{
    public class SukiDialogManager : ISukiDialogManager
    {
        public event SukiDialogManagerEventHandler? OnDialogShown;
        public event SukiDialogManagerEventHandler? OnDialogDismissed;

        private ISukiDialog? _activeDialog;
        private readonly Dictionary<SukiDialog, CancellationTokenSource> _pendingPoolReturns = new();
        
        public bool TryShowDialog(ISukiDialog dialog)
        {
            if (_activeDialog != null) return false;
            if (dialog is SukiDialog sukiDialog)
                CancelPendingPoolReturn(sukiDialog);
            _activeDialog = dialog;
            OnDialogShown?.Invoke(this, new SukiDialogManagerEventArgs(_activeDialog));
            return true;
        }

        public bool TryDismissDialog(ISukiDialog dialog)
        {
            if (_activeDialog == null || _activeDialog != dialog) 
                return false;

            var dismissedDialog = _activeDialog;
            _activeDialog = null;
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(dismissedDialog));
            dismissedDialog.OnDismissed?.Invoke(dismissedDialog);
            ReturnToPoolAfterDismissal(dismissedDialog);

            return true;
        }

        public void DismissDialog()
        {
            if (_activeDialog == null) 
                return;

            var dismissedDialog = _activeDialog;
            _activeDialog = null;
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(dismissedDialog));
            dismissedDialog.OnDismissed?.Invoke(dismissedDialog);
            ReturnToPoolAfterDismissal(dismissedDialog);
        }

        private void ReturnToPoolAfterDismissal(ISukiDialog dialog)
        {
            if (dialog is not SukiDialog sukiDialog)
                return;

            CancelPendingPoolReturn(sukiDialog);
            var cancellation = new CancellationTokenSource();
            _pendingPoolReturns.Add(sukiDialog, cancellation);
            DispatcherTimer.RunOnce(() =>
            {
                if (!_pendingPoolReturns.TryGetValue(sukiDialog, out var pending) ||
                    !ReferenceEquals(pending, cancellation))
                    return;

                _pendingPoolReturns.Remove(sukiDialog);
                cancellation.Dispose();
                DialogPool.Return(sukiDialog);
            }, TimeSpan.FromMilliseconds(500));
        }

        private void CancelPendingPoolReturn(SukiDialog dialog)
        {
            if (!_pendingPoolReturns.Remove(dialog, out var cancellation))
                return;

            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}

using SukiUI.Helpers;

namespace SukiUI.Dialogs
{
    public class SukiDialogManager : ISukiDialogManager
    {
        public event SukiDialogManagerEventHandler? OnDialogShown;
        public event SukiDialogManagerEventHandler? OnDialogDismissed;

        private ISukiDialog? _activeDialog;
        
        public bool TryShowDialog(ISukiDialog dialog)
        {
            if (_activeDialog != null) return false;
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
        }
    }
}
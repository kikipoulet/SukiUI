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
            
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(_activeDialog));
            _activeDialog.OnDismissed?.Invoke(_activeDialog);
            _activeDialog = null;
            
            return true;
        }

        public void DismissDialog()
        {
            if (_activeDialog == null) 
                return;
            
            OnDialogDismissed?.Invoke(this, new SukiDialogManagerEventArgs(_activeDialog));
            _activeDialog.OnDismissed?.Invoke(_activeDialog);
            _activeDialog = null;
        }
    }
}
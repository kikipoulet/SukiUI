namespace SukiUI.Dialogs
{
    public interface ISukiDialogManager
    {
        event SukiDialogManagerEventHandler? OnDialogShown;
        event SukiDialogManagerEventHandler? OnDialogDismissed;

        bool TryShowDialog(ISukiDialog dialog);
        bool TryDismissDialog(ISukiDialog dialog);
        void DismissDialog();
    }
}
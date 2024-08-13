using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class VmDialogViewModel(ISukiDialog dialog) : ObservableObject
{
    [RelayCommand]
    private void CloseDialog() => dialog.Dismiss();
}
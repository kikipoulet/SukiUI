using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.Dashboard;

public partial class DialogViewModel(ISukiDialog dialog) : ObservableObject
{
    [RelayCommand]
    private void CloseDialog()
    {
        dialog.Dismiss();
    }
}
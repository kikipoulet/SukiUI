using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class VmDialogViewModel : ObservableObject
{
    [RelayCommand]
    private static void CloseDialog() => SukiHost.CloseDialog();
}
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogsViewModel() : DemoPageBase("Dialogs", MaterialIconKind.Forum)
{
    [RelayCommand]
    private static void OpenStandardDialog() =>
        SukiHost.ShowDialog(new StandardDialog());

    [RelayCommand]
    private static void OpenBackgroundCloseDialog() =>
        SukiHost.ShowDialog(new BackgroundCloseDialog(), allowBackgroundClose: true);
        
    [RelayCommand]
    private static void OpenViewModelDialog() =>
        SukiHost.ShowDialog(new VmDialogViewModel(), allowBackgroundClose: true);

    [RelayCommand]
    private static void OpenDialogWindowDemo() => new DialogWindowDemo().Show();
        
}
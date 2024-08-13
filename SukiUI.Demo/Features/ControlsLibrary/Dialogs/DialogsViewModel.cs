using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogsViewModel(ISukiDialogManager dialogManager) : DemoPageBase("Dialogs", MaterialIconKind.Forum)
{
    [RelayCommand]
    private void OpenStandardDialog()
    {
        //SukiHost.ShowDialog(new StandardDialog());
        dialogManager.CreateDialog()
            .WithTitle("Hello, World!")
            .WithContent("Dialog Content Goes Here!")
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private static void OpenBackgroundCloseDialog() =>
        SukiHost.ShowDialog(new BackgroundCloseDialog(), allowBackgroundClose: true);
        
    [RelayCommand]
    private static void OpenViewModelDialog() =>
        SukiHost.ShowDialog(new VmDialogViewModel(), allowBackgroundClose: true);

    [RelayCommand]
    private static void OpenDialogWindowDemo() => new DialogWindowDemo().Show();
        
}
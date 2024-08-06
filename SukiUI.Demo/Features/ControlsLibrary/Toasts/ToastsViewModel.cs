using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Enums;
using SukiUI.Models;

namespace SukiUI.Demo.Features.ControlsLibrary.Toasts;

public partial class ToastsViewModel() : DemoPageBase("Toasts", MaterialIconKind.BellRing)
{
    [RelayCommand]
    private static Task ShowSingleStandardToast() =>
        SukiHost.ShowToast("A Simple Toast", "This is the content of a toast.");
        
    [RelayCommand]
    private static Task ShowInfoToast() =>
        SukiHost.ShowToast("A Simple Toast", "This is the content of an info toast.", NotificationType.Info);
    
    [RelayCommand]
    private static Task ShowActionToast() =>
        SukiHost.ShowToast(new ToastModel("Update Available", "A new version is available for you.", NotificationType.Info, TimeSpan.FromSeconds(5), null, "Update Now",
            () => { SukiHost.ShowToast("Update", new ProgressBar(){Value = 43, ShowProgressText = true});}));
        
    [RelayCommand]
    private static Task ShowSuccessToast() =>
        SukiHost.ShowToast("A Simple Toast", "This is the content of a success toast.", NotificationType.Success);
        
    [RelayCommand]
    private static Task ShowWarningToast() =>
        SukiHost.ShowToast("A Simple Toast", "This is the content of a warning toast.", NotificationType.Warning);
        
    [RelayCommand]
    private static Task ShowErrorToast() =>
        SukiHost.ShowToast("A Simple Toast", "This is the content of an error toast.", NotificationType.Error);

    [RelayCommand]
    private static async Task ShowThreeInfoToasts()
    {
        for (var i = 1; i <= 3; i++)
            await SukiHost.ShowToast("A Simple Toast", $"This is toast number {i} of 3.");
    }

    [RelayCommand]
    private static Task ShowToastWithCallback()
    {
        return SukiHost.ShowToast("Click This Toast", "Click this toast to open a dialog.", NotificationType.Info, TimeSpan.FromSeconds(15),
            () => SukiHost.ShowDialog(
                new TextBlock { Text = "You clicked the toast! - Click anywhere outside of this dialog to close." },
                allowBackgroundClose: true));
    }

    [RelayCommand]
    private static void ShowToastWindow() => new ToastWindowDemo().Show();
}
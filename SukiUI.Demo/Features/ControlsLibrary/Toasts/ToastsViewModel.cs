using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Enums;
using SukiUI.Models;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Toasts;

public partial class ToastsViewModel(ISukiToastManager toastManager) : DemoPageBase("Toasts", MaterialIconKind.BellRing)
{
    [RelayCommand]
    private void ShowInfoToast()
    {
        //SukiHost.ShowToast("A Simple Toast", "This is the content of an info toast.", NotificationType.Information);
        toastManager.CreateSimpleInfoToast()
            .WithTitle("A Simple Toast")
            .WithContent("This is the content of an info toast.")
            .Queue();
    }

    [RelayCommand]
    private void ShowActionToast()
    {
        toastManager.CreateToast()
            .WithTitle("Update Available")
            .WithContent("Update v1.0.0 Now Available.")
            .Dismiss().After(TimeSpan.FromSeconds(5)) // TODO: Action button support.
            .Queue();
        // return SukiHost.ShowToast(new ToastModel("Update Available", "A new version is available for you.",
        //     NotificationType.Information, TimeSpan.FromSeconds(5), null, "Update Now",
        //     () => { SukiHost.ShowToast("Update", new ProgressBar() { Value = 43, ShowProgressText = true }); }));
    }

    [RelayCommand]
    private void ShowSuccessToast() => ShowTypeDemoToast(NotificationType.Success);

    [RelayCommand]
    private void ShowWarningToast() => ShowTypeDemoToast(NotificationType.Warning);

    [RelayCommand]
    private void ShowErrorToast() => ShowTypeDemoToast(NotificationType.Error);

    private void ShowTypeDemoToast(NotificationType toastType)
    {
        toastManager.CreateToast()
            .WithTitle($"{toastType}!")
            .WithContent($"This is the content of {char.ToLower(toastType.ToString()[0]) + toastType.ToString()[1..]} toast.")
            .OfType(toastType)
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }

    [RelayCommand]
    private void ShowThreeInfoToasts()
    {
        for (var i = 1; i <= 3; i++)
            toastManager.CreateSimpleInfoToast()
                .WithTitle($"Toast {i} of 3")
                .WithContent($"This is toast {i} of 3 being shown all at once.")
                .Queue();
    }

    [RelayCommand]
    private void ShowToastWithCallback()
    {
        // TODO: Implement dismiss interactions for the toasts.
        // return SukiHost.ShowToast("Click This Toast", "Click this toast to open a dialog.", NotificationType.Information, TimeSpan.FromSeconds(15),
        //     () => SukiHost.ShowDialog(
        //         new TextBlock { Text = "You clicked the toast! - Click anywhere outside of this dialog to close." },
        //         allowBackgroundClose: true));
    }

    [RelayCommand]
    private void ShowToastWindow() => new ToastWindowDemo().Show();
}
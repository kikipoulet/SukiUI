using System;
using System.Reactive.Concurrency;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
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
            .WithContent("Information, Update v1.0.0.0 is Now Available.")
            .WithActionButtonNormal("Later", _ => { }, true)
            .WithActionButton("Update", _ => ShowUpdatingToast(), true)
            .Queue();
    }

    private void ShowUpdatingToast()
    {
        var progress = new ProgressBar() { Value = 0, ShowProgressText = true };
        var toast = toastManager.CreateToast()
            .WithTitle("Updating...")
            .WithContent(progress)
            .Queue();
        var timer = new Timer(20);
        timer.Elapsed += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                progress.Value += 1;
                if (progress.Value < 100) return;
                timer.Dispose();
                toastManager.Dismiss(toast);
            });
        };
        timer.Start();
    }

    [RelayCommand]
    private void ShowSuccessToast() => ShowTypeDemoToast(NotificationType.Success);

    [RelayCommand]
    private void ShowWarningToast() => ShowTypeDemoToast(NotificationType.Warning);

    [RelayCommand]
    private void ShowErrorToast() => ShowTypeDemoToast(NotificationType.Error);

    [RelayCommand]
    private void ShowLoadingToast()
    {
        toastManager.CreateToast()
            .WithTitle("Loading")
            .WithLoadingState(true)
            .WithContent(
                $"This is a loading toast.")
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }
    private void ShowTypeDemoToast(NotificationType toastType)
    {
        toastManager.CreateToast()
            .WithTitle($"{toastType}!")
            .WithContent(
                $"This is the content of {char.ToLower(toastType.ToString()[0]) + toastType.ToString()[1..]} toast.")
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
        toastManager.CreateToast()
            .WithTitle("Callback Toast")
            .WithContent("Click anywhere (other than the button) on this toast to show another toast.")
            .OnClicked(_ => ShowCallbackToast())
            .WithActionButtonNormal("Close", _ => { }, true)
            .Queue();
        return;

        void ShowCallbackToast()
        {
            toastManager.CreateSimpleInfoToast()
                .WithTitle("Opened From Callback.")
                .WithContent("This toast will close in 3 seconds...")
                .Queue();
        }
    }

    [RelayCommand]
    private void ShowToastWindow() => new ToastWindowDemo(toastManager).Show();

    [RelayCommand]
    private void DismissAllToasts() => toastManager.DismissAll();
}
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using SukiUI.Enums;
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
            .WithActionButton("Later", _ => { }, true, SukiButtonStyles.Basic)
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
        var timer = new System.Timers.Timer(20);
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
    private void ShowStyledButtonsToast()
    {
        toastManager.CreateToast()
            .WithTitle("Styled buttons")
            .WithContent("This is the content of styled buttons toast.")
            .OfType(NotificationType.Information)
            .Dismiss().ByClicking()
            .WithActionButton(new MaterialIcon()
            {
                Kind = MaterialIconKind.Check
            }, _ => { }, true, SukiButtonStyles.Flat | SukiButtonStyles.Accent | SukiButtonStyles.Icon)
            .WithActionButton(new MaterialIcon()
            {
                Kind = MaterialIconKind.QuestionMark
            }, _ => { }, true, SukiButtonStyles.Flat | SukiButtonStyles.Icon)
            .WithActionButton(new MaterialIcon()
            {
                Kind = MaterialIconKind.Close
            }, _ => { }, true, SukiButtonStyles.Icon)
            .Queue();
    }

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
                $"This is the content of {toastType.ToString().ToLowerInvariant()} toast.")
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
            .WithActionButton("Close", _ => { }, true, SukiButtonStyles.Basic)
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
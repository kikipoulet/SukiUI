using System;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogsViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) : DemoPageBase("Dialogs", MaterialIconKind.Forum)
{
    [RelayCommand]
    private void OpenStandardDialog()
    {
        dialogManager.CreateDialog()
            .WithTitle("A Standard Dialog")
            .WithContent("This is a standard dialog. Click the button below to dismiss.")
            .WithActionButton("Dismiss", _ => { }, true)
            .TryShow();
    }

    [RelayCommand]
    private void OpenBackgroundCloseDialog()
    {
        dialogManager.CreateDialog()
            .WithTitle("Background Closing Dialog")
            .WithContent("Dismiss this dialog by clicking anywhere outside of it.")
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private void OpenMultiOptionDialog()
    {
        dialogManager.CreateDialog()
            .WithTitle("Multi Option Dialog")
            .WithContent("Select any one of the below options:")
            .WithActionButton("Option 1", _ => ShowOptionToast(1), true)
            .WithActionButton("Option 2", _ => ShowOptionToast(2), true)
            .WithActionButton("Option 3", _ => ShowOptionToast(3), true)
            .TryShow();
    }

    private void ShowOptionToast(int option)
    {
        toastManager.CreateToast()
            .WithTitle("Dialog Option Clicked")
            .WithContent($"You clicked option #{option}")
            .Dismiss().ByClicking()
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Queue();
    }

    [RelayCommand]
    private void OpenViewModelDialog()
    {
        dialogManager.CreateDialog()
            .WithViewModel(dialog => new VmDialogViewModel(dialog))
            .TryShow();
    }

    [RelayCommand]
    private void OpenDialogWindowDemo() => new DialogWindowDemo().Show();
        
}
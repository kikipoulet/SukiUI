using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogsViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager) : DemoPageBase("Dialogs", MaterialIconKind.Forum)
{
    public NotificationType[] NotificationTypes { get; } = Enum.GetValues<NotificationType>();
    
    [ObservableProperty] private NotificationType _selectedType;
    
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
    private void OpenLongDialog()
    {
        var listbox = new ListBox
        {
            ItemsSource = Enumerable.Range(0, 1000)
        };
        dialogManager.CreateDialog()
            .WithTitle("A Long Dialog with a ListBox")
            .WithContent(listbox)
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
    private void OpenMessageBoxStyleDialog()
    {
        dialogManager.CreateDialog()
            .OfType(SelectedType)
            .WithTitle("MessageBox style dialog.")
            .WithContent("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.")
            .WithActionButton("Close " + SelectedType.ToString(), _ => { }, true, "Flat")
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    [RelayCommand]
    private void OpenViewModelDialog()
    {
        dialogManager.CreateDialog()
            .WithViewModel(dialog => new VmDialogViewModel(dialog))
            .TryShow();
    }

    [RelayCommand]
    private void OpenViewModelWithActionButtonsDialog()
    {
        dialogManager.CreateDialog()
            .OfType(NotificationType.Warning)
            .WithTitle("Dialog with content set to a VM")
            .WithViewModel(dialog => new VmDialogViewModel(dialog), false)
            .WithActionButton("Extra Close Button", _ => { }, true, "Flat")
            .TryShow();
    }

    [RelayCommand]
    private void OpenDialogWindowDemo() => new DialogWindowDemo(dialogManager).Show();
        
}
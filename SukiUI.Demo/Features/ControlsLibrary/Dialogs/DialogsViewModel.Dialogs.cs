using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class DialogsViewModel
    {
        [RelayCommand]
        private void OpenStandardDialog()
        {
            _dialogManager.CreateDialog()
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

            _dialogManager.CreateDialog()
                .WithTitle("A Long Dialog with a ListBox")
                .WithContent(listbox)
                .WithActionButton("Dismiss", _ => { }, true)
                .TryShow();
        }

        [RelayCommand]
        private void OpenBackgroundCloseDialog()
        {
            _dialogManager.CreateDialog()
                .WithTitle("Background Closing Dialog")
                .WithContent("Dismiss this dialog by clicking anywhere outside of it.")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }

        [RelayCommand]
        private void OpenMultiOptionDialog()
        {
            _dialogManager.CreateDialog()
                .WithTitle("Multi Option Dialog")
                .WithContent("Select any one of the below options:")
                .WithActionButton("Option 1", _ => ShowOptionToast(1), true)
                .WithActionButton("Option 2", _ => ShowOptionToast(2), true)
                .WithActionButton("Option 3", _ => ShowOptionToast(3), true)
                .TryShow();
        }

        [RelayCommand]
        private void OpenViewModelDialog()
        {
            _dialogManager.CreateDialog()
                .WithViewModel(dialog => new VmDialogViewModel(dialog))
                .TryShow();
        }

        [RelayCommand]
        private void OpenViewModelWithActionButtonsDialog()
        {
            _dialogManager.CreateDialog()
                .OfType(NotificationType.Warning)
                .WithTitle("Dialog with content set to a VM")
                .WithViewModel(dialog => new VmDialogViewModel(dialog), false)
                .WithActionButton("Extra Close Button", _ => { }, true, "Flat")
                .TryShow();
        }
    }
}
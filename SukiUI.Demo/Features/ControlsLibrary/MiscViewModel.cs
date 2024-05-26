using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Content;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class MiscViewModel() : DemoPageBase("Miscellaneous", MaterialIconKind.DotsHorizontalCircle)
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private DateTime _selectedDateTime = DateTime.Today;
    [ObservableProperty] private DateTimeOffset _selectedDateTimeOffset = DateTimeOffset.Now;

    [RelayCommand]
    private async Task ToggleBusy()
    {
        IsBusy = true;
        await Task.Delay(3000);
        IsBusy = false;
    }
    
    [RelayCommand]
    private void OpenBox()
    {
        SukiHost.ShowMessageBox(new MessageBoxModel("Update Available", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.", ToastType.Info, "Update Now", () =>{SukiHost.CloseDialog();} ));
    }
    
    [RelayCommand]
    private void OpenBoxError()
    {
        SukiHost.ShowMessageBox(new MessageBoxModel("Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ", ToastType.Error));
    }
        
    [RelayCommand]
    private void OpenFile()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

        // Start async operation to open the dialog.
        var files = topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions());
    }
       
    [RelayCommand]
    private void OpenFolder()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

        // Start async operation to open the dialog.
        var files = topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
    }
        
    [RelayCommand]
    private void SaveFile()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

        // Start async operation to open the dialog.
        var files = topLevel?.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions());
    }


    partial void OnSelectedDateTimeChanged(DateTime value) => 
        SelectedDateTimeOffset = value;

    partial void OnSelectedDateTimeOffsetChanged(DateTimeOffset value) => 
        SelectedDateTime = value.DateTime;
}
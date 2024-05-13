using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Enums;

namespace SukiUI.Demo.Features.ControlsLibrary.Toasts
{
    public partial class ToastsViewModel() : DemoPageBase("Toasts", MaterialIconKind.BellRing)
    {
        [RelayCommand]
        public void OpenSourceURL() =>
            UrlUtilities.OpenURL(
                $"https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/{nameof(ToastsViewModel)}.cs");

        [RelayCommand]
        public Task ShowSingleStandardToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of a toast.");
        
        [RelayCommand]
        public Task ShowInfoToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of an info toast.", SukiToastType.Info);
        
        [RelayCommand]
        public Task ShowSuccessToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of a success toast.", SukiToastType.Success);
        
        [RelayCommand]
        public Task ShowWarningToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of a warning toast.", SukiToastType.Warning);
        
        [RelayCommand]
        public Task ShowErrorToast() =>
            SukiHost.ShowToast("A Simple Toast", "This is the content of an error toast.", SukiToastType.Error);

        [RelayCommand]
        public async Task ShowThreeInfoToasts()
        {
            for (var i = 1; i <= 3; i++)
                await SukiHost.ShowToast("A Simple Toast", $"This is toast number {i} of 3.");
        }

        [RelayCommand]
        public Task ShowToastWithCallback()
        {
            return SukiHost.ShowToast("Click This Toast", "Click this toast to open a dialog.", SukiToastType.Info, TimeSpan.FromSeconds(15),
                () => SukiHost.ShowDialog(
                    new TextBlock { Text = "You clicked the toast! - Click anywhere outside of this dialog to close." },
                    allowBackgroundClose: true));
        }

        [RelayCommand]
        public void ShowToastWindow() => new ToastWindowDemo().Show();
    }
}
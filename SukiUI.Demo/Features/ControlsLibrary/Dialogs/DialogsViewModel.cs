using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class DialogsViewModel() : DemoPageBase("Dialogs", MaterialIconKind.Forum)
    {
        [RelayCommand]
        private void OpenStandardDialog() =>
            SukiHost.ShowDialog(new StandardDialog());

        [RelayCommand]
        private void OpenBackgroundCloseDialog() =>
            SukiHost.ShowDialog(new BackgroundCloseDialog(), allowBackgroundClose: true);
        
        [RelayCommand]
        private void OpenViewModelDialog() =>
            SukiHost.ShowDialog(new VmDialogViewModel(), allowBackgroundClose: true);

        [RelayCommand]
        private void OpenDialogWindowDemo() => new DialogWindowDemo().Show();

        [RelayCommand]
        private void OpenSourceURL() =>
            UrlUtilities.OpenURL(
                $"https://github.com/kikipoulet/SukiUI/blob/main/SukiUI.Demo/Features/ControlsLibrary/{nameof(DialogsViewModel)}.cs");
        
    }
}
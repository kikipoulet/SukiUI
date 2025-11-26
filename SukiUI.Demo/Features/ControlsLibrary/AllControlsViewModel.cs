using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Demo.Features;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class AllControlsViewModel : DemoPageBase
{
    [ObservableProperty] private ISukiDialogManager dialogManager;
    public AllControlsViewModel(ISukiDialogManager dialogManager) : base("All Controls", MaterialIconKind.ViewDashboard, 100)
    {
        DialogManager = dialogManager;
    }
}

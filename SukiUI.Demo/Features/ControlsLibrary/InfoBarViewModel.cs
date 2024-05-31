using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class InfoBarViewModel() : DemoPageBase("InfoBar", MaterialIconKind.InfoOutline)
{
    [ObservableProperty]
    private bool _isOpen = true;
    
    [ObservableProperty]
    private bool _isClosable = true;
    
    [RelayCommand]
    private void RefreshIsOpenStatus()
    {
        IsOpen = true;
    }
}
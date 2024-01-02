using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel() : DemoPageBase("Welcome", MaterialIconKind.Hand, int.MinValue)
{
    [RelayCommand]
    public void OpenDashboard()
    {
    }
}
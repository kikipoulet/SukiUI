using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;

namespace SukiUI.Demo.Features.Splash;

public partial class SplashViewModel(PageNavigationService nav) : DemoPageBase("Welcome", MaterialIconKind.Hand, int.MinValue)
{
    [ObservableProperty] private bool _dashBoardVisited;
    
    [RelayCommand]
    private void OpenDashboard()
    {
        DashBoardVisited = true;
        nav.RequestNavigation<DashboardViewModel>();
    }
}
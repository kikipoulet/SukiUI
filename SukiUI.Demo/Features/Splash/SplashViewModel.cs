using Material.Icons;
using SukiUI.Demo.Features.Dashboard;
using SukiUI.Demo.Services;

namespace SukiUI.Demo.Features.Splash;

public class SplashViewModel(PageNavigationService nav) : DemoPageBase("Welcome", MaterialIconKind.Hand, int.MinValue)
{
    public void OpenDashboard()
    {
        nav.RequestNavigation<DashboardViewModel>();
    }
}
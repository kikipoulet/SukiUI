using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class ButtonsLibraryViewModel() : DemoPageBase("Buttons", MaterialIconKind.Button)
{
    [ObservableProperty] private bool _isBusy;

    public void ButtonClicked()
    {
        if (IsBusy) return;
        Task.Run(async () =>
        {
            IsBusy = true;
            await Task.Delay(3000);
            IsBusy = false;
        });
    }
}
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary
{
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

        partial void OnSelectedDateTimeChanged(DateTime value) => 
            SelectedDateTimeOffset = value;

        partial void OnSelectedDateTimeOffsetChanged(DateTimeOffset value) => 
            SelectedDateTime = value.DateTime;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Demo.Common;

namespace SukiUI.Demo;

public partial class SukiUIDemoViewModel : ViewAwareObservableObject
{
    [ObservableProperty] private ViewAwareObservableObject _activeViewModel;
}
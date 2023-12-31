using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Demo.Common;

namespace SukiUI.Demo.Features;

public abstract partial class FeatureBase(string displayName, MaterialIconKind icon) : ViewAwareObservableObject
{
    [ObservableProperty] private string _displayName = displayName;
    [ObservableProperty] private MaterialIconKind _icon = icon;
}
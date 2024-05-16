using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SukiUI.Demo.Features.ControlsLibrary.Colors;

public partial class ColorViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private IBrush _brush;

    public ColorViewModel(string name, IBrush brush)
    {
        _brush = brush;
        _name = name;
    }
}
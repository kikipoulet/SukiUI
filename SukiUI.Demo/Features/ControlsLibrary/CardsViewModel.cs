using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class CardsViewModel() : DemoPageBase("Cards", MaterialIconKind.Cards)
{
    [ObservableProperty] private bool _isOpaque;
    [ObservableProperty] private bool _isInteractive;

    [ObservableProperty] private ObservableCollection<int> _items = new ObservableCollection<int>(){1};
    
    [RelayCommand]
    private void AddItem()
    {
        Items.Add(1);
    }
    [RelayCommand]
    private void RemoveItem()
    {
        if(Items.Any())
            Items.RemoveAt(Items.Count-1);
    }

}
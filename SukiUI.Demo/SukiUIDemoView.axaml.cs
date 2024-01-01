using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Models;

namespace SukiUI.Demo;

public partial class SukiUIDemoView : SukiWindow
{
    public SukiUIDemoView()
    {
        InitializeComponent();
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SukiUIDemoViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }
}
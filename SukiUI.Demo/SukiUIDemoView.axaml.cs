using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.Content;
using SukiUI.Controls;
using SukiUI.Demo.Utilities;
using SukiUI.Models;

namespace SukiUI.Demo;

public partial class SukiUIDemoView : SukiWindow
{
    public SukiUIDemoView()
    {
        InitializeComponent();
        var bitmap = BitmapUtilities.CreateBitmap(Icons.SukiLogo, Brushes.White);
        if (bitmap is null) return;
        Icon = new WindowIcon(bitmap);
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SukiUIDemoViewModel vm) return;
        if (e.Source is not MenuItem mItem) return;
        if (mItem.DataContext is not SukiColorTheme cTheme) return;
        vm.ChangeTheme(cTheme);
    }
}
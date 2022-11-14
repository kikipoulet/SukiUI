using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AndroidTest.Views;

public partial class TabViewPage : UserControl
{
    public TabViewPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
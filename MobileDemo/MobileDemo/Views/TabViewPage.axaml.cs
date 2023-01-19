using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SukiUI.Controls;

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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
       InteractiveContainer.ShowToast(new TextBlock(){Text = "test"}, 5);
    }
}
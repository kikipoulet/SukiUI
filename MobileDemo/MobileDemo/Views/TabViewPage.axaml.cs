using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        MobileMenuPage.ShowDialogS(new DialogContent());

        Task.Run(() =>
        {
            MobileMenuPage.WaitUntilDialogClosed();
        
            Console.WriteLine("test");
        });
        
    }
}
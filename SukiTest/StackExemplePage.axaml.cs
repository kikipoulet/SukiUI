using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace SukiTest;

public partial class StackExemplePage : UserControl
{
    public StackExemplePage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void changepage(object? sender, RoutedEventArgs e)
    {
        var b = new Button() { Name = "Network", Content = "go to" };
        b.Click += (o, args) =>
        {
            this.FindControl<SukiStackPage>("StackSettings").Content = new Grid() { Name = "Wifi" };
        };

        this.FindControl<SukiStackPage>("StackSettings").Content = b;
    }
}
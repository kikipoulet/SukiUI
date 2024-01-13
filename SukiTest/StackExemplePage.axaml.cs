using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;
using SukiUI.Extensions;

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

    private void Changepage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var b = new Button() { Name = "Network", Content = "go to" };
        b.Click += (o, args) =>
        {
            this.FindRequiredControl<SukiStackPage>("StackSettings").Content = new Grid() { Name = "Wifi" };
        };

        this.FindRequiredControl<SukiStackPage>("StackSettings").Content = b;
    }
}
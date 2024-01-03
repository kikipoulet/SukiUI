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
        var b = new Button() { Content = "go to" };
        b.Click += (o, args) =>
        {
            this.FindRequiredControl<StackPage>("StackSettings").Push("Wifi", new Grid());
        };

        this.FindRequiredControl<StackPage>("StackSettings").Push("Network", b);
    }
}
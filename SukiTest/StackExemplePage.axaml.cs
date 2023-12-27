using Avalonia;
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
        var b = new Button(){Content = "go to"};
        b.Click += (o, args) =>
        {
               
            this.FindControl<StackPage>("StackSettings").Push("Wifi", new Grid() );
        };
            
        this.FindControl<StackPage>("StackSettings").Push("Network",b );
    }
}
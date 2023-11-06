using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiTest;

public partial class StackTest : UserControl
{
    public StackTest()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
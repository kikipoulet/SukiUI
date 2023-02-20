using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiTest.CircleProgressBarsTestMVVM;

public partial class CircleProgresstestView : UserControl
{
    public CircleProgresstestView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
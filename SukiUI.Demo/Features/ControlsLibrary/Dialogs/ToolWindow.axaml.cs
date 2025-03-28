using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Extensions;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class ToolWindow : SukiWindow
{

    public ToolWindow()
    {
        InitializeComponent();
    }

    private void CenterOnScreenButton_OnClick(object? sender, RoutedEventArgs e)
    {
        this.CenterOnScreen();
    }
}
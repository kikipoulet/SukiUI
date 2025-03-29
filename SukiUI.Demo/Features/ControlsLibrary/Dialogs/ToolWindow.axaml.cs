using Avalonia.Controls;
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

    private void ToggleAutoWindowSize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SizeToContent == SizeToContent.Manual)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
            CanMaximize = false;
            CanFullScreen = false;
        }
        else
        {
            SizeToContent = SizeToContent.Manual;
            CanResize = true;
            CanMaximize = true;
            CanFullScreen = true;
        }
    }
}
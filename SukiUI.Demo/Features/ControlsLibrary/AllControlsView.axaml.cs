using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.ControlsLibrary;

public partial class AllControlsView : UserControl
{
    public AllControlsView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        App.SingleViewDialogManager.CreateDialog().WithContent(new Border(){Height = 400, Width = 400}).Dismiss().ByClickingBackground().TryShow();
    }

    private void GoToAdvanced(object? sender, PointerPressedEventArgs e)
    {
        
        NStack.Push(new TextBlock
        {
            Text = "Advanced Settings",
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        }, "Advanced Settings");
    }
}

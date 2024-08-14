using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs;

public partial class DialogWindowDemo : SukiWindow
{
    private readonly ISukiDialogManager _mainWindowManager;
    private readonly ISukiDialogManager _thisWindowManager = new SukiDialogManager();
    
    public DialogWindowDemo(ISukiDialogManager mainWindowManager)
    {
        _mainWindowManager = mainWindowManager;
        InitializeComponent();
        DialogHost.Manager = _thisWindowManager;
    }

    private void ShowDialogInThisWindowClicked(object? sender, RoutedEventArgs e)
    {
        _thisWindowManager.CreateDialog()
            .WithTitle("Dialog Demo")
            .WithContent("This dialog is shown in this window.\r\nClick outside the dialog or click dismiss to dismiss this dialog.")
            .Dismiss().ByClickingBackground()
            .WithActionButton("Dismiss", _ => {}, true)
            .TryShow();
    }

    private void ShowDialogInMainWindowClicked(object? sender, RoutedEventArgs e)
    {
        _mainWindowManager.CreateDialog()
            .WithTitle("Dialog Demo")
            .WithContent("This dialog is shown in the main window from the opened window.\r\nClick outside the dialog or click dismiss to dismiss this dialog.")
            .Dismiss().ByClickingBackground()
            .WithActionButton("Dismiss", _ => {}, true)
            .TryShow();
    }
    private void ShowDialogInBothWindowsClicked(object? sender, RoutedEventArgs e)
    {
        _mainWindowManager.CreateDialog()
            .WithTitle("Dialog Demo")
            .WithContent("This dialog is shown in both windows.\r\nClick outside the dialog or click dismiss to dismiss this dialog.")
            .Dismiss().ByClickingBackground()
            .WithActionButton("Dismiss", _ => {}, true)
            .TryShow();
        _thisWindowManager.CreateDialog()
            .WithTitle("Dialog Demo")
            .WithContent("This dialog is shown in both windows.\r\nClick outside the dialog or click dismiss to dismiss this dialog.")
            .Dismiss().ByClickingBackground()
            .WithActionButton("Dismiss", _ => {}, true)
            .TryShow();
    }
}
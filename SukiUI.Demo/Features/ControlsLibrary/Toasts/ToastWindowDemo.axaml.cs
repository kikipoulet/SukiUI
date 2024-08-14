using Avalonia.Interactivity;
using SukiUI.Controls;
using SukiUI.Toasts;

namespace SukiUI.Demo.Features.ControlsLibrary.Toasts
{
    public partial class ToastWindowDemo : SukiWindow
    {
        private readonly ISukiToastManager _thisWindowManager = new SukiToastManager();
        private readonly ISukiToastManager _mainWindowManager;
        
        public ToastWindowDemo(ISukiToastManager mainWindowManager)
        {
            _mainWindowManager = mainWindowManager;
            InitializeComponent();
            ToastHost.Manager = _thisWindowManager;
        }

        private void ShowToastInThisWindowClicked(object? sender, RoutedEventArgs e)
        {
            _thisWindowManager.CreateSimpleInfoToast()
                .WithTitle("Window Toast")
                .WithContent("Toast shown in the opened window.")
                .Queue();
        }

        private void ShowToastInMainWindowClicked(object? sender, RoutedEventArgs e)
        {
            _mainWindowManager.CreateSimpleInfoToast()
                .WithTitle("Main Window Toast")
                .WithContent("Toast shown in the main window from the opened window.")
                .Queue();
        }

        private void ShowToastInBothWindowsClicked(object? sender, RoutedEventArgs e)
        {
            // You could quite easily share a `ISukiToastManager` instance if you wanted to this.
            _mainWindowManager.CreateSimpleInfoToast()
                .WithTitle("Both Windows Toast")
                .WithContent("Toast shown in both windows from the opened window.")
                .Queue();
            _thisWindowManager.CreateSimpleInfoToast()
                .WithTitle("Both Windows Toast")
                .WithContent("Toast shown in both windows from the opened window.")
                .Queue();
        }
    }
}
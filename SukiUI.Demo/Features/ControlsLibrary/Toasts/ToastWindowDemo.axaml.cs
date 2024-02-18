using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.Toasts
{
    public partial class ToastWindowDemo : SukiWindow
    {
        public ToastWindowDemo()
        {
            InitializeComponent();
        }

        private void ShowToastInThisWindowClicked(object? sender, RoutedEventArgs e) => 
            SukiHost.ShowToast(this, "Window Toast", "Toast shown in a specific window.");

        private void ShowToastInMainWindowClicked(object? sender, RoutedEventArgs e) => 
            SukiHost.ShowToast("Window Toast", "Toast shown in the earliest open window from the demo window.");
    }
}
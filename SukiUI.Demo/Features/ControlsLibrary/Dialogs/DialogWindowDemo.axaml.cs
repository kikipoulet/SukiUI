using Avalonia.Interactivity;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.Dialogs
{
    public partial class DialogWindowDemo : SukiWindow
    {
        public DialogWindowDemo()
        {
            InitializeComponent();
        }

        private void ShowDialogInThisWindowClicked(object? sender, RoutedEventArgs e) => 
            SukiHost.ShowDialog(this, "A simple dialog that is shown in the demo window.\r\nClick outside dialog to close.", allowBackgroundClose: true);

        private void ShowDialogInMainWindowClicked(object? sender, RoutedEventArgs e) => 
            SukiHost.ShowDialog("A simple dialog that was shown from the demo window.\r\nClick outside dialog to close.", allowBackgroundClose: true);
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace SukiTest
{
    public partial class DialogContent : UserControl
    {
        public DialogContent()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            SukiHost.CloseDialog();
        }
    }
}
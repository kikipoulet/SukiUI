using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SukiUI.Controls
{
    public partial class MobileMenuPage : UserControl
    {
        public MobileMenuPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ShowDialog(Control content)
        {
            var model = (MobileMenuPageViewModel)this.DataContext;
            model.DialogChild = content;
            model.IsDialogOpen = false;
            model.IsDialogOpen = true;
        }
    }
}

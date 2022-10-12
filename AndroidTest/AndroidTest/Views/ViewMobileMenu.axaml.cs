using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;

namespace AndroidTest.Views
{
    public static class MyMenu
    {
        public static MobileMenuPage menu;
    }
    public partial class ViewMobileMenu : UserControl
    {
        public ViewMobileMenu()
        {
            InitializeComponent();
            MyMenu.menu = this.FindControl<MobileMenuPage>("myMenu");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

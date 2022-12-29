using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

        
        private int i = 0;
        private void ChangeTheme(object? sender, RoutedEventArgs e)
        {
            if(i%2 == 0)
                SukiUI.ColorTheme.LoadDarkTheme(Application.Current);
            else
                SukiUI.ColorTheme.LoadLightTheme(Application.Current);
            
            i++;
        }
       
    }
}

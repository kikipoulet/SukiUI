using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;

namespace AndroidTest.Views
{

    public static class GlobalMenu
    {
        public static MobilePage menu;
    }

    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            GlobalMenu.menu = this.FindControl<MobilePage>("MobileMenu");
        
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            this.FindControl<Border>("Signborder").IsVisible = false;
            this.FindControl<Loading>("loadingCircle").IsVisible = true;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(4500);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    GlobalMenu.menu.NewPage("DashBoard", new Dashboard(), true);
                });
               
            });
            

           
        }

        private void LogInTest(object sender, RoutedEventArgs e)
        {
            MobileMenuPage.ShowToastS(new TextBlock(){ FontSize = 20, Text = "You're logged in !", Margin = new Thickness(40,20), VerticalAlignment = VerticalAlignment.Center}, 5);
        }
    }
}
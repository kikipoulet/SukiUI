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

   

    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
           
        
        }

     

        private void LogInTest(object sender, RoutedEventArgs e)
        {
            InteractiveContainer.ShowToast(new TextBlock(){ FontSize = 20, Text = "You're logged in !", Margin = new Thickness(40,20), VerticalAlignment = VerticalAlignment.Center}, 5);
        }

        private void Vibrate(object sender, RoutedEventArgs e)
        {
            Xamarin.Essentials.Vibration.Vibrate(TimeSpan.FromMilliseconds(500));

        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            MobileStack.Pop();
        }
    }
}
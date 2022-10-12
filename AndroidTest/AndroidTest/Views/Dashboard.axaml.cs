using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SukiUI.Controls;
//using LiveChartsCore;
//using LiveChartsCore.SkiaSharpView;
//using LiveChartsCore.SkiaSharpView.Avalonia;
using System.Collections.Generic;

namespace AndroidTest.Views
{
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();

            this.FindControl<Stepper>("myStep").Steps = new List<string>() { "Ordered", "Sent", "In Progress", "Delivered" };
            this.FindControl<Stepper>("myStep").Index = 2;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {

            MyMenu.menu.ShowDialog(
                new Border()
                {
                    Classes = new Classes(new List<string>() { "ElevatedCard"}), BorderThickness = new Thickness(0), 
                    Child = new DialogContent(),
                    
                });



        }


    }
}

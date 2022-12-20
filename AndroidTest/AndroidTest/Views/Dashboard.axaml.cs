using System;
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
        private static readonly Lazy<Dashboard> lazy =
            new Lazy<Dashboard>(() => new Dashboard());
        
        public static Dashboard Instance { get { return lazy.Value; } }
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

            MobileMenuPage.ShowDialogS(
                 new DialogContent()
            );



        }


    }
}

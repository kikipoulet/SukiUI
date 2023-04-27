using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Icons;
using System;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.Linq;
using Avalonia.Interactivity;

namespace SukiUI.Controls
{

    public partial class SideMenu : UserControl
    {
        public delegate void MenuItemChangedEventHandler(object sender, string header);
        public event MenuItemChangedEventHandler MenuItemChanged;
        public SideMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void PaneIsClosing(object sender, CancelRoutedEventArgs ev)
        {
            ((SideMenuModel)this.DataContext).MenuVisibility = false;
        }

        private void MenuItemSelectedChanged(object sender, RoutedEventArgs e)
        {
            RadioButton rButton = (RadioButton)sender;
            if (rButton.IsChecked != true)
                return;
            try
            {
                string header = ((TextBlock)((DockPanel)((Grid)rButton.Content).Children.First()).Children.Last()).Text;
                MenuItemChanged?.Invoke(this, header);
            }catch(Exception exc){}
        }
    }
}
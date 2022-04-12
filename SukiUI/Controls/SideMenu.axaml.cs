using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Icons;
using System;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.Linq;

namespace SukiUI.Controls
{

    public partial class SideMenu : UserControl
    {
        public SideMenu()
        {
            InitializeComponent();

           
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}

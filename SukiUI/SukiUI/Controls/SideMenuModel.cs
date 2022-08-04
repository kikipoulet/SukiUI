using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SukiUI.Controls
{
    public class SideMenuItem
    {
        public string Header { get; set; } = default;
        public MaterialIconKind Icon { get; set; } = MaterialIconKind.Circle;

        public object Content { get; set; } = new Grid();

        public List<SideMenuItem> Items { get; set; } = new List<SideMenuItem>();
    }

    public class SideMenuModel : ReactiveObject
    {

        

        private bool menuvisibility = true;

        public bool MenuVisibility
        {
            get => menuvisibility;
            set => this.RaiseAndSetIfChanged(ref menuvisibility, value);
        }

        public void ChangeMenuVisibility()
        {
            MenuVisibility = !MenuVisibility;

        }

        private object currentPage = new Grid();

        public object CurrentPage
        {
            get => currentPage;
            set => this.RaiseAndSetIfChanged(ref currentPage, value);
        }

        private object headerContent = new Grid();

        public object HeaderContent
        {
            get => headerContent;
            set => this.RaiseAndSetIfChanged(ref headerContent, value);
        }

        private List<SideMenuItem> menuItems = new List<SideMenuItem>();

        public List<SideMenuItem> MenuItems
        {
            get => menuItems;
            set => this.RaiseAndSetIfChanged(ref menuItems, value);
        }
        
        private List<SideMenuItem> footermenuItems = new List<SideMenuItem>();

        public List<SideMenuItem> FooterMenuItems
        {
            get => footermenuItems;
            set => this.RaiseAndSetIfChanged(ref footermenuItems, value);
        }


        public void ChangePage(object o)
        {
            CurrentPage = o;
        }
    }
}

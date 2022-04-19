using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SukiUI.Controls
{
    public class SukiMenuItem
    {
        public string Header { get; set; } = default;
        public MaterialIconKind Icon { get; set; } = MaterialIconKind.Circle;

        public object Content { get; set; } = new Grid();

        public List<SukiMenuItem> Items { get; set; } = new List<SukiMenuItem>();
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

        private List<SukiMenuItem> menuItems = new List<SukiMenuItem>();

        public List<SukiMenuItem> MenuItems
        {
            get => menuItems;
            set => this.RaiseAndSetIfChanged(ref menuItems, value);
        }

        public void ChangePage(object o)
        {
            CurrentPage = o;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SukiUI.Controls
{
    public class SideMenuItem
    {
        public string Header { get; set; } = default;
        public object Icon { get; set; } = new Border();

        public object Content { get; set; } = new Grid();

        public List<SideMenuItem> Items { get; set; } = new List<SideMenuItem>();
    }

    public class SideMenuModel : ReactiveObject
    {
        public SideMenuModel()
        {
            Task.Run(() =>
            {
                // Not proud of this but here we go
                Thread.Sleep(1500);
                ChangePage(MenuItems.First().Content);
            });
        }
        
        private bool menuvisibility = true;

        public bool MenuVisibility
        {
            get => menuvisibility;
            set
            {
                this.RaiseAndSetIfChanged(ref menuvisibility, value);
                this.RaisePropertyChanged("SpacerEnabled");
            }
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

        private ObservableCollection<SideMenuItem> menuItems = new ObservableCollection<SideMenuItem>();

        public ObservableCollection<SideMenuItem> MenuItems
        {
            get => menuItems;
            set
            {
                this.RaiseAndSetIfChanged(ref menuItems, value);
                
            }
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


        private bool headerContentOverlapsToggleSidebarButton = false;
        /// <summary>
        /// Defines if header content can overlap sidebar visibility button.
        /// If true - they can take the same spot in the UI, which can lead to bugs when the content is too wide.
        /// If false - header content moves below the sidebar button.
        /// Default is true.
        /// </summary>
        public bool HeaderContentOverlapsToggleSidebarButton
        {
            get => headerContentOverlapsToggleSidebarButton;
            set => this.RaiseAndSetIfChanged(ref headerContentOverlapsToggleSidebarButton, value);
        }

        /// <summary>
        /// Defines if element that moves menu buttons down is enabled.
        /// </summary>
        // Property name must be equal to string inside MenuVisibility property.
        private bool SpacerEnabled
        {
            get { return HeaderContentOverlapsToggleSidebarButton && !MenuVisibility; }
        }

        public int HeaderMinHeight { get => HeaderContentOverlapsToggleSidebarButton ? 40 : 0; }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using Material.Icons;
using Material.Icons.Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace SukiUI.Controls
{
  
    public partial class DesktopPage : UserControl
    {
        public DesktopPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private DesktopPageViewModel ViewModel = new DesktopPageViewModel();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<IBrush> LogoColorProperty = AvaloniaProperty.Register<DesktopPage, IBrush>(nameof(LogoColor), defaultValue: Brushes.DarkSlateBlue);

        public IBrush LogoColor
        {
            get { return GetValue(LogoColorProperty); }
            set { SetValue(LogoColorProperty, value); }
        }

        public static readonly StyledProperty<MaterialIconKind> LogoKindProperty = AvaloniaProperty.Register<DesktopPage, MaterialIconKind>(nameof(LogoKind), defaultValue: MaterialIconKind.DotNet);

        public MaterialIconKind LogoKind
        {
            get { return GetValue(LogoKindProperty); }
            set { SetValue(LogoKindProperty, value); }
        }

        public static readonly StyledProperty<List<MenuItem>> MenuItemsProperty = AvaloniaProperty.Register<DesktopPage, List<MenuItem>>(nameof(MenuItems), defaultValue: new List<MenuItem>());

        public List<MenuItem> MenuItems
        {
            get { return GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }
        
        public static readonly StyledProperty<string> HeaderProperty = AvaloniaProperty.Register<DesktopPage, string>(nameof(Header), defaultValue: "Avalonia UI");

        public string Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        
        public static readonly StyledProperty<bool> MenuVisibilityProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(MenuVisibility), defaultValue: false);

        public bool MenuVisibility
        {
            get { return GetValue(MenuVisibilityProperty); }
            set { SetValue(MenuVisibilityProperty, value); }
        }

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }

        public void SetPage(Control page)
        {
           
                Content = page;
            
        }

      

        public void ShowDialog(Control Content)
        {
            ViewModel.CurrentDialog = Content;
            ViewModel.DialogOpen = true;
        }

        public static void ShowDialogS(Control content)
        {
            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<DesktopPage>().First().ShowDialog(content);
        }
    }
}

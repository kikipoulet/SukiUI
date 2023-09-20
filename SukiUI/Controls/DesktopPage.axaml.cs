using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using Material.Icons;
using Material.Icons.Avalonia;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Avalonia.Threading;

namespace SukiUI.Controls
{
  
    public partial class DesktopPage : UserControl
    {
        public DesktopPage()
        {
            InitializeComponent();

            

         //   DataContext = ViewModel;
        }

        // private DesktopPageViewModel ViewModel = new DesktopPageViewModel();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<HorizontalAlignment> TitleHorizontalAlignmentProperty = AvaloniaProperty.Register<DesktopPage, HorizontalAlignment>(nameof(TitleHorizontalAlignment), defaultValue: HorizontalAlignment.Left);

        public HorizontalAlignment TitleHorizontalAlignment
        {
            get { return GetValue(TitleHorizontalAlignmentProperty); }
            set { SetValue(TitleHorizontalAlignmentProperty, value); }
        }

        public static readonly StyledProperty<double> TitleFontSizeProperty = AvaloniaProperty.Register<DesktopPage, double>(nameof(TitleFontSize), defaultValue: 14);

        public double TitleFontSize
        {
            get { return GetValue(TitleFontSizeProperty); }
            set { SetValue(TitleFontSizeProperty, value); }
        }


        public static readonly StyledProperty<FontWeight> TitleFontWeightProperty = AvaloniaProperty.Register<DesktopPage, FontWeight>(nameof(TitleFontWeight), defaultValue: FontWeight.Medium);

        public FontWeight TitleFontWeight
        {
            get { return GetValue(TitleFontWeightProperty); }
            set { SetValue(TitleFontWeightProperty, value); }
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

        public static readonly StyledProperty<List<MenuItem>> MenuItemsProperty = 
            AvaloniaProperty.Register<DesktopPage, List<MenuItem>>(nameof(MenuItems), defaultValue: new List<MenuItem>());

        public List<MenuItem> MenuItems
        {
            get { return GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }
        
        public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<DesktopPage, string>(nameof(Title), defaultValue: "Avalonia UI");

        public string Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        
        public static readonly StyledProperty<bool> ShowBottomBorderProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(ShowBottomBorder), defaultValue: true);

        public bool ShowBottomBorder
        {
            get { return GetValue(ShowBottomBorderProperty); }
            set { SetValue(ShowBottomBorderProperty, value); }
        }
        
        public static readonly StyledProperty<bool> MenuVisibilityProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(MenuVisibility), defaultValue: false);

        public bool MenuVisibility
        {
            get { return GetValue(MenuVisibilityProperty); }
            set { SetValue(MenuVisibilityProperty, value); }
        }
        

        public static readonly StyledProperty<bool> IsMinimizeButtonEnabledProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(IsMinimizeButtonEnabled), defaultValue: true);

        public bool IsMinimizeButtonEnabled
        {
            get { return GetValue(IsMinimizeButtonEnabledProperty); }
            set { SetValue(IsMinimizeButtonEnabledProperty, value); }
        }

        public static readonly StyledProperty<bool> IsMaximizeButtonEnabledProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(IsMaximizeButtonEnabled), defaultValue: true);

        public bool IsMaximizeButtonEnabled
        {
            get { return GetValue(IsMaximizeButtonEnabledProperty); }
            set { SetValue(IsMaximizeButtonEnabledProperty, value); }
        }

        /// <summary>
        /// Minimizes Avalonia window
        /// </summary>
        private void MinimizeHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximizes Avalonia window or sets its size to original depending on window state
        /// </summary>
        private void MaximizeHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            var icon = this.GetVisualDescendants().OfType<MaterialIcon>().FirstOrDefault(x => (x.Name ?? "").Equals("MaximizeMaterialIcon"));
            
            if (hostWindow.WindowState != WindowState.Maximized)
            {
                hostWindow.WindowState = WindowState.Maximized;
                icon?.Classes.Remove("WindowMaximize");
                icon?.Classes.Add("WindowRestore");
            }
            else
            {
                hostWindow.WindowState = WindowState.Normal;
                icon?.Classes.Remove("WindowRestore");
                icon?.Classes.Add("WindowMaximize");
            }
        }

        /// <summary>
        /// Closes Avalonia window
        /// </summary>

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }



        public void SetPage(Control page)
        {
           
                Content = page;
            
        }

      

    }
}

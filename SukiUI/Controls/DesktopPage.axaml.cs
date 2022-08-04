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

        public static readonly StyledProperty<List<MenuItem>> MenuItemsProperty = AvaloniaProperty.Register<DesktopPage, List<MenuItem>>(nameof(MenuItems), defaultValue: new List<MenuItem>());

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
        
        public static readonly StyledProperty<bool> MenuVisibilityProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(MenuVisibility), defaultValue: false);

        public bool MenuVisibility
        {
            get { return GetValue(MenuVisibilityProperty); }
            set { SetValue(MenuVisibilityProperty, value); }
        }

        public static readonly StyledProperty<bool> IsDialogOpenProperty = AvaloniaProperty.Register<DesktopPage, bool>(nameof(IsDialogOpen), defaultValue: false);

        public bool IsDialogOpen
        {
            get { return GetValue(IsDialogOpenProperty); }
            set { SetValue(IsDialogOpenProperty, value); }
        }


        public static readonly StyledProperty<Control> DialogChildProperty = AvaloniaProperty.Register<DesktopPage, Control>(nameof(DialogChild), defaultValue: new Grid() {});

        public Control DialogChild
        {
            get { return GetValue(DialogChildProperty); }
            set { SetValue(DialogChildProperty, value); }
        }

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            IsDialogOpen = false;
        }

        public void SetPage(Control page)
        {
           
                Content = page;
            
        }

      

        public void ShowDialog(Control Content)
        {
            //    DialogContent = Content;
            // myGridForDialog

            DialogChild = Content;

            IsDialogOpen = false;
            IsDialogOpen = true;
        }

        public static void ShowDialogS(Control content)
        {
            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<DesktopPage>().First().ShowDialog(content);
        }

        public static void CloseDialogS()
        {
            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow.GetVisualDescendants().OfType<DesktopPage>().First().CloseDialog(null,null);
        }
    }
}

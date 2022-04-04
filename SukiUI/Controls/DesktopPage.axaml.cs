using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using System.Collections.Generic;
namespace SukiUI.Controls
{
    public partial class DesktopPage : UserControl
    {
        public DesktopPage()
        {
            InitializeComponent();
        }

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

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }
    }
}

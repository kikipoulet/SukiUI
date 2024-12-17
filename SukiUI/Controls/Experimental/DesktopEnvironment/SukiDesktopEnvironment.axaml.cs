using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SukiUI.Controls.Experimental.DesktopEnvironment
{
    public partial class SukiDesktopEnvironment : UserControl
    {
        private WindowManager _WM;
        
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _WM = e.NameScope.Find<WindowManager>("WM");
        }
        public SukiDesktopEnvironment()
        {
            InitializeComponent();
        }
        
        public static readonly StyledProperty<IImage?> DesktopBackgroundImageSourceProperty =
            AvaloniaProperty.Register<SukiDesktopEnvironment, IImage?>(nameof(DesktopBackgroundImageSource));
        
        public IImage? DesktopBackgroundImageSource
        {
            get => GetValue(DesktopBackgroundImageSourceProperty);
            set => SetValue(DesktopBackgroundImageSourceProperty, value);
        }
        
        public static readonly StyledProperty<IImage?> HomeImageSourceProperty =
            AvaloniaProperty.Register<SukiDesktopEnvironment, IImage?>(nameof(HomeImageSource));
        
        public IImage? HomeImageSource
        {
            get => GetValue(HomeImageSourceProperty);
            set => SetValue(HomeImageSourceProperty, value);
        }
        
        private void open(object? sender, RoutedEventArgs e)
        {
            SDESoftware soft = (SDESoftware)(((Button)sender).Tag);
            soft.Click(_WM);
        }
        
        public static readonly StyledProperty<ObservableCollection<SDESoftware>> SoftwaresProperty =
            AvaloniaProperty.Register<ChatUI, ObservableCollection<SDESoftware>>(nameof(Softwares), 
                defaultValue: new ObservableCollection<SDESoftware>());

        public ObservableCollection<SDESoftware> Softwares
        {
            get { return GetValue(SoftwaresProperty); }
            set { SetValue(SoftwaresProperty, value); }
        }

        private void InputElement_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var ctl = sender as Control;
            if (ctl != null)
            {
                FlyoutBase.ShowAttachedFlyout(ctl);
            }
        }
    }
}
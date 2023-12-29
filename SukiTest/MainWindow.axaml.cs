using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using SukiUI;
using SukiUI.Enums;

namespace SukiTest
{
    
    public partial class MainWindow : SukiWindow
    {
        public WindowNotificationManager notificationManager;
   

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if(notificationManager == null)
                notificationManager = new WindowNotificationManager(this);
        }
        
        private void ChangeTheme(object? sender, RoutedEventArgs e)
        {
            if (Application.Current is null) return;
            
            SukiTheme.SwitchBaseTheme();
            
            SukiHost.ShowToast("Successfully Changed Theme", $"Changed Theme To {Application.Current.ActualThemeVariant}", onClicked:
                () =>
                {
                    SukiHost.ShowToast("Success!", "You Closed A Toast By Clicking On It!");
                });
        }


        private void ChangeColor(object? sender, RoutedEventArgs e)
        {
            var curr = SukiTheme.ActiveColorTheme;
            if (curr is not { } currentTheme)
                return;
            var newColorTheme = (SukiColor)(((int)currentTheme.Theme + 1) % 4);
            SukiTheme.TryChangeColorTheme(newColorTheme);
            SukiHost.ShowToast("Successfully Changed Color", $"Changed Color To {newColorTheme}.");
        }
    }
}
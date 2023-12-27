using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;
using SukiUI.MessageBox;
using System.Threading;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using SukiUI.Theme;

namespace SukiTest
{


  
    
    public partial class MainWindow : Window
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
        
        private int i = 0;
        private void ChangeTheme(object? sender, RoutedEventArgs e)
        {
            if(i%2 == 0)
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            
            else
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            
            i++;
        }
        
        
    }
}
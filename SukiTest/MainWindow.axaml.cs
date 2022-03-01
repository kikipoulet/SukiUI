using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SukiUI.Controls;
using SukiUI.MessageBox;

namespace SukiTest
{

    public class Person
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }
    public partial class MainWindow : Window
    {
        private List<Person> liste { get; set; }= new List<Person>() {new Person(){Name = "jean", Age = "21"}, new Person(){Name = "Anne", Age = "25"}};

        WindowNotificationManager notificationManager;
        public MainWindow()
        {
            InitializeComponent();
            this.FindControl<DataGrid>("myDG").Items = liste;
          
            notificationManager = new WindowNotificationManager(this); 

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ShowInfoBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Info(this, "Title", "This is an information message that need to be read.");
        }

        private void ShowSuccessBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Success(this, "Title", "This is an Success message that need to be read.");
        }

        private void ShowDangerBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Error(this, "Title", "This is an Success message that need to be read.");
        }

        private void ShowNotification(object sender, RoutedEventArgs e)
        {
            var notif = new Avalonia.Controls.Notifications.Notification("title","message");
            notificationManager.Show(notif);
        }

    }
}
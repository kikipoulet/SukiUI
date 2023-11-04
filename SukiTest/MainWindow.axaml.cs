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

using Avalonia.Media;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using SukiUI.Controls.TouchInput.TouchNumericPad;
using SukiUI.Theme;

namespace SukiTest
{
    public enum Genders
    {
        Male, Female
    }
    public class Person
    {
        [Category("Data")]
        public string Name { get; set; } = "Charles";
        [Category("Data")]
        public int Age { get; set; } = 20;
        [Category("Data")]
        public bool Adult { get; set; } = true;

        public double JustADouble { get; set; } = 20;
        
        public DateTime Birth { get; set; } = DateTime.Now;
        public bool Woman { get; set; } = false;
        public bool Man { get; set; } = true;

        public Genders Genre { get; set; } = Genders.Male;

        public Person Partner { get; set; } = null;

        public List<int> ListOfInts { get; set; } = new List<int>() { 1,2,3,4,5,6,7,8,9,10};
    }

    public class Invoice
    {
        public int Id { get; set; } = 20;
        public string BillingName { get; set; } 
        public int Amount { get; set; }
        public bool Paid { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        private ObservableCollection<Invoice> liste { get; set; }= new ObservableCollection<Invoice>() {
            new Invoice(){Id = 15364, BillingName = "Jean", Amount = 156, Paid = true},
            new Invoice(){Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false},
            new Invoice(){Id = 15364, BillingName = "Jean", Amount = 156, Paid = true},
            new Invoice(){Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false},
            new Invoice(){Id = 15364, BillingName = "Jean", Amount = 156, Paid = true},
            new Invoice(){Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false},
    };

        private WindowNotificationManager notificationManager;
   

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                this.FindControl<DataGrid>("myDG").ItemsSource = liste;
                this.FindControl<DataGrid>("GridBilling").ItemsSource = liste;

                this.FindControl<Stepper>("stepS").Steps = new ObservableCollection<string>() { "Sent", "Progress", "Delivered" };
                this.FindControl<Stepper>("stepS").Index = 1;
           


                 this.FindControl<PropertyGrid>("propertyGrid").Item = new Person() { Adult = true, Age = 20,Name = "Billy", Partner = new Person() };

                 this.FindControl<ListBox>("listTest").ItemsSource = new ObservableCollection<string>() { "one", "two", "thre", "four", "five" };
                 
            }
            catch { }

           

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
            MessageBox.Info(this, "Info", "Here is a random Information message");
            
            
        }

        private void ShowDialog(object sender, RoutedEventArgs e)
        {
           InteractiveContainer.ShowDialog(new DialogContent());
        }
        private void ShowSuccessBox(object sender, RoutedEventArgs e)
        {
          
            MessageBox.Success(this, "Congratulations", "Here is a random success message, youpee !", WindowStartupLocation.CenterScreen);
        }

        private void ShowDangerBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Error(this, "Error", "This is an Error message that need to be read because it is dangerous to ... ");
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if(notificationManager == null)
                notificationManager = new WindowNotificationManager(this);
        }

        private void ShowNotification(object sender, RoutedEventArgs e)
        {
            try
            {
             /*   var notif = new Avalonia.Controls.Notifications.Notification("Info", "message");
                notificationManager.Position = NotificationPosition.BottomRight;
                notificationManager.Show(notif);

                notif = new Avalonia.Controls.Notifications.Notification("Error", "message", NotificationType.Error);
                notificationManager.Position = NotificationPosition.BottomRight;
                notificationManager.Show(notif);
                
                notif = new Avalonia.Controls.Notifications.Notification("Warning", "message", NotificationType.Warning);
                notificationManager.Position = NotificationPosition.BottomRight;
                notificationManager.Show(notif); */
                
                var notif = new Avalonia.Controls.Notifications.Notification("Success", "A new invoice has been created.", NotificationType.Success);
                notificationManager.Position = NotificationPosition.BottomRight;
                notificationManager.Show(notif);
            }catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        private void CloseHandler(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }

        private void Button_OnClickProgress(object? sender, RoutedEventArgs e)
        {
            this.FindControl<CircleProgressBar>("myProgressBar").Value = 50; 
            this.FindControl<TextBlock>("TextPercent").Text =  "50"; 
        }

        private void Button_OnClickProgressBar(object? sender, RoutedEventArgs e)
        {
            this.FindControl<ProgressBar>("myProgressBarLine").Value = 60;
            this.FindControl<PercentProgressBar>("myPercentProgress").Value = 100;

        }
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            this.FindControl<Stepper>("stepstep").Index--;
        }

        private void nextStep(object? sender, RoutedEventArgs e)
        {
            this.FindControl<Stepper>("stepstep").Index++;
        }

        private void ButtonChangeOpacity(object? sender, RoutedEventArgs e)
        {
            var loading = this.FindControl<Loading>("MyLoading");
            loading.Opacity = Math.Abs(loading.Opacity - 1);
        }

        private int i = 0;
        private void ChangeTheme(object? sender, RoutedEventArgs e)
        {
            if(i%2 == 0)
                SukiUI.ColorTheme.LoadDarkTheme(Application.Current);
            else
                SukiUI.ColorTheme.LoadLightTheme(Application.Current);
            
            i++;
        }

        private void BusyMe(object? sender, RoutedEventArgs e)
        {
            this.FindControl<BusyArea>("myBusyArea").IsBusy = true;
        }

        private void GoTo50(object? sender, RoutedEventArgs e)
        {
            this.FindControl<PercentProgressBar>("myPercentProgress").Value = 50;
        }

        private void SideMenu_OnMenuItemChanged(object sender, string header)
        {
           // MessageBox.Info(this,"title",header);
        }
        
        // Write a function that returns the sum of two numbers.

        private void ShowToast(object? sender, RoutedEventArgs e)
        {
            InteractiveContainer.ShowToast(new TextBlock(){Text = "Hello World !", Margin = new Thickness(15,8)}, 5);
        }

        private void SetBusy(object? sender, RoutedEventArgs e)
        {
           
            this.Get<Button>("ButtonSignIn").ShowProgress();
            
            Task.Run(() =>
            {
                Thread.Sleep(3000);
                Dispatcher.UIThread.Invoke(() =>
                {
                   
                    this.Get<TextBox>("PasswordTextBox").Error("Wrong Password");
                    this.Get<Button>("ButtonSignIn").HideProgress();
                });
            });
        }

        private void changepage(object? sender, RoutedEventArgs e)
        {
            var b = new Button(){Content = "go to"};
            b.Click += (o, args) =>
            {
               
                this.FindControl<StackPage>("StackSettings").Push("Wifi", new Grid() );
            };
            
            this.FindControl<StackPage>("StackSettings").Push("Network",b );
        }
    }
}
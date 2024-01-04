using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SukiUI.Controls;
using SukiUI.Theme;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SukiTest;

public class Invoice
{
    public int Id { get; set; } = 20;
    public string BillingName { get; set; }
    public int Amount { get; set; }
    public bool Paid { get; set; }
}

public enum Genders
{
    Male,
    Female
}

public class Person
{
    [Category("Data")] public string Name { get; set; } = "Charles";
    [Category("Data")] public int Age { get; set; } = 20;
    [Category("Data")] public bool Adult { get; set; } = true;

    public double JustADouble { get; set; } = 20;

    public DateTime Birth { get; set; } = DateTime.Now;
    public bool Woman { get; set; } = false;
    public bool Man { get; set; } = true;

    public Genders Genre { get; set; } = Genders.Male;

    public Person Partner { get; set; } = null;

    public List<int> ListOfInts { get; set; } = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
}

public partial class DashboardPage : UserControl
{
    private ObservableCollection<Invoice> liste { get; set; } = new ObservableCollection<Invoice>()
    {
        new Invoice() { Id = 15364, BillingName = "Jean", Amount = 156, Paid = true },
        new Invoice() { Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false },
        new Invoice() { Id = 15364, BillingName = "Jean", Amount = 156, Paid = true },
        new Invoice() { Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false },
        new Invoice() { Id = 15364, BillingName = "Jean", Amount = 156, Paid = true },
        new Invoice() { Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false },
    };

    public DashboardPage()
    {
        InitializeComponent();

        try
        {
            this.FindControl<DataGrid>("myDG").ItemsSource = liste;

            this.FindControl<Stepper>("stepS").Steps = new ObservableCollection<string>()
                { "Sent", "Progress", "Delivered" };
            this.FindControl<Stepper>("stepS").Index = 1;
        }
        catch
        {
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ShowDialog(object sender, RoutedEventArgs e)
    {
        SukiHost.ShowDialog(new DialogContent(), allowBackgroundClose: true);
        
    }

    private void CloseHandler(object sender, RoutedEventArgs e)
    {
        Window hostWindow = (Window)this.VisualRoot;
        hostWindow.Close();
    }

    private void Button_OnClickProgress(object? sender, RoutedEventArgs e)
    {
        this.FindControl<CircleProgressBar>("myProgressBar").Value = 50;
        this.FindControl<TextBlock>("TextPercent").Text = "50";
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

    private void SetBusy(object? sender, RoutedEventArgs e)
    {
        this.Get<ProgressBar>("P1").Value = 80;
        this.Get<ProgressBar>("P2").Value = 80;
        this.Get<ProgressBar>("P3").Value = 80;
        this.Get<CircleProgressBar>("CP").Value = 80;
        this.Get<TextBlock>("TP").Text = "80";
        
        this.Get<Button>("ButtonSignIn").ShowProgress();
        this.Get<BusyArea>("BusySignIn").IsBusy = true;

        Task.Run(() =>
        {
            Thread.Sleep(3000);
            Dispatcher.UIThread.Invoke(() =>
            {
                this.Get<TextBox>("PasswordTextBox").Error("Wrong Password");
                this.Get<Button>("ButtonSignIn").HideProgress();
                this.Get<BusyArea>("BusySignIn").IsBusy = false;
            });
        });
    }
}
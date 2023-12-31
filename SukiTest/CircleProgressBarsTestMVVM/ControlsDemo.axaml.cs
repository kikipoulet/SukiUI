using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SukiUI;
using SukiUI.Controls;

namespace SukiTest.CircleProgressBarsTestMVVM;

public partial class ControlsDemo : UserControl
{
    public ControlsDemo()
    {
        InitializeComponent();
        this.Get<DataGrid>("myDataGrid").ItemsSource = new ObservableCollection<Invoice>()
        {
            new Invoice() { Id = 15364, BillingName = "Jean", Amount = 156, Paid = true },
            new Invoice() { Id = 45689, BillingName = "Fantine", Amount = 82, Paid = false },
            new Invoice() { Id = 15364, BillingName = "Jean", Amount = 156, Paid = true },
        };
        this.Get<ItemsControl>("ThemeList").ItemsSource = SukiTheme.GetInstance().ColorThemes;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Get<BusyArea>("BA").IsBusy = true;

        Task.Run(() =>
        {
            Thread.Sleep(3500);
            
            Dispatcher.UIThread.Invoke(() =>
            {
                this.Get<BusyArea>("BA").IsBusy = false;
            });
        });
    }
}
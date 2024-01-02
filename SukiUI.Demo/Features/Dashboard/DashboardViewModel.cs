using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Models.Dashboard;

namespace SukiUI.Demo.Features.Dashboard;

public partial class DashboardViewModel : DemoPageBase
{
    [ObservableProperty] private bool _isLoggingIn;
    [ObservableProperty] private int _stepperIndex;

    public IReadOnlyList<Invoice> Invoices { get; } = new[]
    {
        new Invoice(15364, "Jean", 156, true),
        new Invoice(45689, "Fantine", 82, false),
        new Invoice(15364, "Jean", 156, true),
        new Invoice(45689, "Fantine", 82, false),
        new Invoice(15364, "Jean", 156, true),
        new Invoice(45689, "Fantine", 82, false),
    };

    public IReadOnlyList<string> Steps { get; } = new[]
    {
        "Dispatched", "En-Route", "Delivered"
    };

    public DashboardViewModel() : base("Dashboard", MaterialIconKind.CircleOutline, -100)
    {
        StepperIndex = 1;
    }

    public void Login()
    {
        IsLoggingIn = true;
        Task.Run(() =>
        {
            Thread.Sleep(3000);
            Dispatcher.UIThread.Invoke(() => { IsLoggingIn = false; });
        });
    }

    public void ShowDialog()
    {
        SukiHost.ShowDialog(new DialogViewModel(), allowBackgroundClose: true);
    }

    public void IncrementIndex() => 
        StepperIndex += StepperIndex >= Steps.Count - 1 ? 0 : 1;

    public void DecrementIndex() => 
        StepperIndex -= StepperIndex <= 0 ? 0 : 1;
}
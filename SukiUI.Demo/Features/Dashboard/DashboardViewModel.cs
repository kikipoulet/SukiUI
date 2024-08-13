using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using System.Threading.Tasks;
using SukiUI.Dialogs;

namespace SukiUI.Demo.Features.Dashboard;

public partial class DashboardViewModel : DemoPageBase
{
    [ObservableProperty] private bool _isLoggingIn;
    [ObservableProperty] private int _stepperIndex;

    public IAvaloniaReadOnlyList<InvoiceViewModel> Invoices { get; } = new AvaloniaList<InvoiceViewModel>()
    {
        new(15364, "Jean", 156, true),
        new(45689, "Fantine", 82, false),
        new(15364, "Jean", 156, true),
        new(45689, "Fantine", 82, false),
        new(15364, "Jean", 156, true),
        new(45689, "Fantine", 82, false),
    };

    public IAvaloniaReadOnlyList<string> Steps { get; } = new AvaloniaList<string>()
    {
        "Dispatched", "En-Route", "Delivered"
    };
    
    private readonly ISukiDialogManager _dialogManager;

    public DashboardViewModel(ISukiDialogManager dialogManager) : base("Dashboard", MaterialIconKind.CircleOutline, -100)
    {
        _dialogManager = dialogManager;
        StepperIndex = 1;
    }

    [RelayCommand]
    private Task Login()
    {
        IsLoggingIn = true;
        return Task.Run(async () =>
        {
            await Task.Delay(3000);
            IsLoggingIn = false;
        });
    }

    [RelayCommand]
    private void ShowDialog()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new DialogViewModel(dialog))
            .TryShow();
    }

    [RelayCommand]
    private void IncrementIndex() =>
        StepperIndex += StepperIndex >= Steps.Count - 1 ? 0 : 1;

    [RelayCommand]
    private void DecrementIndex() =>
        StepperIndex -= StepperIndex <= 0 ? 0 : 1;
}
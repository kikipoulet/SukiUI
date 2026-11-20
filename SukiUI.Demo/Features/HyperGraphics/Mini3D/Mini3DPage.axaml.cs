using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D;

public partial class Mini3DPage : UserControl
{
    private static readonly SynthConsoleModel SharedConsole = new();
    private static readonly Printer3DModel SharedPrinter = new();
    private static readonly SafeVaultModel SharedVault = new();

    private Mini3DViewModel? _vm;

    public Mini3DPage()
    {
        InitializeComponent();

        ConsoleView.Model = SharedConsole;
        PrinterView.Model = SharedPrinter;
        VaultView.Model = SharedVault;

        ConsoleView.Clicked += OnConsoleClicked;
        PrinterView.Clicked += OnPrinterClicked;
        VaultView.Clicked += OnVaultClicked;

        DataContextChanged += (_, _) => HookViewModel();
        HookViewModel();
    }

    private void HookViewModel()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm.VaultKeyPulsed -= OnVaultKeyPulsed;
        }
        _vm = DataContext as Mini3DViewModel;
        if (_vm == null) return;

        _vm.PropertyChanged += OnVmPropertyChanged;
        _vm.VaultKeyPulsed += OnVaultKeyPulsed;
        SyncAll(_vm, animate: false);
    }

    private void OnVaultKeyPulsed(string keyId)
    {
        VaultView.SetChannel(keyId, 1);
        DispatcherTimer.RunOnce(() => VaultView.SetChannel(keyId, 0), TimeSpan.FromMilliseconds(130));
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_vm == null) return;
        switch (e.PropertyName)
        {
            case nameof(Mini3DViewModel.IsPowered):
                ConsoleView.SetChannel("power", _vm.IsPowered ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.IsAutoRunning):
                ConsoleView.SetChannel("auto", _vm.IsAutoRunning ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.IsLidOpen):
                ConsoleView.SetChannel("lid", _vm.IsLidOpen ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.DialTurns):
                ConsoleView.SetChannel("dial", _vm.DialTurns);
                break;
            case nameof(Mini3DViewModel.Level):
                ConsoleView.SetChannel("level", _vm.Level);
                break;

            case nameof(Mini3DViewModel.IsPrinting):
                PrinterView.SetChannel("printing", _vm.IsPrinting ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.BedY):
                PrinterView.SetChannel("bedY", _vm.BedY);
                break;
            case nameof(Mini3DViewModel.ToolX):
                PrinterView.SetChannel("toolX", _vm.ToolX);
                break;
            case nameof(Mini3DViewModel.SpoolTurns):
                PrinterView.SetChannel("spool", _vm.SpoolTurns);
                break;
            case nameof(Mini3DViewModel.PrintProgress):
                PrinterView.SetChannel("progress", _vm.PrintProgress);
                break;

            case nameof(Mini3DViewModel.IsVaultUnlocked):
                VaultView.SetChannel("unlocked", _vm.IsVaultUnlocked ? 1 : 0);
                VaultView.SetChannel("wheelRot", _vm.IsVaultUnlocked ? 1 : 0);
                VaultView.SetChannel("door", _vm.IsVaultUnlocked ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.IsVaultError):
                VaultView.SetChannel("error", _vm.IsVaultError ? 1 : 0);
                break;
            case nameof(Mini3DViewModel.VaultPinEncoded):
                VaultView.SetChannel("pinState", _vm.VaultPinEncoded, animate: false);
                break;
        }
    }

    private void SyncAll(Mini3DViewModel vm, bool animate)
    {
        ConsoleView.SetChannel("power", vm.IsPowered ? 1 : 0, animate);
        ConsoleView.SetChannel("auto", vm.IsAutoRunning ? 1 : 0, animate);
        ConsoleView.SetChannel("lid", vm.IsLidOpen ? 1 : 0, animate);
        ConsoleView.SetChannel("dial", vm.DialTurns, animate);
        ConsoleView.SetChannel("level", vm.Level, animate);

        PrinterView.SetChannel("printing", vm.IsPrinting ? 1 : 0, animate);
        PrinterView.SetChannel("bedY", vm.BedY, animate);
        PrinterView.SetChannel("toolX", vm.ToolX, animate);
        PrinterView.SetChannel("spool", vm.SpoolTurns, animate);
        PrinterView.SetChannel("progress", vm.PrintProgress, animate);

        VaultView.SetChannel("unlocked", vm.IsVaultUnlocked ? 1 : 0, animate);
        VaultView.SetChannel("wheelRot", vm.IsVaultUnlocked ? 1 : 0, animate);
        VaultView.SetChannel("door", vm.IsVaultUnlocked ? 1 : 0, animate);
        VaultView.SetChannel("error", vm.IsVaultError ? 1 : 0, animate);
        VaultView.SetChannel("pinState", vm.VaultPinEncoded, animate: false);
    }

    private void OnConsoleClicked(object? sender, Model3DClickedEventArgs e)
    {
        if (_vm == null) return;
        switch (e.Id)
        {
            case "power":
                _vm.IsPowered = !_vm.IsPowered;
                break;
            case "auto":
                _vm.IsAutoRunning = !_vm.IsAutoRunning;
                break;
            case "lid":
                _vm.IsLidOpen = !_vm.IsLidOpen;
                break;
            case "dial":
                _vm.SpinDial();
                break;
        }
    }

    private void OnPrinterClicked(object? sender, Model3DClickedEventArgs e)
    {
        if (_vm == null) return;
        if (e.Id == "print")
            _vm.IsPrinting = !_vm.IsPrinting;
    }

    private void OnVaultClicked(object? sender, Model3DClickedEventArgs e)
    {
        _vm?.HandleVaultClick(e.Id);
    }
}

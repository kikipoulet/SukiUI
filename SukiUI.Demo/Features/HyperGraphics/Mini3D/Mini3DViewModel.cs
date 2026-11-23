using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D;

public partial class Mini3DViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private double _phase;

    [ObservableProperty] private int _selectedQuality = 50;

    // 1. Synth Console state
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PowerText))] private bool _isPowered = true;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AutoText))] private bool _isAutoRunning = true;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LidText))] private bool _isLidOpen = true;
    [ObservableProperty] private bool _isWireframe;
    [ObservableProperty] private double _dialTurns;
    [ObservableProperty] private double _level = 0.65;

    // 2. 3D Printer state
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PrinterText))] private bool _isPrinting = true;
    [ObservableProperty] private bool _printerWireframe;
    [ObservableProperty] private double _bedY = 0.5;
    [ObservableProperty] private double _toolX = 0.5;
    [ObservableProperty] private double _spoolTurns;
    [ObservableProperty] private double _printProgress = 0.35;

    // 3. Safe Vault state
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VaultStatusText))] private bool _isVaultUnlocked;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VaultStatusText))] private bool _isVaultError;
    [ObservableProperty] private bool _vaultWireframe;
    [ObservableProperty] private string _vaultDisplayHint = "_ _ _ _ (hint: 1337)";

    // 4. Sports Car state
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CarStatusText))] private bool _isCarLightsOn = true;
    [ObservableProperty] private bool _isCarAeroDeployed = true;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CarStatusText))] private bool _isCarDriving = true;
    [ObservableProperty] private bool _carWireframe;
    [ObservableProperty] private double _carWheelTurns;

    public Mini3DViewModel()
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(220), DispatcherPriority.Background, (_, _) => Tick());
        _timer.Start();
    }

    public string PowerText => IsPowered ? "Powered" : "Standby";
    public string AutoText => IsAutoRunning ? "Auto pulse" : "Idle";
    public string LidText => IsLidOpen ? "Open" : "Closed";
    public string PrinterText => IsPrinting ? "Printing" : "Paused";
    public string VaultStatusText => IsVaultUnlocked ? "Unlocked" : IsVaultError ? "Denied" : "Locked";
    public string CarStatusText => IsCarDriving ? "Cruising" : "Parked";

    private void Tick()
    {
        _phase += 0.35;

        if (IsPowered && IsAutoRunning)
        {
            Level = 0.25 + 0.72 * (0.5 + 0.5 * Math.Sin(_phase));
            DialTurns = (_phase / (2 * Math.PI)) % 1.0;
        }

        if (IsPrinting)
        {
            ToolX = 0.5 + 0.48 * Math.Sin(_phase * 1.15);
            BedY = 0.5 + 0.45 * Math.Cos(_phase * 0.72);
            SpoolTurns = (_phase * 0.14) % 1.0;
            PrintProgress = (_phase * 0.04) % 1.0;
        }

        if (IsCarDriving)
            CarWheelTurns = (_phase * 0.22) % 1.0;
    }

    [RelayCommand]
    private void LockVault() => IsVaultUnlocked = false;

    public string ModelSnippet { get; } = """
        // SynthConsole.Scene.cs (pure Model3D, zero Avalonia)
        public sealed class SynthConsoleScene : Model3D
        {
            protected override void Build(Scene3D s)
            {
                s.Camera(yaw: 32, pitch: 25, intro: -10);
                s.Ground(z: 0).Shadow((-6, -4), (246, 174));

                s.Input("power", ms: 260, initial: 1);
                s.Input("lid",   ms: 750, initial: 1);
                s.Input("dial",  ms: 650, initial: 0);
                s.Input("level", ms: 280, initial: 0.65);

                var deck = s.Box((0, 0, 6), (240, 170, 48), Rgb(232, 238, 248));
                deck.Face(Side.Top).DrawWith("level", (g, lvl) =>
                {
                    // Dynamic 2D canvas painted directly on top of the 3D face
                });

                var dial = s.Group().Pivot((182, 70, 56), Axis.Z)
                            .RotateWith("dial", 0, 360).Clickable("dial");
                dial.Extrude(new Path2D().Circle(182, 70, 24),
                             Axis.Z, 56, 73, Rgb(242, 246, 253));
            }
        }
        """;

    public string ViewSnippet { get; } = """
        // SynthConsole.cs (Avalonia contract)
        public sealed class SynthConsole : Model3DControl<SynthConsoleScene>
        {
            public static readonly StyledProperty<bool> IsLidOpenProperty =
                AvaloniaProperty.Register<SynthConsole, bool>(
                    nameof(IsLidOpen), true, defaultBindingMode: BindingMode.TwoWay);

            static SynthConsole() =>
                BindChannel<SynthConsole>(IsLidOpenProperty, "lid");

            public bool IsLidOpen
            {
                get => GetValue(IsLidOpenProperty);
                set => SetValue(IsLidOpenProperty, value);
            }

            protected override void OnModelClicked(string id)
            {
                if (id == "lid")
                    SetCurrentValue(IsLidOpenProperty, !IsLidOpen);
                base.OnModelClicked(id);
            }
        }
        """;
}

using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D;

public partial class Mini3DViewModel : ObservableObject
{
    private const string SecretPin = "1337";

    private readonly DispatcherTimer _timer;
    private double _phase;
    private string _pinBuffer = string.Empty;
    private int _pinErrorFlag;

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
    [ObservableProperty] private bool _isVaultError;
    [ObservableProperty] private bool _vaultWireframe;
    [ObservableProperty] private double _vaultPinEncoded;
    [ObservableProperty] private string _vaultDisplayHint = "_ _ _ _";

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
    }

    public event Action<string>? VaultKeyPulsed;

    public void SpinDial() => DialTurns = (DialTurns + 0.35) % 1.0;

    public void HandleVaultClick(string targetId)
    {
        if (targetId == "wheel")
        {
            if (IsVaultUnlocked) LockVault();
            return;
        }

        if (targetId.StartsWith("key_", StringComparison.Ordinal))
            VaultKeyPulsed?.Invoke(targetId);

        if (IsVaultUnlocked) return;

        if (targetId == "key_c")
        {
            ClearPin();
            return;
        }

        if (targetId == "key_ok")
        {
            SubmitPin();
            return;
        }

        if (targetId.StartsWith("key_", StringComparison.Ordinal) && targetId.Length == 5 && char.IsDigit(targetId[4]))
        {
            if (_pinBuffer.Length >= 4) _pinBuffer = string.Empty;
            _pinBuffer += targetId[4];
            _pinErrorFlag = 0;
            IsVaultError = false;
            UpdatePinEncoding();

            if (_pinBuffer.Length == 4)
                SubmitPin();
        }
    }

    [RelayCommand]
    private void LockVault()
    {
        IsVaultUnlocked = false;
        IsVaultError = false;
        _pinBuffer = string.Empty;
        _pinErrorFlag = 0;
        UpdatePinEncoding();
    }

    private void ClearPin()
    {
        _pinBuffer = string.Empty;
        _pinErrorFlag = 0;
        IsVaultError = false;
        UpdatePinEncoding();
    }

    private void SubmitPin()
    {
        if (_pinBuffer == SecretPin)
        {
            IsVaultUnlocked = true;
            IsVaultError = false;
            _pinErrorFlag = 0;
            UpdatePinEncoding();
            return;
        }

        _pinBuffer = string.Empty;
        _pinErrorFlag = 1;
        IsVaultError = true;
        UpdatePinEncoding();

        DispatcherTimer.RunOnce(() =>
        {
            if (!IsVaultUnlocked)
            {
                _pinErrorFlag = 0;
                IsVaultError = false;
                UpdatePinEncoding();
            }
        }, TimeSpan.FromMilliseconds(950));
    }

    private void UpdatePinEncoding()
    {
        var status = IsVaultUnlocked ? 2 : _pinErrorFlag;
        var len = _pinBuffer.Length;
        var digits = 0;
        for (var i = 0; i < len; i++)
        {
            var pow = i == 0 ? 1000 : i == 1 ? 100 : i == 2 ? 10 : 1;
            digits += (_pinBuffer[i] - '0') * pow;
        }

        VaultPinEncoded = status * 1_000_000 + len * 10_000 + digits;
        VaultDisplayHint = status == 2
            ? "OPEN (click wheel to lock)"
            : status == 1
                ? "ERR! Try 1337"
                : len == 0
                    ? "_ _ _ _ (hint: 1337)"
                    : $"{(len >= 1 ? _pinBuffer[0] : '_')} {(len >= 2 ? _pinBuffer[1] : '_')} {(len >= 3 ? _pinBuffer[2] : '_')} {(len >= 4 ? _pinBuffer[3] : '_')}";
    }

    public string ModelSnippet { get; } = """
        public sealed class SynthConsoleModel : Model3D
        {
            protected override void Build(Scene3D s)
            {
                s.Camera(yaw: 32, pitch: 26, intro: -10);
                s.Ground(z: 0).Shadow();

                s.Input("power", ms: 450, initial: 1);
                s.Input("lid",   ms: 750, initial: 1);
                s.Input("dial",  ms: 650, initial: 0);
                s.Input("level", ms: 320, initial: 0.65);

                var deck = s.Box((0, 0, 6), (240, 170, 48), Rgb(232, 238, 248));
                deck.Face(Side.Top).DrawWith("level", (g, lvl) =>
                {
                    // Dynamic 2D canvas painted directly on top of the 3D face
                });

                var dial = s.Group().Pivot((182, 60, 56), Axis.Z)
                            .RotateWith("dial", 0, 360).Clickable("dial");
                dial.Extrude(new Path2D().Arc(182, 60, 24, 0, 340, 18), Axis.Z, 56, 72, Rgb(242, 246, 253));
            }
        }
        """;

    public string ViewSnippet { get; } = """
        var view = new Model3DView
        {
            Model = new SynthConsoleModel(),
            IsInteractive = true,   // drag to orbit, double-click to recenter
            IsWireframe = false
        };

        view.Clicked += (_, e) =>
        {
            if (e.Id == "lid") isLidOpen = !isLidOpen;
            view.SetChannel("lid", isLidOpen ? 1 : 0);
        };
        """;
}

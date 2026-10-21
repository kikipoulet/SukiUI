using System;
using System.Collections.Generic;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism;

/// <summary>
/// Simulated devices of the Skeumorphism page. All the logic lives here (response curves, anti-spam, safety,
/// refusal of impossible commands); the skeumorphic controls only display values and report the user's intent.
/// </summary>
public partial class SkeumorphismViewModel : ObservableObject
{
    public SkeumorphismViewModel()
    {
        _deviceTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background, (_, _) => SimulateDevices());
        _deviceTimer.Start();
    }

    private void SimulateDevices()
    {
        var now = DateTime.UtcNow;
        var dt = Math.Min(0.1, (now - _lastTick).TotalSeconds);
        _lastTick = now;

        SimulateTank(dt);
        SimulateFan(dt);
        SimulateBattery(dt);
    }

    // ================================================================== Tank + spring lever

    private const double MaxRatePerSecond = 0.12;    // fraction of the tank per second at full opening
    private const double MinFlowChange = 0.03;       // anti-spam threshold on valve commands

    private readonly DispatcherTimer _deviceTimer;
    private DateTime _lastTick = DateTime.UtcNow;

    /// <summary>Lever intent: -1 = fill .. 1 = drain.</summary>
    [ObservableProperty] private double _leverValue;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LevelText))] private double _level = 0.7;
    [ObservableProperty] private double _inflow;
    [ObservableProperty] private double _outflow;

    public string LevelText => $"{Level * 100:0} %";

    /// <summary>Safety first: the valves close as soon as the user lets go of the lever.</summary>
    public void CloseValves()
    {
        Inflow = 0;
        Outflow = 0;
    }

    partial void OnLeverValueChanged(double value)
    {
        // Refuse commands that cannot be honoured: nothing to drain, or no room left.
        if (value == 0 || (value > 0 && Level <= 0) || (value < 0 && Level >= 1))
        {
            CloseValves();
            return;
        }

        var flow = value * Math.Abs(value);           // quadratic response: fine control near the dead zone
        var current = Outflow - Inflow;
        if (Math.Abs(flow - current) < MinFlowChange && Math.Abs(flow) < 1) return;

        Outflow = Math.Max(0, flow);
        Inflow = Math.Max(0, -flow);
    }

    private void SimulateTank(double dt)
    {
        if (Inflow == 0 && Outflow == 0) return;

        Level = Math.Clamp(Level + MaxRatePerSecond * (Inflow - Outflow) * dt, 0, 1);

        // The device closes its valves by itself at the limits.
        if (Level <= 0) Outflow = 0;
        if (Level >= 1) Inflow = 0;
    }

    // ================================================================== Camera + hinge slider

    /// <summary>Exposure notches in microseconds (1/3 stop).</summary>
    public IReadOnlyList<double> ExposureSteps { get; } =
        new double[] { 50, 75, 100, 150, 200, 300, 400, 600, 800, 1000, 1500, 2000, 3000, 4000, 6000, 8000, 10000 };

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ExposureText))] private double _exposure = 600;
    [ObservableProperty] private double _gain = 2;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FpsText))] private double _fps = 10;

    public string ExposureText => $"{Exposure:0.#} µs";
    public string FpsText => $"{Fps:0.##} fps";

    // ================================================================== Syringe + notch switch

    private static readonly double[] SpeedLevels = { 10, 50, 100 };   // % of the pump speed, per notch
    private const double FullStrokeSecondsAt50 = 4;                    // a full stroke at 50 % speed

    private bool _syringeMoving;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SyringeVolumeText))] private double _syringePosition = 0.3;
    [ObservableProperty] private double _syringeFlow;
    [ObservableProperty] private bool _isValveOut;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SyringeVolumeText))] private double _syringeVolume = 10;

    /// <summary>Selected speed notch: 0 = slow, 1 = medium, 2 = fast.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(SpeedText))] private int _speedIndex = 1;

    public string SyringeVolumeText => $"{SyringePosition * SyringeVolume:0.0} mL";
    public string SpeedText => $"{SpeedLevels[SpeedIndex]:0} %";

    /// <summary>
    /// The pump does not report its position while moving: the target is shown at once and the stream runs
    /// for the duration of the stroke. No new command is accepted during a movement.
    /// </summary>
    public void RequestSyringePosition(double target)
    {
        if (_syringeMoving) return;                   // refused: the syringe snaps back to the confirmed position
        target = Math.Round(target, 2);
        var delta = target - SyringePosition;
        if (Math.Abs(delta) < 0.005) return;

        var speed = SpeedLevels[SpeedIndex];
        var seconds = Math.Abs(delta) * FullStrokeSecondsAt50 * 50 / speed;

        SyringePosition = target;
        SyringeFlow = Math.Sign(delta) * speed / 100;  // positive = filling the barrel
        _syringeMoving = true;

        DispatcherTimer.RunOnce(() =>
        {
            SyringeFlow = 0;
            _syringeMoving = false;
        }, TimeSpan.FromSeconds(seconds));
    }

    /// <summary>The valve cannot turn while liquid is moving.</summary>
    public void ToggleValve()
    {
        if (_syringeMoving) return;
        IsValveOut = !IsValveOut;
    }

    // ================================================================== Fan + rotary knob

    private const double FanMaxRpm = 2400, FanRampPerSecond = 0.5;

    /// <summary>Knob setting: 0..10.</summary>
    [ObservableProperty] private double _fanSetting = 4;

    /// <summary>Motor speed reported by the fan (0..1): it ramps towards the setting, it never jumps.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FanRpmText))] private double _fanSpeed;

    public string FanRpmText => $"{Math.Round(FanSpeed * FanMaxRpm / 10) * 10:0} rpm";

    private void SimulateFan(double dt)
    {
        var target = FanSetting / 10;
        if (Math.Abs(target - FanSpeed) < 0.001) return;
        var step = FanRampPerSecond * dt;
        FanSpeed = Math.Abs(target - FanSpeed) <= step ? target : FanSpeed + Math.Sign(target - FanSpeed) * step;
    }

    // ================================================================== Battery + guarded toggle

    private const double ChargeRatePerSecond = 0.06, DrainRatePerSecond = 0.012;

    /// <summary>Charger switch set by the user.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsCharging), nameof(ChargeCurrent))] private bool _chargerOn;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ChargeText), nameof(IsCharging), nameof(ChargeCurrent))] private double _charge = 0.35;

    /// <summary>Current flows only while the charger is on and the cell is not full.</summary>
    public bool IsCharging => ChargerOn && Charge < 1;

    /// <summary>Charging current shown in the wires (0..1).</summary>
    public double ChargeCurrent => IsCharging ? 0.8 : 0;

    public string ChargeText => $"{Charge * 100:0} %";

    private void SimulateBattery(double dt)
    {
        // Charging when the switch is on, otherwise a small load slowly drains the cell
        var c = IsCharging ? Charge + ChargeRatePerSecond * dt : ChargerOn ? Charge : Charge - DrainRatePerSecond * dt;
        c = Math.Clamp(c, 0, 1);
        if (Math.Abs(c - Charge) > 1e-5) Charge = c;
    }

    // ================================================================== Stirrer + three-position selector

    private const double StirMaxRpm = 1500;
    private static readonly double[] StirSpeeds = { 0, 0.45, 1 };        // OFF / SLOW / FAST

    /// <summary>Selector position set by the user: 0 = off, 1 = slow, 2 = fast.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(StirSpeed), nameof(StirRpmText))] private int _stirMode;

    /// <summary>Speed delivered to the stirrer (0..1); its motor follows with its own inertia.</summary>
    public double StirSpeed => StirSpeeds[StirMode];

    public string StirRpmText => $"{Math.Round(StirSpeed * StirMaxRpm / 10) * 10:0} rpm";

    // ================================================================== Lamp + rocker switch

    /// <summary>Switch state set by the user.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(LampPower), nameof(LampText))] private bool _lampOn;

    /// <summary>Power delivered to the lamp through the wire (0 or 1); the filament warms up by itself.</summary>
    public double LampPower => LampOn ? 1 : 0;

    public string LampText => LampOn ? "On" : "Off";
}

using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Mini3D.Controls;

/// <summary>
/// Interactive armored PIN-code safe vault backed by <see cref="SafeVaultScene"/>.
/// Owns keypad input handling, keycap depression pulses, PIN validation, and door/wheel state;
/// 3D scene definition lives in <c>SafeVault.Scene.cs</c>.
/// </summary>
public sealed class SafeVault : Model3DControl<SafeVaultScene>
{
    public static readonly StyledProperty<string> SecretPinProperty =
        AvaloniaProperty.Register<SafeVault, string>(nameof(SecretPin), "1337");

    public static readonly StyledProperty<bool> IsUnlockedProperty =
        AvaloniaProperty.Register<SafeVault, bool>(nameof(IsUnlocked), false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsErrorProperty =
        AvaloniaProperty.Register<SafeVault, bool>(nameof(IsError), false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> PinReadoutProperty =
        AvaloniaProperty.Register<SafeVault, string>(nameof(PinReadout), "_ _ _ _ (hint: 1337)",
            defaultBindingMode: BindingMode.TwoWay);

    private string _pinBuffer = string.Empty;
    private int _pinErrorFlag;
    private double _pinEncoded;

    static SafeVault()
    {
        BindChannel<SafeVault>(IsUnlockedProperty, "unlocked");
        BindChannel<SafeVault>(IsUnlockedProperty, "wheelRot");
        BindChannel<SafeVault>(IsUnlockedProperty, "door");
        BindChannel<SafeVault>(IsErrorProperty, "error");

        IsUnlockedProperty.Changed.AddClassHandler<SafeVault>((v, e) => v.OnUnlockedChanged(e.GetNewValue<bool>()));
    }

    public string SecretPin
    {
        get => GetValue(SecretPinProperty);
        set => SetValue(SecretPinProperty, value);
    }

    public bool IsUnlocked
    {
        get => GetValue(IsUnlockedProperty);
        set => SetValue(IsUnlockedProperty, value);
    }

    public bool IsError
    {
        get => GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    public string PinReadout
    {
        get => GetValue(PinReadoutProperty);
        set => SetValue(PinReadoutProperty, value);
    }

    public void Lock() => SetCurrentValue(IsUnlockedProperty, false);

    protected override void SyncChannels()
    {
        base.SyncChannels();
        SetChannel("pinState", _pinEncoded, animate: false);
    }

    protected override void OnModelClicked(string id)
    {
        if (id == "wheel")
        {
            if (IsUnlocked) Lock();
            base.OnModelClicked(id);
            return;
        }

        if (id.StartsWith("key_", StringComparison.Ordinal))
        {
            SetChannel(id, 1);
            DispatcherTimer.RunOnce(() => SetChannel(id, 0), TimeSpan.FromMilliseconds(130));
        }

        if (!IsUnlocked)
        {
            if (id == "key_c")
                ClearPin();
            else if (id == "key_ok")
                SubmitPin();
            else if (id.StartsWith("key_", StringComparison.Ordinal) && id.Length == 5 && char.IsDigit(id[4]))
                AppendDigit(id[4]);
        }

        base.OnModelClicked(id);
    }

    private void OnUnlockedChanged(bool unlocked)
    {
        _pinErrorFlag = 0;
        if (!unlocked)
            _pinBuffer = string.Empty;
        SetCurrentValue(IsErrorProperty, false);
        UpdatePinState();
    }

    private void AppendDigit(char digit)
    {
        if (_pinBuffer.Length >= 4) _pinBuffer = string.Empty;
        _pinBuffer += digit;
        _pinErrorFlag = 0;
        SetCurrentValue(IsErrorProperty, false);
        UpdatePinState();

        if (_pinBuffer.Length == 4)
            SubmitPin();
    }

    private void ClearPin()
    {
        _pinBuffer = string.Empty;
        _pinErrorFlag = 0;
        SetCurrentValue(IsErrorProperty, false);
        UpdatePinState();
    }

    private void SubmitPin()
    {
        if (_pinBuffer == SecretPin)
        {
            _pinErrorFlag = 0;
            SetCurrentValue(IsErrorProperty, false);
            SetCurrentValue(IsUnlockedProperty, true);
            UpdatePinState();
            return;
        }

        _pinBuffer = string.Empty;
        _pinErrorFlag = 1;
        SetCurrentValue(IsErrorProperty, true);
        UpdatePinState();

        DispatcherTimer.RunOnce(() =>
        {
            if (!IsUnlocked && _pinErrorFlag == 1)
            {
                _pinErrorFlag = 0;
                SetCurrentValue(IsErrorProperty, false);
                UpdatePinState();
            }
        }, TimeSpan.FromMilliseconds(950));
    }

    private void UpdatePinState()
    {
        var status = IsUnlocked ? 2 : _pinErrorFlag;
        var len = _pinBuffer.Length;
        var digits = 0;
        for (var i = 0; i < len; i++)
        {
            var pow = i == 0 ? 1000 : i == 1 ? 100 : i == 2 ? 10 : 1;
            digits += (_pinBuffer[i] - '0') * pow;
        }

        _pinEncoded = status * 1_000_000 + len * 10_000 + digits;
        SetChannel("pinState", _pinEncoded, animate: false);

        var readout = status == 2
            ? "OPEN (click wheel to lock)"
            : status == 1
                ? $"ERR! Try {SecretPin}"
                : len == 0
                    ? $"_ _ _ _ (hint: {SecretPin})"
                    : $"{(len >= 1 ? _pinBuffer[0] : '_')} {(len >= 2 ? _pinBuffer[1] : '_')} {(len >= 3 ? _pinBuffer[2] : '_')} {(len >= 4 ? _pinBuffer[3] : '_')}";
        SetCurrentValue(PinReadoutProperty, readout);
    }
}

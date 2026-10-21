using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SukiUI.ControlsAnimation;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Diagnostics.CodeAnalysis;

namespace SukiUI.Demo.Features.Helpers;

public partial class AnimationProfileLab : UserControl
{
    private sealed record FamilyDef(
        string Name,
        Array Presets,
        Func<Enum, object> GetCalib,
        Func<Enum, object, SukiAnimationProfile> WithCalib,
        Func<SukiAnimationProfile, SukiAnimationProfile> ResetFamily);

    private static readonly FamilyDef[] Families =
    {
        new("Press", Enum.GetValues<SukiPressPreset>(),
            p => SukiAnimationTheme.Current.Press[(SukiPressPreset)(object)p],
            (p, c) => SukiAnimationTheme.Current with { Press = SukiAnimationTheme.Current.Press.With((SukiPressPreset)(object)p, (SukiPressProfile)c) },
            f => f with { Press = SukiAnimationProfile.Normal.Press }),
        new("Popup", Enum.GetValues<SukiPopupPreset>(),
            p => SukiAnimationTheme.Current.Popup[(SukiPopupPreset)(object)p],
            (p, c) => SukiAnimationTheme.Current with { Popup = SukiAnimationTheme.Current.Popup.With((SukiPopupPreset)(object)p, (SukiPopupProfile)c) },
            f => f with { Popup = SukiAnimationProfile.Normal.Popup }),
        new("Dialog", Enum.GetValues<SukiDialogPreset>(),
            p => SukiAnimationTheme.Current.Dialog[(SukiDialogPreset)(object)p],
            (p, c) => SukiAnimationTheme.Current with { Dialog = SukiAnimationTheme.Current.Dialog.With((SukiDialogPreset)(object)p, (SukiDialogProfile)c) },
            f => f with { Dialog = SukiAnimationProfile.Normal.Dialog }),
        new("Toggle", Enum.GetValues<SukiTogglePreset>(),
            p => SukiAnimationTheme.Current.Toggle[(SukiTogglePreset)(object)p],
            (p, c) => SukiAnimationTheme.Current with { Toggle = SukiAnimationTheme.Current.Toggle.With((SukiTogglePreset)(object)p, (SukiToggleProfile)c) },
            f => f with { Toggle = SukiAnimationProfile.Normal.Toggle }),
        new("Toast", Enum.GetValues<SukiToastPreset>(),
            p => SukiAnimationTheme.Current.Toast[(SukiToastPreset)(object)p],
            (p, c) => SukiAnimationTheme.Current with { Toast = SukiAnimationTheme.Current.Toast.With((SukiToastPreset)(object)p, (SukiToastProfile)c) },
            f => f with { Toast = SukiAnimationProfile.Normal.Toast }),
    };

    public sealed class ProfileField(string label, double value, double increment, Action<double> onChanged)
    {
        public string Label { get; } = label;
        public string? Description { get; init; }
        public double Increment { get; } = increment;
        public double Value
        {
            get => _v;
            set
            {
                _v = value;
                onChanged(value);
            }
        }

        private double _v = value;
    }

    private readonly SukiDialogManager _dialogs = new();
    private readonly SukiToastManager _toasts = new();
    private FamilyDef _family = Families[0];
    private Enum _preset = SukiPressPreset.Button;
    private ConstructorInfo _ctor = null!;
    private object?[] _args = [];

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SukiPressProfile))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SukiPopupProfile))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SukiDialogProfile))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SukiToggleProfile))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(SukiToastProfile))]
    public AnimationProfileLab()
    {
        InitializeComponent();
        DialogHost.Manager = _dialogs;
        ToastHost.Manager = _toasts;
        PresetTitle.Text = _family.Name;
        PresetBox.ItemsSource = _family.Presets;
        PresetBox.SelectedIndex = 0;
    }

    private void OnFamilyChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FamilyTabs is null || PresetBox is null || FamilyTabs.SelectedIndex < 0) return;
        _family = Families[FamilyTabs.SelectedIndex];
        PresetTitle.Text = _family.Name;
        PresetBox.ItemsSource = _family.Presets;
        PresetBox.SelectedIndex = 0;
    }

    private void OnPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PresetBox?.SelectedItem is not Enum preset) return;
        _preset = preset;
        LoadFields();
    }

    private void LoadFields()
    {
        var calib = _family.GetCalib(_preset);
        var type = calib.GetType();
        _ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var pars = _ctor.GetParameters();
        _args = new object?[pars.Length];
        var fields = new List<ProfileField>();
        for (var i = 0; i < pars.Length; i++)
        {
            var prop = type.GetProperty(pars[i].Name!)!;
            var v = prop.GetValue(calib);
            var desc = prop.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var idx = i;
            switch (v)
            {
                case TimeSpan ts:
                    _args[i] = ts;
                    fields.Add(new ProfileField(pars[i].Name! + " (ms)", ts.TotalMilliseconds, Inc(ts.TotalMilliseconds),
                        x => { _args[idx] = TimeSpan.FromMilliseconds(Math.Clamp(x, 0, 3.6e6)); Rebuild(); })
                        { Description = desc });
                    break;
                case int n:
                    _args[i] = n;
                    fields.Add(new ProfileField(pars[i].Name!, n, Inc(n),
                        x => { _args[idx] = (int)Math.Round(Math.Clamp(x, -1e9, 1e9)); Rebuild(); })
                        { Description = desc });
                    break;
                case double d:
                    _args[i] = d;
                    fields.Add(new ProfileField(pars[i].Name!, d, Inc(d),
                        x => { _args[idx] = x; Rebuild(); })
                        { Description = desc });
                    break;
                default:
                    // ponytail: Func<int,double> (CascadeStaggerMs) passed through untouched — add a real editor if it ever needs tuning
                    _args[i] = v;
                    break;
            }
        }

        FieldsList.ItemsSource = fields;
    }

    private void Rebuild() => SukiAnimationTheme.Use(_family.WithCalib(_preset, _ctor.Invoke(_args)));

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        SukiAnimationTheme.Use(_family.ResetFamily(SukiAnimationTheme.Current));
        LoadFields();
    }

    private void OnOpenDialog(object? sender, RoutedEventArgs e)
        => _dialogs.CreateDialog()
            .ShowCardBackground(true)
            .WithContent("Open, close, shake — driven by the Dialog profile.")
            .Dismiss().ByClickingBackground()
            .TryShow();

    private void OnShowToast(object? sender, RoutedEventArgs e)
        => _toasts.CreateToast()
            .WithTitle("Toast")
            .WithContent("Show, dismiss, pile — driven by the Toast profile.")
            .Dismiss().After(TimeSpan.FromSeconds(4))
            .Queue();

    private static double Inc(double v) => Math.Abs(v) switch { < 1 => 0.01, < 10 => 0.1, < 1000 => 1.0, _ => 10 };
}

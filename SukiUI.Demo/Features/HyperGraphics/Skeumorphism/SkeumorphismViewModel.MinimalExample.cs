using CommunityToolkit.Mvvm.ComponentModel;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism;

public partial class SkeumorphismViewModel
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(BeaconStatusText))] private bool _beaconActive = true;

    public string BeaconStatusText => BeaconActive ? "Active" : "Paused";

    public string MinimalCsCode { get; } = """
        using Avalonia;
        using Avalonia.Data;
        using Avalonia.Input;
        using SukiUI.HyperGraphics;

        namespace MyApp.Controls;

        public readonly record struct PulseBeaconState(bool IsActive);

        public partial class PulseBeacon : HyperControl<PulseBeaconState>
        {
            public static readonly StyledProperty<bool> IsActiveProperty =
                AvaloniaProperty.Register<PulseBeacon, bool>(
                    nameof(IsActive), true, defaultBindingMode: BindingMode.TwoWay);

            static PulseBeacon() => AffectsRenderState(IsActiveProperty);

            public bool IsActive
            {
                get => GetValue(IsActiveProperty);
                set => SetValue(IsActiveProperty, value);
            }

            protected override void OnPointerPressed(PointerPressedEventArgs e)
            {
                base.OnPointerPressed(e);
                SetCurrentValue(IsActiveProperty, !IsActive);
                e.Handled = true;
            }

            protected override PulseBeaconState CaptureState() => new(IsActive);
        }
        """;

    public string MinimalSvgCode { get; } = """"
        using SukiUI.HyperGraphics;

        namespace MyApp.Controls;

        public partial class PulseBeacon
        {
            private static readonly HyperSource SharedSource = HyperSource.Shared("""
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120">
                  <circle cx="60" cy="60" r="50" fill="var(--SkeumoBody)"
                          stroke="var(--SkeumoEdge)" stroke-width="1.2" filter="url(#skeumo-shadow)" />
                  <circle cx="60" cy="60" r="36" fill="var(--SkeumoDeepRecess)"
                          stroke="var(--SkeumoEdge)" stroke-width="0.8" />
                  <circle data-part="halo" cx="60" cy="60" r="28" fill="none"
                          stroke="var(--SukiPrimaryColor)" stroke-width="4" filter="url(#skeumo-glow)" />
                  <g data-part="rotor" data-pivot="60 60">
                    <rect x="57" y="28" width="6" height="22" rx="3" fill="var(--SukiPrimaryColor)" />
                    <circle cx="60" cy="60" r="10" fill="var(--SkeumoDarkKnob)" stroke="var(--SkeumoEdge)" stroke-width="0.8" />
                    <circle cx="60" cy="60" r="4" fill="var(--SukiPrimaryColor)" />
                  </g>
                </svg>
                """);

            protected override HyperSource Source => SharedSource;
        }
        """";

    public string MinimalRenderingCode { get; } = """
        using System;
        using SukiUI.HyperGraphics;

        namespace MyApp.Controls;

        public partial class PulseBeacon
        {
            protected override HyperStage CreateStage() => new Stage();

            private sealed class Stage : HyperStage
            {
                private HyperLayer _rotor = null!;
                private HyperLayer _halo = null!;
                private HyperSmooth _power;
                private double _angle;

                protected override void OnBind(IHyperBinder ui)
                {
                    _rotor = ui.Layer("rotor");
                    _halo = ui.Layer("halo");
                }

                protected override bool OnFrame(in HyperFrame frame, in PulseBeaconState state)
                {
                    _power.Follow(state.IsActive ? 1.0 : 0.0, in frame, 0.25);
                    _angle = (_angle + _power.Value * 140.0 * frame.Dt) % 360.0;

                    _rotor.Rotation = _angle;
                    _halo.Opacity = 0.15 + _power.Value * (0.45 + 0.40 * Math.Sin(frame.Seconds * 4.5));

                    return _power.Value > 0.002 || state.IsActive;
                }
            }
        }
        """;
}

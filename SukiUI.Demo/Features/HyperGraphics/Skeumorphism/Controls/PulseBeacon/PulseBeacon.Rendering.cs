using System;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

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

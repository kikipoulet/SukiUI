using System;

namespace SukiUI.ControlsAnimation
{
    public enum SukiTogglePreset
    {
        ToggleSwitch
    }

    public sealed record SukiToggleProfile(
        // distance in DIPs between the knob's rest poses (the template's knob travel)
        double Travel,
        // angular frequency of the toggle spring (the snap)
        double SpringOmega,
        // damping of the toggle spring (higher = less overshoot)
        double SpringDecay,
        // motion blur: multiplier applied to the knob's |velocity| in DIP/s
        double BlurFactor,
        // motion blur: cap on the smear radius
        double MaxBlur,
        // squash & stretch: scale delta per DIP/s of the knob's |velocity|
        double SquashFactor,
        // squash & stretch: cap on the stretch (and squeeze) delta
        double MaxSquash)
    {
        #region Normal

        // The knob is a physical puck: it snaps between its rest poses with a slight
        // overshoot, smearing and stretching while fast, crisp and round at rest.
        public static readonly SukiToggleProfile ToggleSwitch = new(
            Travel: 18.0,
            SpringOmega: 24.0,
            SpringDecay: 33.0,
            BlurFactor: 0.015,
            MaxBlur: 6.0,
            SquashFactor: 0.0004,
            MaxSquash: 0.18);

        public static readonly SukiPresetTable<SukiTogglePreset, SukiToggleProfile> Normal =
            new((SukiTogglePreset.ToggleSwitch, ToggleSwitch));

        #endregion

        #region Lite

        // Sober snap: critically damped, no smear, no squash.
        public static readonly SukiToggleProfile ToggleSwitchLite = ToggleSwitch with
        {
            SpringDecay = 48.0,
            BlurFactor = 0.0,
            SquashFactor = 0.0,
        };

        public static readonly SukiPresetTable<SukiTogglePreset, SukiToggleProfile> Lite =
            new((SukiTogglePreset.ToggleSwitch, ToggleSwitchLite));

        #endregion
    }
}

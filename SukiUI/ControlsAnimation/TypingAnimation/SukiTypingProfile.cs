using System.ComponentModel;

namespace SukiUI.ControlsAnimation
{
    public enum SukiTypingPreset
    {
        TextBox
    }

    public sealed record SukiTypingProfile(
        [property: Description("velocity kick added per keystroke (intensity units per second)")]
        double Kick,
        [property: Description("angular frequency of the typing-intensity spring")]
        double SpringOmega,
        [property: Description("damping of the typing-intensity spring (higher = less overshoot)")]
        double SpringDecay)
    {
        #region Normal

        // The border is an OVERDAMPED struck spring (zeta = Decay / (2·Omega) > 1): a
        // keystroke is nearly invisible the instant it lands and builds into a delayed
        // hump that merges with the following keystrokes — the intensity climbs slowly
        // with typing frequency (equilibrium offset ~ Kick·rate / Omega² over the 0.5
        // rest point) and settles back over ~0.7s when typing stops. No per-key flash,
        // no discrete steps, no timers.
        public static readonly SukiTypingProfile TextBox = new(
            Kick: 1.2,
            SpringOmega: 3.5,
            SpringDecay: 9.0);

        public static readonly SukiPresetTable<SukiTypingPreset, SukiTypingProfile> Normal =
            new((SukiTypingPreset.TextBox, TextBox));

        #endregion

        #region Lite

        // Sober: no kick — the border only fades to 50% on focus, no typing dynamics.
        public static readonly SukiTypingProfile TextBoxLite = TextBox with { Kick = 0.0 };

        public static readonly SukiPresetTable<SukiTypingPreset, SukiTypingProfile> Lite =
            new((SukiTypingPreset.TextBox, TextBoxLite));

        #endregion
    }
}

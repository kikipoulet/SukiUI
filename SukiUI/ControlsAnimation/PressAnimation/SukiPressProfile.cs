using System;

namespace SukiUI.ControlsAnimation
{
    public enum SukiPressPreset
    {
        Button,
        ComboBox
    }

    public sealed record SukiPressProfile(
        // scale the control eases toward while the pointer hovers it
        double HoverScale,
        // how far a held press sinks past the press depth (deep floor = DefaultPressDepth - ExtraDeepRange)
        double ExtraDeepRange,
        // duration of the initial press-down
        TimeSpan PressDuration,
        // duration of the long-press stretch down to the deep floor
        TimeSpan DeepDuration,
        // duration of the hover settle ramp
        TimeSpan HoverDuration,
        // angular frequency of the release spring (how quick the rebound is)
        double SpringOmega,
        // damping of the release spring (higher = less bounce)
        double SpringDecay,
        // press scale used when SukiPressMotion.PressDepth is not set
        double DefaultPressDepth)
    {
        #region Normal

        // Lively deep press down to 0.87, springs back with a full yo-yo overshoot.
        public static readonly SukiPressProfile Button = new(
            HoverScale: 1.02,
            ExtraDeepRange: 0.09,
            PressDuration: TimeSpan.FromMilliseconds(150),
            DeepDuration: TimeSpan.FromSeconds(2),
            HoverDuration: TimeSpan.FromMilliseconds(150),
            SpringOmega: 16.0,
            SpringDecay: 9.333,
            DefaultPressDepth: 0.96);

        // Soft, shallow press with a barely-there damped rebound.
        public static readonly SukiPressProfile ComboBox = new(
            HoverScale: 1.02,
            ExtraDeepRange: 0.042,
            PressDuration: TimeSpan.FromMilliseconds(150),
            DeepDuration: TimeSpan.FromSeconds(2),
            HoverDuration: TimeSpan.FromMilliseconds(150),
            SpringOmega: 12.0,
            SpringDecay: 13.5,
            DefaultPressDepth: 0.982);

        public static readonly SukiPresetTable<SukiPressPreset, SukiPressProfile> Normal =
            new((SukiPressPreset.Button, Button), (SukiPressPreset.ComboBox, ComboBox));

        #endregion

        #region Lite

        // Sober button feel: no hover, quick press, fast near-bounce-free release.
        public static readonly SukiPressProfile ButtonLite = Button with
        {
            HoverScale = 1.0,
            PressDuration = TimeSpan.FromMilliseconds(80),
            HoverDuration = TimeSpan.FromMilliseconds(80),
            SpringDecay = 30.4,
        };

        // Sober combo feel: no hover, quick, critically damped so it never overshoots.
        public static readonly SukiPressProfile ComboBoxLite = ComboBox with
        {
            HoverScale = 1.0,
            PressDuration = TimeSpan.FromMilliseconds(80),
            HoverDuration = TimeSpan.FromMilliseconds(80),
            SpringOmega = 16.0,
            SpringDecay = 30.4,
        };

        public static readonly SukiPresetTable<SukiPressPreset, SukiPressProfile> Lite =
            new((SukiPressPreset.Button, ButtonLite), (SukiPressPreset.ComboBox, ComboBoxLite));

        #endregion
    }
}

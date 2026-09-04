using System;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// XAML-selectable press feels (<c>SukiPress.Preset="..."</c>). Each member resolves
    /// to a calibrated <see cref="SukiPressProfile"/> instance.
    /// </summary>
    public enum SukiPressPreset
    {
        Button,
        ComboBox
    }

    /// <summary>
    /// Calibrated constants of the press chain, one profile per <see cref="SukiPressPreset"/>.
    /// The deep floor of a profile is <c>DefaultPressDepth - ExtraDeepRange</c>; for the combo
    /// that reproduces its fixed 0.94 floor, for the button its historical depth-minus-0.09.
    /// </summary>
    public sealed record SukiPressProfile(
        double HoverScale,
        double ExtraDeepRange,
        TimeSpan PressDuration,
        TimeSpan DeepDuration,
        TimeSpan HoverDuration,
        double SpringOmega,
        double SpringDecay,
        double DefaultPressDepth)
    {
        /// <summary>
        /// Button feel: depth 0.96, deep floor 0.87, release spring omega 16 / decay 9.333
        /// (zeta ~0.29 — a full lively yo-yo).
        /// </summary>
        public static readonly SukiPressProfile Button = new(
            HoverScale: 1.02,
            ExtraDeepRange: 0.09,
            PressDuration: TimeSpan.FromMilliseconds(150),
            DeepDuration: TimeSpan.FromSeconds(2),
            HoverDuration: TimeSpan.FromMilliseconds(150),
            SpringOmega: 16.0,
            SpringDecay: 9.333,
            DefaultPressDepth: 0.96);

        /// <summary>
        /// ComboBox feel: softer across the board — depth 0.982, floor 0.94, slower more
        /// damped rebound (omega 12 / decay 13.5, zeta ~0.56 — a barely-there overshoot).
        /// </summary>
        public static readonly SukiPressProfile ComboBox = new(
            HoverScale: 1.02,
            ExtraDeepRange: 0.042, // 0.982 - 0.042 = 0.94, the historical fixed floor
            PressDuration: TimeSpan.FromMilliseconds(150),
            DeepDuration: TimeSpan.FromSeconds(2),
            HoverDuration: TimeSpan.FromMilliseconds(150),
            SpringOmega: 12.0,
            SpringDecay: 13.5,
            DefaultPressDepth: 0.982);

        /// <summary>
        /// Resolves the XAML-selectable preset to its calibrated profile. Every enum
        /// member is listed explicitly — when adding a member, add its case here and
        /// keep names and calibrations in sync; invalid enum values throw.
        /// </summary>
#pragma warning disable CS8524 // the exhaustive member list is intentional; unnamed enum values throw
        public static SukiPressProfile For(SukiPressPreset preset) => preset switch
        {
            SukiPressPreset.Button => Button,
            SukiPressPreset.ComboBox => ComboBox,
        };
#pragma warning restore CS8524
    }
}

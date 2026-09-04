using System;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// XAML-selectable popup feels (<c>SukiPopupAnimation.Preset="..."</c>). Each member
    /// resolves to a calibrated <see cref="SukiPopupProfile"/> instance.
    /// </summary>
    public enum SukiPopupPreset
    {
        ComboBox
    }

    /// <summary>
    /// Calibrated constants of the popup open/close animation chain, one profile per
    /// <see cref="SukiPopupPreset"/>. The open/close motion is a pair of damped springs
    /// (scale X/Y) sharing zeta = 0.65 — a single subtle overshoot (~7% of travel) and a
    /// very smooth settle — plus an opacity lerp, a velocity-driven motion blur on open
    /// (dissolution blur on close) and a staggered item cascade.
    /// </summary>
    public sealed record SukiPopupProfile(
        double ClosedScaleX,
        double ClosedScaleY,
        double CloseScaleX,
        double CloseScaleY,
        double OpenSpringOmega,
        double OpenSpringDecay,
        double CloseSpringOmega,
        double CloseSpringDecay,
        TimeSpan OpenOpacityDuration,
        TimeSpan CloseOpacityDuration,
        double BlurFactor,
        double MaxBlurRadius,
        double CloseBlurRadius,
        double CascadeInitialDelayMs,
        double CascadeDurationMs,
        int CascadeMaxItems,
        Func<int, double> CascadeStaggerMs)
    {
        /// <summary>
        /// ComboBox feel: the historical calibration of the SukiUI drop-down — open spring
        /// omega 16 / decay 20.8 (zeta 0.65), close-only collapse target only 40% of the open
        /// travel with a spring ~40% faster (omega 26.7 / decay 34.7, same zeta).
        /// </summary>
        public static readonly SukiPopupProfile ComboBox = new(
            ClosedScaleX: 0.92,
            ClosedScaleY: 0.72,
            CloseScaleX: 0.968,
            CloseScaleY: 0.888,
            OpenSpringOmega: 16.0,
            OpenSpringDecay: 20.8,
            CloseSpringOmega: 26.7,
            CloseSpringDecay: 34.7,
            OpenOpacityDuration: TimeSpan.FromMilliseconds(350),
            CloseOpacityDuration: TimeSpan.FromMilliseconds(150),
            BlurFactor: 4.0,
            MaxBlurRadius: 12.0,
            CloseBlurRadius: 20.0,
            CascadeInitialDelayMs: 150,
            CascadeDurationMs: 250,
            CascadeMaxItems: 20,
            CascadeStaggerMs: count => count switch
            {
                < 4 => 40.0,
                > 10 => 20.0,
                _ => 40.0 - (count - 4) * (40.0 - 20.0) / (10.0 - 4.0)
            });

        /// <summary>
        /// Resolves the XAML-selectable preset to its calibrated profile. Every enum
        /// member is listed explicitly — when adding a member, add its case here and
        /// keep names and calibrations in sync; invalid enum values throw.
        /// </summary>
#pragma warning disable CS8524 // the exhaustive member list is intentional; unnamed enum values throw
        public static SukiPopupProfile For(SukiPopupPreset preset) => preset switch
        {
            SukiPopupPreset.ComboBox => ComboBox,
        };
#pragma warning restore CS8524
    }
}

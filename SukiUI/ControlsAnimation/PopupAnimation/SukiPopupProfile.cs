using System;
using System.ComponentModel;

namespace SukiUI.ControlsAnimation
{
    public enum SukiPopupPreset
    {
        ComboBox,
        ContextMenu,
        Menu
    }

    public sealed record SukiPopupProfile(
        [property: Description("scale the popup is collapsed to on X just before opening")]
        double ClosedScaleX,
        [property: Description("scale the popup is collapsed to on Y just before opening")]
        double ClosedScaleY,
        [property: Description("scale the popup collapses toward on X at the end of the close")]
        double CloseScaleX,
        [property: Description("scale the popup collapses toward on Y at the end of the close")]
        double CloseScaleY,
        [property: Description("angular frequency of the open spring")]
        double OpenSpringOmega,
        [property: Description("damping of the open spring")]
        double OpenSpringDecay,
        [property: Description("angular frequency of the close spring")]
        double CloseSpringOmega,
        [property: Description("damping of the close spring")]
        double CloseSpringDecay,
        [property: Description("opacity fade-in duration on open")]
        TimeSpan OpenOpacityDuration,
        [property: Description("opacity fade-out duration on close")]
        TimeSpan CloseOpacityDuration,
        [property: Description("motion blur: multiplier applied to the popup's own expansion speed")]
        double BlurFactor,
        [property: Description("motion blur: cap on the open blur radius")]
        double MaxBlurRadius,
        [property: Description("motion blur: fixed radius held while the popup dissolves on close")]
        double CloseBlurRadius,
        [property: Description("cascade: delay before the first item starts fading in (ms)")]
        double CascadeInitialDelayMs,
        [property: Description("cascade: per-item appearance duration — the opacity 0→1 of EACH item (ms)")]
        double CascadeDurationMs,
        [property: Description("cascade: blur radius each item materializes from, reaching 0 at 70% of the appearance duration (0 = plain opacity fade)")]
        double CascadeItemBlur,
        [property: Description("cascade: vertical distance each item travels up to its resting pose (0 = none)")]
        double CascadeItemOffsetY,
        [property: Description("cascade: above this many items the cascade is skipped")]
        int CascadeMaxItems,
        [property: Description("cascade: per-item fade-in stagger as a function of the item count (ms)")]
        Func<int, double> CascadeStaggerMs)
    {
        #region Normal

        // Softly bouncy open with a subtle overshoot, quicker partial collapse on close, motion blur on the spring speed, and a staggered item cascade.
        public static readonly SukiPopupProfile ComboBox = new(
            ClosedScaleX: 0.92,
            ClosedScaleY: 0.72,
            CloseScaleX: 0.968,
            CloseScaleY: 0.888,
            OpenSpringOmega: 20.0,
            OpenSpringDecay: 40.8,
            CloseSpringOmega: 26.7,
            CloseSpringDecay: 34.7,
            OpenOpacityDuration: TimeSpan.FromMilliseconds(250),
            CloseOpacityDuration: TimeSpan.FromMilliseconds(150),
            BlurFactor: 4.0,
            MaxBlurRadius: 12.0,
            CloseBlurRadius: 20.0,
            CascadeInitialDelayMs: 150,
            CascadeDurationMs: 140,
            CascadeItemBlur: 20.0,
            CascadeItemOffsetY: 7.0,
            CascadeMaxItems: 20,
            CascadeStaggerMs: count => count switch
            {
                < 4 => 40.0,
                > 10 => 20.0,
                _ => 40.0 - (count - 4) * (40.0 - 20.0) / (10.0 - 4.0)
            });

        // Pure copies of the ComboBox calibrations: separate presets only so each host's
        // feel can be tuned independently through SukiAnimationTheme — the templates'
        // RenderTransformOrigin (0,0 beside the parent item, 50%,0 under the bar) already
        // gives the menus their growth direction.
        public static readonly SukiPopupProfile ContextMenu = ComboBox;
        public static readonly SukiPopupProfile Menu = ComboBox;

        public static readonly SukiPresetTable<SukiPopupPreset, SukiPopupProfile> Normal =
            new((SukiPopupPreset.ComboBox, ComboBox), (SukiPopupPreset.ContextMenu, ContextMenu), (SukiPopupPreset.Menu, Menu));

        #endregion

        #region Lite

        // Critically damped open (no overshoot), shorter fades, no blur, and every item appears at once.
        public static readonly SukiPopupProfile ComboBoxLite = ComboBox with
        {
            ClosedScaleX= 0.96,
            ClosedScaleY= 0.92,
            CloseScaleX= 0.988,
            CloseScaleY= 0.928,
            OpenSpringOmega = 20.0,
            OpenSpringDecay = 40.0,
            CloseSpringOmega = 26.7,
            CloseSpringDecay = 53.4,
            OpenOpacityDuration = TimeSpan.FromMilliseconds(200),
            CloseOpacityDuration = TimeSpan.FromMilliseconds(200),
            BlurFactor = 0.0,
            MaxBlurRadius = 0.0,
            CloseBlurRadius = 0.0,
            CascadeInitialDelayMs = 0.0,
            CascadeItemBlur = 0.0,
            CascadeItemOffsetY = 0.0,
            CascadeMaxItems = 0,
        };

        public static readonly SukiPresetTable<SukiPopupPreset, SukiPopupProfile> Lite =
            new((SukiPopupPreset.ComboBox, ComboBoxLite), (SukiPopupPreset.ContextMenu, ComboBoxLite), (SukiPopupPreset.Menu, ComboBoxLite));

        #endregion
    }
}

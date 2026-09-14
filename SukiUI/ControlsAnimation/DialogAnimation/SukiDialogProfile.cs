using System.ComponentModel;

namespace SukiUI.ControlsAnimation
{
    public enum SukiDialogPreset
    {
        Default
    }

    public sealed record SukiDialogProfile(
        [property: Description("dialog area at or below which the responsive spring opens on its fastest, snappiest calibration")]
        double SmallDialogArea,
        [property: Description("dialog area at or above which it opens on its slowest, most damped calibration")]
        double LargeDialogArea,
        [property: Description("exponent curving the size -> damping ramp (higher keeps mid-size dialogs bouncier)")]
        double DampingCurveExponent,
        [property: Description("how far below its rest place the dialog rises from on open (and sinks back to on close)")]
        double EmergenceVertical,
        [property: Description("how far toward the summoning click the emerge can pull the open horizontally")]
        double EmergenceHorizontalMax,
        [property: Description("open transform duration for a small dialog (ms)")]
        double OpenTransformDurationSmallMs,
        [property: Description("open transform duration for a large dialog (ms)")]
        double OpenTransformDurationLargeMs,
        [property: Description("open spring omega for a small dialog")]
        double OpenOmegaSmall,
        [property: Description("open spring omega for a large dialog")]
        double OpenOmegaLarge,
        [property: Description("damping ratio for a small dialog at the open (below 1 lets it bounce, 1 is critically damped)")]
        double OpenZetaSmall,
        [property: Description("damping ratio for a large dialog at the open (above 1 settles it with no rebound)")]
        double OpenZetaLarge,
        [property: Description("scale the small dialog opens from")]
        double OpenFromScaleSmall,
        [property: Description("scale the large dialog opens from")]
        double OpenFromScaleLarge,
        [property: Description("opacity fade-in duration on open (ms)")]
        int OpenOpacityDurationMs,
        [property: Description("scale the dialog closes toward (sinks back below the rest pose)")]
        double CloseScale,
        [property: Description("depth-of-field blur radius at the blurred start/end of life (0 = no blur)")]
        double BlurredRadius,
        [property: Description("blur transition duration on the dialog surface (ms)")]
        int SurfaceTransitionDurationMs,
        [property: Description("backdrop glass fade duration (ms)")]
        int GlassFadeMilliseconds,
        [property: Description("angular frequency of the pinned-dialog shake spring")]
        double ShakeOmega,
        [property: Description("damping of the pinned-dialog shake spring (higher = fewer swings)")]
        double ShakeDecay,
        [property: Description("initial velocity kicked into a pinned-dialog shake")]
        double ShakeImpulse,
        [property: Description("distance from rest below which a shake stops (px)")]
        double ShakeSettleDelta,
        [property: Description("speed below which a shake stops (px per second)")]
        double ShakeSettleVelocity)
    {
        #region Normal

        // Small dialogs bounce lively (~14% overshoot), large ones land heavy with no rebound; a pinned shake swings ~4 visible times.
        public static readonly SukiDialogProfile Default = new(
            SmallDialogArea: 48_000.0,
            LargeDialogArea: 346_000.0,
            DampingCurveExponent: 1.6,
            EmergenceVertical: 100.0,
            EmergenceHorizontalMax: 50.0,
            OpenTransformDurationSmallMs: 650.0,
            OpenTransformDurationLargeMs: 400.0,
            OpenOmegaSmall: 10.4,
            OpenOmegaLarge: 5.8,
            OpenZetaSmall: 0.53,
            OpenZetaLarge: 1.05,
            OpenFromScaleSmall: 0.72,
            OpenFromScaleLarge: 0.86,
            OpenOpacityDurationMs: 300,
            CloseScale: 0.8,
            BlurredRadius: 40.0,
            SurfaceTransitionDurationMs: 250,
            GlassFadeMilliseconds: 300,
            ShakeOmega: 15.0,
            ShakeDecay: 9.0,
            ShakeImpulse: 320.0,
            ShakeSettleDelta: 0.5,
            ShakeSettleVelocity: 10.0);

        public static readonly SukiPresetTable<SukiDialogPreset, SukiDialogProfile> Normal =
            new((SukiDialogPreset.Default, Default));

        #endregion

        #region Lite

        // Short, tight rise with no rebound, no blur and quick fades; the pinned shake is snappier and dies faster.
        public static readonly SukiDialogProfile DefaultLite = Default with
        {
            EmergenceVertical = 24.0,
            EmergenceHorizontalMax = 1,
            OpenTransformDurationSmallMs = 500.0,
            OpenTransformDurationLargeMs = 400.0,
            OpenZetaSmall = 1.0,
            OpenZetaLarge = 1.3,
            OpenFromScaleSmall = 0.9,
            OpenFromScaleLarge = 0.95,
            OpenOpacityDurationMs = 150,
            BlurredRadius = 0.0,
            SurfaceTransitionDurationMs = 250,
            GlassFadeMilliseconds = 220,
            ShakeDecay = 24.0,
            ShakeImpulse = 200.0,
        };

        public static readonly SukiPresetTable<SukiDialogPreset, SukiDialogProfile> Lite =
            new((SukiDialogPreset.Default, DefaultLite));

        #endregion
    }
}

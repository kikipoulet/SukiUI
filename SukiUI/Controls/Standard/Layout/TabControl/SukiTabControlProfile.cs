using System;
using System.ComponentModel;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    public enum SukiTabControlPreset
    {
        TabControl
    }

    public sealed record SukiTabControlProfile(
        // ---- hover & press ----
        [property: Description("duration of the hover plate fading in and out")]
        TimeSpan HoverDuration,
        [property: Description("scale the header sinks to while pressed")]
        double PressScale,
        [property: Description("duration of the press-down")]
        TimeSpan PressDuration,
        [property: Description("angular frequency of the spring that brings the header back after a press")]
        double ReleaseOmega,
        [property: Description("damping of the release spring (higher = less rebound)")]
        double ReleaseDecay,

        // ---- the indicator (underline, pill or glass) ----
        [property: Description("angular frequency of the indicator's leading edge (the side it travels toward)")]
        double LeadOmega,
        [property: Description("damping of the leading edge")]
        double LeadDecay,
        [property: Description("angular frequency of the trailing edge; slower than the lead, it stretches the indicator on the way")]
        double TrailOmega,
        [property: Description("damping of the trailing edge")]
        double TrailDecay,
        [property: Description("duration of the indicator fading in or out (first selection, no selection)")]
        TimeSpan IndicatorFade,

        // ---- the page ----
        [property: Description("duration of the page sliding in (and of the old one sliding out)")]
        TimeSpan ContentDuration,
        [property: Description("the curve of the page sliding")]
        SukiEase ContentEase,
        [property: Description("distance the new page comes from, on the side of the new tab (0 for a plain fade)")]
        double ContentOffset,
        [property: Description("duration of the new page fading in")]
        TimeSpan ContentFadeIn,
        [property: Description("duration of the old page fading out")]
        TimeSpan ContentFadeOut)
    {
        public Spring Release => new(ReleaseOmega, ReleaseDecay);
        public Spring Lead => new(LeadOmega, LeadDecay);
        public Spring Trail => new(TrailOmega, TrailDecay);

        #region Normal

        // One indicator travels from tab to tab: its leading edge leaves first and the trailing
        // edge follows, so it stretches on the way and gathers on arrival, with a light rebound.
        // The tabs themselves never move. A header lights up under the pointer, sinks on press
        // and springs back; the new page slides in from the side of the new tab while the old
        // one slips out the other way.
        public static readonly SukiTabControlProfile TabControl = new(
            HoverDuration: TimeSpan.FromMilliseconds(140),
            PressScale: 0.95,
            PressDuration: TimeSpan.FromMilliseconds(90),
            ReleaseOmega: 16.0,
            ReleaseDecay: 11.0,
            LeadOmega: 26.0,
            LeadDecay: 40.0,
            TrailOmega: 20.0,
            TrailDecay: 30.0,
            IndicatorFade: TimeSpan.FromMilliseconds(160),
            ContentDuration: TimeSpan.FromMilliseconds(380),
            ContentEase: SukiEase.DesignLibraries.Material3.EmphasizedDecelerate,
            ContentOffset: 28.0,
            ContentFadeIn: TimeSpan.FromMilliseconds(220),
            ContentFadeOut: TimeSpan.FromMilliseconds(120));

        public static readonly SukiPresetTable<SukiTabControlPreset, SukiTabControlProfile> Normal =
            new((SukiTabControlPreset.TabControl, TabControl));

        #endregion

        #region Lite

        // Sober tabs: no press, both edges on the same critically damped spring (the indicator
        // slides without stretching), and a short page slide.
        public static readonly SukiTabControlProfile TabControlLite = TabControl with
        {
            PressScale = 1.0,
            ReleaseDecay = 32.0,
            LeadOmega = 24.0,
            LeadDecay = 48.0,
            TrailOmega = 24.0,
            TrailDecay = 48.0,
            ContentDuration = TimeSpan.FromMilliseconds(240),
            ContentEase = SukiEase.Classic.Cubic.Out,
            ContentOffset = 12.0,
            ContentFadeIn = TimeSpan.FromMilliseconds(160),
        };

        public static readonly SukiPresetTable<SukiTabControlPreset, SukiTabControlProfile> Lite =
            new((SukiTabControlPreset.TabControl, TabControlLite));

        #endregion

        #region Minimal

        // The Lite tabs, shorter: a stiff indicator and a quick cross-fade of the pages.
        public static readonly SukiTabControlProfile TabControlMinimal = TabControlLite with
        {
            HoverDuration = TimeSpan.FromMilliseconds(80),
            PressDuration = TimeSpan.FromMilliseconds(60),
            ReleaseOmega = 36.0,
            ReleaseDecay = 72.0,
            LeadOmega = 36.0,
            LeadDecay = 72.0,
            TrailOmega = 36.0,
            TrailDecay = 72.0,
            IndicatorFade = TimeSpan.FromMilliseconds(80),
            ContentDuration = TimeSpan.FromMilliseconds(120),
            ContentOffset = 0.0,
            ContentFadeIn = TimeSpan.FromMilliseconds(120),
            ContentFadeOut = TimeSpan.FromMilliseconds(80),
        };

        public static readonly SukiPresetTable<SukiTabControlPreset, SukiTabControlProfile> Minimal =
            new((SukiTabControlPreset.TabControl, TabControlMinimal));

        #endregion
    }
}

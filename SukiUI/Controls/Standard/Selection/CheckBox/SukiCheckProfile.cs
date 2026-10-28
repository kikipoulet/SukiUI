using System;
using System.ComponentModel;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    public enum SukiCheckPreset
    {
        CheckBox
    }

    public sealed record SukiCheckProfile(
        [property: Description("scale of the box while the pointer hovers it")]
        double HoverScale,
        [property: Description("scale of the box while pressed")]
        double PressDepth,
        [property: Description("duration of the hover ramp")]
        TimeSpan HoverDuration,
        [property: Description("duration of the press-down")]
        TimeSpan PressDuration,
        [property: Description("angular frequency of the box spring (release and pop)")]
        double BoxOmega,
        [property: Description("damping of the box spring (lower = more bounce)")]
        double BoxDecay,
        [property: Description("velocity added to the box scale when the state changes: the pop")]
        double PopKick,
        [property: Description("angular frequency of the stroke head spring")]
        double DrawOmega,
        [property: Description("damping of the stroke head spring")]
        double DrawDecay,
        [property: Description("angular frequency of the trail spring (softer than the head: it lags)")]
        double TrailOmega,
        [property: Description("damping of the trail spring")]
        double TrailDecay,
        [property: Description("angular frequency of the indeterminate bar spring")]
        double BarOmega,
        [property: Description("damping of the indeterminate bar spring")]
        double BarDecay,
        [property: Description("scale the fill starts from (1 = it only fades in)")]
        double FillStartScale,
        [property: Description("how much faster than the stroke the fill appears (1 = same pace)")]
        double FillLead)
    {
        public Spring BoxSpring => new(BoxOmega, BoxDecay);
        public Spring DrawSpring => new(DrawOmega, DrawDecay);
        public Spring TrailSpring => new(TrailOmega, TrailDecay);
        public Spring BarSpring => new(BarOmega, BarDecay);

        #region Normal

        // The box sinks when pressed, pops slightly on the change; the mark is drawn by a
        // fast head with a softer trail behind it.
        public static readonly SukiCheckProfile CheckBox = new(
            HoverScale: 1.02,
            PressDepth: 0.95,
            HoverDuration: TimeSpan.FromMilliseconds(150),
            PressDuration: TimeSpan.FromMilliseconds(120),
            BoxOmega: 16.0,
            BoxDecay: 11.0,
            PopKick: 0.9,
            DrawOmega: 36.0,
            DrawDecay: 46.0,
            TrailOmega: 20.0,
            TrailDecay: 38.0,
            BarOmega: 22.0,
            BarDecay: 30.0,
            FillStartScale: 0.8,
            FillLead: 1.6);

        public static readonly SukiPresetTable<SukiCheckPreset, SukiCheckProfile> Normal =
            new((SukiCheckPreset.CheckBox, CheckBox));

        #endregion

        #region Lite

        // Sober: no hover, no pop, and the trail follows the head exactly (so it never shows).
        public static readonly SukiCheckProfile CheckBoxLite = CheckBox with
        {
            HoverScale = 1.0,
            PopKick = 0.0,
            BoxDecay = 30.0,
            TrailOmega = 36.0,
            TrailDecay = 46.0,
            FillStartScale = 1.0,
            FillLead = 1.0,
        };

        public static readonly SukiPresetTable<SukiCheckPreset, SukiCheckProfile> Lite =
            new((SukiCheckPreset.CheckBox, CheckBoxLite));

        #endregion

        #region Minimal

        // The Lite feel, quicker: a short press, and the mark and the bar land almost at once.
        public static readonly SukiCheckProfile CheckBoxMinimal = CheckBoxLite with
        {
            HoverDuration = TimeSpan.FromMilliseconds(60),
            PressDuration = TimeSpan.FromMilliseconds(60),
            BoxOmega = 30.0,
            BoxDecay = 60.0,
            DrawOmega = 50.0,
            DrawDecay = 100.0,
            TrailOmega = 50.0,
            TrailDecay = 100.0,
            BarOmega = 50.0,
            BarDecay = 100.0,
        };

        public static readonly SukiPresetTable<SukiCheckPreset, SukiCheckProfile> Minimal =
            new((SukiCheckPreset.CheckBox, CheckBoxMinimal));

        #endregion
    }
}

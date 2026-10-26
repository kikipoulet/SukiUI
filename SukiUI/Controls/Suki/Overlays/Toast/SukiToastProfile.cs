using System;
using System.ComponentModel;

namespace SukiUI.ControlsAnimation
{
    public enum SukiToastPreset
    {
        Toast
    }

    public sealed record SukiToastProfile(
        [property: Description("duration of the card's own materialization on show (scale, translate, fade)")]
        TimeSpan ShowDuration,
        [property: Description("duration of the layout growth on show — the pile above rides it")]
        TimeSpan GrowDuration,
        [property: Description("the MaxHeight ceiling the growth targets (clamps to content height)")]
        double GrowMaxHeight,
        [property: Description("scale the card starts from on show")]
        double FromScale,
        [property: Description("horizontal distance the card slides in from (from the right)")]
        double FromOffsetX,
        [property: Description("vertical distance the card slides in from (from below)")]
        double FromOffsetY,
        [property: Description("blur radius the card materializes from on show")]
        double ShowBlurRadius,
        [property: Description("fraction of the show duration at which the blur has landed (crisp)")]
        double BlurSettleRatio,
        [property: Description("angular frequency of the card's spring ease (critically damped: decay = 2·omega)")]
        double ShowSpringOmega,
        [property: Description("damping of the card's spring ease")]
        double ShowSpringDecay,
        [property: Description("angular frequency of the layout growth spring ease (softer than the card's)")]
        double GrowSpringOmega,
        [property: Description("damping of the layout growth spring ease")]
        double GrowSpringDecay,
        [property: Description("opacity fade-out duration on dismiss")]
        TimeSpan DismissFade,
        [property: Description("duration of the right exit and the layout collapse on dismiss")]
        TimeSpan DismissDuration,
        [property: Description("horizontal distance the card exits toward on dismiss")]
        double DismissExitX,
        [property: Description("fixed blur radius the card dissolves into on dismiss")]
        double DissolveBlurRadius)
    {
        #region Normal

        // The cascade recipe scaled to the whole toast: the height grows into the
        // stack on a soft critical spring while the card scales up from 80%, slides
        // in from below-right and fades in from a blur that lands crisp at 60%.
        public static readonly SukiToastProfile Toast = new(
            ShowDuration: TimeSpan.FromMilliseconds(500),
            GrowDuration: TimeSpan.FromMilliseconds(1000),
            GrowMaxHeight: 500.0,
            FromScale: 0.8,
            FromOffsetX: 50.0,
            FromOffsetY: 20.0,
            ShowBlurRadius: 20.0,
            BlurSettleRatio: 0.6,
            ShowSpringOmega: 6.43,
            ShowSpringDecay: 12.86,
            GrowSpringOmega: 4.5,
            GrowSpringDecay: 9.0,
            DismissFade: TimeSpan.FromMilliseconds(200),
            DismissDuration: TimeSpan.FromMilliseconds(300),
            DismissExitX: 50.0,
            DissolveBlurRadius: 20.0);

        public static readonly SukiPresetTable<SukiToastPreset, SukiToastProfile> Normal =
            new((SukiToastPreset.Toast, Toast));

        #endregion

        #region Lite

        // Sober: no scale, no offset, no blur — just the fade and the layout push,
        // quicker.
        public static readonly SukiToastProfile ToastLite = Toast with
        {
            ShowDuration = TimeSpan.FromMilliseconds(250),
            GrowDuration = TimeSpan.FromMilliseconds(400),
            FromScale = 1.0,
            FromOffsetX = 0.0,
            FromOffsetY = 0.0,
            ShowBlurRadius = 0.0,
            DissolveBlurRadius = 0.0,
            DismissFade = TimeSpan.FromMilliseconds(150),
            DismissDuration = TimeSpan.FromMilliseconds(200),
        };

        public static readonly SukiPresetTable<SukiToastPreset, SukiToastProfile> Lite =
            new((SukiToastPreset.Toast, ToastLite));

        #endregion
    }
}

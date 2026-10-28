using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside the SukiUI namespace, the simple name "Motion" is the sibling namespace
    // SukiUI.Motion: this alias, declared in the namespace body, names the entry class.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The toast arrival and departure. Arrival: the height grows into the stack on a soft
    /// spring while the card scales up, slides in from below-right and fades in from a blur
    /// that is crisp before the end. Departure: the card exits to the right while fading,
    /// dissolving into blur as its height collapses (the stack slides closed); the host
    /// removes the toast only when the departure went to the end. Calibrations live in
    /// <see cref="SukiToastProfile"/>, read at each toast.
    /// </summary>
    internal sealed class SukiToastMotion
    {
        private readonly SukiToastHost _host;

        internal SukiToastMotion(SukiToastHost host) => _host = host;

        // read from the host: a new toast is not in the visual tree yet when its show starts
        private SukiToastProfile P() => SukiAnimationTheme.For(_host).Toast[SukiToastPreset.Toast];

        internal async void PlayShow(SukiToast toast)
        {
            var p = P();
            toast.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative); // the scale is centered
            toast.ClipToBounds = true; // the bottom-anchored card unrolls from its slot — clip the overflow above
            var card = new SukiSpringEaseOut { Omega = p.ShowSpringOmega, Decay = p.ShowSpringDecay };

            // The toast was just added to the host: it is not in the visual tree yet, so the
            // host drives the frames.
            bool shown = await Motion.Of(toast).UsingClockOf(_host)
                .Animate(Layoutable.MaxHeightProperty)
                    .From(0.0)
                    .To(p.GrowMaxHeight)
                    .Over(p.GrowDuration)
                    .WithEasing(new SukiSpringEaseOut { Omega = p.GrowSpringOmega, Decay = p.GrowSpringDecay })
                .And()
                .Animate(SukiMotion.ScaleProperty).From(p.FromScale).To(1.0).Over(p.ShowDuration).WithEasing(card)
                .And()
                .Animate(SukiMotion.TranslateXProperty).From(p.FromOffsetX).To(0.0).Over(p.ShowDuration).WithEasing(card)
                .And()
                .Animate(SukiMotion.TranslateYProperty).From(p.FromOffsetY).To(0.0).Over(p.ShowDuration).WithEasing(card)
                .And()
                .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(p.ShowDuration).WithEasing(card)
                .And()
                // Linear fall to zero: "crisp at BlurSettleRatio" means what it says.
                .Animate(SukiMotion.BlurProperty)
                    .From(p.ShowBlurRadius)
                    .To(0.0)
                    .Over(TimeSpan.FromMilliseconds(p.ShowDuration.TotalMilliseconds * p.BlurSettleRatio))
                .PlayAsync();

            if (shown)
                toast.ClipToBounds = false; // the shadow renders again at rest
        }

        /// <summary>True when the departure went to the end (the toast can be removed).</summary>
        internal async Task<bool> PlayDismiss(SukiToast toast)
        {
            var p = P();
            toast.ClipToBounds = true; // the icon circle lives outside the clipped card — keep the collapse clean

            bool gone = await Motion.Of(toast).UsingClockOf(_host)
                .Animate(Visual.OpacityProperty).To(0.0).Over(p.DismissFade)
                .And()
                .Animate(SukiMotion.TranslateXProperty)
                    .To(p.DismissExitX)
                    .Over(p.DismissDuration)
                    .WithEasing(new SukiSpringEaseOut { Omega = p.ShowSpringOmega, Decay = p.ShowSpringDecay })
                .And()
                .Animate(Layoutable.MaxHeightProperty)
                    .To(0.0)
                    .Over(p.DismissDuration)
                    .WithEasing(new SukiSpringEaseOut { Omega = p.GrowSpringOmega, Decay = p.GrowSpringDecay })
                .And()
                .Drive(SukiMotion.BlurProperty)
                    .From(Visual.OpacityProperty)
                    .With(ctx => (1.0 - ctx.ValueOf(Visual.OpacityProperty)) * p.DissolveBlurRadius)
                .Then()
                .Set(SukiMotion.BlurProperty).To(0.0) // drop the dissolve shader
                .PlayAsync();

            if (gone)
                toast.ClipToBounds = false;
            return gone;
        }
    }
}

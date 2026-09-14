using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // The SukiPressMotion precedent: the simple name "Motion" would bind to the
    // sibling namespace — this alias restores the class binding.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The toast show/dismiss choreographies over the SukiUI.Motion engine — the old
    /// fire-and-forget helpers (and the host's Task.Delay(300) guess of their end)
    /// replaced by settle-gated removal: Items.Remove + pool return run in the dismiss
    /// choreography's Then, never before. The show is the cascade item recipe scaled
    /// to the whole toast: the height grows into the stack on a soft spring while the
    /// card scales up, slides in from below-right and fades in from a blur that lands
    /// crisp before the end; the dismiss exits to the right while fading, dissolving
    /// into blur as the height collapses (the stack slides closed). Every channel is
    /// rested at settle — the pooled toast's next From pre-poses whatever pose this
    /// run left. Calibrations live in <see cref="SukiToastProfile"/> (see
    /// <see cref="SukiAnimationTheme"/>), resolved per gesture.
    /// </summary>
    internal sealed class SukiToastMotion
    {
        private readonly SukiToastHost _host;
        private readonly ConditionalWeakTable<SukiToast, Choreography> _current = new();

        internal SukiToastMotion(SukiToastHost host) => _host = host;

        // Per-gesture profile snapshot — a live SukiAnimationTheme switch applies to
        // the NEXT toast, never mid-flight.
        private static SukiToastProfile P() => SukiAnimationTheme.Current.Toast[SukiToastPreset.Toast];

        /// <summary>The queued toast materializes: the height grows into the stack
        /// while the card scales up and slides in from below-right, fading in from a
        /// blur that lands crisp part-way — the whole appearance on critically
        /// damped springs.</summary>
        internal void PlayShow(SukiToast toast)
        {
            var p = P();
            toast.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative); // the scale is centered
            toast.ClipToBounds = true; // the bottom-anchored card unrolls from the slot — clip the overflow above
            var s = Motion.For(toast);
            var cardEase = new SukiSpringEaseOut { Omega = p.ShowSpringOmega, Decay = p.ShowSpringDecay };
            var show = new Choreography()
                .And(s.Property(SukiToast.MaxHeightProperty).From(0.0).To(p.GrowMaxHeight)
                    .Over(p.GrowDuration)
                    .Ease(new SukiSpringEaseOut { Omega = p.GrowSpringOmega, Decay = p.GrowSpringDecay }))
                .And(s.Scale.From(p.FromScale).To(1.0).Over(p.ShowDuration).Ease(cardEase))
                .And(s.TranslateX.From(p.FromOffsetX).To(0.0).Over(p.ShowDuration).Ease(cardEase))
                .And(s.TranslateY.From(p.FromOffsetY).To(0.0).Over(p.ShowDuration).Ease(cardEase))
                .And(s.Opacity.From(0.0).To(1.0).Over(p.ShowDuration).Ease(cardEase))
                // Linear fall to zero — the cascade rule: the blur value tracks the
                // raw progress so "crisp at BlurSettleRatio" means what it says (an
                // eased fall would go crisp far earlier).
                .And(s.Blur.From(p.ShowBlurRadius).To(0.0)
                    .Over(TimeSpan.FromMilliseconds(p.ShowDuration.TotalMilliseconds * p.BlurSettleRatio)))
                .Then(() => toast.ClipToBounds = false); // the shadow renders again at rest
            Play(toast, show);
        }

        /// <summary>The dismissed toast exits to the right on a soft spring while
        /// fading, dissolving into blur as its height collapses — the stack slides
        /// closed. Settle drops the dissolve shader, rests every channel and hands
        /// the removal back to the host.</summary>
        internal void PlayDismiss(SukiToast toast, Action onSettled)
        {
            var p = P();
            var s = Motion.For(toast);
            var fade = s.Opacity.To(0.0).Over(p.DismissFade);
            toast.ClipToBounds = true; // the icon circle lives outside the clipped card — keep the collapse clean

            var dismiss = new Choreography()
                .And(fade)
                .And(s.TranslateX.To(p.DismissExitX).Over(p.DismissDuration)
                    .Ease(new SukiSpringEaseOut { Omega = p.ShowSpringOmega, Decay = p.ShowSpringDecay }))
                .And(new DerivedTrajectory(s.Blur,
                    () => (1.0 - s.Opacity.Value) * p.DissolveBlurRadius,
                    () => fade.Done))
                .And(s.Property(SukiToast.MaxHeightProperty).To(0.0).Over(p.DismissDuration)
                    .Ease(new SukiSpringEaseOut { Omega = p.GrowSpringOmega, Decay = p.GrowSpringDecay }))
                .Then(() =>
                {
                    toast.ClipToBounds = false;
                    s.Blur.Write(0.0); // drop the dissolve shader (the PopupHandle.Hide precedent)
                    s.Opacity.Rest();
                    s.TranslateX.Rest();
                    s.TranslateY.Rest();
                    s.Scale.Rest();
                    s.Blur.Rest();
                    s.Property(SukiToast.MaxHeightProperty).Rest();
                    onSettled(); // Items.Remove + ToastPool.Return — never before settle
                });
            Play(toast, dismiss);
        }

        private void Play(SukiToast toast, Choreography choreography)
        {
            // One choreography at a time per toast — a dismiss mid-show preempts it
            // frozen at its pose (the single-state-machine rule).
            if (_current.TryGetValue(toast, out var running))
            {
                running.Stop();
                _current.Remove(toast);
            }
            _current.Add(toast, choreography);
            choreography.Start(TickOwner(toast));
        }

        // The freshly queued toast is not in the visual tree yet (Items.Add just
        // happened) — the host drives the frames until it attaches (the
        // PopupHandle.TickOwner rule).
        private Visual TickOwner(SukiToast toast) =>
            TopLevel.GetTopLevel(toast) is not null ? toast : _host;
    }
}

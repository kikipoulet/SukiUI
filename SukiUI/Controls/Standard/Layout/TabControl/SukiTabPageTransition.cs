using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The page change of a <see cref="TabControl"/> (its <c>PageTransition</c>): the new page
    /// slides in from the side of the new tab while it fades in, the old one slips out the other
    /// way and fades out faster. Along the strip: horizontally for a strip on top or at the
    /// bottom, vertically for one on a side. Calibrated by the strip's
    /// <see cref="SukiTabControlProfile"/>. The TabControl owns the two pages: it calls
    /// <see cref="Start"/> after a new selection, cancels it when another one comes, and resets
    /// the pages once it is over.
    /// </summary>
    public sealed class SukiTabPageTransition : IPageTransition
    {
        private static readonly Easing FadeEasing = SukiEase.Classic.Cubic.Out;

        // The last transition to move each page: a cancelled one only puts its old page back
        // if no newer transition has taken it over.
        private static readonly ConditionalWeakTable<Visual, object> Owners = new();

        public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var tabs = (to ?? from)?.FindAncestorOfType<TabControl>();
            var preset = tabs is null ? SukiTabControlPreset.TabControl : SukiTabControlMotion.GetPreset(tabs);
            var profile = SukiAnimationTheme.Current.TabControl[preset];
            var vertical = tabs?.TabStripPlacement is Dock.Left or Dock.Right;
            var along = vertical ? SukiMotion.TranslateYProperty : SukiMotion.TranslateXProperty;
            var across = vertical ? SukiMotion.TranslateXProperty : SukiMotion.TranslateYProperty;
            var offset = forward ? profile.ContentOffset : -profile.ContentOffset;

            var owner = new object();
            if (from is not null)
                Owners.AddOrUpdate(from, owner);
            if (to is not null)
                Owners.AddOrUpdate(to, owner);

            // Another selection: the TabControl hides the new page at once, so its motion stops
            // there. The old page keeps its pose: the next transition carries it on from there.
            using var registration = cancellationToken.Register(() => Cancelled(from, to, owner));

            var moves = new List<Task<bool>>(2);
            if (from is not null)
                moves.Add(Motion.Of(from)
                    .Animate(Visual.OpacityProperty)
                        .To(0.0)
                        .Over(profile.ContentFadeOut)
                        .WithEasing(FadeEasing)
                    .And()
                    .Animate(along)
                        .To(-offset / 2)
                        .Over(profile.ContentDuration)
                        .WithEasing(profile.ContentEase)
                    .PlayAsync());
            if (to is not null)
                moves.Add(Motion.Of(to)
                    .Set(across).To(0.0)
                    .And()
                    .Animate(Visual.OpacityProperty)
                        .From(0.0).To(1.0)
                        .Over(profile.ContentFadeIn)
                        .WithEasing(FadeEasing)
                    .And()
                    .Animate(along)
                        .From(offset).To(0.0)
                        .Over(profile.ContentDuration)
                        .WithEasing(profile.ContentEase)
                    .PlayAsync());

            await Task.WhenAll(moves);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void Cancelled(Visual? from, Visual? to, object owner)
        {
            if (to is not null)
                Motion.Stop(to);
            if (from is null)
                return;
            // A new selection starts its own transition on the next layout pass, from the old
            // page's current pose. If none comes (the strip left the tree), put the page back.
            Dispatcher.UIThread.Post(() =>
            {
                if (!Owners.TryGetValue(from, out var current) || current != owner)
                    return;
                Motion.Stop(from);
                Motion.Of(from)
                      .Set(Visual.OpacityProperty).To(1.0)
                      .And()
                      .Set(SukiMotion.TranslateXProperty).To(0.0)
                      .And()
                      .Set(SukiMotion.TranslateYProperty).To(0.0)
                      .PlayAsync();
            }, DispatcherPriority.Background);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace SukiUI.Helpers
{
    public class CompositionAnimationHelper
    {
        private enum AnimationKind
        {
            Scrollable,
            Opacity,
            Size,
            SizeOpacity
        }

        /// <summary>
        /// An <see cref="ImplicitAnimationCollection"/> holds no per-visual state, so a single instance can be
        /// reused by every visual sharing the same compositor and duration instead of being rebuilt per call.
        /// </summary>
        private static readonly ConditionalWeakTable<Compositor, Dictionary<(AnimationKind, double), ImplicitAnimationCollection>> Cache = new();

        public static void MakeScrollable(CompositionVisual compositionVisual, double millis = 250) =>
            Apply(compositionVisual, AnimationKind.Scrollable, millis);

        public static void MakeOpacityAnimated(CompositionVisual compositionVisual, double millis = 700) =>
            Apply(compositionVisual, AnimationKind.Opacity, millis);

        public static void MakeSizeAnimated(CompositionVisual compositionVisual, double millis = 450) =>
            Apply(compositionVisual, AnimationKind.Size, millis);

        public static void MakeSizeOpacityAnimated(CompositionVisual compositionVisual, double millis = 450) =>
            Apply(compositionVisual, AnimationKind.SizeOpacity, millis);

        private static void Apply(CompositionVisual? compositionVisual, AnimationKind kind, double millis)
        {
            if (compositionVisual == null)
                return;

            var compositor = compositionVisual.Compositor;
            var cache = Cache.GetOrCreateValue(compositor);
            var key = (kind, millis);

            if (!cache.TryGetValue(key, out var animations))
            {
                animations = Create(compositor, kind, TimeSpan.FromMilliseconds(millis));
                cache[key] = animations;
            }

            compositionVisual.ImplicitAnimations = animations;
        }

        private static ImplicitAnimationCollection Create(Compositor compositor, AnimationKind kind, TimeSpan duration)
        {
            var animationGroup = compositor.CreateAnimationGroup();

            if (kind is AnimationKind.Size or AnimationKind.SizeOpacity)
            {
                var sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
                sizeAnimation.Target = "Size";
                sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                sizeAnimation.Duration = duration;
                animationGroup.Add(sizeAnimation);
            }

            if (kind is AnimationKind.Opacity or AnimationKind.SizeOpacity)
            {
                var opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
                opacityAnimation.Target = "Opacity";
                opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                opacityAnimation.Duration = duration;
                animationGroup.Add(opacityAnimation);
            }

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = duration;
            animationGroup.Add(offsetAnimation);

            var implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
            implicitAnimationCollection["Offset"] = animationGroup;

            if (kind is AnimationKind.Size or AnimationKind.SizeOpacity)
                implicitAnimationCollection["Size"] = animationGroup;

            if (kind is AnimationKind.Opacity or AnimationKind.SizeOpacity)
                implicitAnimationCollection["Opacity"] = animationGroup;

            return implicitAnimationCollection;
        }
    }
}

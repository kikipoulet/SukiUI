using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SukiUI.Motion
{
    public sealed partial class SukiEase
    {
        /// <summary>A CSS-style cubic Bézier, <c>cubic-bezier(x1, y1, x2, y2)</c>.</summary>
        public static SukiEase CubicBezier(double x1, double y1, double x2, double y2)
        {
            if (!(x1 >= 0.0 && x1 <= 1.0 && x2 >= 0.0 && x2 <= 1.0))
                throw new ArgumentOutOfRangeException(nameof(x1), "The x of a control point must be between 0 and 1.");
            return new SukiEase(string.Create(CultureInfo.InvariantCulture, $"CubicBezier({x1}, {y1}, {x2}, {y2})"),
                Shapes.Bezier(x1, y1, x2, y2));
        }

        /// <summary>A damped spring from 0 to 1, by damping ratio: 1 arrives without overshoot, below 1 it
        /// overshoots and rings (0.5 once or twice, 0.2 many times), above 1 it creeps. The curve is the
        /// shape of the motion, stretched to settle at t = 1: the duration is yours.</summary>
        public static SukiEase Spring(double dampingRatio)
        {
            if (!(dampingRatio > 0.0 && double.IsFinite(dampingRatio)))
                throw new ArgumentOutOfRangeException(nameof(dampingRatio), dampingRatio, "A damping ratio must be above 0.");
            return new SukiEase(string.Create(CultureInfo.InvariantCulture, $"Spring({dampingRatio})"), Shapes.Spring(dampingRatio));
        }

        private static SukiEase Make(string name, Func<double, double> function) => new(name, function);

        private static SukiEase Bezier(string name, double x1, double y1, double x2, double y2) =>
            Make(name, Shapes.Bezier(x1, y1, x2, y2));

        private static SukiEase Damped(string name, double dampingRatio) => Make(name, Shapes.Spring(dampingRatio));

        /// <summary>The curve to use when you have no opinion: <see cref="Signature.Smooth"/>.</summary>
        public static SukiEase Default => Signature.Smooth;

        /// <summary>Our own presets, all springs: they settle gently and differ by how much they bounce.</summary>
        public static class Signature
        {
            /// <summary>No overshoot, a calm arrival. The default.</summary>
            public static SukiEase Smooth { get; } = Damped("Signature.Smooth", 1.0);

            /// <summary>A light touch of overshoot.</summary>
            public static SukiEase Lively { get; } = Damped("Signature.Lively", 0.71);

            /// <summary>One clear bounce.</summary>
            public static SukiEase Springy { get; } = Damped("Signature.Springy", 0.5);

            /// <summary>Several bounces, like a toy.</summary>
            public static SukiEase Bouncy { get; } = Damped("Signature.Bouncy", 0.3);

            /// <summary>Many soft, round bounces.</summary>
            public static SukiEase Jelly { get; } = Damped("Signature.Jelly", 0.23);

            /// <summary>One sharp pop, then still.</summary>
            public static SukiEase Pop { get; } = Damped("Signature.Pop", 0.6);

            /// <summary>Falls on the target and bounces off it, like a ball.</summary>
            public static SukiEase Drop { get; } = Bouncy.BounceOffTarget().Named("Signature.Drop");

            /// <summary>Leaves gently and arrives gently.</summary>
            public static SukiEase Glide { get; } = Make("Signature.Glide", Shapes.Warp(Shapes.Spring(1.0), 2.0, 1.5));

            /// <summary>A very gentle landing, like a soft-close drawer.</summary>
            public static SukiEase SoftClose { get; } = Make("Signature.SoftClose", Shapes.Warp(Shapes.Spring(1.0), 1.0, 2.5));

            /// <summary>Gets pulled onto the target.</summary>
            public static SukiEase Magnetic { get; } = Make("Signature.Magnetic", Shapes.Warp(Shapes.Spring(1.0), 1.0, 1.0 / 3.0));
        }

        /// <summary>The classic easing families (Penner, as on easings.net) and the CSS keywords.</summary>
        public static class Classic
        {
            public static SukiEase Linear { get; } = Make("Classic.Linear", t => t);
            public static SukiEase Smoothstep { get; } = Make("Classic.Smoothstep", Shapes.Smoothstep);
            public static SukiEase Smootherstep { get; } = Make("Classic.Smootherstep", Shapes.Smootherstep);

            private static SukiEase In(string name, Func<double, double> ease) => Make(name, ease);
            private static SukiEase Out(string name, Func<double, double> ease) => Make(name, Shapes.Out(ease));
            private static SukiEase InOut(string name, Func<double, double> ease) => Make(name, Shapes.InOut(ease));

            public static class Sine
            {
                public static SukiEase In { get; } = Classic.In("Classic.Sine.In", Shapes.SineIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Sine.Out", Shapes.SineIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Sine.InOut", Shapes.SineIn);
            }

            public static class Quad
            {
                public static SukiEase In { get; } = Classic.In("Classic.Quad.In", Shapes.QuadIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Quad.Out", Shapes.QuadIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Quad.InOut", Shapes.QuadIn);
            }

            public static class Cubic
            {
                public static SukiEase In { get; } = Classic.In("Classic.Cubic.In", Shapes.CubicIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Cubic.Out", Shapes.CubicIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Cubic.InOut", Shapes.CubicIn);
            }

            public static class Quart
            {
                public static SukiEase In { get; } = Classic.In("Classic.Quart.In", Shapes.QuartIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Quart.Out", Shapes.QuartIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Quart.InOut", Shapes.QuartIn);
            }

            public static class Quint
            {
                public static SukiEase In { get; } = Classic.In("Classic.Quint.In", Shapes.QuintIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Quint.Out", Shapes.QuintIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Quint.InOut", Shapes.QuintIn);
            }

            public static class Expo
            {
                public static SukiEase In { get; } = Classic.In("Classic.Expo.In", Shapes.ExpoIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Expo.Out", Shapes.ExpoIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Expo.InOut", Shapes.ExpoIn);
            }

            public static class Circ
            {
                public static SukiEase In { get; } = Classic.In("Classic.Circ.In", Shapes.CircIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Circ.Out", Shapes.CircIn);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Circ.InOut", Shapes.CircIn);
            }

            /// <summary>Pulls back (In) or overshoots (Out) by about 10 %.</summary>
            public static class Back
            {
                public static SukiEase In { get; } = Classic.In("Classic.Back.In", Shapes.BackIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Back.Out", Shapes.BackIn);
                public static SukiEase InOut { get; } = Classic.In("Classic.Back.InOut", Shapes.BackInOut);
            }

            /// <summary>Wobbles like a rubber band.</summary>
            public static class Elastic
            {
                public static SukiEase In { get; } = Classic.In("Classic.Elastic.In", Shapes.ElasticIn);
                public static SukiEase Out { get; } = Classic.Out("Classic.Elastic.Out", Shapes.ElasticIn);
                public static SukiEase InOut { get; } = Classic.In("Classic.Elastic.InOut", Shapes.ElasticInOut);
            }

            /// <summary>Bounces like a ball dropped on the target.</summary>
            public static class Bounce
            {
                public static SukiEase In { get; } = Classic.In("Classic.Bounce.In", Shapes.BounceIn);
                public static SukiEase Out { get; } = Classic.In("Classic.Bounce.Out", Shapes.BounceOut);
                public static SukiEase InOut { get; } = Classic.InOut("Classic.Bounce.InOut", Shapes.BounceIn);
            }

            /// <summary>The CSS keywords.</summary>
            public static class Css
            {
                public static SukiEase Ease { get; } = Bezier("Classic.Css.Ease", 0.25, 0.1, 0.25, 1.0);
                public static SukiEase EaseIn { get; } = Bezier("Classic.Css.EaseIn", 0.42, 0.0, 1.0, 1.0);
                public static SukiEase EaseOut { get; } = Bezier("Classic.Css.EaseOut", 0.0, 0.0, 0.58, 1.0);
                public static SukiEase EaseInOut { get; } = Bezier("Classic.Css.EaseInOut", 0.42, 0.0, 0.58, 1.0);
            }
        }

        /// <summary>Easings and springs taken from the design systems and animation libraries,
        /// as curves. A spring-based entry carries the shape of its motion (how much it bounces), not
        /// its original timing: the duration is yours.</summary>
        public static class DesignLibraries
        {
            /// <summary>Material 3 easing tokens. <c>Emphasized</c> is a two-part path in the spec; this is the
            /// single Bézier usually used in its place.</summary>
            public static class Material3
            {
                public static SukiEase Standard { get; } = Bezier("DesignLibraries.Material3.Standard", 0.2, 0.0, 0.0, 1.0);
                public static SukiEase StandardDecelerate { get; } = Bezier("DesignLibraries.Material3.StandardDecelerate", 0.0, 0.0, 0.0, 1.0);
                public static SukiEase StandardAccelerate { get; } = Bezier("DesignLibraries.Material3.StandardAccelerate", 0.3, 0.0, 1.0, 1.0);
                public static SukiEase Emphasized { get; } = Bezier("DesignLibraries.Material3.Emphasized", 0.2, 0.0, 0.0, 1.0);
                public static SukiEase EmphasizedDecelerate { get; } = Bezier("DesignLibraries.Material3.EmphasizedDecelerate", 0.05, 0.7, 0.1, 1.0);
                public static SukiEase EmphasizedAccelerate { get; } = Bezier("DesignLibraries.Material3.EmphasizedAccelerate", 0.3, 0.0, 0.8, 0.15);
            }

            /// <summary>Material 3 Expressive motion springs, as cubic Béziers: spatial ones overshoot
            /// slightly, effects ones (color, opacity) never do.</summary>
            public static class Material3Expressive
            {
                public static SukiEase SpatialFast { get; } = Bezier("DesignLibraries.Material3Expressive.SpatialFast", 0.42, 1.67, 0.21, 0.90);
                public static SukiEase SpatialDefault { get; } = Bezier("DesignLibraries.Material3Expressive.SpatialDefault", 0.38, 1.21, 0.22, 1.0);
                public static SukiEase SpatialSlow { get; } = Bezier("DesignLibraries.Material3Expressive.SpatialSlow", 0.39, 1.29, 0.35, 0.98);
                public static SukiEase EffectsFast { get; } = Bezier("DesignLibraries.Material3Expressive.EffectsFast", 0.31, 0.94, 0.34, 1.0);
                public static SukiEase EffectsDefault { get; } = Bezier("DesignLibraries.Material3Expressive.EffectsDefault", 0.34, 0.80, 0.34, 1.0);
                public static SukiEase EffectsSlow { get; } = Bezier("DesignLibraries.Material3Expressive.EffectsSlow", 0.34, 0.88, 0.34, 1.0);
                public static SukiEase StandardSpatial { get; } = Bezier("DesignLibraries.Material3Expressive.StandardSpatial", 0.27, 1.06, 0.18, 1.0);
            }

            /// <summary>The SwiftUI spring presets: a damping ratio of 1 minus the bounce (0, 0.15, 0.3, 0.14).</summary>
            public static class Apple
            {
                public static SukiEase Smooth { get; } = Damped("DesignLibraries.Apple.Smooth", 1.0);
                public static SukiEase Snappy { get; } = Damped("DesignLibraries.Apple.Snappy", 0.85);
                public static SukiEase Bouncy { get; } = Damped("DesignLibraries.Apple.Bouncy", 0.7);
                public static SukiEase Interactive { get; } = Damped("DesignLibraries.Apple.Interactive", 0.86);
            }

            /// <summary>The Jetpack Compose damping ratios.</summary>
            public static class Compose
            {
                public static SukiEase HighBouncy { get; } = Damped("DesignLibraries.Compose.HighBouncy", 0.2);
                public static SukiEase MediumBouncy { get; } = Damped("DesignLibraries.Compose.MediumBouncy", 0.5);
                public static SukiEase LowBouncy { get; } = Damped("DesignLibraries.Compose.LowBouncy", 0.75);
                public static SukiEase NoBouncy { get; } = Damped("DesignLibraries.Compose.NoBouncy", 1.0);
            }

            /// <summary>The react-spring presets, whose damping ratio is friction / (2 √tension) for a unit mass.</summary>
            public static class ReactSpring
            {
                private static SukiEase Preset(string name, double tension, double friction) =>
                    Damped($"DesignLibraries.ReactSpring.{name}", friction / (2.0 * Math.Sqrt(tension)));

                public static SukiEase Default { get; } = Preset("Default", 170, 26);
                public static SukiEase Gentle { get; } = Preset("Gentle", 120, 14);
                public static SukiEase Wobbly { get; } = Preset("Wobbly", 180, 12);
                public static SukiEase Stiff { get; } = Preset("Stiff", 210, 20);
                public static SukiEase Slow { get; } = Preset("Slow", 280, 60);
                public static SukiEase Molasses { get; } = Preset("Molasses", 280, 120);
            }

            /// <summary>The Material Design 2 curves, still everywhere in Android code.</summary>
            public static class MaterialLegacy
            {
                public static SukiEase FastOutSlowIn { get; } = Bezier("DesignLibraries.MaterialLegacy.FastOutSlowIn", 0.4, 0.0, 0.2, 1.0);
                public static SukiEase LinearOutSlowIn { get; } = Bezier("DesignLibraries.MaterialLegacy.LinearOutSlowIn", 0.0, 0.0, 0.2, 1.0);
                public static SukiEase FastOutLinearIn { get; } = Bezier("DesignLibraries.MaterialLegacy.FastOutLinearIn", 0.4, 0.0, 1.0, 1.0);
                public static SukiEase Sharp { get; } = Bezier("DesignLibraries.MaterialLegacy.Sharp", 0.4, 0.0, 0.6, 1.0);
            }

            /// <summary>Fluent 2 motion curves (the Fluent UI curve tokens).</summary>
            public static class Fluent
            {
                public static SukiEase DecelerateMax { get; } = Bezier("DesignLibraries.Fluent.DecelerateMax", 0.1, 0.9, 0.2, 1.0);
                public static SukiEase DecelerateMid { get; } = Bezier("DesignLibraries.Fluent.DecelerateMid", 0.0, 0.0, 0.0, 1.0);
                public static SukiEase DecelerateMin { get; } = Bezier("DesignLibraries.Fluent.DecelerateMin", 0.33, 0.0, 0.1, 1.0);
                public static SukiEase AccelerateMax { get; } = Bezier("DesignLibraries.Fluent.AccelerateMax", 0.9, 0.1, 1.0, 0.2);
                public static SukiEase AccelerateMid { get; } = Bezier("DesignLibraries.Fluent.AccelerateMid", 1.0, 0.0, 1.0, 1.0);
                public static SukiEase AccelerateMin { get; } = Bezier("DesignLibraries.Fluent.AccelerateMin", 0.8, 0.0, 0.78, 1.0);
                public static SukiEase EasyEaseMax { get; } = Bezier("DesignLibraries.Fluent.EasyEaseMax", 0.8, 0.0, 0.2, 1.0);
                public static SukiEase EasyEase { get; } = Bezier("DesignLibraries.Fluent.EasyEase", 0.33, 0.0, 0.67, 1.0);
            }

            /// <summary>IBM Carbon motion curves.</summary>
            public static class Carbon
            {
                public static SukiEase ProductiveStandard { get; } = Bezier("DesignLibraries.Carbon.ProductiveStandard", 0.2, 0.0, 0.38, 0.9);
                public static SukiEase ProductiveEntrance { get; } = Bezier("DesignLibraries.Carbon.ProductiveEntrance", 0.0, 0.0, 0.38, 0.9);
                public static SukiEase ProductiveExit { get; } = Bezier("DesignLibraries.Carbon.ProductiveExit", 0.2, 0.0, 1.0, 0.9);
                public static SukiEase ExpressiveStandard { get; } = Bezier("DesignLibraries.Carbon.ExpressiveStandard", 0.4, 0.14, 0.3, 1.0);
                public static SukiEase ExpressiveEntrance { get; } = Bezier("DesignLibraries.Carbon.ExpressiveEntrance", 0.0, 0.0, 0.3, 1.0);
                public static SukiEase ExpressiveExit { get; } = Bezier("DesignLibraries.Carbon.ExpressiveExit", 0.4, 0.14, 1.0, 1.0);
            }

            /// <summary>The Tailwind CSS timing functions.</summary>
            public static class Tailwind
            {
                public static SukiEase EaseIn { get; } = Bezier("DesignLibraries.Tailwind.EaseIn", 0.4, 0.0, 1.0, 1.0);
                public static SukiEase EaseOut { get; } = Bezier("DesignLibraries.Tailwind.EaseOut", 0.0, 0.0, 0.2, 1.0);
                public static SukiEase EaseInOut { get; } = Bezier("DesignLibraries.Tailwind.EaseInOut", 0.4, 0.0, 0.2, 1.0);
            }

            /// <summary>The Android interpolators that are not a Bézier, with their default tension of 2.</summary>
            public static class Android
            {
                public static SukiEase Anticipate { get; } = Make("DesignLibraries.Android.Anticipate", Shapes.Anticipate(2.0));
                public static SukiEase Overshoot { get; } = Make("DesignLibraries.Android.Overshoot", Shapes.Overshoot(2.0));
                public static SukiEase AnticipateOvershoot { get; } = Make("DesignLibraries.Android.AnticipateOvershoot", Shapes.AnticipateOvershoot(2.0));
            }
        }

        // ---- finding a preset by its path ---------------------------------------------------

        private static readonly Lazy<IReadOnlyList<SukiEasePreset>> AllPresets = new(() =>
        {
            var found = new List<SukiEasePreset>();
            Collect(typeof(Signature), nameof(Signature), found);
            Collect(typeof(Classic), nameof(Classic), found);
            Collect(typeof(DesignLibraries), nameof(DesignLibraries), found);
            return found;
        });

        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "The nested types are the preset groups of this assembly, all public.")]
        private static void Collect(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
            Type group, string path, List<SukiEasePreset> found)
        {
            foreach (var property in group.GetProperties(BindingFlags.Public | BindingFlags.Static)
                         .Where(p => p.PropertyType == typeof(SukiEase)).OrderBy(p => p.MetadataToken))
                found.Add(new SukiEasePreset($"{path}.{property.Name}", (SukiEase)property.GetValue(null)!));
            foreach (var nested in group.GetNestedTypes(BindingFlags.Public).OrderBy(t => t.MetadataToken))
                Collect(nested, $"{path}.{nested.Name}", found);
        }

        /// <summary>Every preset, in the order of the tree, with its path.</summary>
        public static IReadOnlyList<SukiEasePreset> Presets => AllPresets.Value;

        /// <summary>The preset at <paramref name="path"/>, for example <c>Classic.Cubic.Out</c> or
        /// <c>DesignLibraries.Material3.Standard</c> (a leading <c>SukiEase.</c> is accepted).</summary>
        public static SukiEase? Find(string path)
        {
            if (path.StartsWith("SukiEase.", StringComparison.Ordinal)) path = path["SukiEase.".Length..];
            foreach (var preset in Presets)
                if (preset.Path == path) return preset.Ease;
            return null;
        }
    }

    /// <summary>A preset and where it sits in the tree.</summary>
    public sealed record SukiEasePreset(string Path, SukiEase Ease)
    {
        /// <summary>The group it belongs to, for example <c>DesignLibraries.Material3</c>.</summary>
        public string Group => Path[..Path.LastIndexOf('.')];

        public string Name => Path[(Path.LastIndexOf('.') + 1)..];
    }
}

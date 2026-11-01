using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// Something the engine can animate on a visual. Every Avalonia property is one (pass the
    /// <see cref="AvaloniaProperty{TValue}"/> directly); <see cref="SukiMotion"/> adds the
    /// render properties Avalonia does not expose as plain values (scale, translate, rotate,
    /// skew, blur, shadow).
    /// </summary>
    public abstract class MotionProperty
    {
        private protected MotionProperty(string name) => Name = name;

        public string Name { get; }

        internal abstract int Components { get; }

        public override string ToString() => Name;
    }

    /// <inheritdoc cref="MotionProperty"/>
    public abstract class MotionProperty<T> : MotionProperty
    {
        private protected MotionProperty(string name) : base(name) => Value = MotionValue<T>.Require();

        internal MotionValue<T> Value { get; }

        internal override int Components => Value.Components;

        internal abstract T Read(Visual target);

        internal abstract void Write(Visual target, T value);
    }

    /// <summary>A render property with its own read/write protocol (transform block, effect slot).</summary>
    internal sealed class RenderMotionProperty : MotionProperty<double>
    {
        private readonly Func<Visual, double> _read;
        private readonly Action<Visual, double> _write;

        internal RenderMotionProperty(string name, Func<Visual, double> read, Action<Visual, double> write)
            : base(name)
        {
            _read = read;
            _write = write;
        }

        internal override double Read(Visual target) => _read(target);

        internal override void Write(Visual target, double value) => _write(target, value);
    }

    /// <summary>
    /// An Avalonia property seen by the engine: reading and writing the property is the whole
    /// semantics (local value, like a plain SetValue). One instance per property, so the same
    /// property always maps to the same animation track.
    /// </summary>
    internal sealed class AvaloniaMotionProperty<T> : MotionProperty<T>
    {
        private static readonly Dictionary<AvaloniaProperty, AvaloniaMotionProperty<T>> Cache = new();

        private readonly AvaloniaProperty<T> _property;
        private readonly StyledProperty<T>? _styled;
        private readonly DirectPropertyBase<T>? _direct;

        private AvaloniaMotionProperty(AvaloniaProperty<T> property) : base(property.Name)
        {
            _property = property;
            _styled = property as StyledProperty<T>;
            _direct = property as DirectPropertyBase<T>;
        }

        internal static AvaloniaMotionProperty<T> For(AvaloniaProperty<T> property)
        {
            if (!Cache.TryGetValue(property, out var motion))
                Cache[property] = motion = new AvaloniaMotionProperty<T>(property);
            return motion;
        }

        internal override T Read(Visual target)
        {
            if (_styled is not null)
                return target.GetValue(_styled);
            if (_direct is not null)
                return target.GetValue(_direct);
            return (T)target.GetValue(_property)!;
        }

        internal override void Write(Visual target, T value)
        {
            if (_styled is not null)
                target.SetValue(_styled, value);
            else if (_direct is not null)
                target.SetValue(_direct, value);
            else
                target.SetValue(_property, value);
        }
    }

    /// <summary>
    /// The render properties of the motion layer — values Avalonia only exposes through
    /// objects (RenderTransform, Effect). The transform properties compose through one shared
    /// block per visual (translate · skew · rotate · scale around RenderTransformOrigin)
    /// instead of fighting over RenderTransform; blur and shadow share the single Effect slot
    /// (last writer owns it) and drop their effect entirely at rest — no shader pass is paid
    /// once they have faded.
    /// </summary>
    public static class SukiMotion
    {
        /// <summary>Uniform render scale (X and Y). Render-only: no relayout.</summary>
        public static readonly MotionProperty<double> ScaleProperty =
            new RenderMotionProperty("Scale", Transforms.ReadScaleX, Transforms.WriteScale);

        /// <summary>Horizontal render scale.</summary>
        public static readonly MotionProperty<double> ScaleXProperty =
            new RenderMotionProperty("ScaleX", Transforms.ReadScaleX, Transforms.WriteScaleX);

        /// <summary>Vertical render scale.</summary>
        public static readonly MotionProperty<double> ScaleYProperty =
            new RenderMotionProperty("ScaleY", Transforms.ReadScaleY, Transforms.WriteScaleY);

        /// <summary>Horizontal translation in DIPs, in parent coordinates.</summary>
        public static readonly MotionProperty<double> TranslateXProperty =
            new RenderMotionProperty("TranslateX", Transforms.ReadTranslateX, Transforms.WriteTranslateX);

        /// <summary>Vertical translation in DIPs, in parent coordinates.</summary>
        public static readonly MotionProperty<double> TranslateYProperty =
            new RenderMotionProperty("TranslateY", Transforms.ReadTranslateY, Transforms.WriteTranslateY);

        /// <summary>Rotation in degrees around RenderTransformOrigin.</summary>
        public static readonly MotionProperty<double> RotateProperty =
            new RenderMotionProperty("Rotate", Transforms.ReadRotate, Transforms.WriteRotate);

        /// <summary>Horizontal skew in degrees.</summary>
        public static readonly MotionProperty<double> SkewXProperty =
            new RenderMotionProperty("SkewX", Transforms.ReadSkewX, Transforms.WriteSkewX);

        /// <summary>Vertical skew in degrees.</summary>
        public static readonly MotionProperty<double> SkewYProperty =
            new RenderMotionProperty("SkewY", Transforms.ReadSkewY, Transforms.WriteSkewY);

        /// <summary>Gaussian blur radius in DIPs. Below 0.5 the BlurEffect leaves the slot.</summary>
        public static readonly MotionProperty<double> BlurProperty =
            new RenderMotionProperty("Blur", ReadBlur, WriteBlur);

        /// <summary>Drop-shadow opacity (0..1). Below 0.02 the DropShadowEffect leaves the slot.</summary>
        public static readonly MotionProperty<double> ShadowOpacityProperty =
            new RenderMotionProperty("ShadowOpacity",
                t => t.Effect is DropShadowEffect s ? s.Opacity : 0.0, ShadowEffects.WriteOpacity);

        /// <summary>Drop-shadow blur radius in DIPs.</summary>
        public static readonly MotionProperty<double> ShadowBlurProperty =
            new RenderMotionProperty("ShadowBlur",
                t => t.Effect is DropShadowEffect s ? s.BlurRadius : 0.0, ShadowEffects.WriteBlur);

        /// <summary>Drop-shadow horizontal offset in DIPs.</summary>
        public static readonly MotionProperty<double> ShadowOffsetXProperty =
            new RenderMotionProperty("ShadowOffsetX",
                t => t.Effect is DropShadowEffect s ? s.OffsetX : 0.0, ShadowEffects.WriteOffsetX);

        /// <summary>Drop-shadow vertical offset in DIPs.</summary>
        public static readonly MotionProperty<double> ShadowOffsetYProperty =
            new RenderMotionProperty("ShadowOffsetY",
                t => t.Effect is DropShadowEffect s ? s.OffsetY : 0.0, ShadowEffects.WriteOffsetY);

        private static double ReadBlur(Visual t) => t.Effect is BlurEffect b ? b.Radius : 0.0;

        private static void WriteBlur(Visual t, double v)
        {
            if (v >= 0.5)
                OwnedEffects.Blur(t).Radius = v;
            else if (t.Effect is BlurEffect)
                t.Effect = null;
        }
    }
}

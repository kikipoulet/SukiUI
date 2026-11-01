using System;
using Avalonia;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// How a value type maps onto the engine's numeric components: a spring integrates each
    /// component on its own, a tween interpolates each one. One instance per supported type,
    /// resolved once — an unsupported type fails when the animation is declared, never in
    /// the middle of a gesture.
    /// </summary>
    internal abstract class MotionValue<T>
    {
        public static readonly MotionValue<T>? Default = (MotionValue<T>?)MotionValues.Create(typeof(T));

        public abstract int Components { get; }

        public abstract void Split(T value, Span<double> into);

        public abstract T Join(ReadOnlySpan<double> components);

        public static MotionValue<T> Require() => Default ?? throw new NotSupportedException(
            $"SukiUI.Motion cannot animate values of type {typeof(T).Name}. Supported types: " +
            "double, float, int, Color, Point, Vector, Size, Thickness, CornerRadius.");
    }

    internal static class MotionValues
    {
        internal static object? Create(Type type)
        {
            if (type == typeof(double)) return new DoubleValue();
            if (type == typeof(float)) return new FloatValue();
            if (type == typeof(int)) return new IntValue();
            if (type == typeof(Color)) return new ColorValue();
            if (type == typeof(Point)) return new PointValue();
            if (type == typeof(Vector)) return new VectorValue();
            if (type == typeof(Size)) return new SizeValue();
            if (type == typeof(Thickness)) return new ThicknessValue();
            if (type == typeof(CornerRadius)) return new CornerRadiusValue();
            return null;
        }

        private sealed class DoubleValue : MotionValue<double>
        {
            public override int Components => 1;
            public override void Split(double value, Span<double> into) => into[0] = value;
            public override double Join(ReadOnlySpan<double> c) => c[0];
        }

        private sealed class FloatValue : MotionValue<float>
        {
            public override int Components => 1;
            public override void Split(float value, Span<double> into) => into[0] = value;
            public override float Join(ReadOnlySpan<double> c) => (float)c[0];
        }

        private sealed class IntValue : MotionValue<int>
        {
            public override int Components => 1;
            public override void Split(int value, Span<double> into) => into[0] = value;
            public override int Join(ReadOnlySpan<double> c) => (int)Math.Round(c[0]);
        }

        private sealed class ColorValue : MotionValue<Color>
        {
            public override int Components => 4;

            public override void Split(Color value, Span<double> into)
            {
                into[0] = value.A;
                into[1] = value.R;
                into[2] = value.G;
                into[3] = value.B;
            }

            public override Color Join(ReadOnlySpan<double> c) =>
                Color.FromArgb(Channel(c[0]), Channel(c[1]), Channel(c[2]), Channel(c[3]));

            private static byte Channel(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);
        }

        private sealed class PointValue : MotionValue<Point>
        {
            public override int Components => 2;

            public override void Split(Point value, Span<double> into)
            {
                into[0] = value.X;
                into[1] = value.Y;
            }

            public override Point Join(ReadOnlySpan<double> c) => new(c[0], c[1]);
        }

        private sealed class VectorValue : MotionValue<Vector>
        {
            public override int Components => 2;

            public override void Split(Vector value, Span<double> into)
            {
                into[0] = value.X;
                into[1] = value.Y;
            }

            public override Vector Join(ReadOnlySpan<double> c) => new(c[0], c[1]);
        }

        private sealed class SizeValue : MotionValue<Size>
        {
            public override int Components => 2;

            public override void Split(Size value, Span<double> into)
            {
                into[0] = value.Width;
                into[1] = value.Height;
            }

            // A spring overshoot must never produce a negative size.
            public override Size Join(ReadOnlySpan<double> c) => new(Math.Max(0, c[0]), Math.Max(0, c[1]));
        }

        private sealed class ThicknessValue : MotionValue<Thickness>
        {
            public override int Components => 4;

            public override void Split(Thickness value, Span<double> into)
            {
                into[0] = value.Left;
                into[1] = value.Top;
                into[2] = value.Right;
                into[3] = value.Bottom;
            }

            public override Thickness Join(ReadOnlySpan<double> c) => new(c[0], c[1], c[2], c[3]);
        }

        private sealed class CornerRadiusValue : MotionValue<CornerRadius>
        {
            public override int Components => 4;

            public override void Split(CornerRadius value, Span<double> into)
            {
                into[0] = value.TopLeft;
                into[1] = value.TopRight;
                into[2] = value.BottomRight;
                into[3] = value.BottomLeft;
            }

            public override CornerRadius Join(ReadOnlySpan<double> c) =>
                new(Math.Max(0, c[0]), Math.Max(0, c[1]), Math.Max(0, c[2]), Math.Max(0, c[3]));
        }
    }
}

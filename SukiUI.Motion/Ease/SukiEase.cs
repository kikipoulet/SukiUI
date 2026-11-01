using System;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    /// <summary>
    /// An easing function f(t), with f(0) = 0 and f(1) = 1, usable anywhere Avalonia takes an
    /// <see cref="Easing"/>: a Transition, a KeyFrame, or <c>Over(...).WithEasing(...)</c>.
    /// <para>
    /// The catalog (<see cref="Signature"/>, <see cref="Classic"/>, <see cref="DesignLibraries"/>) holds the
    /// well-known curves, each one the published formula. A few small modifiers (<see cref="Reverse"/>,
    /// <see cref="BounceOffTarget"/>, …) turn one curve into another and chain fluently:
    /// <c>SukiEase.Classic.Back.Out.BounceOffTarget()</c>.
    /// </para>
    /// </summary>
    public sealed partial class SukiEase : Easing, IEquatable<SukiEase>
    {
        private readonly Func<double, double> _function;

        /// <summary>Wraps your own function. It is only called for t strictly between 0 and 1.</summary>
        public SukiEase(string name, Func<double, double> function)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(function);
            Name = name;
            _function = function;
        }

        /// <summary>Where the curve comes from: its path in the catalog, followed by the modifiers applied to it.</summary>
        public string Name { get; }

        public override double Ease(double progress) =>
            progress <= 0.0 ? 0.0 : progress >= 1.0 ? 1.0 : _function(progress);

        // ---- modifiers --------------------------------------------------------------------

        /// <summary>The curve turned over, <c>1 - f(1 - t)</c>: an ease-out becomes the matching ease-in and
        /// the other way round. The way to get the closing animation of an opening one.</summary>
        public SukiEase Reverse() => Derive("Reverse()", t => 1.0 - Ease(1.0 - t));

        /// <summary>Plays the curve over the first half and its mirror image over the second, so an
        /// ease-in (or ease-out) becomes an ease-in-out.</summary>
        public SukiEase InOut() => Derive("InOut()", t => t < 0.5 ? Ease(2.0 * t) / 2.0 : 1.0 - Ease(2.0 - 2.0 * t) / 2.0);

        /// <summary>Puts a wall on the target: what would go beyond it is mirrored back inside, like a ball
        /// dropped on the floor. Turns an overshoot into a bounce.</summary>
        public SukiEase BounceOffTarget() => Derive("BounceOffTarget()", t => Fold(Ease(t)));

        /// <summary>Cuts what goes beyond the target or before the start: the curve keeps its shape
        /// but stays between 0 and 1. Handy for properties that cannot overshoot, like an opacity.</summary>
        public SukiEase NoOvershoot() => Derive("NoOvershoot()", t => Math.Clamp(Ease(t), 0.0, 1.0));

        /// <summary>Brings the curve closer to the straight line, keeping its character: <paramref name="amount"/>
        /// 0 changes nothing, 1 is the linear curve, in between is the mix. A bounce shrinks along the way,
        /// since the straight line has none.</summary>
        public SukiEase Softer(double amount)
        {
            amount = Amount(amount);
            if (amount == 0.0) return this;
            return Derive(Text("Softer", amount), t => t + (Ease(t) - t) * (1.0 - amount));
        }

        /// <summary>Exaggerates the contrast between the fast and the slow parts of the curve: its speed is raised to
        /// the power 1 + <paramref name="amount"/> (and the curve rescaled to still end on 1). 0 changes nothing; at 1
        /// an ease-out gets steeper and its landing longer, an ease-in waits longer then goes faster, an ease-in-out is
        /// more marked. For the power curves it is exactly the next family (<c>Cubic.Out.Stronger(1)</c> is
        /// <c>Quint.Out</c>), and a straight line is left as it is. Chain it for more. The curve never gets more turns
        /// than it had. What goes beyond the target (or before the start) is slow, so it comes back toward the target:
        /// a bounce calms down.</summary>
        public SukiEase Stronger(double amount)
        {
            amount = Amount(amount);
            if (amount == 0.0) return this;
            return Derive(Text("Stronger", amount), SpeedPower(1.0 + amount));
        }

        private Func<double, double> SpeedPower(double power)
        {
            // the speed of the curve on 2048 steps, raised to the power (keeping its sign), summed back up
            const int n = 2048;
            var speed = new double[n];
            var level = new double[n + 1];
            double previous = 0.0;
            for (int i = 0; i < n; i++)
            {
                double next = Ease((i + 1.0) / n);
                double v = (next - previous) * n;
                speed[i] = Math.CopySign(Math.Pow(Math.Abs(v), power), v);
                level[i + 1] = level[i] + speed[i] / n;
                previous = next;
            }

            // a curve that makes (almost) no progress on balance, like a wild elastic, has nothing to rescale
            double total = level[n];
            if (total < 0.2) return t => Ease(t);

            double Slope(int knot) => (speed[Math.Max(knot - 1, 0)] + speed[Math.Min(knot, n - 1)]) / 2.0 / total;
            return t =>
            {
                double x = t * n;
                int i = Math.Min((int)x, n - 1);
                double u = x - i, u2 = u * u, u3 = u2 * u, h = 1.0 / n;
                double from = level[i] / total, to = level[i + 1] / total;
                // cubic Hermite between two steps: the value of the sum and its slope at both ends
                double value = (2 * u3 - 3 * u2 + 1) * from + (u3 - 2 * u2 + u) * h * Slope(i)
                               + (-2 * u3 + 3 * u2) * to + (u3 - u2) * h * Slope(i + 1);
                // where the curve only goes one way, it does not wiggle between two steps
                bool rises = speed[Math.Max(i - 1, 0)] >= 0.0 && speed[i] >= 0.0 && speed[Math.Min(i + 1, n - 1)] >= 0.0;
                bool falls = speed[Math.Max(i - 1, 0)] <= 0.0 && speed[i] <= 0.0 && speed[Math.Min(i + 1, n - 1)] <= 0.0;
                return rises || falls ? Math.Clamp(value, Math.Min(from, to), Math.Max(from, to)) : value;
            };
        }

        private static double Amount(double amount) =>
            amount >= 0.0 && amount <= 1.0 ? amount
                : throw new ArgumentOutOfRangeException(nameof(amount), amount, "An amount goes from 0 (no change) to 1 (full effect).");

        private static string Text(string modifier, double amount) =>
            FormattableString.Invariant($"{modifier}({amount:0.##})");

        /// <summary>The same curve under a new name: a preset built from modifiers.</summary>
        internal SukiEase Named(string name) => new(name, _function);

        private SukiEase Derive(string modifier, Func<double, double> function) =>
            new($"{Name}.{modifier}", function);

        private static double Fold(double value)
        {
            if (value <= 1.0) return value;
            double over = (value - 1.0) % 2.0; // a wall on the target: out and back, as many times as needed
            return over <= 1.0 ? 1.0 - over : over - 1.0;
        }

        // ---- reading ----------------------------------------------------------------------

        /// <summary>The curve at <paramref name="count"/> evenly spaced moments, first and last included.</summary>
        public double[] Trace(int count)
        {
            if (count < 2) throw new ArgumentOutOfRangeException(nameof(count), count, "A trace needs at least 2 points.");
            var points = new double[count];
            for (int i = 0; i < count; i++) points[i] = Ease((double)i / (count - 1));
            return points;
        }

        /// <summary>The speed of the curve at <paramref name="t"/>, in moves per unit of duration.</summary>
        public double VelocityAt(double t)
        {
            const double h = 1e-4;
            double a = Math.Clamp(t - h, 0.0, 1.0), b = Math.Clamp(t + h, 0.0, 1.0);
            return b > a ? (Ease(b) - Ease(a)) / (b - a) : 0.0;
        }

        // ---- identity ---------------------------------------------------------------------

        public override string ToString() => Name;

        public bool Equals(SukiEase? other) => other is not null && Name == other.Name;

        public override bool Equals(object? obj) => obj is SukiEase other && Equals(other);

        public override int GetHashCode() => Name.GetHashCode();
    }
}

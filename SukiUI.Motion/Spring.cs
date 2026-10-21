using System;

namespace SukiUI.Motion
{
    /// <summary>
    /// The damped-spring parameters of a trajectory: <c>x'' = -omega²(x - target) - decay·x'</c>.
    /// The low-level truth of the engine — the exact pixels of the integrated physics.
    /// SwiftUI-style sugar (<c>Spring(duration:, bounce:)</c>) is deliberately absent
    /// for now: it is a spelling convenience on top of these two numbers.
    /// Both parameters must be finite and positive: a spring without stiffness or without
    /// damping never settles, and would keep the frame loop awake forever.
    /// </summary>
    public readonly record struct Spring
    {
        private readonly double _omega, _decay;

        public Spring(double Omega, double Decay)
        {
            this.Omega = Omega;
            this.Decay = Decay;
        }

        /// <summary>Angular frequency in rad/s. Higher = snappier.</summary>
        public double Omega
        {
            get => _omega;
            init => _omega = double.IsFinite(value) && value > 0 ? value
                : throw new ArgumentOutOfRangeException(nameof(Omega), value, "Spring stiffness (omega) must be finite and > 0.");
        }

        /// <summary>Damping in 1/s (<c>2·zeta·omega</c>). Higher = fewer/softer overshoots.</summary>
        public double Decay
        {
            get => _decay;
            init => _decay = double.IsFinite(value) && value > 0 ? value
                : throw new ArgumentOutOfRangeException(nameof(Decay), value, "Spring damping (decay) must be finite and > 0 — an undamped spring never settles.");
        }

        /// <summary>True for a spring built through the constructor; false for <c>default(Spring)</c>.</summary>
        internal bool IsValid => _omega > 0 && _decay > 0;

        public void Deconstruct(out double Omega, out double Decay)
        {
            Omega = _omega;
            Decay = _decay;
        }
    }

    /// <summary>
    /// The shared integrator: semi-implicit Euler with substeps capped at 8 ms — the exact
    /// numeric behavior of the proven engine, so trajectories settle to the same poses at the
    /// same substep cadence. Stiff or heavily damped springs get finer substeps: the scheme
    /// is only stable while omega·h and decay·h stay small (both kept ≤ 1 here). Time base
    /// is caller-provided <c>dt</c>, always derived from <see cref="SukiTicker"/> so every
    /// spring shares the same monotonic clock.
    /// </summary>
    internal static class Integrator
    {
        /// <summary>Settle threshold on position (|x - target|).</summary>
        internal const double SettlePosition = 0.0005;

        /// <summary>Settle threshold on velocity.</summary>
        internal const double SettleVelocity = 0.02;

        /// <summary>The historical substep cap — unchanged for every spring soft enough.</summary>
        private const double MaxSubstep = 0.008;

        internal static void Step(ref double x, ref double v, double target, double dt, in Spring spring)
        {
            // omega·h ≤ 1 and decay·h ≤ 1 keep semi-implicit Euler stable for any spring
            // (its update matrix has |eigenvalues| < 1 there); softer springs keep 8 ms.
            double maxStep = Math.Min(MaxSubstep, 1.0 / Math.Max(spring.Omega, spring.Decay));
            int steps = Math.Max(1, (int)Math.Ceiling(dt / maxStep));
            double h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                double accel = -spring.Omega * spring.Omega * (x - target) - spring.Decay * v;
                v += accel * h;
                x += v * h;
            }
        }

        internal static double Lerp(double from, double to, double t) => from + (to - from) * t;
    }
}

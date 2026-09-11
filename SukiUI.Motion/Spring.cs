using System;

namespace SukiUI.Motion
{
    /// <summary>
    /// The damped-spring parameters of a trajectory: <c>x'' = -omega²(x - target) - decay·x'</c>.
    /// The low-level truth of the engine — the exact pixels of the integrated physics.
    /// SwiftUI-style sugar (<c>Spring(duration:, bounce:)</c>) is deliberately absent
    /// for now: it is a spelling convenience on top of these two numbers.
    /// </summary>
    public readonly record struct Spring(double Omega, double Decay);

    /// <summary>
    /// The shared integrator: semi-implicit Euler with substeps capped at 8 ms — the exact
    /// numeric behavior of the proven engine, so trajectories settle to the same poses at the
    /// same substep cadence. Time base is caller-provided <c>dt</c>, always derived from
    /// <see cref="SukiTicker"/> so every spring shares the same monotonic clock.
    /// </summary>
    internal static class Integrator
    {
        /// <summary>Settle threshold on position (|x - target|).</summary>
        internal const double SettlePosition = 0.0005;

        /// <summary>Settle threshold on velocity.</summary>
        internal const double SettleVelocity = 0.02;

        internal static void Step(ref double x, ref double v, double target, double dt, in Spring spring)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(dt / 0.008));
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

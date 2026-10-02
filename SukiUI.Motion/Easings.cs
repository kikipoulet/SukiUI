using System;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    /// <summary>
    /// A physically-derived ease-out: the exact closed-form solution of the damped spring
    /// the engine integrates (<c>x'' = -omega^2·(x - target) - decay·x'</c>), normalized so
    /// the motion starts at 0 and settles at 1 within t ∈ [0, 1] regardless of the
    /// transition duration it drives: the whole spring stretches or compresses with the
    /// Duration it is attached to.
    ///
    /// Unlike a generic ease-out, the initial acceleration is real (force pulls toward the
    /// target from rest), the arrival decelerates on genuine stored momentum, and there is
    /// exactly one gentle overshoot before settling — no artificial curve shape.
    ///
    /// Defaults are calibrated for dialog-sized surfaces: zeta = 0.7 (a single ~4.6%
    /// overshoot) with the envelope under ~1.2% by t = 1, so the trajectory can end on the
    /// target without a visible snap.
    /// </summary>
    public class SukiSpringEaseOut : Easing
    {
        // decay = 2 * zeta * omega with zeta = 0.7.
        private const double DefaultOmega = 6.43;
        private const double DefaultDecay = 9.0;

        /// <summary>Angular frequency in rad/s of normalized time. Higher = snappier.</summary>
        public double Omega { get; set; } = DefaultOmega;

        /// <summary>Damping in 1/s of normalized time. Higher = fewer/softer overshoots.</summary>
        public double Decay { get; set; } = DefaultDecay;

        public override double Ease(double t)
        {
            if (t <= 0.0)
                return 0.0;
            if (t >= 1.0)
                return 1.0;
            // Renormalized against the value at t = 1 so the curve lands EXACTLY on 1:
            // a truncated spring otherwise ends mid-decay, and the trajectory's final
            // write to the target value reads as a snap. Both endpoints stay anchored
            // (x(0) = 0) and the shape is preserved to within the residual envelope.
            double end = Spring(1.0);
            double raw = Spring(t);
            return Math.Abs(end) > 0.5 ? raw / end : raw;
        }

        private double Spring(double t)
        {
            // Step response from rest of x'' = -omega²(x - 1) - decay·x', all three regimes.
            double a = Decay / 2.0; // zeta·omega
            double disc = Omega * Omega - a * a;
            double scale = Omega * Omega;

            if (Math.Abs(disc) <= 1e-9 * scale)
            {
                // Critically damped: monotonic, the fastest approach without overshoot.
                return 1.0 - Math.Exp(-a * t) * (1.0 + a * t);
            }

            if (disc > 0)
            {
                // Underdamped: x(t) = 1 - e^(-a·t) (cos(wd·t) + a/wd · sin(wd·t)).
                double wd = Math.Sqrt(disc);
                return 1.0 - Math.Exp(-a * t) * (Math.Cos(wd * t) + a / wd * Math.Sin(wd * t));
            }

            // Overdamped: two real decaying modes r1,2 = -a ± wo. Written with plain
            // exponentials of negative rates (never cosh/sinh of large arguments), so it
            // cannot overflow for stiff springs: x(t) = 1 + (r2·e^(r1·t) - r1·e^(r2·t)) / (r1 - r2).
            double wo = Math.Sqrt(-disc);
            double r1 = -a + wo, r2 = -a - wo;
            return 1.0 + (r2 * Math.Exp(r1 * t) - r1 * Math.Exp(r2 * t)) / (r1 - r2);
        }
    }

    /// <summary>
    /// True spring-physics easing for a tension (press-in) phase: a damped harmonic
    /// oscillator, <c>1 - e^(-damping·t) · cos(frequency·t)</c>. Higher damping = less
    /// oscillation (snappy), higher frequency = faster response.
    /// </summary>
    public class SukiEaseElasticIn : Easing
    {
        public double Damping { get; set; } = 10.0;

        public double Frequency { get; set; } = 25.0;

        public override double Ease(double progress)
        {
            if (progress <= 0) return 0;
            if (progress >= 1) return 1;

            // EaseIn = 1 - EaseOut(1 - t)
            double t = 1.0 - progress;
            double raw = 1.0 - Math.Exp(-Damping * t) * Math.Cos(Frequency * t);
            double rawAt1 = 1.0 - Math.Exp(-Damping) * Math.Cos(Frequency);

            if (Math.Abs(rawAt1) < 1e-10)
                return progress;

            return 1.0 - raw / rawAt1;
        }
    }
}

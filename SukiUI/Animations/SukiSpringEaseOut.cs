using System;
using Avalonia.Animation.Easings;

namespace SukiUI.Animations
{
    /// <summary>
    /// A physically-derived ease-out: the exact closed-form solution of the damped spring
    /// used across the library (SukiButtonPress, SukiComboBoxPopup, SukiComboBoxPress —
    /// <c>x'' = -omega^2·(x - target) - decay·x'</c>), normalized so the motion starts at 0
    /// and settles at 1 within t ∈ [0, 1] regardless of the transition duration it drives:
    /// the whole spring stretches or compresses with the Duration it is attached to.
    ///
    /// Unlike a generic ease-out, the initial acceleration is real (force pulls toward the
    /// target from rest), the arrival decelerates on genuine stored momentum, and there is
    /// exactly one gentle overshoot before settling — no artificial curve shape.
    ///
    /// Defaults are calibrated for the SukiDialog host: zeta = 0.7 (a single ~4.6%
    /// overshoot) with the envelope under ~1.2% by t = 1, so the transition can end on the
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
            // a truncated spring otherwise ends mid-decay, and the transition's final
            // write to the target value reads as a snap. Both endpoints stay anchored
            // (x(0) = 0) and the shape is preserved to within the residual envelope.
            double end = Spring(1.0);
            double raw = Spring(t);
            return Math.Abs(end) > 0.5 ? raw / end : raw;
        }

        private double Spring(double t)
        {
            double zetaOmega = Decay / 2.0;
            double omegaD = Math.Sqrt(Math.Max(Omega * Omega - zetaOmega * zetaOmega, 1e-12));
            double envelope = Math.Exp(-zetaOmega * t);
            // x(t) = 1 - e^(-zeta*omega*t) * (cos(omega_d*t) + zeta*omega/omega_d * sin(omega_d*t))
            // For zeta >= 1 the sine term degenerates to zeta*omega*t (critical damping) —
            // monotonic approach, no overshoot at all.
            return 1.0 - envelope * (Math.Cos(omegaD * t) + zetaOmega / omegaD * Math.Sin(omegaD * t));
        }
    }
}

using System;

namespace SukiUI.Motion
{
    /// <summary>The raw formulas of the catalog, each with f(0) = 0 and f(1) = 1. The Penner ones are
    /// those published on easings.net; the "out" and "in-out" forms are derived from the "in" form
    /// wherever the published formula is exactly that derivation.</summary>
    internal static class Shapes
    {
        // ---- cubic Bézier, as published by every design tool -------------------------------

        public static Func<double, double> Bezier(double x1, double y1, double x2, double y2)
        {
            double Axis(double s, double p1, double p2)
            {
                double u = 1.0 - s;
                return 3.0 * u * u * s * p1 + 3.0 * u * s * s * p2 + s * s * s;
            }

            return t =>
            {
                double lo = 0.0, hi = 1.0, s = t;
                for (int i = 0; i < 48; i++)
                {
                    double x = Axis(s, x1, x2);
                    if (Math.Abs(x - t) < 1e-12) break;
                    if (x < t) lo = s; else hi = s;
                    s = 0.5 * (lo + hi);
                }
                return Axis(s, y1, y2);
            };
        }

        // ---- the Penner families: the "in" formula, the rest follows --------------------------

        public static Func<double, double> Out(Func<double, double> ease) => t => 1.0 - ease(1.0 - t);

        public static Func<double, double> InOut(Func<double, double> ease) =>
            t => t < 0.5 ? ease(2.0 * t) / 2.0 : 1.0 - ease(2.0 - 2.0 * t) / 2.0;

        public static double SineIn(double t) => 1.0 - Math.Cos(t * Math.PI / 2.0);
        public static double QuadIn(double t) => t * t;
        public static double CubicIn(double t) => t * t * t;
        public static double QuartIn(double t) => t * t * t * t;
        public static double QuintIn(double t) => t * t * t * t * t;
        public static double ExpoIn(double t) => Math.Pow(2.0, 10.0 * t - 10.0);
        public static double CircIn(double t) => 1.0 - Math.Sqrt(1.0 - t * t);

        private const double C1 = 1.70158;
        private const double C2 = C1 * 1.525;
        private const double C3 = C1 + 1.0;
        private const double C4 = 2.0 * Math.PI / 3.0;
        private const double C5 = 2.0 * Math.PI / 4.5;

        public static double BackIn(double t) => C3 * t * t * t - C1 * t * t;

        public static double BackInOut(double t) => t < 0.5
            ? Math.Pow(2.0 * t, 2.0) * ((C2 + 1.0) * 2.0 * t - C2) / 2.0
            : (Math.Pow(2.0 * t - 2.0, 2.0) * ((C2 + 1.0) * (t * 2.0 - 2.0) + C2) + 2.0) / 2.0;

        public static double ElasticIn(double t) => -Math.Pow(2.0, 10.0 * t - 10.0) * Math.Sin((t * 10.0 - 10.75) * C4);

        public static double ElasticInOut(double t) => t < 0.5
            ? -(Math.Pow(2.0, 20.0 * t - 10.0) * Math.Sin((20.0 * t - 11.125) * C5)) / 2.0
            : Math.Pow(2.0, -20.0 * t + 10.0) * Math.Sin((20.0 * t - 11.125) * C5) / 2.0 + 1.0;

        public static double BounceOut(double t)
        {
            const double n = 7.5625, d = 2.75;
            if (t < 1.0 / d) return n * t * t;
            if (t < 2.0 / d) { t -= 1.5 / d; return n * t * t + 0.75; }
            if (t < 2.5 / d) { t -= 2.25 / d; return n * t * t + 0.9375; }
            t -= 2.625 / d;
            return n * t * t + 0.984375;
        }

        public static double BounceIn(double t) => 1.0 - BounceOut(1.0 - t);

        public static double Smoothstep(double t) => t * t * (3.0 - 2.0 * t);
        public static double Smootherstep(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);

        // ---- the Android interpolators (AOSP), with their default tension -----------------------

        public static Func<double, double> Anticipate(double tension) => t => t * t * ((tension + 1.0) * t - tension);

        public static Func<double, double> Overshoot(double tension) => t =>
        {
            t -= 1.0;
            return t * t * ((tension + 1.0) * t + tension) + 1.0;
        };

        public static Func<double, double> AnticipateOvershoot(double tension)
        {
            double s = tension * 1.5;
            return t => t < 0.5
                ? 0.5 * (2.0 * t) * (2.0 * t) * ((s + 1.0) * 2.0 * t - s)
                : 0.5 * ((2.0 * t - 2.0) * (2.0 * t - 2.0) * ((s + 1.0) * (2.0 * t - 2.0) + s) + 2.0);
        }

        // ---- bending the time axis, for the Signature curves -------------------------------------

        private static double WarpStart(double x, double k)
        {
            if (x <= 0.0) return 0.0;
            if (k == 1.0) return x;
            if (x >= 1.0) return 1.0;
            return Math.Pow(x, 1.0 + (k - 1.0) * (1.0 - x) * (1.0 - x));
        }

        /// <summary>The curve with its time axis bent at the start and at the landing: above 1 is softer, below 1 sharper.</summary>
        public static Func<double, double> Warp(Func<double, double> curve, double start, double landing) =>
            t => curve(WarpStart(1.0 - WarpStart(1.0 - t, landing), start));

        // ---- the damped spring, as a shape ---------------------------------------------------

        private const double SettledWithin = 0.002;

        /// <summary>The step response of a unit-mass spring with the given damping ratio, with its
        /// time axis stretched so the motion has settled (within 0.2 %) at t = 1: the shape
        /// only, the duration belongs to whoever plays it. The leftover 0.2 % is spread over the
        /// curve so that it lands exactly on 1.</summary>
        public static Func<double, double> Spring(double dampingRatio)
        {
            double z = dampingRatio;
            double Step(double x)
            {
                if (Math.Abs(z - 1.0) < 1e-6)
                    return 1.0 - Math.Exp(-x) * (1.0 + x);
                if (z < 1.0)
                {
                    double wd = Math.Sqrt(1.0 - z * z);
                    return 1.0 - Math.Exp(-z * x) * (Math.Cos(wd * x) + z / wd * Math.Sin(wd * x));
                }
                // Overdamped, written with exponentials of negative rates only (no cosh overflow).
                double wo = Math.Sqrt(z * z - 1.0);
                double k = z / wo;
                return 1.0 - 0.5 * ((1.0 + k) * Math.Exp((wo - z) * x) + (1.0 - k) * Math.Exp(-(wo + z) * x));
            }

            double slowest = z < 1.0 ? z : z - Math.Sqrt(Math.Max(0.0, z * z - 1.0));
            slowest = Math.Max(slowest, 0.02);
            double limit = 14.0 / slowest, settle = limit;
            const double dx = 0.004;
            for (double x = limit; x > 0.0; x -= dx)
                if (Math.Abs(1.0 - Step(x)) > SettledWithin) { settle = x; break; }

            double residual = 1.0 - Step(settle);
            return t => Step(settle * t) + residual * t * t * t;
        }
    }
}

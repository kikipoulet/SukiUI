namespace SukiUI.Motion.Tests;

public class EasingTests
{
    // SukiSpringEaseOut claims the exact closed-form damped spring, but for zeta > 1
    // it clamps omega_d to ~0 and evaluates the CRITICALLY damped curve instead of the
    // overdamped one (cosh/sinh) — far too fast for a heavy spring. SukiDialogMotion feeds
    // it zeta up to 1.05 (default profile) and 1.3 (alternate profile).
    [Theory]
    [InlineData(6.43, 9.0)]     // zeta 0.70 — underdamped, the default (control case)
    [InlineData(5.8, 12.18)]    // zeta 1.05 — the large-dialog default profile
    [InlineData(5.8, 15.08)]    // zeta 1.30 — the large-dialog alternate profile
    [InlineData(6.0, 30.0)]     // zeta 2.50 — clearly overdamped
    public void Spring_ease_matches_the_exact_damped_solution(double omega, double decay)
    {
        var ease = new SukiSpringEaseOut { Omega = omega, Decay = decay };
        double end = Exact(omega, decay, 1.0);

        foreach (double t in new[] { 0.1, 0.25, 0.5, 0.75, 0.9 })
            Assert.Equal(Exact(omega, decay, t) / end, ease.Ease(t), precision: 3);
    }

    /// <summary>Step response of x'' = -omega²(x - 1) - decay·x' from rest at 0.</summary>
    private static double Exact(double omega, double decay, double t)
    {
        double a = decay / 2.0;           // zeta·omega
        double zeta = a / omega;
        if (Math.Abs(zeta - 1.0) < 1e-9)
            return 1.0 - Math.Exp(-a * t) * (1.0 + a * t);
        if (zeta < 1.0)
        {
            double wd = omega * Math.Sqrt(1.0 - zeta * zeta);
            return 1.0 - Math.Exp(-a * t) * (Math.Cos(wd * t) + a / wd * Math.Sin(wd * t));
        }
        double wo = omega * Math.Sqrt(zeta * zeta - 1.0);
        return 1.0 - Math.Exp(-a * t) * (Math.Cosh(wo * t) + a / wo * Math.Sinh(wo * t));
    }
}

using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

public class SpringTests
{
    // The integrator's substep is capped at 8 ms whatever the stiffness. Explicit
    // (semi-implicit Euler) integration is only stable for roughly omega·h < 2 and
    // decay·h < 2, so a stiff or heavily damped spring diverges instead of settling.
    [AvaloniaTheory]
    [InlineData(400.0, 560.0)] // stiff: omega·h = 3.2
    [InlineData(60.0, 400.0)]  // heavily overdamped: decay·h = 3.2 (slow mode ~9/s: settles well within 3 s)
    public void Stiff_or_heavily_damped_spring_settles_on_its_target(double omega, double decay)
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;

        scale.Offer(scale.To(1.1).Spring(new Spring(omega, decay)));
        h.Run(TimeSpan.FromSeconds(3));

        Assert.Equal(1.1, scale.Value, precision: 3);
    }

    // Nothing rejects parameters for which a spring can never settle — zero or
    // negative decay oscillates (or grows) forever and keeps the frame loop awake for good.
    [Theory]
    [InlineData(10.0, 0.0)]
    [InlineData(10.0, -1.0)]
    [InlineData(0.0, 5.0)]
    [InlineData(double.NaN, 5.0)]
    [InlineData(10.0, double.PositiveInfinity)]
    public void Spring_rejects_parameters_that_can_never_settle(double omega, double decay)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Spring(omega, decay));
    }

    // The validation runs in the init accessors, so `with` keeps working and is validated too.
    [Fact]
    public void With_expressions_keep_working_and_are_validated()
    {
        var spring = new Spring(10, 6);

        Assert.Equal(new Spring(20, 6), spring with { Omega = 20 });
        Assert.Throws<ArgumentOutOfRangeException>(() => spring with { Decay = 0 });
        var (omega, decay) = spring;
        Assert.Equal((10.0, 6.0), (omega, decay));
    }
}

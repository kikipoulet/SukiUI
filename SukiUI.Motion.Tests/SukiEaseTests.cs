using Avalonia.Animation.Easings;

namespace SukiUI.Motion.Tests;

public class SukiEaseTests
{
    public static IEnumerable<object[]> AllPresets() => SukiEase.Presets.Select(p => new object[] { p.Path });

    private static readonly double[] Moments = { 0.05, 0.13, 0.25, 0.37, 0.5, 0.62, 0.75, 0.88, 0.97 };

    // The reference formulas, as published by easings.net (written out in full, not derived).
    private const double C1 = 1.70158, C2 = C1 * 1.525, C3 = C1 + 1, C4 = 2 * Math.PI / 3, C5 = 2 * Math.PI / 4.5;

    private static double BounceOut(double x)
    {
        const double n1 = 7.5625, d1 = 2.75;
        if (x < 1 / d1) return n1 * x * x;
        if (x < 2 / d1) return n1 * (x -= 1.5 / d1) * x + 0.75;
        if (x < 2.5 / d1) return n1 * (x -= 2.25 / d1) * x + 0.9375;
        return n1 * (x -= 2.625 / d1) * x + 0.984375;
    }

    public static IEnumerable<object[]> Reference() => new (string, Func<double, double>)[]
    {
        ("Classic.Sine.In", x => 1 - Math.Cos(x * Math.PI / 2)),
        ("Classic.Sine.Out", x => Math.Sin(x * Math.PI / 2)),
        ("Classic.Sine.InOut", x => -(Math.Cos(Math.PI * x) - 1) / 2),
        ("Classic.Quad.In", x => x * x),
        ("Classic.Quad.Out", x => 1 - (1 - x) * (1 - x)),
        ("Classic.Quad.InOut", x => x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2),
        ("Classic.Cubic.In", x => x * x * x),
        ("Classic.Cubic.Out", x => 1 - Math.Pow(1 - x, 3)),
        ("Classic.Cubic.InOut", x => x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2),
        ("Classic.Quart.In", x => x * x * x * x),
        ("Classic.Quart.Out", x => 1 - Math.Pow(1 - x, 4)),
        ("Classic.Quart.InOut", x => x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2),
        ("Classic.Quint.In", x => x * x * x * x * x),
        ("Classic.Quint.Out", x => 1 - Math.Pow(1 - x, 5)),
        ("Classic.Quint.InOut", x => x < 0.5 ? 16 * x * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 5) / 2),
        ("Classic.Expo.In", x => Math.Pow(2, 10 * x - 10)),
        ("Classic.Expo.Out", x => 1 - Math.Pow(2, -10 * x)),
        ("Classic.Expo.InOut", x => x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2 : (2 - Math.Pow(2, -20 * x + 10)) / 2),
        ("Classic.Circ.In", x => 1 - Math.Sqrt(1 - Math.Pow(x, 2))),
        ("Classic.Circ.Out", x => Math.Sqrt(1 - Math.Pow(x - 1, 2))),
        ("Classic.Circ.InOut", x => x < 0.5
            ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2
            : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2),
        ("Classic.Back.In", x => C3 * x * x * x - C1 * x * x),
        ("Classic.Back.Out", x => 1 + C3 * Math.Pow(x - 1, 3) + C1 * Math.Pow(x - 1, 2)),
        ("Classic.Back.InOut", x => x < 0.5
            ? Math.Pow(2 * x, 2) * ((C2 + 1) * 2 * x - C2) / 2
            : (Math.Pow(2 * x - 2, 2) * ((C2 + 1) * (x * 2 - 2) + C2) + 2) / 2),
        ("Classic.Elastic.In", x => -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * C4)),
        ("Classic.Elastic.Out", x => Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * C4) + 1),
        ("Classic.Elastic.InOut", x => x < 0.5
            ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * C5)) / 2
            : Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * C5) / 2 + 1),
        ("Classic.Bounce.In", x => 1 - BounceOut(1 - x)),
        ("Classic.Bounce.Out", BounceOut),
        ("Classic.Bounce.InOut", x => x < 0.5 ? (1 - BounceOut(1 - 2 * x)) / 2 : (1 + BounceOut(2 * x - 1)) / 2),
        ("Classic.Smoothstep", x => x * x * (3 - 2 * x)),
        ("Classic.Smootherstep", x => x * x * x * (x * (x * 6 - 15) + 10)),
        // AOSP interpolators, tension 2
        ("DesignLibraries.Android.Anticipate", x => x * x * (3 * x - 2)),
        ("DesignLibraries.Android.Overshoot", x => { x -= 1; return x * x * (3 * x + 2) + 1; }),
    }.Select(r => new object[] { r.Item1, r.Item2 });

    [Fact]
    public void The_tree_holds_the_presets_of_every_group()
    {
        var groups = SukiEase.Presets.Select(p => p.Path.Split('.')[0]).Distinct().ToList();
        Assert.Equal(new[] { "Signature", "Classic", "DesignLibraries" }, groups);
        Assert.True(SukiEase.Presets.Count >= 80, $"only {SukiEase.Presets.Count} presets");
        Assert.Equal(SukiEase.Presets.Count, SukiEase.Presets.Select(p => p.Path).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(AllPresets))]
    public void Every_preset_starts_at_0_ends_at_1_and_is_found_by_its_name(string path)
    {
        var ease = SukiEase.Find(path)!;
        Assert.NotNull(ease);
        Assert.Equal(path, ease.ToString());
        Assert.Equal(0.0, ease.Ease(0.0));
        Assert.Equal(1.0, ease.Ease(1.0));
        Assert.Equal(0.0, ease.Ease(-0.5));
        Assert.Equal(1.0, ease.Ease(1.5));
        foreach (double v in ease.Trace(200))
        {
            Assert.True(double.IsFinite(v), path);
            Assert.InRange(v, -1.0, 3.0);
        }
    }

    [Theory]
    [MemberData(nameof(Reference))]
    public void The_catalog_matches_the_published_formulas(string path, Func<double, double> reference)
    {
        var ease = SukiEase.Find(path)!;
        foreach (double t in Moments)
            Assert.Equal(reference(t), ease.Ease(t), precision: 9);
    }

    [Fact]
    public void Android_anticipate_overshoot_matches_the_aosp_formula()
    {
        double A(double t, double s) => t * t * ((s + 1) * t - s);
        double O(double t, double s) => t * t * ((s + 1) * t + s);
        var ease = SukiEase.DesignLibraries.Android.AnticipateOvershoot;
        foreach (double t in Moments)
        {
            double expected = t < 0.5 ? 0.5 * A(t * 2, 3) : 0.5 * (O(t * 2 - 2, 3) + 2);
            Assert.Equal(expected, ease.Ease(t), precision: 9);
        }
    }

    [Fact]
    public void Beziers_follow_the_css_definition()
    {
        // linear control points give the identity
        var linear = SukiEase.CubicBezier(0.25, 0.25, 0.75, 0.75);
        foreach (double t in Moments) Assert.Equal(t, linear.Ease(t), precision: 8);

        // symmetric curves cross the middle
        Assert.Equal(0.5, SukiEase.Classic.Css.EaseInOut.Ease(0.5), precision: 8);
        Assert.Equal(0.5, SukiEase.DesignLibraries.Fluent.EasyEase.Ease(0.5), precision: 8);

        // a known point, solved by hand: ease-out (0, 0, 0.58, 1) has x(s) = 0.5 at s = 0.625, where y = 0.6836
        Assert.Equal(0.6836, SukiEase.Classic.Css.EaseOut.Ease(0.5), precision: 2);

        // the Material expressive-style overshoot (control point above 1) really overshoots
        Assert.True(SukiEase.CubicBezier(0.38, 1.21, 0.22, 1.0).Trace(100).Max() > 1.0);
    }

    [Fact]
    public void Springs_arrive_on_the_target_and_ring_less_when_more_damped()
    {
        Assert.True(SukiEase.Spring(1.0).Trace(400).Max() <= 1.0 + 1e-9);
        double loose = SukiEase.Spring(0.3).Trace(400).Max(), tight = SukiEase.Spring(0.7).Trace(400).Max();
        Assert.True(loose > tight && tight > 1.0, $"{loose} / {tight}");
        Assert.InRange(SukiEase.Spring(0.5).Trace(400).Max() - 1.0, 0.1, 0.2); // exp(-pi*0.5/sqrt(0.75)) = 16 %
        Assert.Throws<ArgumentOutOfRangeException>(() => SukiEase.Spring(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SukiEase.CubicBezier(1.5, 0, 0, 1));
    }

    [Fact]
    public void The_signature_curves_keep_their_character()
    {
        static bool Rises(SukiEase e) { var p = e.Trace(600); return p.Zip(p.Skip(1), (a, b) => b >= a - 1e-9).All(x => x); }

        Assert.Same(SukiEase.Signature.Smooth, SukiEase.Default);
        Assert.True(SukiEase.Signature.Smooth.Trace(600).Max() <= 1.0 + 1e-9);
        Assert.True(SukiEase.Signature.Springy.Trace(600).Max() > 1.1);
        Assert.True(SukiEase.Signature.Jelly.Trace(600).Max() > SukiEase.Signature.Bouncy.Trace(600).Max() - 0.2);

        // the drop is a ball: it hits the target and comes back without ever passing it
        Assert.All(SukiEase.Signature.Drop.Trace(600), v => Assert.True(v <= 1.0 + 1e-9));
        Assert.Equal("Signature.Drop", SukiEase.Signature.Drop.ToString());

        // the time-shaped ones are still a smooth rise: softer at the start / landing, sharper landing for Magnetic
        Assert.All(new[] { SukiEase.Signature.Glide, SukiEase.Signature.SoftClose, SukiEase.Signature.Magnetic }, e => Assert.True(Rises(e), e.Name));
        Assert.True(SukiEase.Signature.SoftClose.VelocityAt(0.98) < SukiEase.Signature.Smooth.VelocityAt(0.98));
        Assert.True(SukiEase.Signature.Glide.VelocityAt(0.02) < SukiEase.Signature.Smooth.VelocityAt(0.02));
        Assert.True(SukiEase.Signature.Magnetic.VelocityAt(0.98) > SukiEase.Signature.Smooth.VelocityAt(0.98));
    }

    [Fact]
    public void It_is_an_avalonia_easing()
    {
        Easing easing = SukiEase.Classic.Cubic.Out;
        Assert.Equal(1.0 - Math.Pow(1.0 - 0.3, 3), easing.Ease(0.3), precision: 9);
        Assert.Equal(0.7, new SukiEase("Mine", t => t).Ease(0.7), precision: 12);
    }

    [Fact]
    public void Reverse_turns_an_ease_out_into_the_matching_ease_in_and_back()
    {
        var cubicIn = SukiEase.Classic.Cubic.Out.Reverse();
        foreach (double t in Moments)
            Assert.Equal(t * t * t, cubicIn.Ease(t), precision: 9);
        Assert.Equal(SukiEase.Classic.Cubic.Out.Trace(40), cubicIn.Reverse().Trace(40));
        Assert.Equal("Classic.Cubic.Out.Reverse()", cubicIn.ToString());
    }

    [Fact]
    public void InOut_makes_a_symmetric_curve_out_of_an_in_or_out_curve()
    {
        foreach (double t in Moments)
            Assert.Equal(SukiEase.Classic.Cubic.InOut.Ease(t), SukiEase.Classic.Cubic.In.InOut().Ease(t), precision: 9);
        var quint = SukiEase.Classic.Quint.Out.InOut();
        foreach (double t in Moments)
            Assert.Equal(1.0, quint.Ease(t) + quint.Ease(1.0 - t), precision: 9);
    }

    [Fact]
    public void BounceOffTarget_mirrors_what_goes_beyond_the_target_back_inside()
    {
        var back = SukiEase.Classic.Back.Out;
        var wall = back.BounceOffTarget();
        Assert.True(back.Trace(300).Max() > 1.05);
        Assert.True(wall.Trace(300).Max() <= 1.0);
        foreach (double t in Moments)
        {
            double raw = back.Ease(t);
            Assert.Equal(raw > 1.0 ? 2.0 - raw : raw, wall.Ease(t), precision: 9);
        }

        // a curve that never passes the target is left as it is
        Assert.Equal(SukiEase.Classic.Cubic.Out.Trace(50), SukiEase.Classic.Cubic.Out.BounceOffTarget().Trace(50));

        // the wall is real: a spring that rings turns into a ball that bounces and stays above the floor
        Assert.All(SukiEase.DesignLibraries.Compose.HighBouncy.BounceOffTarget().Trace(500), v => Assert.True(v <= 1.0 + 1e-12));
    }

    [Fact]
    public void NoOvershoot_keeps_the_curve_between_0_and_1()
    {
        var both = SukiEase.Classic.Elastic.InOut.NoOvershoot();
        Assert.All(both.Trace(400), v => Assert.InRange(v, 0.0, 1.0));
        Assert.True(SukiEase.Classic.Elastic.InOut.Trace(400).Any(v => v < 0.0 || v > 1.0));
    }

    [Fact]
    public void Softer_mixes_the_curve_with_the_straight_line()
    {
        var cubic = SukiEase.Classic.Cubic.Out;
        Assert.Same(cubic, cubic.Softer(0));
        foreach (double t in Moments)
        {
            Assert.Equal(t, cubic.Softer(1).Ease(t), precision: 9);
            Assert.Equal((cubic.Ease(t) + t) / 2.0, cubic.Softer(0.5).Ease(t), precision: 9);
        }
        Assert.Equal("Classic.Cubic.Out.Softer(0.5)", cubic.Softer(0.5).ToString());

        // the more softer, the closer to the line, and the character stays: still an ease-out
        double Gap(SukiEase e) => Moments.Sum(t => Math.Abs(e.Ease(t) - t));
        Assert.True(Gap(cubic.Softer(0.7)) < Gap(cubic.Softer(0.3)) && Gap(cubic.Softer(0.3)) < Gap(cubic));
        Assert.All(Moments, t => Assert.True(cubic.Softer(0.7).Ease(t) > t));

        // a bounce shrinks
        double Top(SukiEase e) => e.Trace(500).Max();
        var spring = SukiEase.Signature.Springy;
        Assert.True(Top(spring.Softer(0.1)) < Top(spring) && Top(spring.Softer(0.1)) > 1.0);
        Assert.Equal(1.0, spring.Softer(0.1).Ease(1.0));
    }

    /// <summary>How many times the curve changes direction, ignoring the numerical noise of the flat parts.</summary>
    private static int Turns(SukiEase ease, double deadBand = 1e-7)
    {
        var p = ease.Trace(3000);
        int turns = 0, direction = 0;
        for (int i = 1; i < p.Length; i++)
        {
            double step = p[i] - p[i - 1];
            if (Math.Abs(step) < deadBand) continue;
            int sign = Math.Sign(step);
            if (direction != 0 && sign != direction) turns++;
            direction = sign;
        }
        return turns;
    }

    [Fact]
    public void Stronger_raises_the_family_of_the_power_curves()
    {
        // the speed of a power curve, squared, is the speed of the next one
        foreach (double t in Moments)
        {
            Assert.Equal(SukiEase.Classic.Cubic.Out.Ease(t), SukiEase.Classic.Quad.Out.Stronger(1).Ease(t), precision: 4);
            Assert.Equal(SukiEase.Classic.Quint.Out.Ease(t), SukiEase.Classic.Cubic.Out.Stronger(1).Ease(t), precision: 4);
            Assert.Equal(SukiEase.Classic.Quint.In.Ease(t), SukiEase.Classic.Cubic.In.Stronger(1).Ease(t), precision: 4);
            Assert.Equal(SukiEase.Classic.Quart.Out.Ease(t), SukiEase.Classic.Cubic.Out.Stronger(0.5).Ease(t), precision: 4); // speed^1.5
            Assert.Equal(SukiEase.Classic.Quint.InOut.Ease(t), SukiEase.Classic.Cubic.InOut.Stronger(1).Ease(t), precision: 4);
            Assert.Equal(t, SukiEase.Classic.Linear.Stronger(1).Ease(t), precision: 6); // no character to mark
        }
        Assert.Same(SukiEase.Classic.Cubic.Out, SukiEase.Classic.Cubic.Out.Stronger(0));
        Assert.Equal("Classic.Cubic.Out.Stronger(0.5)", SukiEase.Classic.Cubic.Out.Stronger(0.5).ToString());
    }

    [Fact]
    public void Stronger_marks_the_character_more_and_keeps_the_ends()
    {
        double Gap(SukiEase e) => Moments.Sum(t => Math.Abs(e.Ease(t) - t));
        foreach (var ease in new[] { SukiEase.Classic.Sine.InOut, SukiEase.Classic.Css.Ease, SukiEase.Signature.Smooth, SukiEase.Classic.Expo.Out })
        {
            Assert.True(Gap(ease.Stronger(0.4)) > Gap(ease) && Gap(ease.Stronger(1)) > Gap(ease.Stronger(0.4)), ease.Name);
            Assert.Equal(0.0, ease.Stronger(1).Ease(0.0));
            Assert.Equal(1.0, ease.Stronger(1).Ease(1.0));
        }

        // an ease-out stays an ease-out (above the line), an ease-in stays an ease-in (below it)
        Assert.All(Moments, t => Assert.True(SukiEase.Classic.Quad.Out.Stronger(1).Ease(t) > t));
        Assert.All(Moments, t => Assert.True(SukiEase.Classic.Quad.In.Stronger(1).Ease(t) < t));
    }

    [Theory]
    [MemberData(nameof(AllPresets))]
    public void Stronger_never_adds_turns_nor_leaves_a_rise_and_stays_finite(string path)
    {
        var ease = SukiEase.Find(path)!;
        foreach (double amount in new[] { 0.3, 1.0 })
        {
            var stronger = ease.Stronger(amount);
            Assert.Equal(0.0, stronger.Ease(0.0));
            Assert.Equal(1.0, stronger.Ease(1.0));
            Assert.All(stronger.Trace(400), v => Assert.InRange(v, -2.0, 3.0));
            Assert.True(Turns(stronger) <= Turns(ease), $"{path}: {Turns(ease)} turns became {Turns(stronger)}");
        }
    }

    [Fact]
    public void Stronger_on_a_bouncy_curve_calms_the_bounce_without_replaying_it()
    {
        var spring = SukiEase.Signature.Springy;
        Assert.Equal(3, Turns(spring));
        Assert.Equal(3, Turns(spring.Stronger(0.3)));
        double Top(SukiEase e) => e.Trace(1000).Max();
        Assert.True(Top(spring.Stronger(1)) < Top(spring.Stronger(0.3)) && Top(spring.Stronger(0.3)) < Top(spring));

        // the old trap: playing a curve on itself replays its whole bounce in the first part of the move
        Assert.Equal(6, Turns(SukiEase.Classic.Bounce.Out));
        Assert.Equal(6, Turns(SukiEase.Classic.Bounce.Out.Stronger(1)));
    }

    [Fact]
    public void Softer_and_Stronger_take_an_amount_from_0_to_1()
    {
        var ease = SukiEase.Classic.Cubic.Out;
        Assert.Throws<ArgumentOutOfRangeException>(() => ease.Softer(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ease.Softer(1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ease.Stronger(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ease.Stronger(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ease.Stronger(-1));
        Assert.Equal("Classic.Cubic.Out.Stronger(1).Softer(0.5)", ease.Stronger(1).Softer(0.5).ToString());
    }

    [Fact]
    public void Modifiers_chain_and_leave_the_original_alone()
    {
        var ease = SukiEase.Classic.Back.Out.Reverse().InOut().BounceOffTarget().Softer(0.3).Stronger(0.5);
        Assert.Equal("Classic.Back.Out.Reverse().InOut().BounceOffTarget().Softer(0.3).Stronger(0.5)", ease.ToString());
        Assert.Equal(0.0, ease.Ease(0.0));
        Assert.Equal(1.0, ease.Ease(1.0));
        Assert.Equal("Classic.Back.Out", SukiEase.Classic.Back.Out.ToString());
        Assert.Equal(SukiEase.Classic.Back.Out.Reverse(), SukiEase.Classic.Back.Out.Reverse());
        Assert.NotEqual(SukiEase.Classic.Back.Out, SukiEase.Classic.Back.Out.Reverse());
    }

    [Fact]
    public void Velocity_is_the_slope_of_the_curve()
    {
        Assert.Equal(1.0, SukiEase.Classic.Linear.VelocityAt(0.4), precision: 6);
        Assert.Equal(0.0, SukiEase.Classic.Cubic.In.VelocityAt(0.0), precision: 3);
        Assert.Equal(3.0, SukiEase.Classic.Cubic.In.VelocityAt(1.0), precision: 2);
    }
}

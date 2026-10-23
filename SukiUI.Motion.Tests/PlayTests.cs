using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

namespace SukiUI.Motion.Tests;

/// <summary>One-off motions: Motion.Of(...)…PlayAsync().</summary>
public class PlayTests
{
    private static readonly MotionProperty<double> X = SukiMotion.TranslateXProperty;
    private static readonly MotionProperty<double> Scale = SukiMotion.ScaleProperty;

    [AvaloniaFact]
    public void Then_starts_the_next_step_once_the_previous_one_finished()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var play = Motion.Of(border)
            .Animate(Scale).To(2.0).Over(TimeSpan.FromMilliseconds(100))
            .Then()
            .Animate(X).To(100.0).Over(TimeSpan.FromMilliseconds(100))
            .PlayAsync();
        h.Frames(4);
        Assert.Equal(0.0, X.Read(border)); // still scaling
        h.Frames(10);

        Assert.True(play.Outcome());
        Assert.Equal(2.0, Scale.Read(border));
        Assert.Equal(100.0, X.Read(border));
    }

    [AvaloniaFact]
    public void And_plays_actions_together()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border)
            .Animate(Scale).To(2.0).Over(TimeSpan.FromMilliseconds(160))
            .And()
            .Animate(Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(160))
            .PlayAsync();
        h.Frames(5);

        Assert.InRange(Scale.Read(border), 1.3, 1.7);
        Assert.InRange(border.Opacity, 0.3, 0.7);
    }

    [AvaloniaFact]
    public void Without_a_curve_an_animation_uses_a_smooth_spring()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var play = Motion.Of(border).Animate(Visual.OpacityProperty).To(0.5).PlayAsync();
        h.Run(TimeSpan.FromSeconds(2));

        Assert.True(play.Outcome());
        Assert.Equal(0.5, border.Opacity);
    }

    [AvaloniaFact]
    public void From_poses_a_property_at_rest()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border).Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(160)).PlayAsync();

        Assert.Equal(0.0, border.Opacity); // posed synchronously, before the first frame
    }

    [AvaloniaFact]
    public void From_is_ignored_while_the_property_moves()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border).Animate(X).To(100.0).Over(TimeSpan.FromMilliseconds(200)).PlayAsync();
        h.Frames(6);
        double pose = X.Read(border);
        _ = Motion.Of(border).Animate(X).From(0.0).To(50.0).Over(TimeSpan.FromMilliseconds(200)).PlayAsync();

        Assert.Equal(pose, X.Read(border)); // resumed from the pose on screen
    }

    [AvaloniaFact]
    public void A_motion_on_a_visual_outside_any_window_completes_at_once()
    {
        var border = new Border(); // never shown

        var play = Motion.Of(border)
            .Animate(Scale).From(0.5).To(2.0).Over(TimeSpan.FromSeconds(1))
            .Then()
            .Animate(X).To(30.0)
            .PlayAsync();

        Assert.True(play.IsCompleted && play.Result);
        Assert.Equal(2.0, Scale.Read(border));
        Assert.Equal(30.0, X.Read(border));
    }

    [AvaloniaFact]
    public void UsingClockOf_animates_a_visual_that_is_not_in_the_tree_yet()
    {
        var host = new Border();
        using var h = new MotionHarness(host);
        var detached = new Border();

        var play = Motion.Of(detached).UsingClockOf(host)
            .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(160))
            .PlayAsync();
        h.Frames(5);
        Assert.InRange(detached.Opacity, 0.3, 0.7);
        h.Frames(10);

        Assert.True(play.Outcome());
    }

    [AvaloniaFact]
    public void A_null_target_is_a_no_op()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        Border? missing = null;

        var play = Motion.Of(border)
            .Animate(missing, SukiMotion.BlurProperty).To(10.0).Over(TimeSpan.FromMilliseconds(100))
            .And()
            .Animate(Visual.OpacityProperty).To(0.0).Over(TimeSpan.FromMilliseconds(100))
            .PlayAsync();
        h.Frames(10);

        Assert.True(play.Outcome());
        Assert.Equal(0.0, border.Opacity);
    }

    [AvaloniaFact]
    public void Stop_interrupts_the_motions_of_a_visual()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var play = Motion.Of(border).Animate(X).To(100.0).Over(TimeSpan.FromMilliseconds(200)).PlayAsync();
        h.Frames(3);
        Motion.Stop(border);
        double pose = X.Read(border);
        h.Frames(10);

        Assert.False(play.Outcome());
        Assert.Equal(pose, X.Read(border));
    }

    [AvaloniaFact]
    public void Any_supported_value_type_animates()
    {
        var swatch = new Swatch { Glow = Colors.Black };
        using var h = new MotionHarness(swatch);

        _ = Motion.Of(swatch).Animate(Swatch.GlowProperty).To(Colors.White).Over(TimeSpan.FromMilliseconds(160)).PlayAsync();
        h.Frames(5);
        Assert.InRange(swatch.Glow.R, (byte)80, (byte)180);
        h.Frames(10);

        Assert.Equal(Colors.White, swatch.Glow);
    }

    [AvaloniaFact]
    public void A_motion_without_To_fails_when_played()
    {
        var border = new Border();

        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = Motion.Of(border).Animate(Scale).Over(TimeSpan.FromMilliseconds(100)).PlayAsync();
        });

        Assert.Contains("To(", error.Message);
    }

    [AvaloniaFact]
    public void AddVelocity_kicks_a_spring_around_its_target()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var pulse = Motion.Of(border).Animate(Scale).To(1.0).WithSpring(Spring.Bouncy).AddVelocity(4.0).PlayAsync();
        h.Frames(3);
        Assert.True(Scale.Read(border) > 1.05);
        h.Run(TimeSpan.FromSeconds(3));

        Assert.True(pulse.Outcome());
        Assert.Equal(1.0, Scale.Read(border));
    }

    private sealed class Swatch : Border
    {
        public static readonly StyledProperty<Color> GlowProperty =
            AvaloniaProperty.Register<Swatch, Color>(nameof(Glow));

        public Color Glow
        {
            get => GetValue(GlowProperty);
            set => SetValue(GlowProperty, value);
        }
    }
}

internal static class TaskOutcome
{
    /// <summary>The result of a motion that must have ended by now (never blocks).</summary>
    public static bool Outcome(this Task<bool> play)
    {
        Assert.True(play.IsCompleted, "the motion is still running");
        return play.Result;
    }
}

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;

namespace SukiUI.Motion.Tests;

/// <summary>The three rules that decide which motion owns a property.</summary>
public class ArbitrationTests
{
    private static readonly MotionProperty<double> X = SukiMotion.TranslateXProperty;
    private static readonly MotionProperty<double> Scale = SukiMotion.ScaleProperty;

    // ---- rule 1: the latest motion wins, from the live pose (and velocity) -------------

    [AvaloniaFact]
    public void A_new_spring_takes_over_with_the_live_velocity()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var slow = new Spring(10, 4);

        var first = Motion.Of(border).Animate(X).To(100.0).WithSpring(slow).PlayAsync();
        h.Frames(6);
        double before = X.Read(border);

        var second = Motion.Of(border).Animate(X).To(0.0).WithSpring(slow).PlayAsync();
        h.Frame();

        Assert.True(X.Read(border) > before); // the momentum carries on before turning around
        h.Run(TimeSpan.FromSeconds(6));
        Assert.False(first.Outcome());
        Assert.True(second.Outcome());
        Assert.Equal(0.0, X.Read(border), precision: 3);
    }

    [AvaloniaFact]
    public void A_new_tween_starts_from_the_pose_on_screen()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border).Animate(X).To(100.0).Over(TimeSpan.FromMilliseconds(200)).PlayAsync();
        h.Frames(5);
        double pose = X.Read(border);

        _ = Motion.Of(border).Animate(X).To(0.0).Over(TimeSpan.FromMilliseconds(200)).PlayAsync();
        h.Frame();

        Assert.InRange(X.Read(border), pose * 0.8, pose); // no jump back to 100, no jump to 0
    }

    [AvaloniaFact]
    public void An_interrupted_motion_stops_as_a_whole()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var duration = TimeSpan.FromMilliseconds(200);

        var both = Motion.Of(border)
            .Animate(X).To(100.0).Over(duration)
            .And()
            .Animate(Scale).To(2.0).Over(duration)
            .PlayAsync();
        h.Frames(3);
        _ = Motion.Of(border).Animate(X).To(0.0).Over(duration).PlayAsync();
        double scale = Scale.Read(border);
        h.Frames(20);

        Assert.False(both.Outcome());
        Assert.Equal(scale, Scale.Read(border)); // the scale stopped where it was
    }

    // ---- rule 2: MustFinish ---------------------------------------------------------------

    [AvaloniaFact]
    public void Another_motion_waits_for_a_MustFinish_step_and_takes_over_from_the_pose_reached()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var press = Motion.Of(border)
            .Animate(Scale).To(0.9).Over(TimeSpan.FromMilliseconds(160)).MustFinish()
            .Then()
            .Animate(Scale).To(0.8).Over(TimeSpan.FromSeconds(2))
            .PlayAsync();
        h.Frames(2);
        var release = Motion.Of(border).Animate(Scale).To(1.0).WithSpring(Spring.Snappy).PlayAsync();

        double lowest = 1.0;
        for (int i = 0; i < 80; i++)
        {
            h.Frame();
            lowest = Math.Min(lowest, Scale.Read(border));
        }

        Assert.Equal(0.9, lowest, precision: 2); // the descent played to the end, the deep step never ran
        Assert.False(press.Outcome());
        Assert.True(release.Outcome());
        Assert.Equal(1.0, Scale.Read(border), precision: 3);
    }

    [AvaloniaFact]
    public void Only_the_latest_waiting_motion_is_kept()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border).Animate(Scale).To(0.9).Over(TimeSpan.FromMilliseconds(160)).MustFinish().PlayAsync();
        var first = Motion.Of(border).Animate(Scale).To(1.2).PlayAsync();
        var second = Motion.Of(border).Animate(Scale).To(1.0).PlayAsync();
        h.Run(TimeSpan.FromSeconds(2));

        Assert.False(first.Outcome());
        Assert.True(second.Outcome());
        Assert.Equal(1.0, Scale.Read(border), precision: 3);
    }

    [AvaloniaFact]
    public void The_same_reaction_triggered_again_restarts_instead_of_waiting()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        int completed = 0;
        using var _ = Motion.Configure(border, motion =>
            motion.On(InputElement.PointerPressedEvent)
                .Animate(Scale).To(0.5).Over(TimeSpan.FromMilliseconds(160)).MustFinish()
                .Then()
                .Do(() => completed++));

        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(5); // 80 ms
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(11); // 256 ms: a restart has finished (240 ms), a wait would not have (320 ms)

        Assert.Equal(1, completed);
    }

    [AvaloniaFact]
    public void A_Set_wins_even_over_a_MustFinish_step()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var press = Motion.Of(border).Animate(Scale).To(0.5).Over(TimeSpan.FromMilliseconds(160)).MustFinish().PlayAsync();
        h.Frames(2);
        _ = Motion.Of(border).Set(Scale).To(1.0).PlayAsync();
        h.Frames(15);

        Assert.False(press.Outcome());
        Assert.Equal(1.0, Scale.Read(border));
    }

    // ---- rule 3: RetargetIfMoving -------------------------------------------------------

    [AvaloniaFact]
    public void A_redirect_moves_the_target_of_a_moving_spring_without_restarting_it()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var spring = Motion.Of(border).Animate(X).To(100.0).WithSpring(new Spring(10, 4)).PlayAsync();
        h.Frames(6);
        double velocityBefore = Track.Get(border, X).Velocity[0];

        var redirect = Motion.Of(border).Animate(X).To(50.0).Over(TimeSpan.FromMilliseconds(150)).RetargetIfMoving().PlayAsync();

        Assert.True(redirect.IsCompleted && redirect.Result); // consumed at once
        Assert.Equal(velocityBefore, Track.Get(border, X).Velocity[0]);
        h.Run(TimeSpan.FromSeconds(6));
        Assert.True(spring.Outcome()); // never interrupted
        Assert.Equal(50.0, X.Read(border), precision: 3);
    }

    [AvaloniaFact]
    public void A_redirect_reaches_the_spring_waiting_behind_a_MustFinish_step()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border).Animate(Scale).To(0.9).Over(TimeSpan.FromMilliseconds(160)).MustFinish().PlayAsync();
        var release = Motion.Of(border).Animate(Scale).To(1.0).WithSpring(Spring.Snappy).PlayAsync();
        var hover = Motion.Of(border).Animate(Scale).To(1.2).Over(TimeSpan.FromMilliseconds(150)).RetargetIfMoving().PlayAsync();
        h.Run(TimeSpan.FromSeconds(2));

        Assert.True(hover.Outcome());
        Assert.True(release.Outcome());
        Assert.Equal(1.2, Scale.Read(border), precision: 3);
    }

    [AvaloniaFact]
    public void A_redirect_plays_normally_when_nothing_moves()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var hover = Motion.Of(border).Animate(Scale).To(1.2).Over(TimeSpan.FromMilliseconds(150)).RetargetIfMoving().PlayAsync();
        h.Frames(3);
        Assert.InRange(Scale.Read(border), 1.01, 1.19);
        h.Frames(10);

        Assert.True(hover.Outcome());
        Assert.Equal(1.2, Scale.Read(border));
    }
}

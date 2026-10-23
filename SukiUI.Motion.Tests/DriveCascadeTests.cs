using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

namespace SukiUI.Motion.Tests;

public class DriveTests
{
    [AvaloniaFact]
    public void A_drive_follows_the_velocity_of_its_source_and_rests_with_it()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        double peak = 0;

        var play = Motion.Of(border)
            .Animate(SukiMotion.TranslateXProperty).To(100.0).WithSpring(Spring.Snappy)
            .And()
            .Drive(SukiMotion.BlurProperty)
                .From(SukiMotion.TranslateXProperty)
                .With(ctx => Math.Abs(ctx.VelocityOf(SukiMotion.TranslateXProperty)) * 0.01)
            .PlayAsync();
        for (int i = 0; i < 120; i++)
        {
            h.Frame();
            peak = Math.Max(peak, SukiMotion.BlurProperty.Read(border));
        }

        Assert.True(play.Outcome());
        Assert.True(peak > 1.0);
        Assert.Null(border.Effect); // at rest the blur has left the effect slot
    }

    [AvaloniaFact]
    public void A_drive_can_follow_the_value_of_its_source()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        _ = Motion.Of(border)
            .Animate(Visual.OpacityProperty).From(1.0).To(0.0).Over(TimeSpan.FromMilliseconds(160))
            .And()
            .Drive(SukiMotion.TranslateYProperty)
                .From(Visual.OpacityProperty)
                .With(ctx => (1.0 - ctx.ValueOf(Visual.OpacityProperty)) * 100.0)
            .PlayAsync();
        h.Frames(5);
        Assert.InRange(SukiMotion.TranslateYProperty.Read(border), 30.0, 70.0);
        h.Frames(10);

        Assert.Equal(100.0, SukiMotion.TranslateYProperty.Read(border));
    }
}

public class CascadeTests
{
    private static (StackPanel Panel, Border[] Items) Panel(int count)
    {
        var items = Enumerable.Range(0, count).Select(_ => new Border { Height = 10 }).ToArray();
        var panel = new StackPanel();
        panel.Children.AddRange(items);
        return (panel, items);
    }

    private static Task<bool> FadeIn(StackPanel panel, int fewerThan = int.MaxValue, bool complete = false)
    {
        var cascade = Motion.Of(panel)
            .Cascade(ctx => ctx.Control.Children, item => item
                .Animate(Visual.OpacityProperty).From(0.0).To(1.0).Over(TimeSpan.FromMilliseconds(100)))
            .WithStagger(TimeSpan.FromMilliseconds(50))
            .OnlyWhenFewerThan(fewerThan);
        if (complete)
            cascade.CompleteIfInterrupted();
        return cascade.PlayAsync();
    }

    [AvaloniaFact]
    public void Items_animate_one_after_the_other()
    {
        var (panel, items) = Panel(3);
        using var h = new MotionHarness(panel);

        var play = FadeIn(panel);
        h.Frames(4); // 64 ms (items are collected on the first frame)

        Assert.True(items[0].Opacity > items[1].Opacity);
        Assert.Equal(0.0, items[2].Opacity);
        h.Frames(20);
        Assert.True(play.Outcome());
        Assert.All(items, i => Assert.Equal(1.0, i.Opacity));
    }

    [AvaloniaFact]
    public void Too_many_items_skip_the_cascade()
    {
        var (panel, items) = Panel(3);
        using var h = new MotionHarness(panel);

        _ = FadeIn(panel, fewerThan: 3);
        h.Frames(2);

        Assert.All(items, i => Assert.Equal(1.0, i.Opacity));
    }

    [AvaloniaFact]
    public void An_interrupted_cascade_can_land_its_items()
    {
        var (panel, items) = Panel(3);
        using var h = new MotionHarness(panel);

        var play = FadeIn(panel, complete: true);
        h.Frames(3);
        Motion.Stop(items[0]);

        Assert.False(play.Outcome());
        Assert.All(items, i => Assert.Equal(1.0, i.Opacity));
    }
}

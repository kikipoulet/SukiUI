using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

public class ChainTests
{
    // Chain read each step's easing through its FACTORY on every frame, so an easing
    // recipe that builds an instance allocated one per frame — and a recipe reading live
    // state could change the curve mid-flight. Lazy arguments are a per-gesture snapshot
    // everywhere else (TimedTrajectory.Start): the factory must run once per step start.
    [AvaloniaFact]
    public void Step_easing_factory_is_resolved_once_per_step()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        int calls = 0;

        var chain = scale.To(0.9).Over(TimeSpan.FromMilliseconds(160))
            .Ease(() => { calls++; return new CubicEaseOut(); })
            .MustFinish();
        scale.Offer(chain);
        h.Run(TimeSpan.FromMilliseconds(300));

        Assert.Equal(1, calls);
    }
}

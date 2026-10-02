using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

/// <summary>The defensive pose-clamp window of a channel (Channel.ClampPose / Track).</summary>
public class ClampWindowTests
{
    // The clamp window only grows from tracked targets/Froms, never from the pose the
    // channel actually starts at. On a fresh channel with a single target the window
    // degenerates to [target, target], the start pose is clamped onto the target and the
    // program teleports there — no animation at all.
    [AvaloniaFact]
    public void Lone_spring_on_a_fresh_channel_animates_from_the_current_pose()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;

        scale.Offer(scale.To(1.1).Spring(new Spring(Omega: 20, Decay: 28)));
        h.Frame();

        Assert.InRange(scale.Value, 1.0, 1.09);
    }

    [AvaloniaFact]
    public void Lone_chain_on_a_fresh_channel_animates_from_the_current_pose()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;

        scale.Offer(scale.To(0.9).Over(TimeSpan.FromMilliseconds(160)).MustFinish());
        h.Frame();

        Assert.InRange(scale.Value, 0.91, 1.0);
    }
}

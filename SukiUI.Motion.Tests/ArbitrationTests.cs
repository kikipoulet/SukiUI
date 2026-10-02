using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

/// <summary>Channel.Offer — the "who owns the channel" rules.</summary>
public class ArbitrationTests
{
    private static readonly Spring Hover = new(Omega: 20, Decay: 28); // zeta = 0.7

    // A spring offered while another spring runs was silently dropped, so a
    // spring-driven hover that is left mid-flight stayed at the hover pose forever.
    [AvaloniaFact]
    public void Spring_offered_during_a_running_spring_takes_over()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        var enter = scale.To(1.1).Spring(Hover);
        var exit = scale.To(1.0).Spring(Hover);

        scale.Offer(enter);
        h.Frames(3);           // pointer enters, spring in flight
        scale.Offer(exit);     // pointer leaves mid-flight
        h.Run(TimeSpan.FromSeconds(2));

        Assert.Equal(1.0, scale.Value, precision: 3);
    }

    // The takeover keeps the live velocity: no kink in the trajectory at the switch.
    [AvaloniaFact]
    public void Spring_takeover_carries_the_live_velocity()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        var enter = scale.To(1.1).Spring(Hover);
        var exit = scale.To(1.0).Spring(Hover);

        scale.Offer(enter);
        h.Frames(3);
        double velocityBefore = scale.Velocity;
        scale.Offer(exit);

        Assert.True(velocityBefore > 0);
        Assert.Equal(velocityBefore, scale.Velocity, precision: 6);
    }

    // Run (choreographies) and Offer (triggers) share the channel's active program.
    // A choreography stopped mid-flight leaves its frozen spring as the channel's active
    // program — nobody advances it any more — and a plain timed trajectory offered later
    // is dropped by the "only a press preempts a spring" rule: the channel is stuck.
    [AvaloniaFact]
    public void Offer_after_a_stopped_choreography_is_not_dropped()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;

        var choreography = new Choreography().And(scale.To(1.5).Spring(Hover));
        choreography.Start(border);
        h.Frames(3);
        choreography.Stop(); // preempted / abandoned mid-flight

        scale.Offer(scale.To(1.0).Over(TimeSpan.FromMilliseconds(100)));
        h.Run(TimeSpan.FromMilliseconds(300));

        Assert.Equal(1.0, scale.Value);
    }

    // The other way round: a channel animated by Offer, then claimed by a
    // choreography (SukiTypingMotion: hover probe via Offer, kick via a choreography).
    // Both drivers must not advance the same channel — the choreography owns it.
    [AvaloniaFact]
    public void Choreography_claiming_an_offered_channel_is_its_only_driver()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        int advances = 0;
        var counted = new CountingProgram(scale, () => advances++);

        scale.Offer(scale.To(2.0).Over(TimeSpan.FromSeconds(1))); // channel subscribes itself
        h.Frame();
        new Choreography().And(counted).Start(border);
        h.Frames(5);

        Assert.Equal(5, advances); // one advance per frame — not two
    }

    /// <summary>Records every Advance; never finishes on its own.</summary>
    private sealed class CountingProgram : Program
    {
        private readonly Action _onAdvance;

        public CountingProgram(Channel channel, Action onAdvance)
        {
            Channel = channel;
            _onAdvance = onAdvance;
        }

        public override void Start() => Done = false;

        public override bool Advance(TimeSpan now)
        {
            _onAdvance();
            Channel!.Write(1.0 + Random.Shared.NextDouble() * 0.01); // keep frames coming
            return true;
        }
    }

    // Guard for the press behavior: release and capture-lost both offer the SAME relax
    // spring — the second offer must not restart it from rest.
    [AvaloniaFact]
    public void Same_spring_reoffered_is_not_restarted()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        var relax = scale.To(1.1).Spring(Hover);
        scale.Track(1.0); // the rest pose in the clamp window, as the press behavior has

        scale.Offer(relax);
        h.Frames(3);
        double velocity = scale.Velocity;
        scale.Offer(relax);

        Assert.True(velocity > 0);
        Assert.Equal(velocity, scale.Velocity, precision: 9);
    }
}

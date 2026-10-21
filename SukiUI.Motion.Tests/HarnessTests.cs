using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

/// <summary>Validates the test harness itself before any bug test relies on it.</summary>
public class HarnessTests
{
    // A subscriber must write a visual property to keep RAF on the compositor cadence:
    // Avalonia 12 re-arms a RAF requested from inside a frame either on composition-batch
    // completion (something changed) or on a real-time 16 ms DispatcherTimer (nothing
    // changed) — the latter does not follow the virtual clock. Every engine channel writes.
    [AvaloniaFact]
    public void Ticker_runs_one_callback_per_pumped_frame_on_virtual_time()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var seen = new List<TimeSpan>();

        using var token = SukiTicker.Subscribe(border, now =>
        {
            seen.Add(now);
            border.Opacity = 1.0 - seen.Count * 0.01;
        });
        h.Frames(3);

        Assert.Equal(3, seen.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(16), seen[1] - seen[0]);
    }

    [AvaloniaFact]
    public void Timed_trajectory_reaches_its_target_on_virtual_time()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;

        scale.Offer(scale.To(2.0).Over(TimeSpan.FromMilliseconds(160)));
        h.Frames(5);
        Assert.InRange(scale.Value, 1.3, 1.7); // halfway, linear easing

        h.Frames(10);
        Assert.Equal(2.0, scale.Value);
    }
}

public class HarnessRealTimeTests
{
    // Harness guard: HeadlessRenderTimer also runs a REAL-time 60 Hz
    // DispatcherTimer. When it fires inside a pumped frame it processes the batch early, the
    // ticker dispatches a second time at the SAME virtual time, writes nothing new, and
    // Avalonia re-arms the RAF on its real-time 16 ms fallback timer — which headless never
    // promotes without another job (Dispatcher.RunJobs only promotes due timers after running
    // some other job): the animation freezes.
    // Reproduced deterministically by stalling one frame past the render timer's interval.
    [AvaloniaFact]
    public void A_real_time_render_tick_inside_a_frame_does_not_freeze_the_ticker()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var scale = Motion.For(border).Scale;
        bool stalled = false;
        using var stall = SukiTicker.Subscribe(border, _ =>
        {
            if (stalled)
                return;
            stalled = true;
            Thread.Sleep(40); // > 1/60 s of real time: the render timer is due inside this frame
        });

        scale.Offer(scale.To(2.0).Over(TimeSpan.FromMilliseconds(160)));
        h.Frames(20);

        Assert.Equal(2.0, scale.Value);
    }
}

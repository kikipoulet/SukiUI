using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;

namespace SukiUI.Motion.Tests;

/// <summary>
/// Deterministic frame driver: installs a virtual clock into <see cref="SukiTicker"/> and
/// pumps render frames by hand — each frame advances virtual time, ticks the headless
/// render timer (which services RequestAnimationFrame) and flushes the dispatcher.
/// </summary>
public sealed class MotionHarness : IDisposable
{
    public const double FrameMs = 16.0;

    public MotionHarness(Control? content = null)
    {
        // Start away from zero so "time since epoch" arithmetic never degenerates.
        Now = TimeSpan.FromSeconds(10);
        SukiTicker.ClockOverride = () => Now;
        Window = new Window { Width = 400, Height = 300, Content = content };
        Window.Show();
        Flush();
    }

    public TimeSpan Now { get; private set; }

    public Window Window { get; }

    /// <summary>Advances virtual time by one frame and lets every RAF subscriber run.</summary>
    public void Frame(double ms = FrameMs)
    {
        Now += TimeSpan.FromMilliseconds(ms);
        // Guarantees a render pass this frame. Without it a pending RAF can be left on
        // Avalonia's real-time fallback timer, which headless never fires on its own: the
        // headless render timer ALSO ticks on real time, and when that lands inside a frame the
        // ticker dispatches twice at the same virtual time, the second dispatch writes nothing
        // and the RAF chain moves to that fallback.
        Window.InvalidateVisual();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Flush();
    }

    public void Frames(int count, double ms = FrameMs)
    {
        for (int i = 0; i < count; i++)
            Frame(ms);
    }

    /// <summary>Runs frames for the given virtual duration.</summary>
    public void Run(TimeSpan duration) => Frames((int)Math.Ceiling(duration.TotalMilliseconds / FrameMs));

    public static void Flush() => Dispatcher.UIThread.RunJobs();

    public void Dispose()
    {
        // Drain in-flight RAF registrations and composition batches so the next test
        // starts from an idle, process-wide MediaContext.
        Frames(3);
        Window.Close();
        Flush();
        SukiTicker.ClockOverride = null;
    }
}

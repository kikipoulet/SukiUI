using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SukiUI.Motion.Tests;

/// <summary>Mover / SukiMotion lifecycle: Enable, Dispose, Simulate.</summary>
public class LifecycleTests
{
    // A plain routed event: real input can be raised with ordinary RoutedEventArgs.
    private static readonly RoutedEvent<RoutedEventArgs> PokeEvent =
        RoutedEvent.Register<Border, RoutedEventArgs>("Poke", RoutingStrategies.Bubble);

    // Dispose is not idempotent — a second call re-runs every dispose hook
    // (PopupHandle.Dispose → StopAndRest, the popup forced to the host state again, ...).
    [AvaloniaFact]
    public void Mover_dispose_runs_its_hooks_once()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        int hooks = 0;
        var mover = new Mover(border).OnDispose(() => hooks++);

        mover.Dispose();
        mover.Dispose();

        Assert.Equal(1, hooks);
    }

    // Simulate on an element whose motion is NOT enabled attached the behavior for
    // good (EnsureMover), so from then on real pointer input animated an element the
    // author never enabled — and nothing ever disposed that Mover.
    [AvaloniaFact]
    public void Simulate_on_a_disabled_element_does_not_wire_the_behavior()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        CountingMotion.Fired = 0;

        CountingMotion.Simulate(border, PokeEvent);
        border.RaiseEvent(new RoutedEventArgs(PokeEvent)); // real input later

        Assert.Equal(0, CountingMotion.Fired);
    }

    [AvaloniaFact]
    public void Simulate_on_an_enabled_element_drives_its_behavior()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        CountingMotion.Fired = 0;
        CountingMotion.SetEnable(border, true);

        CountingMotion.Simulate(border, PokeEvent);

        Assert.Equal(1, CountingMotion.Fired);
    }

    private sealed class CountingMotion : SukiMotion<CountingMotion>
    {
        public static int Fired;

        internal override Mover? Attach(AvaloniaObject element) =>
            element is InputElement input
                ? new Mover(input).OnEvent(PokeEvent, () => Fired++)
                : null;
    }
}

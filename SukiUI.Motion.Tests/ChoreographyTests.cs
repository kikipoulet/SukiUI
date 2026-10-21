using Avalonia.Controls;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

public class ChoreographyTests
{
    // Start on a running choreography overwrote its ticker subscription without
    // disposing it. The orphaned subscription kept calling OnFrame forever: every member is
    // done, so it re-ran Release + the settle action on EVERY frame and the loop never idled.
    [AvaloniaFact]
    public void Restarting_a_running_choreography_settles_exactly_once()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        var s = Motion.For(border);
        int settled = 0;

        var choreography = new Choreography()
            .And(s.Opacity.To(0.5).Over(TimeSpan.FromMilliseconds(100)))
            // The settle writes a visual property, as PopupHandle.Hide does, so a settle
            // storm keeps frames coming and stays observable in headless.
            .Then(() => { settled++; border.Width = 100 + settled; });

        choreography.Start(border);
        h.Frame();
        choreography.Start(border); // restart mid-flight (e.g. a reused open choreography)
        h.Frames(30);

        Assert.Equal(1, settled);
        Assert.False(choreography.Running);
    }
}

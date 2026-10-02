using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Templates;

namespace SukiUI.Motion.Tests;

/// <summary>Choreographies started on an element with no TopLevel (detached).</summary>
public class DetachedHostTests
{
    // Choreography.Start subscribed to the ticker unconditionally, and the ticker
    // throws without a TopLevel. Channel.StartProgram guards the same case; choreographies
    // did not. Expected instead: nothing can animate off-screen, so the choreography
    // completes at once — final poses written, settle action run (toasts are removed and
    // popups hidden in their settle: skipping it would leak them).
    [AvaloniaFact]
    public void Choreography_on_a_detached_owner_completes_at_once()
    {
        var border = new Border(); // never attached
        var s = Motion.For(border);
        int settled = 0;

        var choreography = new Choreography()
            .And(s.Opacity.To(0.25).Over(TimeSpan.FromMilliseconds(200)))
            .And(s.Scale.To(0.5).Spring(new Spring(Omega: 20, Decay: 28)))
            .Then(() => settled++);

        var error = Record.Exception(() => choreography.Start(border));

        Assert.Null(error);
        Assert.Equal(0.25, border.Opacity);
        Assert.Equal(0.5, s.Scale.Value);
        Assert.Equal(1, settled);
        Assert.False(choreography.Running);
    }

    // The production path: a popup host removed from the tree (a tab switched away) whose
    // open state is driven by a binding — PopupHandle.Play reaches Choreography.Start.
    [AvaloniaFact]
    public void Popup_play_on_a_detached_host_does_not_throw()
    {
        var combo = new ComboBox();
        using var h = new MotionHarness(combo);
        Popup? FindPopup() => combo.GetTemplateDescendants().OfType<Popup>().FirstOrDefault();
        var popup = Motion.For(combo).Popup(
            resolvePopup: FindPopup,
            resolveRoot: p => p?.Child as Control,
            resolveItems: _ => null,
            isHostOpen: () => combo.IsDropDownOpen);

        h.Window.Content = null; // detached, template and popup parts intact
        MotionHarness.Flush();
        // Preconditions: the stage resolves (otherwise Play just parks the choreography).
        Assert.NotNull(FindPopup()?.Child);
        Assert.Null(TopLevel.GetTopLevel(combo));

        var root = Motion.For(combo); // any channel works for the repro
        var error = Record.Exception(() =>
            popup.Play(popup.Show().And(root.Opacity.To(1.0).Over(TimeSpan.FromMilliseconds(100)))));

        Assert.Null(error);
    }
}

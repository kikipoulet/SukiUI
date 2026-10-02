using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

/// <summary>PopupHandle: choreographies parked while the stage does not resolve yet.</summary>
public class PopupPendingTests
{
    // The host asks for open before its template exists (a bound IsDropDownOpen at
    // load) and changes its mind before the template is applied — the consumer's close path
    // has nothing to animate, so the parked OPEN choreography stays parked and plays at
    // TemplateApplied: the popup opens although the host is closed.
    [AvaloniaFact]
    public void Parked_open_is_dropped_when_the_host_closes_before_the_stage_resolves()
    {
        bool wantOpen = false;
        var combo = new ComboBox(); // template not applied yet
        var popup = Motion.For(combo).Popup(
            popupPart: "PART_Popup", rootPart: "PopupBorder", itemsPart: "PART_ItemsPresenter",
            isHostOpen: () => wantOpen);

        int opened = 0;

        wantOpen = true;
        popup.Play(popup.Show().Then(() => opened++)); // no stage yet: parked
        wantOpen = false;         // closed again; nothing is open, nothing to animate

        using var h = new MotionHarness(combo); // template applied
        h.Frames(3);
        // The template walk at TemplateApplied cannot see the parts yet (the engine's own
        // fail-safe note), so the parked open survives — and fires at the next logical
        // attach: re-host the control, as a tab switch does.
        h.Window.Content = null;
        MotionHarness.Flush();
        h.Window.Content = combo;
        h.Frames(3);

        // Fluent binds PART_Popup.IsOpen TwoWay to IsDropDownOpen, so the popup's own
        // IsOpen is not a reliable witness here — the replayed choreography is.
        Assert.IsType<Border>(FindPopup(combo)?.Child); // precondition: the stage resolves
        Assert.Equal(0, opened);
    }

    // A successful Play never cleared the parked choreography, so a stale one
    // replayed at the next logical attach — on top of whatever ran since.
    [AvaloniaFact]
    public void Successful_play_supersedes_the_parked_choreography()
    {
        bool stageReady = false, wantOpen = false;
        var combo = new ComboBox();
        using var h = new MotionHarness(combo);
        var popup = Motion.For(combo).Popup(
            resolvePopup: () => FindPopup(combo),
            resolveRoot: p => stageReady ? p?.Child as Control : null,
            resolveItems: _ => null,
            isHostOpen: () => wantOpen);

        int opened = 0;

        wantOpen = true;
        popup.Play(popup.Show().Then(() => opened++)); // stage not ready: parked
        stageReady = true;
        wantOpen = false;
        int closes = 0;
        popup.Play(new Choreography().Then(() => closes++)); // resolves now: runs, supersedes
        h.Frames(2);

        h.Window.Content = null; // re-host: logical detach + attach
        MotionHarness.Flush();
        h.Window.Content = combo;
        h.Frames(3);

        Assert.Equal(1, closes);
        Assert.Equal(0, opened);
    }

    private static Popup? FindPopup(ComboBox combo) =>
        combo.GetTemplateDescendants().OfType<Popup>().FirstOrDefault();
}

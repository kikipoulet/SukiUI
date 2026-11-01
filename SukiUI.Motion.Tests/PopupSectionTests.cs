using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;

namespace SukiUI.Motion.Tests;

/// <summary>A host whose template popup has no binding on IsOpen: the section owns it.</summary>
public sealed class PopupHost : TemplatedControl
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<PopupHost, bool>(nameof(IsOpen));

    public PopupHost()
    {
        Template = new FuncControlTemplate<PopupHost>((host, scope) =>
        {
            var root = new Border { Name = "Root", Width = 80, Height = 40 };
            var popup = new Popup { Name = "PART_Popup", Child = root, PlacementTarget = host };
            var panel = new Panel { Children = { popup } };
            scope.Register(popup.Name, popup);
            return panel;
        });
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public Popup Popup => this.GetTemplateDescendants().OfType<Popup>().Single();

    public Border Root => (Border)Popup.Child!;
}

public class PopupSectionTests
{
    private static readonly TimeSpan Fade = TimeSpan.FromMilliseconds(160);

    private static IDisposable Animate(PopupHost host, Action? opened = null) =>
        Motion.Configure(host, motion =>
        {
            var popup = motion.Popup()
                .PopupPart("PART_Popup")
                .RootPart("Root")
                .OpenWhen(PopupHost.IsOpenProperty);

            popup.OnOpening()
                 .Animate(popup.Root, Visual.OpacityProperty).From(0.0).To(1.0).Over(Fade)
                 .Then()
                 .Do(() => opened?.Invoke());

            popup.OnClosing()
                 .Animate(popup.Root, Visual.OpacityProperty).To(0.0).Over(Fade);
        });

    [AvaloniaFact]
    public void Opening_reveals_the_popup_from_its_start_pose()
    {
        var host = new PopupHost();
        using var h = new MotionHarness(host);
        using var _ = Animate(host);

        host.IsOpen = true;

        Assert.True(host.Popup.IsOpen);
        Assert.Equal(0.0, host.Root.Opacity);
        h.Frames(15);
        Assert.Equal(1.0, host.Root.Opacity);
    }

    [AvaloniaFact]
    public void The_popup_closes_only_at_the_end_of_the_closing_motion()
    {
        var host = new PopupHost();
        using var h = new MotionHarness(host);
        using var _ = Animate(host);
        host.IsOpen = true;
        h.Frames(15);

        host.IsOpen = false;
        h.Frames(5);
        Assert.True(host.Popup.IsOpen);
        h.Frames(10);

        Assert.False(host.Popup.IsOpen);
    }

    [AvaloniaFact]
    public void Reopening_mid_collapse_resumes_from_the_live_pose()
    {
        var host = new PopupHost();
        using var h = new MotionHarness(host);
        using var _ = Animate(host);
        host.IsOpen = true;
        h.Frames(15);
        host.IsOpen = false;
        h.Frames(5);
        double pose = host.Root.Opacity;

        host.IsOpen = true;

        Assert.Equal(pose, host.Root.Opacity); // no jump back to the From pose
        h.Frames(15);
        Assert.True(host.Popup.IsOpen);
        Assert.Equal(1.0, host.Root.Opacity);
    }

    [AvaloniaFact]
    public void A_popup_closed_behind_the_section_syncs_the_host()
    {
        var host = new PopupHost();
        using var h = new MotionHarness(host);
        using var _ = Animate(host);
        host.IsOpen = true;
        h.Frames(15);

        host.Popup.IsOpen = false; // window teardown and the like

        Assert.False(host.IsOpen);
    }

    // The host asks for open before its template exists and changes its mind before the
    // template is applied: the parked opening must be dropped, not played on a closed host.
    [AvaloniaFact]
    public void A_parked_opening_is_dropped_when_the_host_closes_before_the_content_resolves()
    {
        var host = new PopupHost();
        int opened = 0;
        using var _ = Animate(host, () => opened++);

        host.IsOpen = true; // no template yet: parked
        host.IsOpen = false;
        using var h = new MotionHarness(host);
        h.Frames(15);

        Assert.False(host.Popup.IsOpen);
        Assert.Equal(0, opened);
    }
}

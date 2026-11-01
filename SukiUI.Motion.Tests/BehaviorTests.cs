using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;

namespace SukiUI.Motion.Tests;

/// <summary>A reusable behavior with a profile and a XAML Enable property.</summary>
public sealed class GrowMotion : MotionBehavior<Border, GrowMotion.Profile>
{
    public static readonly AttachedProperty<bool> EnableProperty =
        RegisterEnable<GrowMotion, Border>();

    public static bool GetEnable(Border element) => element.GetValue(EnableProperty);
    public static void SetEnable(Border element, bool value) => element.SetValue(EnableProperty, value);

    public sealed record Profile(double Scale);

    public static Profile Current { get; set; } = new(2.0);

    protected override Profile GetProfile(Border control) => Current;

    protected override void Configure(MotionBuilder<Border, Profile> motion)
    {
        motion.On(InputElement.PointerPressedEvent)
              .Animate(SukiMotion.ScaleProperty)
                  .To(ctx => ctx.Profile.Scale)
                  .Over(TimeSpan.FromMilliseconds(100));

        motion.OnDisabled()
              .Set(SukiMotion.ScaleProperty).To(1.0);
    }
}

/// <summary>A second behavior on the same kind of control, without profile.</summary>
public sealed class TiltMotion : MotionBehavior<Border>
{
    public static readonly AttachedProperty<bool> EnableProperty =
        RegisterEnable<TiltMotion, Border>();

    public static bool GetEnable(Border element) => element.GetValue(EnableProperty);
    public static void SetEnable(Border element, bool value) => element.SetValue(EnableProperty, value);

    protected override void Configure(MotionBuilder<Border> motion) =>
        motion.On(InputElement.PointerPressedEvent)
              .Set(SukiMotion.RotateProperty).To(10.0);
}

public class BehaviorTests
{
    [AvaloniaFact]
    public void Each_behavior_class_gets_its_own_Enable_property()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        Assert.NotSame(GrowMotion.EnableProperty, TiltMotion.EnableProperty);
        Assert.Equal(typeof(GrowMotion), GrowMotion.EnableProperty.OwnerType);

        TiltMotion.SetEnable(border, true); // only the tilt
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(10);

        Assert.Equal(10.0, SukiMotion.RotateProperty.Read(border));
        Assert.Equal(1.0, SukiMotion.ScaleProperty.Read(border));
        TiltMotion.SetEnable(border, false);
    }

    [AvaloniaFact]
    public void Attach_wires_a_behavior_from_code_until_disposed()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        var attached = Motion.Attach(border, new TiltMotion());
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        Assert.Equal(10.0, SukiMotion.RotateProperty.Read(border));

        attached.Dispose();
        SukiMotion.RotateProperty.Write(border, 0.0);
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        Assert.Equal(0.0, SukiMotion.RotateProperty.Read(border));
    }

    [AvaloniaFact]
    public void Attach_rejects_a_control_of_the_wrong_type()
    {
        var error = Assert.Throws<ArgumentException>(() => Motion.Attach(new TextBlock(), new TiltMotion()));

        Assert.Contains("Border", error.Message);
    }

    [AvaloniaFact]
    public void Enable_attaches_the_behavior_and_disable_puts_the_control_back_at_rest()
    {
        var border = new Border();
        using var h = new MotionHarness(border);

        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(10);
        Assert.Equal(1.0, SukiMotion.ScaleProperty.Read(border)); // not enabled: nothing wired

        GrowMotion.SetEnable(border, true);
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(10);
        Assert.Equal(2.0, SukiMotion.ScaleProperty.Read(border));

        GrowMotion.SetEnable(border, false);
        Assert.Equal(1.0, SukiMotion.ScaleProperty.Read(border));
        Motion.Simulate(border, InputElement.PointerPressedEvent);
        h.Frames(10);
        Assert.Equal(1.0, SukiMotion.ScaleProperty.Read(border));
    }

    [AvaloniaFact]
    public void The_profile_is_read_when_the_event_fires()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        GrowMotion.SetEnable(border, true);
        try
        {
            GrowMotion.Current = new GrowMotion.Profile(3.0);
            Motion.Simulate(border, InputElement.PointerPressedEvent);
            h.Frames(10);

            Assert.Equal(3.0, SukiMotion.ScaleProperty.Read(border));
        }
        finally
        {
            GrowMotion.Current = new GrowMotion.Profile(2.0);
            GrowMotion.SetEnable(border, false);
        }
    }

    [AvaloniaFact]
    public void OnChanged_with_a_condition_reacts_to_a_property()
    {
        var toggle = new ToggleButton();
        using var h = new MotionHarness(toggle);
        using var _ = Motion.Configure(toggle, motion =>
            motion.OnChanged(ToggleButton.IsCheckedProperty)
                  .Where(ctx => ctx.Control.IsChecked == true)
                  .Animate(SukiMotion.TranslateXProperty)
                      .To(20.0)
                      .Over(TimeSpan.FromMilliseconds(100)));

        toggle.IsChecked = true;
        h.Frames(10);
        Assert.Equal(20.0, SukiMotion.TranslateXProperty.Read(toggle));

        SukiMotion.TranslateXProperty.Write(toggle, 0.0);
        toggle.IsChecked = false;
        h.Frames(10);
        Assert.Equal(0.0, SukiMotion.TranslateXProperty.Read(toggle)); // filtered out
    }

    [AvaloniaFact]
    public void Or_shares_one_reaction_between_events()
    {
        var border = new Border();
        using var h = new MotionHarness(border);
        int count = 0;
        using var _ = Motion.Configure(border, motion =>
            motion.On(InputElement.PointerEnteredEvent)
                  .Or(InputElement.PointerExitedEvent)
                  .Do(() => count++));

        Motion.Simulate(border, InputElement.PointerEnteredEvent);
        Motion.Simulate(border, InputElement.PointerExitedEvent);

        Assert.Equal(2, count);
    }

    [AvaloniaFact]
    public void OnEnabled_and_a_template_part_pose_the_part_at_once()
    {
        var toggle = new ToggleButton { IsChecked = true };
        using var h = new MotionHarness(toggle);
        using var _ = Motion.Configure(toggle, motion =>
        {
            var presenter = motion.TemplatePart("PART_ContentPresenter");
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Set(presenter, SukiMotion.TranslateXProperty)
                  .To(ctx => ctx.Control.IsChecked == true ? 18.0 : 0.0);
        });

        var part = toggle.GetTemplateDescendants().OfType<Control>().First(c => c.Name == "PART_ContentPresenter");
        Assert.Equal(18.0, SukiMotion.TranslateXProperty.Read(part));
    }

    [AvaloniaFact]
    public void A_trigger_without_action_fails_when_attached()
    {
        var border = new Border();

        Assert.Throws<InvalidOperationException>(() =>
            Motion.Configure(border, motion => motion.On(InputElement.PointerPressedEvent)));
    }
}

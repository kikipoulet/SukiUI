using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;

namespace SukiUI.Motion.Tests;

/// <summary>The single Visual.Effect slot shared by the blur and shadow channels.</summary>
public class EffectSlotTests
{
    // The blur and shadow channels ADOPT the effect already in the slot and mutate it
    // in place. An effect coming from a style setter is one instance shared by every control
    // the style matches, so animating one control's shadow changes them all.
    [AvaloniaFact]
    public void Animating_a_styled_shadow_does_not_touch_other_controls()
    {
        var (first, second, h) = TwoStyledBorders(new DropShadowEffect { Opacity = 0.5, BlurRadius = 10 });
        using var _ = h;
        Assert.Same(first.Effect, second.Effect); // precondition: the setter value is shared

        Motion.For(first).ShadowOpacity.Write(0.9);
        Motion.For(first).ShadowBlur.Write(30);

        var other = Assert.IsType<DropShadowEffect>(second.Effect);
        Assert.Equal(0.5, other.Opacity);
        Assert.Equal(10, other.BlurRadius);
    }

    [AvaloniaFact]
    public void Animating_a_styled_blur_does_not_touch_other_controls()
    {
        var (first, second, h) = TwoStyledBorders(new BlurEffect { Radius = 4 });
        using var _ = h;
        Assert.Same(first.Effect, second.Effect); // precondition: the setter value is shared

        Motion.For(first).Blur.Write(12);

        Assert.Equal(4, Assert.IsType<BlurEffect>(second.Effect).Radius);
    }

    private static (Border First, Border Second, MotionHarness Harness) TwoStyledBorders(IEffect effect)
    {
        var first = new Border { Width = 50, Height = 50 };
        var second = new Border { Width = 50, Height = 50 };
        var panel = new StackPanel { Children = { first, second } };
        var h = new MotionHarness(panel);
        h.Window.Styles.Add(new Style(x => x.OfType<Border>())
        {
            Setters = { new Setter(Visual.EffectProperty, effect) },
        });
        MotionHarness.Flush();
        return (first, second, h);
    }
}

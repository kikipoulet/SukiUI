using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

namespace SukiUI.Motion.Tests;

/// <summary>The shared render-transform block (Channels.cs, <c>Transforms</c>).</summary>
public class TransformBlockTests
{
    // An external RenderTransform replacement (style, theme, user code) makes the
    // block re-attach a new group carrying ONLY the scale child — translate, rotate and
    // skew keep pointing at orphaned transforms outside the group, so their writes vanish.
    [AvaloniaTheory]
    [InlineData("TranslateX")]
    [InlineData("TranslateY")]
    [InlineData("Rotate")]
    [InlineData("SkewX")]
    [InlineData("SkewY")]
    [InlineData("Scale")]
    public void Channel_keeps_working_after_RenderTransform_is_replaced_externally(string channelName)
    {
        var border = new Border { Width = 100, Height = 100 };
        using var h = new MotionHarness(border);
        var channel = ChannelByName(Motion.For(border), channelName);

        channel.Write(10);
        var before = border.RenderTransform!.Value;

        border.RenderTransform = new ScaleTransform(1, 1); // someone else takes the slot
        channel.Write(10);

        Assert.Equal(10, channel.Value);
        Assert.Equal(before, border.RenderTransform!.Value);
    }

    // The block keeps the composition of every channel through a re-attach.
    [AvaloniaFact]
    public void Composed_pose_survives_RenderTransform_replacement()
    {
        var border = new Border { Width = 100, Height = 100 };
        using var h = new MotionHarness(border);
        var s = Motion.For(border);

        s.TranslateX.Write(15);
        s.Rotate.Write(30);
        s.Scale.Write(1.2);
        var before = border.RenderTransform!.Value;

        border.RenderTransform = null;
        s.Scale.Write(1.2); // any channel write re-attaches the block

        Assert.Equal(before, border.RenderTransform!.Value);
    }

    private static Channel ChannelByName(Surface s, string name) => name switch
    {
        "TranslateX" => s.TranslateX,
        "TranslateY" => s.TranslateY,
        "Rotate" => s.Rotate,
        "SkewX" => s.SkewX,
        "SkewY" => s.SkewY,
        "Scale" => s.Scale,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
}

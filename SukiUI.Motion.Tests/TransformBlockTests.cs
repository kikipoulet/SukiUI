using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

namespace SukiUI.Motion.Tests;

/// <summary>The shared render-transform block (Rendering.cs, <c>Transforms</c>).</summary>
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
    public void Transform_property_keeps_working_after_RenderTransform_is_replaced_externally(string channelName)
    {
        var border = new Border { Width = 100, Height = 100 };
        using var h = new MotionHarness(border);
        var property = PropertyByName(channelName);

        property.Write(border, 10);
        var before = border.RenderTransform!.Value;

        border.RenderTransform = new ScaleTransform(1, 1); // someone else takes the slot
        property.Write(border, 10);

        Assert.Equal(10, property.Read(border));
        Assert.Equal(before, border.RenderTransform!.Value);
    }

    // The block keeps the composition of every channel through a re-attach.
    [AvaloniaFact]
    public void Composed_pose_survives_RenderTransform_replacement()
    {
        var border = new Border { Width = 100, Height = 100 };
        using var h = new MotionHarness(border);
        SukiMotion.TranslateXProperty.Write(border, 15);
        SukiMotion.RotateProperty.Write(border, 30);
        SukiMotion.ScaleProperty.Write(border, 1.2);
        var before = border.RenderTransform!.Value;

        border.RenderTransform = null;
        SukiMotion.ScaleProperty.Write(border, 1.2); // any transform write re-attaches the block

        Assert.Equal(before, border.RenderTransform!.Value);
    }

    private static MotionProperty<double> PropertyByName(string name) => name switch
    {
        "TranslateX" => SukiMotion.TranslateXProperty,
        "TranslateY" => SukiMotion.TranslateYProperty,
        "Rotate" => SukiMotion.RotateProperty,
        "SkewX" => SukiMotion.SkewXProperty,
        "SkewY" => SukiMotion.SkewYProperty,
        "Scale" => SukiMotion.ScaleProperty,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
}

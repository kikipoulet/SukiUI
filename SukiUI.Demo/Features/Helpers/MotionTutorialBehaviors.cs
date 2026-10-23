using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SukiUI.Motion;

namespace SukiUI.Demo.Features.Helpers;

/// <summary>
/// A reusable behavior without profile: hovering the element strikes it — it rocks around
/// its rest angle and settles. Attach it from XAML with <c>WobbleMotion.Enable="True"</c>.
/// </summary>
public sealed class WobbleMotion : MotionBehavior<InputElement>
{
    public static readonly AttachedProperty<bool> EnableProperty =
        RegisterEnable<WobbleMotion, InputElement>();

    public static bool GetEnable(InputElement element) => element.GetValue(EnableProperty);
    public static void SetEnable(InputElement element, bool value) => element.SetValue(EnableProperty, value);

    protected override void Configure(MotionBuilder<InputElement> motion)
    {
        // Hover: an angular kick on a lightly damped spring around 0°.
        motion.On(InputElement.PointerEnteredEvent)
              .Animate(SukiMotion.RotateProperty)
                  .To(0.0)
                  .WithSpring(new Spring(20.0, 5.0))
                  .AddVelocity(240.0);

        // Removed: the element goes back upright.
        motion.OnDisabled()
              .Set(SukiMotion.RotateProperty).To(0.0);
    }
}

/// <summary>The calibration of <see cref="SquishMotion"/> — switched live by the tutorial.</summary>
public sealed record SquishProfile(double PressedScale, Spring Release)
{
    public static readonly SquishProfile Soft = new(0.9, Spring.Gentle);

    public static readonly SquishProfile Snappy = new(0.75, Spring.Bouncy);

    /// <summary>The active profile: read each time the element is pressed.</summary>
    public static SquishProfile Current { get; set; } = Soft;
}

/// <summary>
/// A reusable behavior with a profile: pressed, the element squishes (flatter and wider);
/// released, it springs back. <see cref="GetProfile"/> runs at every gesture, so switching
/// <see cref="SquishProfile.Current"/> changes the next press.
/// </summary>
public sealed class SquishMotion : MotionBehavior<InputElement, SquishProfile>
{
    public static readonly AttachedProperty<bool> EnableProperty =
        RegisterEnable<SquishMotion, InputElement>();

    public static bool GetEnable(InputElement element) => element.GetValue(EnableProperty);
    public static void SetEnable(InputElement element, bool value) => element.SetValue(EnableProperty, value);

    protected override SquishProfile GetProfile(InputElement control) => SquishProfile.Current;

    protected override void Configure(MotionBuilder<InputElement, SquishProfile> motion)
    {
        // Press: capture the pointer (the release then comes back here even outside), and squish.
        motion.On(InputElement.PointerPressedEvent)
              .Do(ctx => ctx.Args.Pointer.Capture(ctx.Control))
              .And()
              .Animate(SukiMotion.ScaleYProperty)
                  .To(ctx => ctx.Profile.PressedScale)
                  .Over(TimeSpan.FromMilliseconds(100))
              .And()
              .Animate(SukiMotion.ScaleXProperty)
                  .To(ctx => 2.0 - ctx.Profile.PressedScale)
                  .Over(TimeSpan.FromMilliseconds(100));

        // Release: both axes spring back with the profile's spring.
        motion.On(InputElement.PointerReleasedEvent)
              .Or(InputElement.PointerCaptureLostEvent)
              .Animate(SukiMotion.ScaleYProperty)
                  .To(1.0)
                  .WithSpring(ctx => ctx.Profile.Release)
              .And()
              .Animate(SukiMotion.ScaleXProperty)
                  .To(1.0)
                  .WithSpring(ctx => ctx.Profile.Release);
    }
}

/// <summary>A custom control with a Color property — animated like any other property.</summary>
public sealed class GlowSwatch : Border
{
    public static readonly StyledProperty<Color> GlowProperty =
        AvaloniaProperty.Register<GlowSwatch, Color>(nameof(Glow), Colors.SlateBlue);

    private readonly SolidColorBrush _brush = new(Colors.SlateBlue);

    static GlowSwatch()
    {
        GlowProperty.Changed.AddClassHandler<GlowSwatch>((swatch, _) => swatch._brush.Color = swatch.Glow);
    }

    public GlowSwatch() => Background = _brush;

    public Color Glow
    {
        get => GetValue(GlowProperty);
        set => SetValue(GlowProperty, value);
    }
}

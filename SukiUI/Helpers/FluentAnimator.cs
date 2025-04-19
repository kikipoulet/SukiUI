using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace SukiUI.Helpers;

/*
 * _myButton.Animate(OpacityProperty)
 *          .From(0).To(1)
 *          .WithDuration(TimeSpan.FromMilliseconds(1000))
 *          .WithEasing(new CubicEaseOut())
 *          .Start();
 */

public static class FluentAnimatorExtensions
{
    public static FluentAnimator<T> Animate<T>(this Animatable control, AvaloniaProperty<T> property) => new FluentAnimator<T>(control, property);
}

public ref struct FluentAnimator<T>(Animatable control, AvaloniaProperty<T> property)
{
    private static readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(.5);
    private static readonly Easing _defaultEasing = new CubicEaseInOut();

    private T? _from;
    private T? _to;
    private TimeSpan? _duration;
    private Easing? _easing;

    private CancellationToken _cancellation;

    public FluentAnimator<T> From(T value) => this with { _from = value };

    public FluentAnimator<T> To(T value) => this with { _to = value };

    public FluentAnimator<T> WithDuration(TimeSpan duration) => this with { _duration = duration };

    public FluentAnimator<T> WithEasing(Easing easing) => this with { _easing = easing };

    public FluentAnimator<T> WithCancellationToken(CancellationToken cancellation) => this with { _cancellation = cancellation };

    public readonly void Start() => _ = RunAsync();

    public readonly Task RunAsync()
    {
        if (_to == null)
        {
            throw new InvalidOperationException("The 'To' value must be set before starting the animation.");
        }

        var duration = _duration ?? _defaultDuration;
        var easing = _easing ?? _defaultEasing;

        var animation = new Animation
        {
            Duration = duration,
            Easing = easing,
            FillMode = FillMode.Forward,
        };

        if (_from != null)
        {
            var fromKeyFrame = new KeyFrame
            {
                Cue = new Cue(0),
                Setters =
                {
                    new Setter { Property = property, Value = _from }
                }
            };

            animation.Children.Add(fromKeyFrame);
        }

        var toKeyFrame = new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Setter { Property = property, Value = _to }
            }
        };

        animation.Children.Add(toKeyFrame);

        return animation.RunAsync(control, _cancellation);
    }
}
using Avalonia.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;
using System.Threading;

namespace SukiUI.Helpers;


public static class FluentAnimatorExtensions
{
    public static FluentAnimator<T> Animate<T>(this Animatable control, AvaloniaProperty<T> property)
    {
        return new FluentAnimator<T>(control, property);
    }
}


/*
 * var tokensource = MyButton.Animate(OpacityProperty)
                             .From(0).To(1)
                             .WithDuration(TimeSpan.FromMilliseconds(1000))
                             .WithEasing(new CubicEaseOut())
                             .Start();
 * 
 */

public class FluentAnimator<T>
{
    private readonly Animatable _control;
    private readonly AvaloniaProperty<T> _property;
    private T _from;
    private T _to;
    private TimeSpan _duration = TimeSpan.FromMilliseconds(500);
    private Easing _easing = new CubicEaseInOut();

    public FluentAnimator(Animatable control, AvaloniaProperty<T> property)
    {
        _control = control;
        _property = property;
    }
    
    public FluentAnimator<T> From(T value)
    {
        _from = value;
        return this;
    }

    public FluentAnimator<T> To(T value)
    {
        _to = value;
        return this;
    }

    public FluentAnimator<T> WithDuration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    public FluentAnimator<T> WithEasing(Easing easing)
    {
        _easing = easing;
        return this;
    }

    public CancellationTokenSource Start()
    {
        var tokenSource = new CancellationTokenSource();
        
        new Animation
        {
            Duration = _duration,
            FillMode = FillMode.Forward,
            Easing = _easing,
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter { Property = _property, Value = _from } },
                    KeyTime = TimeSpan.Zero
                },
                new KeyFrame
                {
                    Setters = { new Setter { Property = _property, Value = _to } },
                    KeyTime = _duration
                }
            }
        }.RunAsync(_control, tokenSource.Token);

        return tokenSource;
    }
}


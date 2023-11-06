using System;
using System.Dynamic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;

namespace SukiUI;

public static class Helper
{
    

    public static void Animate<T>(this Animatable control, AvaloniaProperty Property,T from, T to,  TimeSpan duration )
    {
        new Avalonia.Animation.Animation
        {
            Duration = duration, FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1), PlaybackDirection = PlaybackDirection.Normal, 
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = to } },
                    KeyTime = duration
                }
            }
        }.RunAsync(control);
    }
    
    public static void Animate<T>( this Animatable control, AvaloniaProperty Property,T from, T to)
    {
        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(500), FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1), PlaybackDirection = PlaybackDirection.Normal, 
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = Property, Value = to } },
                    KeyTime = TimeSpan.FromMilliseconds(500)
                }
            }
        }.RunAsync(control);
    }
}
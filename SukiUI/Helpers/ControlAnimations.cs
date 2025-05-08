using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using SukiUI.Animations;
using SukiUI.Converters;
using SukiUI.Helpers;

namespace SukiUI;

public static class ControlAnimationHelper
{
    public static void Jump(this Control control)
    {
        var currentMargin = control.Margin;

        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new QuadraticEaseOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top - 8, currentMargin.Right, currentMargin.Bottom +8)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(300)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(600)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top-2, currentMargin.Right, currentMargin.Bottom +2)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(700)
                },new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = Control.MarginProperty, Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right, currentMargin.Bottom)},
                    },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(control);
    }

    public static void Vibrate(this Animatable control, TimeSpan duration)
    {
        var count = duration.TotalMilliseconds / 75;
        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(75),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount((ulong)(count)),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 0.995},
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 0.995},
                        new Setter { Property = RotateTransform.AngleProperty, Value = 0.4}
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters =
                    {
                        new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1.005},
                        new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1.005},
                        new Setter { Property = RotateTransform.AngleProperty, Value = -0.3}
                    },
                    KeyTime = TimeSpan.FromMilliseconds(75)
                }
            }
        }.RunAsync(control);
    }

    public static CancellationTokenSource Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to, TimeSpan duration, ulong count = 1)
    {
        var tokensource = new CancellationTokenSource();
        
        new Avalonia.Animation.Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(count),
            PlaybackDirection = PlaybackDirection.Normal,
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
        }.RunAsync(control, tokensource.Token);
        
        return tokensource;
    }

    public static CancellationTokenSource Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to)
    {
        var tokensource = new CancellationTokenSource();
        
        new Avalonia.Animation.Animation
        {
            Duration = TimeSpan.FromMilliseconds(500),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
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
        }.RunAsync(control, tokensource.Token);

        return tokensource;
    }
 
    
 

    public static CancellationTokenSource MoveToOrganic(this Animatable control, Thickness destination, TimeSpan duration)
    {
        double StretchAmplitude1 = 0.05; 
        double SquashAmplitude1 = -0.04; 
        double SquashAmplitude2 = -0.02; 
        double StretchAmplitude2 = 0.03; 
        
        var tokensource = new CancellationTokenSource();
        var start = ((Control)control).Margin;

        double deltaX = (destination.Left + destination.Right) - (start.Left + start.Right);
        double deltaY = (destination.Top + destination.Bottom) - (start.Top + start.Bottom);

        double magnitudeSq = deltaX * deltaX + deltaY * deltaY;
        
        double scaleX1 = 1.0;
        double scaleY1 = 1.0;
        double scaleX2 = 1.0;
        double scaleY2 = 1.0;
        
        if (magnitudeSq > 1e-6)
        {
            double invMagnitude = 1.0 / Math.Sqrt(magnitudeSq);
            double normalizedDeltaX = deltaX * invMagnitude;
            double normalizedDeltaY = deltaY * invMagnitude;
            
            double weightX = normalizedDeltaX * normalizedDeltaX;
            double weightY = normalizedDeltaY * normalizedDeltaY;

            double currentStretch1 = weightX * StretchAmplitude1 + weightY * SquashAmplitude1;
            double currentSquash1 = weightX * SquashAmplitude1 + weightY * StretchAmplitude1;

            double currentSquash2 = weightX * SquashAmplitude2 + weightY * StretchAmplitude2;
            double currentStretch2 = weightX * StretchAmplitude2 + weightY * SquashAmplitude2;
            
            scaleX1 = 1.0 + currentStretch1;
            scaleY1 = 1.0 + currentSquash1;

            scaleX2 = 1.0 + currentSquash2;
            scaleY2 = 1.0 + currentStretch2;
        }
        
        control.Animate(Control.MarginProperty).From(start).To(destination)
             .WithDuration(duration).WithEasing(new SukiEaseInOutBack() { BounceIntensity = EasingIntensity.Strong })
             .Start(); 
        
        var scaleAnimation = new Avalonia.Animation.Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new SukiEaseInOut(), 
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                    },
                    KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.1) 
                },
                new KeyFrame()
                {
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, scaleX1), 
                        new Setter(ScaleTransform.ScaleYProperty, scaleY1), 
                    },
                    KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.03)
                },
                new KeyFrame()
                {
                     Setters = {
                         new Setter(ScaleTransform.ScaleXProperty, scaleX2), 
                         new Setter(ScaleTransform.ScaleYProperty, scaleY2), 
                     },
                     KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.005)
                 },
                new KeyFrame()
                {
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
                    },
                    KeyTime = duration
                },
            }
        };
        
        scaleAnimation.RunAsync(control, tokensource.Token); 

        return tokensource;
    }
    

  
}


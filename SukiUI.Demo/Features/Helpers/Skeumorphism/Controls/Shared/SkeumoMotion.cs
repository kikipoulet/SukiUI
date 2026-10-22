using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Demo.Features.Helpers.Skeumorphism.Controls;

/// <summary>
/// Shared motion and theme helpers of the Skeumo-Suki controls: timer tweens (cancellable), easing curves,
/// accent glows and theme-bound brushes for elements created in code.
/// </summary>
internal static class SkeumoMotion
{
    /// <summary>Timer tween on any property (works inside a RenderTransform). Returns the timer so it can be stopped.</summary>
    public static DispatcherTimer Tween(double from, double to, int ms, Action<double> set, Func<double, double>? ease = null)
    {
        ease ??= EaseOutCubic;
        var start = DateTime.UtcNow;
        DispatcherTimer? timer = null;
        timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
        {
            var t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / ms);
            set(from + (to - from) * ease(t));
            if (t >= 1) timer!.Stop();
        });
        timer.Start();
        return timer;
    }

    public static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);

    /// <summary>Damped spring: overshoot then decreasing oscillations.</summary>
    public static double ElasticOut(double t) => Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;

    /// <summary>Small overshoot: the feel of a detent.</summary>
    public static double BackOut(double t)
    {
        const double c1 = 2.2, c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    /// <summary>Signed shortest rotation from one angle to another, in degrees (-180..180).</summary>
    public static double AngleDelta(double fromDeg, double toDeg)
    {
        var d = (toDeg - fromDeg) % 360;
        if (d > 180) d -= 360;
        else if (d < -180) d += 360;
        return d;
    }

    public static bool TryAccent(Control control, out Color accent)
    {
        if (control.TryFindResource("SukiPrimaryColor", control.ActualThemeVariant, out var res) && res is Color c)
        {
            accent = c;
            return true;
        }
        accent = default;
        return false;
    }

    public static BoxShadows Glow(Color c, double blur, byte alpha, double spread = 0) =>
        new(new BoxShadow { Blur = blur, Spread = spread, Color = Color.FromArgb(alpha, c.R, c.G, c.B) });

    /// <summary>Binds a brush property of an element created in code to a theme resource (brush or color), so it follows the theme.</summary>
    public static void BindBrush(this AvaloniaObject target, AvaloniaProperty property, Control host, string key) =>
        target.Bind(property, host.GetResourceObservable(key, r => r is Color c ? new SolidColorBrush(c) : r));
}

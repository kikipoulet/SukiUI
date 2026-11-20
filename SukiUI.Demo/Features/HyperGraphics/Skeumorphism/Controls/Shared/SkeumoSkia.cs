using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

/// <summary>Theme colours captured on the UI thread for render-thread drawing.</summary>
public readonly record struct SkeumoPalette(
    SKColor Accent,
    SKColor Accent75,
    SKColor Text,
    SKColor LowText,
    SKColor LedOff,
    SKColor Track,
    SKColor Engrave,
    bool IsDark);

internal static class SkeumoSkia
{
    public static readonly SKTypeface DefaultTypeface = SKTypeface.Default;
    public static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);
    public static readonly SKTypeface SemiBoldTypeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    public static readonly SKTypeface MonoTypeface = SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold);

    public static SkeumoPalette CapturePalette(Control host)
    {
        var theme = host.ActualThemeVariant;
        var isDark = theme == ThemeVariant.Dark;
        var accent = ResolveColor(host, "SukiPrimaryColor", theme, new SKColor(0x5B, 0x8C, 0xFF));
        var accent75 = ResolveColor(host, "SukiPrimaryColor75", theme, new SKColor(accent.Red, accent.Green, accent.Blue, 191));
        var text = ResolveColor(host, "SukiText", theme, isDark ? new SKColor(0xED, 0xFF, 0xFF, 0xFF) : new SKColor(0x22, 0x22, 0x22, 0xFF));
        var lowText = ResolveColor(host, "SukiLowText", theme, isDark ? new SKColor(0xCA, 0xFF, 0xFF, 0xFF) : new SKColor(0x55, 0x55, 0x55, 0xFF));
        var ledOff = ResolveCatalogColor("SkeumoLedOff", theme, isDark ? new SKColor(0x3A, 0x45, 0x58) : new SKColor(0xB0, 0xBB, 0xCE));
        var track = ResolveCatalogColor("SkeumoTrack", theme, isDark ? new SKColor(0x00, 0x00, 0x00, 0x80) : new SKColor(0x40, 0x50, 0x70, 0x26));
        var engrave = ResolveCatalogColor("SkeumoEngrave", theme, isDark ? new SKColor(0x8F, 0xA4, 0xCC) : new SKColor(0x5A, 0x65, 0x80));
        return new SkeumoPalette(accent, accent75, text, lowText, ledOff, track, engrave, isDark);
    }

    public static SKColor ResolveColor(Control host, string key, ThemeVariant theme, SKColor fallback)
    {
        if (host.TryFindResource(key, theme, out var res))
            return ToSkColor(res, fallback);
        return fallback;
    }

    public static SKColor ResolveCatalogColor(string key, ThemeVariant theme, SKColor fallback)
    {
        if (SkeumoMaterials.TryFind(key, theme, out var res))
            return ToSkColor(res, fallback);
        return fallback;
    }

    private static SKColor ToSkColor(object? res, SKColor fallback) => res switch
    {
        Color c => new SKColor(c.R, c.G, c.B, c.A),
        ISolidColorBrush b => new SKColor(b.Color.R, b.Color.G, b.Color.B, (byte)Math.Clamp(Math.Round(b.Color.A * b.Opacity), 0, 255)),
        _ => fallback
    };

    public static SKColor WithAlpha(SKColor c, double alpha01) =>
        new(c.Red, c.Green, c.Blue, (byte)Math.Clamp(Math.Round(alpha01 * 255.0), 0, 255));

    public static SKColor ScaleAlpha(SKColor c, double factor) =>
        new(c.Red, c.Green, c.Blue, (byte)Math.Clamp(Math.Round(c.Alpha * factor), 0, 255));

    public static double AngleDelta(double fromDeg, double toDeg)
    {
        var d = (toDeg - fromDeg) % 360;
        if (d > 180) d -= 360;
        else if (d < -180) d += 360;
        return d;
    }

    public static double Smooth(double t)
    {
        t = Math.Clamp(t, 0, 1);
        return t * t * (3 - 2 * t);
    }

    public static double EaseOutCubic(double t)
    {
        t = Math.Clamp(t, 0, 1);
        return 1 - Math.Pow(1 - t, 3);
    }

    public static double ElasticOut(double t)
    {
        t = Math.Clamp(t, 0, 1);
        if (t is <= 0 or >= 1) return t;
        return Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * (2 * Math.PI / 3)) + 1;
    }

    public static double BackOut(double t)
    {
        t = Math.Clamp(t, 0, 1);
        const double c1 = 2.2, c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    public static void DrawLabel(
        SKCanvas canvas,
        SKFont font,
        SKPaint paint,
        string? text,
        float x,
        float yTop,
        float width,
        SKColor color,
        SKTextAlign align = SKTextAlign.Center)
    {
        if (string.IsNullOrEmpty(text)) return;
        paint.Style = SKPaintStyle.Fill;
        paint.Color = color;
        font.GetFontMetrics(out var metrics);
        var baseline = yTop - metrics.Ascent;
        var anchorX = align switch
        {
            SKTextAlign.Center => x + width * 0.5f,
            SKTextAlign.Right => x + width,
            _ => x
        };
        canvas.DrawText(text, anchorX, baseline, align, font, paint);
    }
}

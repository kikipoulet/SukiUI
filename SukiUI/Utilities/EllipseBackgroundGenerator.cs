using System;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Models;

namespace SukiUI.Utilities;

public sealed class EllipseBackgroundGenerator : ISukiBackgroundGenerator
{
    private static readonly Random Rand = new();
    private readonly SKImageFilter _blurFilter = SKImageFilter.CreateBlur(20, 20);
    private readonly SKPaint _paint = new() { IsDither = true };
    
    public void Draw(SKImageInfo info, SKCanvas canvas, SukiColorTheme colorTheme, ThemeVariant baseTheme)
    {
        var themeColor = colorTheme.Primary;
        var intBorder = colorTheme.Accent;

        int a = themeColor.R;
        int b = themeColor.G;
        int c = themeColor.B;

        var minValue = Math.Min(Math.Min(a, b), c);
        var maxValue = Math.Max(Math.Max(a, b), c);

        a = (a == minValue) ? 24 : ((a == maxValue) ? 33 : 20);
        b = (b == minValue) ? 24 : ((b == maxValue) ? 33 : 20);
        c = (c == minValue) ? 24 : ((c == maxValue) ? 33 : 20);


        canvas.Clear(baseTheme == ThemeVariant.Light
            ? new SKColor(241, 241, 241)
            : new SKColor((byte)a, (byte)b, (byte)c));

        _paint.ImageFilter = _blurFilter;
        _paint.Style = SKPaintStyle.Fill;


        _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 30);
        canvas.DrawPath(GenerateRandomEllipse(info, info.Width, info.Height, 10, info.Height - 11), _paint);
        _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 18);
        canvas.DrawPath(GenerateRandomEllipse(info, info.Width, info.Height, info.Width - 20, 12), _paint);

        // Neutral top left color
        _paint.Color = new SKColor(100, 100, 100, 16);
        canvas.DrawPath(GenerateRandomEllipse(info, info.Width, info.Height, 20, 20), _paint);


        _paint.Color = new SKColor(intBorder.R, intBorder.G, intBorder.B, 18);
        canvas.DrawPath(GenerateRandomEllipse(info, info.Width, info.Height, info.Width - 20, info.Height - 10), _paint);
    }
    
    
    private static SKPath GenerateRandomEllipse(SKImageInfo info, int maxWidth, int maxHeight, float centerX = 0, float centerY = 0)
    {
        var randomEllipse = new SKPath();

        if (centerX == 0)
        {
            centerX = Rand.Next(0, maxWidth);
            centerY = Rand.Next(0, maxHeight);
        }

        float radiusX = Rand.Next(info.Width / 5, info.Width / 2);
        float radiusY = Rand.Next(info.Width / 5, info.Width / 2);

        randomEllipse.AddOval(new SKRect(centerX - radiusX, centerY - radiusY, centerX + radiusX, centerY + radiusY));

        return randomEllipse;
    }
    
    
    public void Dispose()
    {
        _blurFilter.Dispose();
        _paint.Dispose();
    }
}
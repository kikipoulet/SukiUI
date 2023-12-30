using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Models;

namespace SukiUI.Controls;

public class SukiBackground : Image, IDisposable
{
    private const int ImageWidth = 340;
    private const int ImageHeight = 180;

    private readonly WriteableBitmap _bmp = new(new PixelSize(ImageWidth, ImageHeight), new Vector(96, 96),
        PixelFormats.Rgba8888);

    private readonly SKImageFilter _blurFilter = SKImageFilter.CreateBlur(20, 20);
    private readonly SKPaint _paint = new() { IsDither = true };

    public SukiBackground()
    {
        Source = _bmp;
        Stretch = Stretch.Fill;
    }

    public override void EndInit()
    {
        base.EndInit();

        SukiTheme.OnColorThemeChanged += Draw;
        SukiTheme.OnBaseThemeChanged += Draw;

        Draw(SukiTheme.ActiveColorTheme, Application.Current!.RequestedThemeVariant);
    }

    private void Draw(SukiColorTheme theme) => Draw(theme, Application.Current!.ActualThemeVariant);

    private void Draw(ThemeVariant baseTheme) => Draw(SukiTheme.ActiveColorTheme, baseTheme);

    private void Draw(SukiColorTheme theme, ThemeVariant baseTheme)
    {
        var themeColor = theme.Primary;
        var intBorder = theme.Accent;

        using var framebuffer = _bmp.Lock();
        var info = new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(), SKAlphaType.Premul);
        using var surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes);
        var canvas = surface.Canvas;

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
        canvas.DrawPath(GenerateRandomEllipse(ImageWidth, ImageHeight, 10, ImageHeight - 11), _paint);
        _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 18);
        canvas.DrawPath(GenerateRandomEllipse(ImageWidth, ImageHeight, ImageWidth - 20, 12), _paint);

        // Neutral top left color
        _paint.Color = new SKColor(100, 100, 100, 16);
        canvas.DrawPath(GenerateRandomEllipse(ImageWidth, ImageHeight, 20, 20), _paint);


        _paint.Color = new SKColor(intBorder.R, intBorder.G, intBorder.B, 18);
        canvas.DrawPath(GenerateRandomEllipse(ImageWidth, ImageHeight, ImageWidth - 20, ImageHeight - 10), _paint);
    }

    static SKPath GenerateRandomEllipse(int maxWidth, int maxHeight, float centerX = 0, float centerY = 0)
    {
        var randomEllipse = new SKPath();

        var random = new Random();
        if (centerX == 0)
        {
            centerX = random.Next(0, maxWidth);
            centerY = random.Next(0, maxHeight);
        }

        float radiusX = random.Next(ImageWidth / 5, ImageWidth / 2);
        float radiusY = random.Next(ImageWidth / 5, ImageWidth / 2);

        randomEllipse.AddOval(new SKRect(centerX - radiusX, centerY - radiusY, centerX + radiusX, centerY + radiusY));

        return randomEllipse;
    }

    public void Dispose()
    {
        _bmp.Dispose();
        _blurFilter.Dispose();
        _paint.Dispose();
    }
}
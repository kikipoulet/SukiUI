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
using SukiUI.Utilities;

namespace SukiUI.Controls;

public class SukiBackground : Image, IDisposable
{
    private const int ImageWidth = 340;
    private const int ImageHeight = 180;

    private readonly WriteableBitmap _bmp = new(new PixelSize(ImageWidth, ImageHeight), new Vector(96, 96),
        PixelFormats.Rgba8888);

    /// <summary>
    /// Quickly and easily assign a generator either for testing, or in future allow dev-defined generators...
    /// </summary>
    private readonly ISukiBackgroundGenerator _generator = 
        new NoiseBackgroundGenerator(FastNoiseLite.NoiseType.OpenSimplex2);

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
        using var framebuffer = _bmp.Lock();
        
        var info = new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(), SKAlphaType.Premul);
        
        using var surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes);
        
        _generator.Draw(info, surface.Canvas, theme, baseTheme);
    }

    public void Dispose()
    {
        _bmp.Dispose();
    }
}
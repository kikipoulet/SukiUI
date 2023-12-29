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
    private readonly WriteableBitmap _bmp = new(new PixelSize(1280,720), new Vector(96,96), PixelFormat.Rgba8888);
    
    private readonly SKImageFilter _blurFilter = SKImageFilter.CreateBlur(1000, 1000);
    private readonly SKPaint _paint = new();

    public SukiBackground()
    {
        Source = _bmp;
        Stretch = Stretch.UniformToFill;
    }
    
    public override void EndInit()
    {
        base.EndInit();
        // 60 FPS Ticks...
        Draw(SukiTheme.ActiveColorTheme, Application.Current!.ActualThemeVariant);
        SukiTheme.OnColorThemeChanged += Draw;
        SukiTheme.OnBaseThemeChanged += Draw;
    }

    private void Draw(SukiColorTheme theme) => Draw(theme, Application.Current!.ActualThemeVariant);

    private void Draw(ThemeVariant baseTheme) => Draw(SukiTheme.ActiveColorTheme, baseTheme);

    private void Draw(SukiColorTheme theme, ThemeVariant baseTheme)
    {
        var themeColor = theme.Primary;
        var intBorder = theme.Accent;
        
        using var framebuffer = _bmp.Lock();
        var info =  new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(),SKAlphaType.Premul);
        using var surface =  SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes);
        var canvas = surface.Canvas;
        
        canvas.Clear(baseTheme == ThemeVariant.Light ? SKColors.White : SKColors.Black); // Uses IntBorder as base color
        
        _paint.ImageFilter = _blurFilter;
        _paint.Style = SKPaintStyle.Fill;
        
        _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 50);
        canvas.DrawPath(BottomRightPath(), _paint);
        
        // generates a "complementary" color by just spinning the H value by 180 deg.
        _paint.Color = new SKColor(intBorder.R, intBorder.G, intBorder.B, 40);
        canvas.DrawPath(TopLeftPath(), _paint);
    }


    // Draw arbitrary looking shapes in a variety of places
    private SKPath? _bottomRightPath;
    private SKPath BottomRightPath()
    {
        if (_bottomRightPath is not null) return _bottomRightPath;
        _bottomRightPath = new SKPath();
        _bottomRightPath.MoveTo(1500,900);
        _bottomRightPath.RLineTo(-700, 0);
        _bottomRightPath.RLineTo(100, -200);
        _bottomRightPath.RLineTo(50, -50);
        _bottomRightPath.RLineTo(200, -100);
        _bottomRightPath.LineTo(1500, 420);
        _bottomRightPath.Close();
        return _bottomRightPath;
    }

    private SKPath? _topLeftPath;
    private SKPath TopLeftPath()
    {
        if (_topLeftPath is not null) return _topLeftPath;
        _topLeftPath = new SKPath();
        _topLeftPath.MoveTo(-200,-200);
        _topLeftPath.RLineTo(720, 0);
        _topLeftPath.RLineTo(150, 300);
        _topLeftPath.RLineTo(-350, 300);
        _topLeftPath.LineTo(-200, 400);
        _topLeftPath.Close();
        return _topLeftPath;
    }

    public void Dispose()
    {
        _bmp.Dispose();
        _blurFilter.Dispose();
        _paint.Dispose();
        _bottomRightPath?.Dispose();
        _topLeftPath?.Dispose();
    }
}
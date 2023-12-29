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
    private const int ImageWidth = 120;
    private const int ImageHeight = 90;
    private readonly WriteableBitmap _bmp = new(new PixelSize(ImageWidth,ImageHeight), new Vector(96,96), PixelFormat.Rgba8888);
    
    private readonly SKImageFilter _blurFilter = SKImageFilter.CreateBlur(20, 20);
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
        
        canvas.Clear(baseTheme == ThemeVariant.Light ? new SKColor(238,241,250) : new SKColor(16,25,47)); // Uses IntBorder as base color

             _paint.ImageFilter = _blurFilter;
        _paint.Style = SKPaintStyle.Fill;

      
            _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 20);
            canvas.DrawPath(GenerateRandomEllipse(ImageWidth,ImageHeight,10,ImageHeight - 11), _paint);
            _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 12);
            canvas.DrawPath(GenerateRandomEllipse(ImageWidth,ImageHeight,ImageWidth-20,12), _paint);
            _paint.Color = new SKColor(themeColor.R, themeColor.G, themeColor.B, 5);
            canvas.DrawPath(GenerateRandomEllipse(ImageWidth,ImageHeight), _paint);
           
        
        // generates a "complementary" color by just spinning the H value by 180 deg.
     
            _paint.Color = new SKColor(intBorder.R, intBorder.G, intBorder.B, 10);
            canvas.DrawPath(GenerateRandomEllipse(ImageWidth,ImageHeight, ImageWidth-20,ImageHeight - 10), _paint);
        
        
      
    }

    static SKPath GenerateRandomEllipse(int maxWidth, int maxHeight, float centerX = 0, float centerY = 0)
    {
        // Création d'une instance de SKPath pour l'ellipse
        SKPath randomEllipse = new SKPath();

        // Génération de valeurs aléatoires pour la position de l'ellipse
        Random random = new Random();
        if (centerX == 0)
        {
            centerX = random.Next(0, maxWidth);
            centerY = random.Next(0, maxHeight);
        }

        // Génération de valeurs aléatoires pour les dimensions de l'ellipse
        float radiusX = random.Next(ImageWidth /6, ImageWidth/2);
        float radiusY = random.Next(ImageWidth /6, ImageWidth/2);

        // Création de l'ellipse avec les valeurs aléatoires
        randomEllipse.AddOval(new SKRect(centerX - radiusX, centerY - radiusY, centerX + radiusX, centerY + radiusY));

        return randomEllipse;
    }
    
    // Draw arbitrary looking shapes in a variety of places
    private SKPath? _bottomRightPath;
    private SKPath BottomRightPath()
    {
        if (_bottomRightPath is not null) return _bottomRightPath;
        _bottomRightPath = new SKPath();
        _bottomRightPath.MoveTo(1300,750);
        _bottomRightPath.RLineTo(-700, 0);
        _bottomRightPath.RLineTo(100, -200);
        _bottomRightPath.RLineTo(50, -50);
        _bottomRightPath.RLineTo(200, -100);
        _bottomRightPath.LineTo(1500, 420);
        _bottomRightPath.Close();
        return _bottomRightPath;
    }

    private SKPath? _bottomLeftPath;
    private SKPath BottomLeftPath()
    {
        if (_bottomLeftPath is not null) return _bottomLeftPath;
        _bottomLeftPath = new SKPath();
        _bottomLeftPath.MoveTo(-200,900);
        _bottomLeftPath.RLineTo(720, 0);
        _bottomLeftPath.RLineTo(150, 300);
        _bottomLeftPath.RLineTo(-350, 300);
        _bottomLeftPath.LineTo(-200, 400);
        _bottomLeftPath.Close();
        return _bottomLeftPath;
    }

    public void Dispose()
    {
        _bmp.Dispose();
        _blurFilter.Dispose();
        _paint.Dispose();
        _bottomRightPath?.Dispose();
        _bottomLeftPath?.Dispose();
    }
}
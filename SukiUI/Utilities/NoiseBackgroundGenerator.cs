using System;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;
using SukiUI.Models;

namespace SukiUI.Utilities;

public sealed class NoiseBackgroundGenerator : ISukiBackgroundGenerator
{
    private static readonly Random Rand = new();
    private static readonly FastNoiseLite NoiseGen = new();
    
    private SKBitmap? _primary;
    private SKBitmap? _accent;

    public NoiseBackgroundGenerator(FastNoiseLite.NoiseType type)
    {
        NoiseGen.SetNoiseType(type);
    }
    
    public void Draw(SKImageInfo imageInfo, SKCanvas canvas, SukiColorTheme colorTheme, ThemeVariant baseTheme)
    {
        var themeColor = colorTheme.Primary;
        var accentColor = colorTheme.Accent;
        
        _primary ??= new SKBitmap(imageInfo);
        _accent ??= new SKBitmap(imageInfo);
        
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

        const float scale = 0.5f;
        const float accentAlpha = 0.1f;
        
        var pOffset = Rand.Next(1000);
        var aOffset = Rand.Next(1000);
        
        for(var y = 0; y < imageInfo.Height; y++)
        for (var x = 0; x < imageInfo.Width; x++)
        {
            // primary layer.
            var noise = NoiseGen.GetNoise((pOffset + x) * scale, (pOffset + y) * scale);
            noise = (noise + 1f) / 2f; // noise returns -1 to +1 which isn't useful.
            var alpha = (byte)(noise * 255);
            _primary.SetPixel(x, y, themeColor.ToSKColor().WithAlpha(alpha));
            
            // accent layer.
            noise = NoiseGen.GetNoise((aOffset + x) * scale, (aOffset + y) * scale);
            noise = (noise + 1f) / 2f * accentAlpha;
            alpha = (byte)(noise * 255);
            _accent.SetPixel(x,y, accentColor.ToSKColor().WithAlpha(alpha));
        }
        canvas.DrawBitmap(_primary, 0,0, new SKPaint() { BlendMode = SKBlendMode.Overlay} );
        canvas.DrawBitmap(_accent, 0,0 );
    }

    public void Dispose()
    {
        _primary?.Dispose();
        _accent?.Dispose();
    }
}
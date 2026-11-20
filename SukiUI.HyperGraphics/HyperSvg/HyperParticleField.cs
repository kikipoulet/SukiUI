using System;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Champ de particules ultra-bas coût CPU :
///     - Table trigonométrique précalculée (zéro Math.Sin par particule)
///     - Sprite 3-en-1 pré-baké dans l'atlas partagé (1 quad au lieu de 3 par particule)
///     - Remplit directement un buffer partagé SKRotationScaleMatrix[] / SKColor[] pour émettre 1 seul appel DrawAtlas
///       avec les autres couches de la fenêtre.
/// </summary>
public sealed class HyperParticleField : IDisposable
{
    private const int LutSize = 1024;
    private const int LutMask = LutSize - 1;
    private const float RadToLut = LutSize / (2f * MathF.PI);

    private static readonly float[] SinLut = BuildSinLut();

    private static float[] BuildSinLut()
    {
        var lut = new float[LutSize];
        for (var i = 0; i < LutSize; i++)
            lut[i] = MathF.Sin(i * (2f * MathF.PI / LutSize));
        return lut;
    }

    private readonly int _count;
    private readonly float _originX, _originY, _width, _height, _span, _invSpan, _maxX;
    private readonly float[] _x0, _speedNorm, _phase, _sway, _omegaLut, _phaseLut, _scale, _halfSprite, _radius, _oxMinusHs, _oyTopMinusHs, _b;

    // Pre-allocated exact-size buffers for zero-allocation, zero-loop DrawAtlas
    private readonly SKRect[] _src;
    private readonly SKRotationScaleMatrix[] _xf;
    private readonly SKColor[] _col;
    private SKRect _cachedSpriteRect;
    private float _cachedOpacity = -1f;
    private float _cachedBrightness = -1f;

    // Optional low-res pixelated buffer when pixel > 0.4
    private SKBitmap? _low;
    private SKCanvas? _lowCanvas;
    private int _lowWidth, _lowHeight;
    private SKPaint? _bitmapPaint;

    /// <summary>Nombre total de particules dans le champ.</summary>
    public int Count => _count;

    /// <summary>Lit la position et le rayon courants de la particule <paramref name="index"/>.</summary>
    public void GetParticle(int index, out float cx, out float cy, out float radius)
    {
        var hs = _halfSprite[index];
        cx = _xf[index].TX + hs;
        cy = _xf[index].TY + hs;
        radius = _radius[index];
    }

    /// <summary>Initialise un champ de particules déterministe dans la fenêtre donnée.</summary>
    public HyperParticleField(int count, double originX, double originY, double width, double height, int seed)
    {
        _count = count;
        _originX = (float)originX;
        _originY = (float)originY;
        _width = (float)width;
        _height = (float)height;
        _span = _height + 6f;
        _invSpan = 1f / _span;
        _maxX = _width - 3f;

        _src = new SKRect[count];
        _xf = new SKRotationScaleMatrix[count];
        _col = new SKColor[count];

        var rng = new Random(seed);
        _x0 = new float[count];
        _speedNorm = new float[count];
        _phase = new float[count];
        _sway = new float[count];
        _omegaLut = new float[count];
        _phaseLut = new float[count];
        _scale = new float[count];
        _halfSprite = new float[count];
        _radius = new float[count];
        _oxMinusHs = new float[count];
        _oyTopMinusHs = new float[count];
        _b = new float[count];

        const float spriteBodyScale = (HyperAtlas.ParticleSpriteSize - 1f) / 2.4f;
        const float half = HyperAtlas.ParticleSpriteSize * 0.5f;
        for (var i = 0; i < count; i++)
        {
            _x0[i] = 4f + (float)rng.NextDouble() * Math.Max(1f, _width - 8f);
            var r = 0.7f + (float)Math.Pow(rng.NextDouble(), 2) * 2.0f;
            var sc = (2f * r) / spriteBodyScale;
            _scale[i] = sc;
            var hs = sc * half;
            _halfSprite[i] = hs;
            _radius[i] = r;
            _oxMinusHs[i] = _originX - hs;
            _oyTopMinusHs[i] = _originY - 3f - hs;
            _b[i] = 0.5f + (float)rng.NextDouble() * 0.5f;
            var speed = (0.04f + (float)rng.NextDouble() * 0.1f) * 60f;
            _speedNorm[i] = speed * _invSpan;
            var phase = (float)rng.NextDouble();
            _phase[i] = phase;
            _sway[i] = 0.6f + (float)rng.NextDouble() * 1.4f;
            var omega = 0.3f + (float)rng.NextDouble() * 0.5f;
            _omegaLut[i] = omega * RadToLut;
            _phaseLut[i] = phase * LutSize;
        }
    }

    /// <summary>Met à jour les positions des particules pour l'instant <paramref name="seconds"/>.</summary>
    public void Update(float seconds)
    {
        var span = _span;
        var maxX = _maxX;

        for (var i = 0; i < _count; i++)
        {
            var cycle = seconds * _speedNorm[i] + _phase[i];
            cycle -= MathF.Floor(cycle);
            var y = _oyTopMinusHs[i] + cycle * span;

            var lutIdx = (int)(seconds * _omegaLut[i] + _phaseLut[i]) & LutMask;
            var x = _x0[i] + _sway[i] * SinLut[lutIdx];
            if (x < 3f) x = 3f;
            else if (x > maxX) x = maxX;

            _xf[i] = new SKRotationScaleMatrix(_scale[i], 0f, _oxMinusHs[i] + x, y);
        }
    }

    internal void Draw(
        SKCanvas canvas,
        SKImage atlasImage,
        SKRect spriteRect,
        SKPaint atlasPaint,
        SKSamplingOptions sampling,
        float opacity,
        float brightness)
    {
        if (_cachedSpriteRect != spriteRect)
        {
            _cachedSpriteRect = spriteRect;
            Array.Fill(_src, spriteRect);
        }

        if (_cachedOpacity != opacity || _cachedBrightness != brightness)
        {
            _cachedOpacity = opacity;
            _cachedBrightness = brightness;
            for (var i = 0; i < _count; i++)
            {
                var a = brightness * _b[i];
                if (a > 1f) a = 1f;
                var c = (byte)(a * opacity * 255f);
                _col[i] = new SKColor(c, c, c, c);
            }
        }

        canvas.DrawAtlas(atlasImage, _src, _xf, _col, SKBlendMode.Modulate, sampling, atlasPaint);
    }

    internal void DrawPixelated(
        SKCanvas canvas,
        SKImage atlasImage,
        SKRect spriteRect,
        SKPaint atlasPaint,
        double opacity,
        double brightness,
        double pixel)
    {
        var lw = Math.Max(2, (int)Math.Ceiling(_width / pixel));
        var lh = Math.Max(2, (int)Math.Ceiling(_height / pixel));
        if (_low == null || _lowWidth != lw || _lowHeight != lh)
        {
            FreeLow();
            _low = new SKBitmap(new SKImageInfo(lw, lh, SKColorType.Bgra8888, SKAlphaType.Premul));
            _lowCanvas = new SKCanvas(_low);
            _lowWidth = lw;
            _lowHeight = lh;
        }

        _lowCanvas!.Clear(SKColors.Transparent);
        _lowCanvas.SetMatrix(SKMatrix.Concat(
            SKMatrix.CreateScale(lw / _width, lh / _height),
            SKMatrix.CreateTranslation(-_originX, -_originY)));
        Draw(_lowCanvas, atlasImage, spriteRect, atlasPaint, new SKSamplingOptions(SKFilterMode.Linear), 1f, (float)brightness);

        _bitmapPaint ??= new SKPaint();
        _bitmapPaint.Color = new SKColor(255, 255, 255, (byte)(255 * opacity));
        using var lowImg = SKImage.FromBitmap(_low);
        canvas.DrawImage(lowImg, new SKRect(0, 0, lw, lh), new SKRect(0, 0, _width, _height), new SKSamplingOptions(SKFilterMode.Nearest), _bitmapPaint);
    }

    private void FreeLow()
    {
        _lowCanvas?.Dispose();
        _low?.Dispose();
        _lowCanvas = null;
        _low = null;
    }

    /// <summary>Libère les ressources Skia allouées pour le rendu pixelisé.</summary>
    public void Dispose()
    {
        FreeLow();
        _bitmapPaint?.Dispose();
    }
}

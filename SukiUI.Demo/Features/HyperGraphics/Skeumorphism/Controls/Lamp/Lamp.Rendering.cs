using System;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Lamp
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const double GlowEnd = 0.35, LightStart = 0.25;
        private static readonly SKColor FilamentGlow = new(0xFF, 0x9A, 0x40);
        private static readonly SKColor FilamentHeatColor = new(0xFF, 0x8C, 0x3A);
        private static readonly SKColor FilamentCoreColor = new(0xFF, 0xF6, 0xDE);

        private HyperLayer _bulbGlow = null!, _filamentCold = null!;
        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _filBlur = null!;
        private SKPath _filamentPath = null!;
        private double _heat = -1, _glow, _light, _flicker = 1;

        protected override void OnBind(IHyperBinder ui)
        {
            _bulbGlow = ui.Layer("bulbGlow");
            _filamentCold = ui.Layer("filamentCold");
            _paint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round });
            _filBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f));
            _glowPaint.MaskFilter = _filBlur;
            _filamentPath = ui.Own(SKPath.ParseSvgPathData("M 62,70 L 65,63 L 68,70 L 71,63 L 74,70 L 77,63 L 80,70 L 83,63 L 86,70 L 88,70")!);
        }

        protected override bool OnFrame(in HyperFrame frame, in LampState state)
        {
            if (_heat < 0) _heat = state.Intensity;
            var rate = state.Intensity > _heat ? 4.4 : 2.8;
            _heat += (state.Intensity - _heat) * (1.0 - Math.Exp(-frame.Dt * rate));
            if (Math.Abs(state.Intensity - _heat) < 0.001) _heat = state.Intensity;

            _glow = SkeumoSkia.Smooth(_heat / GlowEnd);
            _light = SkeumoSkia.Smooth((_heat - LightStart) / (1.0 - LightStart));
            _flicker = 1.0 + 0.015 * Math.Sin(frame.Seconds * 60.0 * 0.7) * _light;

            _filamentCold.Opacity = 1.0 - 0.7 * _glow;
            _bulbGlow.Opacity = 0.75 * _light * _flicker;

            return _heat > 0.002 || state.Intensity > 0.002;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            // Light spread behind the lamp (centered at bulb 95, 85)
            var spreadAlpha = 0.55 * _light * _flicker * opacity;
            if (spreadAlpha > 0.005)
            {
                using var spreadShader = SKShader.CreateRadialGradient(
                    new SKPoint(95f, 85f), 85f,
                    new[]
                    {
                        SkeumoSkia.WithAlpha(new SKColor(0xFF, 0xE7, 0xB8), spreadAlpha),
                        SkeumoSkia.WithAlpha(new SKColor(0xFF, 0xC8, 0x77), 0.7 * spreadAlpha),
                        SkeumoSkia.WithAlpha(new SKColor(0xFF, 0xB0, 0x50), 0.25 * spreadAlpha),
                        new SKColor(0xFF, 0xB0, 0x50, 0)
                    },
                    new[] { 0f, 0.25f, 0.55f, 1f }, SKShaderTileMode.Clamp);
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                _paint.Shader = spreadShader;
                canvas.DrawCircle(95f, 85f, 85f, _paint);
                _paint.Shader = null;
            }

            base.OnDraw(canvas, opacity);

            // Heated filament + white-hot core
            if (_glow > 0.01)
            {
                canvas.Save();
                canvas.Translate(20f, 21f);
                _glowPaint.StrokeWidth = 2.6f;
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(FilamentGlow, _glow * opacity);
                canvas.DrawPath(_filamentPath, _glowPaint);

                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeWidth = 1.8f;
                _paint.Color = SkeumoSkia.ScaleAlpha(FilamentHeatColor, _glow * opacity);
                canvas.DrawPath(_filamentPath, _paint);

                if (_light > 0.01)
                {
                    _paint.StrokeWidth = 0.9f;
                    _paint.Color = SkeumoSkia.ScaleAlpha(FilamentCoreColor, _light * opacity);
                    canvas.DrawPath(_filamentPath, _paint);
                }
                canvas.Restore();
            }
        }
    }
}

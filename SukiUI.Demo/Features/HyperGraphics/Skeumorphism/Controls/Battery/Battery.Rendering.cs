using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Battery
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const int CellCount = 4;
        private const float CellTop = 68f, CellHeight = 17f, CellGap = 5f, BodyLeft = 31f, BodyWidth = 58f;
        private const double LowCharge = 0.1;

        private static readonly Led Green = new(new(0x8E, 0xF0, 0xB8), new(0x46, 0xD3, 0x8B), new(0x24, 0x8F, 0x58));
        private static readonly Led Amber = new(new(0xFF, 0xD8, 0x87), new(0xF2, 0xB3, 0x3D), new(0xAE, 0x76, 0x12));
        private static readonly Led Red = new(new(0xFF, 0x8A, 0x8A), new(0xE5, 0x48, 0x4D), new(0x9E, 0x23, 0x28));
        private static readonly SKColor BoltCharging = new(0xFF, 0xD2, 0x4A);

        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _cellGlow = null!, _boltGlow = null!, _textGlow = null!;
        private SKPath _boltPath = null!;
        private SKFont _polarityFont = null!, _readoutFont = null!, _ratingFont = null!;
        private double _shown = -1;
        private double _seconds;
        private BatteryState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _cellGlow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3.5f));
            _boltGlow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f));
            _textGlow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2.5f));
            _boltPath = ui.Own(SKPath.ParseSvgPathData("M 63,24 L 55,36 L 60,36 L 57,44 L 66,32 L 61,32 L 64,24 Z")!);
            _polarityFont = ui.Own(new SKFont(SkeumoSkia.BoldTypeface, 10f));
            _readoutFont = ui.Own(new SKFont(SkeumoSkia.MonoTypeface, 11f));
            _ratingFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 5.5f));

            ui.Paint("cells", DrawCells);
        }

        protected override bool OnFrame(in HyperFrame frame, in BatteryState state)
        {
            _state = state;
            _seconds = frame.Seconds;
            if (_shown < 0) _shown = state.Charge;
            _shown += (state.Charge - _shown) * (1.0 - Math.Exp(-frame.Dt * 5.0));
            if (Math.Abs(state.Charge - _shown) < 0.0005) _shown = state.Charge;

            return state.IsCharging || _shown < LowCharge || Math.Abs(state.Charge - _shown) > 0.0005;
        }

        private void DrawCells(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var shown = Math.Clamp(_shown, 0, 1);
            var led = shown > 0.5 ? Green : shown > 0.25 ? Amber : Red;
            var pal = _state.Palette;

            // Readout
            var pctText = $"{Math.Round(shown * 100):0}%";
            _paint.MaskFilter = _textGlow;
            SkeumoSkia.DrawLabel(canvas, _readoutFont, _paint, pctText, 22f, 52f, 76f, SkeumoSkia.ScaleAlpha(led.Mid, 0.6 * opacity));
            _paint.MaskFilter = null;
            SkeumoSkia.DrawLabel(canvas, _readoutFont, _paint, pctText, 22f, 52f, 76f, SkeumoSkia.ScaleAlpha(led.Top, opacity));

            var front = -1;
            for (var i = 0; i < CellCount; i++)
            {
                var level = Math.Clamp(shown * CellCount - i, 0, 1);
                if (level > 0 && level < 1) front = i;
            }

            for (var i = 0; i < CellCount; i++)
            {
                var top = CellTop + (CellCount - 1 - i) * (CellHeight + CellGap);
                var r = CellHeight * 0.5f;

                _paint.Style = SKPaintStyle.Fill;
                _paint.Shader = null;
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.Track, opacity);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(BodyLeft, top, BodyLeft + BodyWidth, top + CellHeight), r, r), _paint);

                var level = Math.Clamp(shown * CellCount - i, 0, 1);
                var width = (float)(BodyWidth * level);
                if (width <= CellHeight * 0.4f) continue;

                var cellOpacity = 1.0;
                if (_state.IsCharging && i == front)
                    cellOpacity = 0.8 + 0.2 * Math.Sin(_seconds * 60.0 * 0.06);
                else if (!_state.IsCharging && shown < LowCharge && i == 0)
                    cellOpacity = 0.45 + 0.55 * (0.5 + 0.5 * Math.Sin(_seconds * 60.0 * 0.08));

                var effAlpha = cellOpacity * opacity;
                var fillRect = new SKRoundRect(new SKRect(BodyLeft, top, BodyLeft + width, top + CellHeight), r, r);

                _glowPaint.MaskFilter = _cellGlow;
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(led.Mid, 0.44 * effAlpha);
                canvas.DrawRoundRect(fillRect, _glowPaint);

                using var grad = SKShader.CreateLinearGradient(
                    new SKPoint(0, top), new SKPoint(0, top + CellHeight),
                    new[] { SkeumoSkia.ScaleAlpha(led.Top, effAlpha), SkeumoSkia.ScaleAlpha(led.Mid, effAlpha), SkeumoSkia.ScaleAlpha(led.Bottom, effAlpha) },
                    new[] { 0f, 0.45f, 1f }, SKShaderTileMode.Clamp);
                _paint.Color = SKColors.White;
                _paint.Shader = grad;
                canvas.DrawRoundRect(fillRect, _paint);
                _paint.Shader = null;
            }
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var pal = _state.Palette;
            var engrave = SkeumoSkia.ScaleAlpha(pal.Engrave, opacity);

            SkeumoSkia.DrawLabel(canvas, _polarityFont, _paint, "+", 27f, 24f, 14f, engrave);
            SkeumoSkia.DrawLabel(canvas, _polarityFont, _paint, "âˆ’", 79f, 24f, 14f, engrave);
            SkeumoSkia.DrawLabel(canvas, _ratingFont, _paint, "LI-ION Â· 12 V Â· 20 Ah", 8f, 158f, 104f, engrave);

            // Charge bolt
            var isFull = _shown >= 0.995 && _state.Charge >= 0.999;
            var boltAlpha = !isFull && _state.IsCharging
                ? (0.7 + 0.3 * (0.5 + 0.5 * Math.Sin(_seconds * 60.0 * 0.05))) * opacity
                : opacity;
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;

            if (!isFull && !_state.IsCharging)
            {
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.LedOff, opacity);
                canvas.DrawPath(_boltPath, _paint);
            }
            else
            {
                var c = isFull ? Green.Mid : BoltCharging;
                _glowPaint.MaskFilter = _boltGlow;
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(c, 0.9 * boltAlpha);
                canvas.DrawPath(_boltPath, _glowPaint);

                _paint.Color = SkeumoSkia.ScaleAlpha(c, boltAlpha);
                canvas.DrawPath(_boltPath, _paint);
            }
        }

        private sealed record Led(SKColor Top, SKColor Mid, SKColor Bottom);
    }
}

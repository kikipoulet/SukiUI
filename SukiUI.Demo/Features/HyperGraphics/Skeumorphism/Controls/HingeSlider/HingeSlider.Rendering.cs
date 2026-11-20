using System;
using System.Collections.Generic;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class HingeSlider
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float ViewW = 268f, ViewH = 28f;

        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _fillGlow = null!, _dotGlow = null!, _thumbShadow = null!;
        private SKFont _captionFont = null!;
        private HingeSliderState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _fillGlow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f));
            _dotGlow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f));
            _thumbShadow = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3.5f));
            _captionFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 11f));
        }

        protected override bool OnFrame(in HyperFrame frame, in HingeSliderState state)
        {
            _state = state;
            return false;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            var w = (float)_state.BoundsWidth;
            var h = (float)_state.BoundsHeight;
            if (w <= 1f || h <= 1f) return;

            canvas.Save();
            canvas.Scale(ViewW / w, ViewH / h);

            var trackLeft = (float)TrackPadX;
            var trackW = Math.Max(1f, w - trackLeft * 2f);
            var cy = 14f;
            var pal = _state.Palette;

            // Notches
            var steps = _state.Steps;
            if (steps.Count >= 2)
            {
                _paint.Style = SKPaintStyle.Fill;
                _paint.Shader = null;
                _paint.Color = SkeumoSkia.WithAlpha(new SKColor(0x80, 0x90, 0xA8), 0.4 * opacity);
                for (var i = 0; i < steps.Count; i++)
                {
                    var tx = Math.Clamp(trackLeft + i / (float)(steps.Count - 1) * trackW, trackLeft, trackLeft + trackW - 1f);
                    canvas.DrawRect(tx, 21f, 1f, 4f, _paint);
                }
            }

            // Rail
            var railRect = new SKRoundRect(new SKRect(trackLeft, cy - 3.5f, trackLeft + trackW, cy + 3.5f), 4f, 4f);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            _paint.Color = SkeumoSkia.WithAlpha(pal.IsDark ? new SKColor(0, 0, 0) : new SKColor(0xE1, 0xE7, 0xF1), (pal.IsDark ? 0.6 : 1.0) * opacity);
            canvas.DrawRoundRect(railRect, _paint);

            // Rail top inner shadow
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.Black, (pal.IsDark ? 0.4 : 0.16) * opacity);
            canvas.DrawRect(trackLeft + 2f, cy - 3.5f, trackW - 4f, 1.2f, _paint);

            var fraction = steps.Count >= 2 ? (float)(IndexOf(steps, _state.Value) / (steps.Count - 1)) : 0f;
            var fillW = Math.Max(0f, fraction * trackW);
            var thumbX = trackLeft + fillW;

            if (fillW > 1f)
            {
                var fillRect = new SKRoundRect(new SKRect(trackLeft, cy - 3.5f, trackLeft + fillW, cy + 3.5f), 4f, 4f);
                _glowPaint.MaskFilter = _fillGlow;
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.4 * opacity);
                canvas.DrawRoundRect(fillRect, _glowPaint);

                using var fillShader = SKShader.CreateLinearGradient(
                    new SKPoint(trackLeft, 0), new SKPoint(trackLeft + fillW, 0),
                    new[] { SkeumoSkia.ScaleAlpha(pal.Accent75, opacity), SkeumoSkia.ScaleAlpha(pal.Accent, opacity) },
                    new[] { 0f, 1f }, SKShaderTileMode.Clamp);
                _paint.Color = SKColors.White;
                _paint.Shader = fillShader;
                canvas.DrawRoundRect(fillRect, _paint);
                _paint.Shader = null;
            }

            // Pearl thumb shadow
            _glowPaint.MaskFilter = _thumbShadow;
            _glowPaint.Color = SkeumoSkia.WithAlpha(SKColors.Black, (pal.IsDark ? 0.65 : 0.35) * opacity);
            canvas.DrawCircle(thumbX, cy + 2f, 11f, _glowPaint);

            // Pearl thumb sphere
            var c0 = pal.IsDark ? new SKColor(0xF4, 0xF7, 0xFC) : SKColors.White;
            var c1 = pal.IsDark ? new SKColor(0xB9, 0xC4, 0xD8) : new SKColor(0xF1, 0xF4, 0xFA);
            var c2 = pal.IsDark ? new SKColor(0x6F, 0x7C, 0x93) : new SKColor(0xD3, 0xDB, 0xE8);
            using (var thumbShader = SKShader.CreateRadialGradient(
                new SKPoint(thumbX - 3.3f, cy - 4.8f), 14f,
                new[] { SkeumoSkia.ScaleAlpha(c0, opacity), SkeumoSkia.ScaleAlpha(c1, opacity), SkeumoSkia.ScaleAlpha(c2, opacity) },
                new[] { 0f, 0.6f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Color = SKColors.White;
                _paint.Shader = thumbShader;
                canvas.DrawCircle(thumbX, cy, 11f, _paint);
                _paint.Shader = null;
            }

            // Luminous accent dot
            _glowPaint.MaskFilter = _dotGlow;
            _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, opacity);
            canvas.DrawCircle(thumbX, cy, 3.5f, _glowPaint);
            _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, opacity);
            canvas.DrawCircle(thumbX, cy, 3.5f, _paint);

            // Optional captions
            if (!string.IsNullOrEmpty(_state.LeftCaption) || !string.IsNullOrEmpty(_state.RightCaption))
            {
                var capColor = SkeumoSkia.ScaleAlpha(pal.LowText, opacity);
                SkeumoSkia.DrawLabel(canvas, _captionFont, _paint, _state.LeftCaption, 2f, 29f, w * 0.5f, capColor, SKTextAlign.Left);
                SkeumoSkia.DrawLabel(canvas, _captionFont, _paint, _state.RightCaption, w * 0.5f - 2f, 29f, w * 0.5f, capColor, SKTextAlign.Right);
            }

            canvas.Restore();
        }

        private static double IndexOf(IReadOnlyList<double> steps, double value)
        {
            value = Math.Clamp(value, steps[0], steps[^1]);
            for (var i = 0; i < steps.Count - 1; i++)
                if (value <= steps[i + 1])
                    return i + Math.Log(value / steps[i]) / Math.Log(steps[i + 1] / steps[i]);
            return steps.Count - 1;
        }
    }
}

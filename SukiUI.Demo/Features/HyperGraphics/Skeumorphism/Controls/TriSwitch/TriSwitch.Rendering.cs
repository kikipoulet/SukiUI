using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class TriSwitch
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float FillLeft = 14f;
        private const double SlideSec = 0.22;

        private HyperLayer _knob = null!;
        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _fillBlur = null!;
        private SKFont _posFont = null!;
        private double _x = double.NaN, _from, _to, _elapsed;
        private bool _sliding;
        private TriSwitchState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _knob = ui.Layer("knob");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _fillBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f));
            _glowPaint.MaskFilter = _fillBlur;
            _posFont = ui.Own(new SKFont(SkeumoSkia.BoldTypeface, 8f));

            ui.Paint("fillLayer", DrawFill);
        }

        protected override bool OnFrame(in HyperFrame frame, in TriSwitchState state)
        {
            _state = state;
            var target = DetentX[Math.Clamp(state.SelectedIndex, 0, 2)];

            if (double.IsNaN(_x))
            {
                _x = _from = _to = target;
                _sliding = false;
            }
            else if (Math.Abs(target - _to) > 0.01)
            {
                _from = _x;
                _to = target;
                _elapsed = 0;
                _sliding = true;
            }

            if (_sliding)
            {
                _elapsed += frame.Dt;
                var t = Math.Min(1.0, _elapsed / SlideSec);
                _x = _from + (_to - _from) * SkeumoSkia.BackOut(t);
                if (t >= 1.0)
                {
                    _x = _to;
                    _sliding = false;
                }
            }

            _knob.X = _x - DetentX[0];
            return _sliding;
        }

        private void DrawFill(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var w = (float)Math.Max(0.0, _x - FillLeft - 3.0);
            if (w <= 0.5f) return;

            var pal = _state.Palette;
            var rect = new SKRoundRect(new SKRect(FillLeft, 11f, FillLeft + w, 15f), 2f, 2f);
            _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.4 * opacity);
            canvas.DrawRoundRect(rect, _glowPaint);

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(FillLeft, 0), new SKPoint(FillLeft + w, 0),
                new[] { SkeumoSkia.ScaleAlpha(pal.Accent75, opacity), SkeumoSkia.ScaleAlpha(pal.Accent, opacity) },
                new[] { 0f, 1f }, SKShaderTileMode.Clamp);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SKColors.White;
            _paint.Shader = shader;
            canvas.DrawRoundRect(rect, _paint);
            _paint.Shader = null;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var pal = _state.Palette;
            var idx = _state.SelectedIndex;
            SkeumoSkia.DrawLabel(canvas, _posFont, _paint, _state.LeftLabel, 5f, 26f, 30f, SkeumoSkia.ScaleAlpha(idx == 0 ? pal.Text : pal.LowText, opacity));
            SkeumoSkia.DrawLabel(canvas, _posFont, _paint, _state.MiddleLabel, 33f, 26f, 30f, SkeumoSkia.ScaleAlpha(idx == 1 ? pal.Text : pal.LowText, opacity));
            SkeumoSkia.DrawLabel(canvas, _posFont, _paint, _state.RightLabel, 61f, 26f, 30f, SkeumoSkia.ScaleAlpha(idx == 2 ? pal.Text : pal.LowText, opacity));
        }
    }
}

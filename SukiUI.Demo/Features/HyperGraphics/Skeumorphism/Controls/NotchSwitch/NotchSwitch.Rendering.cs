using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class NotchSwitch
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private static readonly double[] KnobOffsetY = { 0, -27, -54 };

        private HyperLayer _knob = null!;
        private SKPaint _paint = null!;
        private SKFont _labelFont = null!;
        private HyperSmooth _knobY;
        private NotchSwitchState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _knob = ui.Layer("knob");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _labelFont = ui.Own(new SKFont(SkeumoSkia.SemiBoldTypeface, 14f));
            ui.Paint("fillLayer", DrawFill);
        }

        protected override bool OnFrame(in HyperFrame frame, in NotchSwitchState state)
        {
            _state = state;
            var target = KnobOffsetY[Math.Clamp(state.SelectedIndex, 0, 2)];
            var moving = _knobY.Follow(target, in frame, 0.07);
            _knob.Y = _knobY.Value;
            return moving;
        }

        private void DrawFill(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var top = (float)(KnobBottomY + _knobY.Value);
            var h = (float)(7.0 - _knobY.Value);
            var pal = _state.Palette;
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, top), new SKPoint(0, top + h),
                new[] { SkeumoSkia.ScaleAlpha(pal.Accent, opacity), SkeumoSkia.ScaleAlpha(pal.Accent75, opacity) },
                new[] { 0f, 1f }, SKShaderTileMode.Clamp);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SKColors.White;
            _paint.Shader = shader;
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(95f, top, 103f, top + h), 4f, 4f), _paint);
            _paint.Shader = null;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var pal = _state.Palette;
            var idx = _state.SelectedIndex;
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, _state.TopLabel, 16f, 13f, 60f, SkeumoSkia.ScaleAlpha(idx == 2 ? pal.Text : pal.LowText, opacity), SKTextAlign.Right);
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, _state.MiddleLabel, 16f, 40f, 60f, SkeumoSkia.ScaleAlpha(idx == 1 ? pal.Text : pal.LowText, opacity), SKTextAlign.Right);
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, _state.BottomLabel, 16f, 67f, 60f, SkeumoSkia.ScaleAlpha(idx == 0 ? pal.Text : pal.LowText, opacity), SKTextAlign.Right);
        }
    }
}

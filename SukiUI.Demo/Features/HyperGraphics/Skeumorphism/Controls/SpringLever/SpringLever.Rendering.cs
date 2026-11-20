using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class SpringLever
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float ArcR = 60f, KnobR = 57f, MaxDegF = 35f, DeadDegF = 4f;

        private HyperLayer _lever = null!;
        private SKPaint _paint = null!;
        private SKFont _labelFont = null!;
        private SKPath _arcPath = null!;

        private double _angle;
        private double _springFrom;
        private double _springElapsed;
        private bool _springing;
        private int _lastReleaseId;
        private SpringLeverState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _lever = ui.Layer("lever");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _labelFont = ui.Own(new SKFont(SkeumoSkia.SemiBoldTypeface, 11f));
            _arcPath = ui.Own(new SKPath());
            ui.Paint("overlay", DrawOverlay);
        }

        protected override bool OnFrame(in HyperFrame frame, in SpringLeverState state)
        {
            _state = state;
            if (state.Engaged)
            {
                _springing = false;
                _angle = state.Angle;
                _lastReleaseId = state.ReleaseId;
            }
            else if (state.ReleaseId != _lastReleaseId)
            {
                _lastReleaseId = state.ReleaseId;
                _springFrom = _angle;
                _springElapsed = 0;
                _springing = Math.Abs(_springFrom) > 0.05;
            }

            if (_springing)
            {
                _springElapsed += frame.Dt;
                var t = Math.Min(1.0, _springElapsed / 0.7);
                _angle = _springFrom * (1.0 - SkeumoSkia.ElasticOut(t));
                if (t >= 1.0)
                {
                    _angle = 0;
                    _springing = false;
                }
            }

            _lever.Rotation = _angle;
            return _springing;
        }

        private void DrawOverlay(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var deg = (float)_angle;
            if (Math.Abs(deg) >= 0.5f)
            {
                _arcPath.Reset();
                _arcPath.AddArc(new SKRect((float)(PivotX - ArcR), (float)(PivotY - ArcR), (float)(PivotX + ArcR), (float)(PivotY + ArcR)), -90f, deg);
                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeCap = SKStrokeCap.Round;
                _paint.StrokeWidth = 3f;
                _paint.Shader = null;
                _paint.Color = SkeumoSkia.ScaleAlpha(_state.Palette.Accent, opacity);
                canvas.DrawPath(_arcPath, _paint);
            }

            var abs = Math.Abs(_angle);
            var n = abs <= DeadDeg ? 0 : Math.Sign(_angle) * (abs - DeadDeg) / (MaxDegF - DeadDeg);
            if (_state.Engaged && n != 0)
            {
                var rad = _angle * Math.PI / 180.0;
                var gx = (float)(PivotX + KnobR * Math.Sin(rad));
                var gy = (float)(PivotY - KnobR * Math.Cos(rad));
                var glowAlpha = (0.25 + 0.6 * Math.Abs(n)) * opacity;
                using var shader = SKShader.CreateRadialGradient(
                    new SKPoint(gx, gy), 25f,
                    new[] { SkeumoSkia.ScaleAlpha(_state.Palette.Accent75, glowAlpha), new SKColor(0, 0, 0, 0) },
                    new[] { 0f, 1f }, SKShaderTileMode.Clamp);
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                _paint.Shader = shader;
                canvas.DrawCircle(gx, gy, 25f, _paint);
                _paint.Shader = null;
            }
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            var n = _state.Engaged ? _state.NormalizedValue : 0;
            var leftColor = SkeumoSkia.ScaleAlpha(n < 0 ? _state.Palette.Text : _state.Palette.LowText, opacity);
            var rightColor = SkeumoSkia.ScaleAlpha(n > 0 ? _state.Palette.Text : _state.Palette.LowText, opacity);
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, _state.LeftLabel, 0f, 4f, 41f, leftColor);
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, _state.RightLabel, 69f, 4f, 41f, rightColor);
        }
    }
}

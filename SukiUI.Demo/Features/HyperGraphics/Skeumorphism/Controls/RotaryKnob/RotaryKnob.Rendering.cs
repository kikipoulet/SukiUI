using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class RotaryKnob
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float CxF = 60f, CyF = 60f, ArcR = 47f;
        private const double SnapSec = 0.18;

        private HyperLayer _knob = null!;
        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _arcBlur = null!;
        private SKPath _trackPath = null!, _arcPath = null!;
        private double _angle = double.NaN;
        private double _snapFrom, _snapTo, _snapElapsed;
        private bool _snapping;
        private RotaryKnobState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _knob = ui.Layer("knob");
            _paint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round });
            _arcBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f));
            _glowPaint.MaskFilter = _arcBlur;

            _trackPath = ui.Own(new SKPath());
            _trackPath.AddArc(new SKRect(CxF - ArcR, CyF - ArcR, CxF + ArcR, CyF + ArcR), (float)(MinDeg - 90), (float)(MaxDeg - MinDeg));
            _arcPath = ui.Own(new SKPath());

            ui.Paint("arcOverlay", DrawArcOverlay);
        }

        protected override bool OnFrame(in HyperFrame frame, in RotaryKnobState state)
        {
            _state = state;
            if (double.IsNaN(_angle))
            {
                _angle = _snapFrom = _snapTo = state.TargetAngle;
                _snapping = false;
            }
            else if (Math.Abs(state.TargetAngle - _snapTo) > 0.01)
            {
                _snapFrom = _angle;
                _snapTo = state.TargetAngle;
                _snapElapsed = 0;
                _snapping = true;
            }

            if (_snapping)
            {
                _snapElapsed += frame.Dt;
                var t = Math.Min(1.0, _snapElapsed / SnapSec);
                _angle = _snapFrom + (_snapTo - _snapFrom) * SkeumoSkia.BackOut(t);
                if (t >= 1.0)
                {
                    _angle = _snapTo;
                    _snapping = false;
                }
            }

            _knob.Rotation = _angle;
            return _snapping;
        }

        private void DrawArcOverlay(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var pal = _state.Palette;
            var steps = _state.Steps;

            // Ticks
            _paint.Style = SKPaintStyle.Stroke;
            var tickColor = pal.IsDark ? new SKColor(0x87, 0x94, 0xA8) : new SKColor(0x7A, 0x86, 0x9C);
            for (var i = 0; i <= steps; i++)
            {
                var d = (MinDeg + (MaxDeg - MinDeg) * i / steps) * Math.PI / 180.0;
                var major = i == 0 || i == steps || (steps % 2 == 0 && i == steps / 2);
                var r0 = major ? 51.5 : 52.5;
                _paint.StrokeWidth = major ? 1.8f : 1.2f;
                _paint.Color = SkeumoSkia.ScaleAlpha(tickColor, (major ? 1.0 : 0.7) * opacity);
                canvas.DrawLine(
                    (float)(CxF + r0 * Math.Sin(d)), (float)(CyF - r0 * Math.Cos(d)),
                    (float)(CxF + 55.5 * Math.Sin(d)), (float)(CyF - 55.5 * Math.Cos(d)), _paint);
            }

            // Track
            _paint.StrokeWidth = 4f;
            _paint.Color = SkeumoSkia.ScaleAlpha(pal.Track, opacity);
            canvas.DrawPath(_trackPath, _paint);

            // Accent fill arc + glow
            var sweep = Math.Min(_angle, MaxDeg) - MinDeg;
            if (sweep >= 1.0)
            {
                _arcPath.Reset();
                _arcPath.AddArc(new SKRect(CxF - ArcR, CyF - ArcR, CxF + ArcR, CyF + ArcR), (float)(MinDeg - 90), (float)sweep);

                _glowPaint.StrokeWidth = 4f;
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.7 * opacity);
                canvas.DrawPath(_arcPath, _glowPaint);

                _paint.StrokeWidth = 3f;
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, opacity);
                canvas.DrawPath(_arcPath, _paint);
            }
        }
    }
}

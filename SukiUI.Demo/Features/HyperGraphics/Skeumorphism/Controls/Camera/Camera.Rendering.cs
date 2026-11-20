using System;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Camera
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float BeamX0 = 166f, BeamX1 = 92f, AxisY = 57f;
        private const double MaxPixel = 4.4;

        private HyperLayer _lensGlow = null!;
        private HyperParticleField _particles = null!;
        private SKPaint _paint = null!;
        private SKPaint _beamPaint = null!;
        private SKMaskFilter _beamBlur = null!;
        private SKPath _beamPath = null!;
        private SKFont _plateFont = null!, _subFont = null!, _engraveFont = null!;
        private CameraState _state;
        private double _lum;

        protected override void OnBind(IHyperBinder ui)
        {
            _lensGlow = ui.Layer("lensGlow");
            _particles = ui.Particles("chamber", count: 38, seed: 19, brightness: 0.7, pixelSize: 0.5);
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _beamPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _beamBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f));
            _beamPaint.MaskFilter = _beamBlur;
            _beamPath = ui.Own(new SKPath());

            _plateFont = ui.Own(new SKFont(SkeumoSkia.SemiBoldTypeface, 8.5f));
            _subFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 5.5f));
            _engraveFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 4.5f));

            ui.Paint("chamber", DrawChamberBg);
            ui.Paint("beamLayer", DrawBeam);
        }

        protected override bool OnFrame(in HyperFrame frame, in CameraState state)
        {
            _state = state;
            _lum = Math.Clamp(0.5 + Math.Log2(Math.Max(state.Exposure, 1) / 600) * 0.16, 0.05, 1);
            _lensGlow.Opacity = 0.2 + _lum * 0.8;

            var basis = 0.06 + 0.5 * _lum;
            var gainFactor = Math.Pow(Math.Max(state.Gain, 1), 0.6);
            var brightness = Math.Clamp(basis * gainFactor * 1.3, 0.05, 2.0);
            var pixelSize = 0.2 + (Math.Clamp(state.Gain, 1, 8) - 1) / 7.0 * (MaxPixel - 0.2);
            SetParticleParams(brightness, pixelSize);

            _particles.Update((float)frame.Seconds);
            return true;
        }

        private void DrawChamberBg(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var r = new SKRect((float)windowBounds.X, (float)windowBounds.Y, (float)windowBounds.Right, (float)windowBounds.Bottom);
            var liqAlpha = (0.35 + _lum * 0.65) * opacity;
            var inlAlpha = (0.1 + _lum * 0.9) * opacity;

            using (var liq = SKShader.CreateLinearGradient(
                new SKPoint(r.Right, 0), new SKPoint(r.Left, 0),
                new[] { SkeumoSkia.WithAlpha(new SKColor(0x3A, 0x5A, 0xA8), liqAlpha), SkeumoSkia.WithAlpha(new SKColor(0x27, 0x40, 0x7F), liqAlpha), SkeumoSkia.WithAlpha(new SKColor(0x1B, 0x2A, 0x55), liqAlpha) },
                new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Style = SKPaintStyle.Fill;
                _paint.Shader = liq;
                canvas.DrawRect(r, _paint);
            }

            using (var slide = SKShader.CreateLinearGradient(
                new SKPoint(r.Right, 0), new SKPoint(r.Left, 0),
                new[] { SkeumoSkia.WithAlpha(new SKColor(0xB9, 0xCD, 0xF5), inlAlpha), SkeumoSkia.WithAlpha(new SKColor(0x5F, 0x7C, 0xC0), 0.6 * inlAlpha), SkeumoSkia.WithAlpha(new SKColor(0x2A, 0x3A, 0x66), 0.25 * inlAlpha) },
                new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Shader = slide;
                canvas.DrawRect(r, _paint);
            }
            _paint.Shader = null;
        }

        private void DrawBeam(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var half = (float)(6 + _lum * 34);
            _beamPath.Reset();
            _beamPath.MoveTo(BeamX0, AxisY - 9f);
            _beamPath.LineTo(BeamX1, AxisY - half);
            _beamPath.LineTo(BeamX1, AxisY + half);
            _beamPath.LineTo(BeamX0, AxisY + 9f);
            _beamPath.Close();

            var alpha = (0.15 + _lum * 0.75) * opacity;
            var beamStart = _state.Palette.IsDark ? new SKColor(0xDB, 0xE8, 0xFF) : new SKColor(0x3D, 0x63, 0xC9);
            var beamEnd = _state.Palette.IsDark ? new SKColor(0x5B, 0x8C, 0xFF, 0) : new SKColor(0x3D, 0x63, 0xC9, 0);
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(BeamX0, AxisY), new SKPoint(BeamX1, AxisY),
                new[] { SkeumoSkia.ScaleAlpha(beamStart, alpha), beamEnd },
                new[] { 0f, 1f }, SKShaderTileMode.Clamp);
            _beamPaint.Shader = shader;
            canvas.DrawPath(_beamPath, _beamPaint);
            _beamPaint.Shader = null;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);

            // Nameplate & C-mount labels (in camera offset 156, -38)
            var plateText = _state.Palette.IsDark ? new SKColor(0xCF, 0xE0, 0xFF) : SKColors.White;
            SkeumoSkia.DrawLabel(canvas, _plateFont, _paint, "CAMERA", 156f + 88f, -38f + 86f, 52f, SkeumoSkia.ScaleAlpha(plateText, opacity));
            SkeumoSkia.DrawLabel(canvas, _subFont, _paint, "GigE Â· 5 MP", 156f + 88f, -38f + 98f, 52f, SkeumoSkia.ScaleAlpha(_state.Palette.Accent, opacity));
            SkeumoSkia.DrawLabel(canvas, _engraveFont, _paint, "C-MOUNT Â· f/1.8", 156f + 48f, -38f + 67f, 60f, SkeumoSkia.ScaleAlpha(_state.Palette.Engrave, opacity), SKTextAlign.Left);

            // GigE cable data pulse
            var periodSec = Math.Max(1.0 / Math.Max(_state.Fps, 0.1) * 20.0, 0.2);
            var p = (HyperGraphicsTimeSeconds() % periodSec) / periodSec;
            var pt = CablePoint(p);
            var pulseColor = _state.Palette.IsDark ? new SKColor(0x7F, 0xA3, 0xFF) : new SKColor(0x3D, 0x63, 0xC9);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            _paint.Color = SkeumoSkia.ScaleAlpha(pulseColor, Math.Sin(p * Math.PI) * opacity);
            canvas.DrawCircle((float)(156 + pt.X), (float)(-38 + pt.Y), 3.5f, _paint);
        }

        private static double HyperGraphicsTimeSeconds() =>
            System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency;

        private static Point CablePoint(double p) => p < 0.5
            ? Bezier(new Point(156, 112), new Point(170, 112), new Point(170, 138), new Point(142, 144), p * 2)
            : Bezier(new Point(142, 144), new Point(114, 150), new Point(100, 146), new Point(68, 146), (p - 0.5) * 2);

        private static Point Bezier(Point a, Point b, Point c, Point d, double t)
        {
            var u = 1 - t;
            return new Point(
                u * u * u * a.X + 3 * u * u * t * b.X + 3 * u * t * t * c.X + t * t * t * d.X,
                u * u * u * a.Y + 3 * u * u * t * b.Y + 3 * u * t * t * c.Y + t * t * t * d.Y);
        }
    }
}

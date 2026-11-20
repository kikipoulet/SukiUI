using System;
using System.Collections.Generic;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Stirrer
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float BeakerX = 44f, BeakerY = 28f, W = 92f, H = 106f, LiquidMaxH = 92f, BarLength = 30f, MaxRadPerFrame = 0.55f;
        private const int ParticleCount = 18;

        private readonly Random _rng = new(41);
        private readonly List<Swirl> _swirls = new(ParticleCount);

        private SKPaint _paint = null!, _glowPaint = null!;
        private SKMaskFilter _fieldBlur = null!;
        private SKPath _beakerClip = null!, _surfacePath = null!, _liquidPath = null!;
        private SKFont _rpmFont = null!, _unitFont = null!, _labelFont = null!;
        private double _shown, _barAngle, _seconds;
        private StirrerState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _fieldBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14f));
            _glowPaint.MaskFilter = _fieldBlur;

            _beakerClip = ui.Own(BuildBeakerClip());
            _surfacePath = ui.Own(new SKPath());
            _liquidPath = ui.Own(new SKPath());

            _rpmFont = ui.Own(new SKFont(SkeumoSkia.MonoTypeface, 13f));
            _unitFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 5f));
            _labelFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 5f));

            SeedParticles();
            ui.Paint("beaker", DrawBeaker);
        }

        private static SKPath BuildBeakerClip()
        {
            var path = new SKPath();
            var rr = new SKRoundRect();
            rr.SetRectRadii(new SKRect(BeakerX, BeakerY, BeakerX + W, BeakerY + H),
                new[] { new SKPoint(3, 3), new SKPoint(3, 3), new SKPoint(12, 12), new SKPoint(12, 12) });
            path.AddRoundRect(rr);
            return path;
        }

        private void SeedParticles()
        {
            for (var i = 0; i < ParticleCount; i++)
            {
                var dark = i % 5 == 0;
                _swirls.Add(new Swirl
                {
                    Dark = dark,
                    R = 6 + _rng.NextDouble() * 36,
                    Phi = _rng.NextDouble() * Math.PI * 2,
                    Y = H - 10 - _rng.NextDouble() * 50,
                    Size = dark ? 1.4 + _rng.NextDouble() : 1.6 + _rng.NextDouble() * 1.8,
                    Alpha = dark ? 0.7 : 0.4 + _rng.NextDouble() * 0.5,
                    Phase = _rng.NextDouble() * Math.PI * 2
                });
            }
        }

        protected override bool OnFrame(in HyperFrame frame, in StirrerState state)
        {
            _state = state;
            _seconds = frame.Seconds;
            _shown += (state.Speed - _shown) * (1.0 - Math.Exp(-frame.Dt * 1.8));
            if (Math.Abs(state.Speed - _shown) < 0.001) _shown = state.Speed;

            var dt60 = (float)(frame.Dt * 60.0);
            if (dt60 <= 0f) dt60 = 1f;

            var s = _shown;
            var omega = s * MaxRadPerFrame * dt60;
            _barAngle += omega;

            var y0 = H - state.Level * LiquidMaxH;
            var depth = s * Math.Min(34, state.Level * LiquidMaxH * 0.55);
            var rise = depth * 0.3;
            var top = y0 - rise;
            var bottom = y0 + depth;
            var tick = frame.Seconds * 60.0;

            for (var i = 0; i < _swirls.Count; i++)
            {
                var p = _swirls[i];
                p.Phi += omega * (0.6 + 0.8 * (1 - p.R / 42)) + 0.004 * dt60;
                p.Y += (Math.Sin(tick * 0.02 + p.Phase) * 0.08 + (_rng.NextDouble() - 0.5) * omega * 0.8) * dt60;
                var x = W * 0.5 + p.R * Math.Cos(p.Phi);
                var u = Math.Abs(x - W * 0.5) / (W * 0.5);
                var surfY = bottom + (top - bottom) * Math.Pow(u, 1.6) + 4;
                if (p.Y < surfY) p.Y = surfY;
                if (p.Y > H - 6) p.Y = H - 6 - _rng.NextDouble() * 4;
            }

            return s > 0.001 || state.Speed > 0.001;
        }

        private void DrawBeaker(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            canvas.Save();
            canvas.ClipPath(_beakerClip, SKClipOperation.Intersect, true);
            canvas.Translate(BeakerX, BeakerY);

            var pal = _state.Palette;
            var s = _shown;

            if (_state.Level > 0.01)
            {
                var y0 = (float)(H - _state.Level * LiquidMaxH);
                var depth = (float)(s * Math.Min(34, _state.Level * LiquidMaxH * 0.55));
                var rise = depth * 0.3f;
                var ripple = (float)(Math.Sin(_seconds * 60.0 * 0.09) * (0.6 + 1.2 * s));
                var top = y0 - rise;
                var bottom = y0 + depth;

                _surfacePath.Reset();
                _surfacePath.MoveTo(0f, top + ripple * 0.4f);
                _surfacePath.CubicTo(W * 0.3f, top + ripple, W * 0.38f, bottom, W * 0.5f, bottom);
                _surfacePath.CubicTo(W * 0.62f, bottom, W * 0.7f, top - ripple, W, top - ripple * 0.4f);

                _liquidPath.Reset();
                _liquidPath.AddPath(_surfacePath);
                _liquidPath.LineTo(W, H);
                _liquidPath.LineTo(0f, H);
                _liquidPath.Close();

                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                using (var bg = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0), new SKPoint(W, 0),
                    new[] { SkeumoSkia.ScaleAlpha(pal.Accent75, 0.95 * opacity), SkeumoSkia.ScaleAlpha(pal.Accent, 0.95 * opacity), SkeumoSkia.ScaleAlpha(pal.Accent75, 0.95 * opacity) },
                    new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
                {
                    _paint.Shader = bg;
                    canvas.DrawPath(_liquidPath, _paint);
                }

                var shadeBottom = pal.IsDark ? new SKColor(0, 0, 0, (byte)(0x47 * opacity)) : new SKColor(0x40, 0x50, 0x70, (byte)(0x26 * opacity));
                using (var shade = SKShader.CreateLinearGradient(
                    new SKPoint(0, top), new SKPoint(0, H),
                    new[] { SkeumoSkia.WithAlpha(SKColors.White, (pal.IsDark ? 0.22 : 0.30) * opacity), new SKColor(255, 255, 255, 0), new SKColor(0, 0, 0, 0), shadeBottom },
                    new[] { 0f, 0.12f, 0.8f, 1f }, SKShaderTileMode.Clamp))
                {
                    _paint.Shader = shade;
                    canvas.DrawPath(_liquidPath, _paint);
                }

                _paint.Shader = null;
                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeWidth = 1.5f;
                _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.5 * opacity);
                canvas.DrawPath(_surfacePath, _paint);
            }

            // Swirling particles
            if (_state.Level > 0.05)
            {
                _paint.Style = SKPaintStyle.Fill;
                _paint.Shader = null;
                for (var i = 0; i < _swirls.Count; i++)
                {
                    var p = _swirls[i];
                    var x = (float)(W * 0.5 + p.R * Math.Cos(p.Phi));
                    var z = Math.Sin(p.Phi);
                    var size = (float)(p.Size * (0.75 + 0.35 * z));
                    var a = p.Alpha * (0.45 + 0.55 * (z + 1) * 0.5) * opacity;
                    _paint.Color = p.Dark
                        ? new SKColor(5, 8, 13, (byte)(0x80 * a))
                        : new SKColor(255, 255, 255, (byte)(0xE6 * a));
                    canvas.DrawCircle(x, (float)p.Y, size * 0.5f, _paint);
                }
            }

            // Stir bar seen side-on
            var len = (float)Math.Max(6.0, BarLength * Math.Abs(Math.Cos(_barAngle)));
            var barLeft = W * 0.5f - len * 0.5f;
            var barRect = new SKRoundRect(new SKRect(barLeft, 96f, barLeft + len, 102f), 3f, 3f);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SKColors.White;
            using (var barGrad = SKShader.CreateLinearGradient(
                new SKPoint(0, 96), new SKPoint(0, 102),
                new[] { SkeumoSkia.WithAlpha(SKColors.White, opacity), SkeumoSkia.WithAlpha(new SKColor(0xE3, 0xE9, 0xF4), opacity), SkeumoSkia.WithAlpha(new SKColor(0x8D, 0x99, 0xB1), opacity) },
                new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Shader = barGrad;
                canvas.DrawRoundRect(barRect, _paint);
                _paint.Shader = null;
            }
            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeWidth = 0.5f;
            _paint.Color = SkeumoSkia.WithAlpha(pal.IsDark ? new SKColor(0x66, 0x72, 0x89) : new SKColor(0xA3, 0xAE, 0xC1), opacity);
            canvas.DrawRoundRect(barRect, _paint);

            canvas.Restore();
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            var pal = _state.Palette;
            var s = _shown;

            // Magnetic field glow behind beaker & top plate
            if (s > 0.005)
            {
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.7 * s * opacity);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(56f, 122f, 124f, 134f), 6f, 6f), _glowPaint);
            }

            base.OnDraw(canvas, opacity);

            // Display readout, "rpm", "MAGÂ·STIR", LED
            var rpm = Math.Round(s * _state.MaxRpm / 10.0) * 10.0;
            SkeumoSkia.DrawLabel(canvas, _rpmFont, _paint, $"{rpm:0}", 26f, 157f, 44f, SkeumoSkia.ScaleAlpha(pal.Accent, opacity), SKTextAlign.Right);
            SkeumoSkia.DrawLabel(canvas, _unitFont, _paint, "rpm", 72f, 162f, 16f, SkeumoSkia.ScaleAlpha(pal.Engrave, opacity), SKTextAlign.Left);
            SkeumoSkia.DrawLabel(canvas, _labelFont, _paint, "MAGÂ·STIR", 90f, 157f, 28f, SkeumoSkia.ScaleAlpha(pal.Engrave, opacity), SKTextAlign.Left);

            var running = s > 0.01;
            var ledAlpha = running ? (0.6 + 0.4 * Math.Sin(_seconds * 60.0 * 0.1)) * opacity : opacity;
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            _paint.Color = SkeumoSkia.ScaleAlpha(running ? pal.Accent : pal.LedOff, ledAlpha);
            canvas.DrawCircle(98.2f, 172.2f, 2.2f, _paint);
        }

        private sealed class Swirl
        {
            public bool Dark;
            public double R, Phi, Y, Size, Alpha, Phase;
        }
    }
}

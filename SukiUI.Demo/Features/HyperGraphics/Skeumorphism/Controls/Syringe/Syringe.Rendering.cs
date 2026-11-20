using System;
using System.Collections.Generic;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Syringe
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float BarrelLeft = 120f, BarrelTopF = 50f, BarrelHeightF = 200f, PlungerHeight = 12f, HandleTop = 262f;
        private const float JunctionX = 150f, ManifoldY = 22f, TubeEndInX = 34f, TubeEndOutX = 266f, TubeLength = 116f, NeckLength = 30f;

        private readonly List<StreamBubble> _streamBubbles = new(7);
        private readonly List<Particle> _particles = new(10);

        private HyperLayer _chanIn = null!, _chanOut = null!, _valveRotor = null!, _plunger = null!;
        private SKPaint _paint = null!, _auraPaint = null!;
        private SKMaskFilter _auraBlur = null!;
        private SKPath _barrelClip = null!;
        private SKFont _wayFont = null!, _scaleFont = null!;
        private HyperSmooth _rotorAngle;
        private double _flowFade, _glintPhase, _lastSpeed = 0.5, _lastSign = 1;
        private SyringeState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _chanIn = ui.Layer("chanIn");
            _chanOut = ui.Layer("chanOut");
            _valveRotor = ui.Layer("valveRotor");
            _plunger = ui.Layer("plunger");

            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _auraPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _auraBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14f));
            _auraPaint.MaskFilter = _auraBlur;

            _barrelClip = ui.Own(BuildBarrelClip());
            _wayFont = ui.Own(new SKFont(SkeumoSkia.SemiBoldTypeface, 11f));
            _scaleFont = ui.Own(new SKFont(SkeumoSkia.SemiBoldTypeface, 10f));

            InitStream();
            InitLiquidParticles();

            ui.Paint("streamLayer", DrawStream);
            ui.Paint("rodLayer", DrawRod);
            ui.Paint("barrel", DrawBarrel);
        }

        private static SKPath BuildBarrelClip()
        {
            var path = new SKPath();
            var rr = new SKRoundRect();
            rr.SetRectRadii(new SKRect(122, 50, 178, 250), new[] { new SKPoint(12, 12), new SKPoint(12, 12), new SKPoint(0, 0), new SKPoint(0, 0) });
            path.AddRoundRect(rr);
            return path;
        }

        private void InitStream()
        {
            var rng = new Random(11);
            for (var i = 0; i < 7; i++)
                _streamBubbles.Add(new StreamBubble { Size = 2.2 + rng.NextDouble(), S = i * 20 + rng.NextDouble() * 8 });
        }

        private void InitLiquidParticles()
        {
            var rng = new Random(7);
            for (var i = 0; i < 10; i++)
            {
                var size = 2 + rng.Next(2);
                _particles.Add(new Particle
                {
                    Size = size,
                    Dark = i % 3 == 2,
                    Alpha = 0.6 + rng.NextDouble() * 0.3,
                    X = 3 + rng.NextDouble() * (56 - 6 - size),
                    Y = 4 + rng.NextDouble() * 186,
                    Vx = (rng.NextDouble() - 0.5) * 0.24,
                    Vy = (rng.NextDouble() - 0.5) * 0.18,
                    Phase = rng.NextDouble() * 6.28
                });
            }
        }

        protected override bool OnFrame(in HyperFrame frame, in SyringeState state)
        {
            _state = state;
            _chanIn.Opacity = state.IsValveOut ? 0.1 : 0.7;
            _chanOut.Opacity = state.IsValveOut ? 0.7 : 0.1;

            _rotorAngle.Follow(state.IsValveOut ? -90.0 : 0.0, in frame, 0.08);
            _valveRotor.Rotation = _rotorAngle.Value;

            _plunger.Y = state.ShownPosition * BarrelHeightF;

            var dt60 = (float)(frame.Dt * 60.0);
            if (dt60 <= 0f) dt60 = 1f;

            _flowFade += ((state.Flow != 0 ? 1.0 : 0.0) - _flowFade) * (1.0 - Math.Exp(-frame.Dt * 8.0));
            if (_flowFade > 0.01)
            {
                if (state.Flow != 0)
                {
                    _lastSpeed = Math.Abs(state.Flow);
                    _lastSign = Math.Sign(state.Flow);
                }
                var step = Math.Max(0.4, _lastSpeed * 100 / 60.0) * _lastSign * dt60;
                _glintPhase += step;
                const double total = TubeLength + NeckLength;
                for (var i = 0; i < _streamBubbles.Count; i++)
                    _streamBubbles[i].S = ((_streamBubbles[i].S + step) % total + total) % total;
            }

            var t = frame.Seconds * 60.0;
            for (var i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.X += (p.Vx + 0.04 * Math.Sin(t * 0.02 + p.Phase)) * dt60;
                p.Y += p.Vy * dt60;
                if (p.X < 3) { p.X = 3; p.Vx = -p.Vx; }
                if (p.X > 56 - 3 - p.Size) { p.X = 56 - 3 - p.Size; p.Vx = -p.Vx; }
                if (p.Y < 4) { p.Y = 4; p.Vy = -p.Vy; }
                if (p.Y > 190) { p.Y = 190; p.Vy = -p.Vy; }
            }
            return true;
        }

        private void DrawStream(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            if (_flowFade < 0.01) return;
            var o = _flowFade * opacity;
            var startX = _state.IsValveOut ? TubeEndOutX : TubeEndInX;
            var pal = _state.Palette;

            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeCap = SKStrokeCap.Round;
            _paint.Shader = null;

            // Tube stream
            _paint.StrokeWidth = 7f;
            _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.9 * o);
            canvas.DrawLine(startX, ManifoldY, JunctionX, ManifoldY, _paint);

            _paint.StrokeWidth = 3.2f;
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.45 * o);
            canvas.DrawLine(startX, ManifoldY, JunctionX, ManifoldY, _paint);

            // Neck stream
            _paint.StrokeWidth = 9f;
            _paint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.9 * o);
            canvas.DrawLine(JunctionX, ManifoldY, JunctionX, 52f, _paint);

            _paint.StrokeWidth = 4f;
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.45 * o);
            canvas.DrawLine(JunctionX, ManifoldY, JunctionX, 52f, _paint);

            // Sliding glints
            using (var dashTube = SKPathEffect.CreateDash(new[] { 3f, 15f }, (float)(-_glintPhase % 18)))
            {
                _paint.PathEffect = dashTube;
                _paint.StrokeWidth = 1.4f;
                _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.8 * o);
                canvas.DrawLine(startX, ManifoldY - 1f, JunctionX, ManifoldY - 1f, _paint);
            }
            using (var dashNeck = SKPathEffect.CreateDash(new[] { 3f, 14f }, (float)(-_glintPhase % 17)))
            {
                _paint.PathEffect = dashNeck;
                _paint.StrokeWidth = 1.6f;
                canvas.DrawLine(JunctionX - 1f, ManifoldY, JunctionX - 1f, 52f, _paint);
            }
            _paint.PathEffect = null;

            // Stream bubbles
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.9 * o);
            var bubblePhase = _glintPhase * 0.05;
            for (var i = 0; i < _streamBubbles.Count; i++)
            {
                var b = _streamBubbles[i];
                var r = (float)(b.Size * 0.5);
                if (b.S < TubeLength)
                {
                    var x = (float)(_state.IsValveOut ? TubeEndOutX - b.S : TubeEndInX + b.S);
                    var y = (float)(ManifoldY + Math.Sin(b.S / 6 + bubblePhase) * 1.4);
                    canvas.DrawCircle(x, y, r, _paint);
                }
                else
                {
                    var x = (float)(JunctionX + Math.Sin(b.S / 4) * 1.6);
                    var y = (float)(ManifoldY + (b.S - TubeLength));
                    canvas.DrawCircle(x, y, r, _paint);
                }
            }
        }

        private void DrawRod(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var fill = (float)(_state.ShownPosition * BarrelHeightF);
            var rodTop = BarrelTopF + fill + PlungerHeight;
            var rodH = Math.Max(0f, HandleTop - rodTop);
            if (rodH <= 0.5f) return;

            var pal = _state.Palette;
            var c0 = pal.IsDark ? new SKColor(0x3B, 0x45, 0x58) : new SKColor(0x98, 0xA4, 0xB9);
            var c1 = pal.IsDark ? new SKColor(0xE3, 0xE9, 0xF4) : SKColors.White;
            var c2 = pal.IsDark ? new SKColor(0x85, 0x90, 0xA6) : new SKColor(0xCB, 0xD3, 0xE0);
            var c3 = pal.IsDark ? new SKColor(0x2A, 0x32, 0x42) : new SKColor(0x8D, 0x99, 0xB1);

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(143, 0), new SKPoint(157, 0),
                new[] { SkeumoSkia.ScaleAlpha(c0, opacity), SkeumoSkia.ScaleAlpha(c1, opacity), SkeumoSkia.ScaleAlpha(c2, opacity), SkeumoSkia.ScaleAlpha(c3, opacity) },
                new[] { 0f, 0.35f, 0.6f, 1f }, SKShaderTileMode.Clamp);
            _paint.Style = SKPaintStyle.Fill;
            _paint.Color = SKColors.White;
            _paint.Shader = shader;
            canvas.DrawRect(143f, rodTop, 14f, rodH, _paint);
            _paint.Shader = null;

            _paint.Style = SKPaintStyle.Stroke;
            _paint.StrokeWidth = 0.5f;
            _paint.Color = SkeumoSkia.WithAlpha(pal.IsDark ? new SKColor(0x66, 0x72, 0x89) : new SKColor(0xA3, 0xAE, 0xC1), opacity);
            canvas.DrawLine(143f, rodTop, 143f, rodTop + rodH, _paint);
            canvas.DrawLine(157f, rodTop, 157f, rodTop + rodH, _paint);
        }

        private void DrawBarrel(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var fill = (float)(_state.ShownPosition * BarrelHeightF);
            if (fill <= 0.5f) return;

            var pal = _state.Palette;
            canvas.Save();
            canvas.ClipPath(_barrelClip, SKClipOperation.Intersect, true);

            var liqRect = new SKRect(122f, BarrelTopF, 178f, BarrelTopF + fill);
            using (var bg = SKShader.CreateLinearGradient(
                new SKPoint(122, 0), new SKPoint(178, 0),
                new[] { SkeumoSkia.ScaleAlpha(pal.Accent75, 0.95 * opacity), SkeumoSkia.ScaleAlpha(pal.Accent, 0.95 * opacity), SkeumoSkia.ScaleAlpha(pal.Accent75, 0.95 * opacity) },
                new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                _paint.Shader = bg;
                canvas.DrawRect(liqRect, _paint);
            }

            // Luminous core
            using (var core = SKShader.CreateLinearGradient(
                new SKPoint(137, 0), new SKPoint(163, 0),
                new[] { new SKColor(255, 255, 255, 0), SkeumoSkia.WithAlpha(SKColors.White, 0.28 * opacity), new SKColor(255, 255, 255, 0) },
                new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Shader = core;
                canvas.DrawRect(137f, BarrelTopF, 26f, fill, _paint);
            }

            // Vertical shade
            var shadeBottom = pal.IsDark ? new SKColor(0, 0, 0, (byte)(0x47 * opacity)) : new SKColor(0x40, 0x50, 0x70, (byte)(0x26 * opacity));
            using (var shade = SKShader.CreateLinearGradient(
                new SKPoint(0, BarrelTopF), new SKPoint(0, BarrelTopF + fill),
                new[] { SkeumoSkia.WithAlpha(SKColors.White, (pal.IsDark ? 0.22 : 0.30) * opacity), new SKColor(255, 255, 255, 0), new SKColor(0, 0, 0, 0), shadeBottom },
                new[] { 0f, 0.12f, 0.8f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Shader = shade;
                canvas.DrawRect(liqRect, _paint);
            }

            // Caustic against plunger seal
            var causticH = Math.Min(10f, fill);
            using (var caustic = SKShader.CreateLinearGradient(
                new SKPoint(0, BarrelTopF + fill - causticH), new SKPoint(0, BarrelTopF + fill),
                new[] { new SKColor(255, 255, 255, 0), SkeumoSkia.WithAlpha(SKColors.White, 0.28 * opacity) },
                new[] { 0f, 1f }, SKShaderTileMode.Clamp))
            {
                _paint.Shader = caustic;
                canvas.DrawRect(122f, BarrelTopF + fill - causticH, 56f, causticH, _paint);
            }
            _paint.Shader = null;

            // Suspended particles
            if (fill > 12f)
            {
                for (var i = 0; i < _particles.Count; i++)
                {
                    var p = _particles[i];
                    if (p.Y >= fill - 4) continue;
                    _paint.Color = p.Dark
                        ? new SKColor(0, 0, 0, (byte)(0x33 * p.Alpha * opacity))
                        : new SKColor(255, 255, 255, (byte)(0x66 * p.Alpha * opacity));
                    canvas.DrawCircle((float)(122 + p.X + p.Size * 0.5), (float)(BarrelTopF + p.Y + p.Size * 0.5), (float)(p.Size * 0.5), _paint);
                }
            }

            canvas.Restore();
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            var fill = (float)(_state.ShownPosition * BarrelHeightF);
            var pal = _state.Palette;
            if (fill > 6f)
            {
                _auraPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.65 * opacity);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(122f, 54f, 178f, 54f + fill), 12f, 12f), _auraPaint);
            }

            base.OnDraw(canvas, opacity);

            // In / Out labels
            SkeumoSkia.DrawLabel(canvas, _wayFont, _paint, "In", 61f, 0f, 30f, SkeumoSkia.ScaleAlpha(!_state.IsValveOut ? pal.Text : pal.LowText, opacity));
            SkeumoSkia.DrawLabel(canvas, _wayFont, _paint, "Out", 209f, 0f, 30f, SkeumoSkia.ScaleAlpha(_state.IsValveOut ? pal.Text : pal.LowText, opacity));

            // Volume scale (50 ticks + major labels)
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            for (var k = 0; k <= 50; k++)
            {
                var y = (float)(BarrelTopF + BarrelHeightF * k / 50.0);
                var major = k % 10 == 0;
                var medium = k % 5 == 0;
                var length = major ? 12f : medium ? 8f : 4f;
                _paint.Color = SkeumoSkia.ScaleAlpha(pal.LowText, (major ? 1.0 : 0.55) * opacity);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(BarrelLeft - 27f - length, y, BarrelLeft - 27f, y + (major ? 1.5f : 1f)), 0.5f, 0.5f), _paint);

                if (major)
                {
                    var text = $"{_state.Volume * k / 50.0:0.#}";
                    SkeumoSkia.DrawLabel(canvas, _scaleFont, _paint, text, BarrelLeft - 79f, y - 6f, 36f, SkeumoSkia.ScaleAlpha(pal.LowText, opacity), SKTextAlign.Right);
                }
            }
        }

        private sealed class StreamBubble
        {
            public double Size, S;
        }

        private sealed class Particle
        {
            public double Size, X, Y, Vx, Vy, Phase, Alpha;
            public bool Dark;
        }
    }
}

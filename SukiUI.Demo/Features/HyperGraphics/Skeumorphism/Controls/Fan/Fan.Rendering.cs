using System;
using System.Collections.Generic;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Fan
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const int BladeCount = 5;
        private const int GhostCount = 6;
        private const float MaxDegPerFrame = 26f, CenterX = 80f, CenterY = 80f;
        private const string BladeData = "M 70,58 C 78,36 100,22 120,30 C 122,48 104,62 78,68 Z";

        private readonly Random _rng = new(31);
        private readonly List<AirSpeck> _air = new(32);

        private HyperLayer _rotor = null!, _hubRing = null!;
        private SKPaint _paint = null!, _ghostPaint = null!, _glowPaint = null!;
        private SKMaskFilter _glowBlur = null!;
        private SKPath _bladePath = null!;
        private SKFont _engraveFont = null!;
        private double _shown, _angle;
        private float _lastStep;
        private FanState _state;

        protected override void OnBind(IHyperBinder ui)
        {
            _rotor = ui.Layer("rotor");
            _hubRing = ui.Layer("hubRing");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _ghostPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _glowBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20f));
            _glowPaint.MaskFilter = _glowBlur;
            _bladePath = ui.Own(SKPath.ParseSvgPathData(BladeData)!);
            _engraveFont = ui.Own(new SKFont(SkeumoSkia.DefaultTypeface, 5.5f));

            ui.Paint("venturiFx", DrawVenturiFx);
            ui.Paint("air", DrawAir);
        }

        protected override bool OnFrame(in HyperFrame frame, in FanState state)
        {
            _state = state;
            var factor = 1.0 - Math.Exp(-frame.Dt * 2.2);
            _shown += (state.Speed - _shown) * factor;
            if (Math.Abs(state.Speed - _shown) < 0.001) _shown = state.Speed;

            var dt60 = (float)(frame.Dt * 60.0);
            if (dt60 <= 0f) dt60 = 1f;

            var s = _shown;
            _lastStep = (float)(s * MaxDegPerFrame * dt60);
            _angle = (_angle + _lastStep) % 360.0;

            _rotor.Rotation = _angle;
            _rotor.Opacity = 1.0 - 0.85 * Math.Pow(s, 1.4);
            _hubRing.Opacity = 0.35 + 0.65 * s;

            UpdateAir(s, dt60);
            return s > 0.001 || state.Speed > 0.001 || _air.Count > 0;
        }

        private void UpdateAir(double s, float dt60)
        {
            var expected = 0.8 * s * dt60;
            while (_rng.NextDouble() < expected)
            {
                _air.Add(new AirSpeck
                {
                    Size = 1.2 + _rng.NextDouble() * 1.6,
                    R = 22 + _rng.NextDouble() * 8,
                    Theta = _rng.NextDouble() * Math.PI * 2,
                    Speed = 0.6 + _rng.NextDouble() * 0.9,
                    Alpha = 0.35 + _rng.NextDouble() * 0.5
                });
                expected -= 1;
            }

            for (var i = _air.Count - 1; i >= 0; i--)
            {
                var p = _air[i];
                p.R += p.Speed * (0.4 + 2.2 * s) * dt60;
                p.Theta += 0.035 * s * dt60;
                if (p.R > 66) _air.RemoveAt(i);
            }
        }

        private void DrawVenturiFx(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            var s = _shown;
            var pal = _state.Palette;

            // Motor glow
            if (s > 0.005)
            {
                _glowPaint.Color = SkeumoSkia.ScaleAlpha(pal.Accent, 0.72 * s * opacity);
                canvas.DrawCircle(CenterX, CenterY, 26f, _glowPaint);
            }

            // Blur disc at high speed
            var discAlpha = 0.9 * Math.Pow(Math.Max(0, s - 0.35) / 0.65, 1.5) * opacity;
            if (discAlpha > 0.01)
            {
                var discBase = pal.IsDark ? new SKColor(0x58, 0x62, 0x78) : new SKColor(0xCB, 0xD3, 0xE0);
                var discMid = pal.IsDark ? new SKColor(0x6A, 0x76, 0x8E) : new SKColor(0xE3, 0xE9, 0xF4);
                using var radial = SKShader.CreateRadialGradient(
                    new SKPoint(CenterX, CenterY), 68f,
                    new[]
                    {
                        new SKColor(discBase.Red, discBase.Green, discBase.Blue, 0),
                        new SKColor(discBase.Red, discBase.Green, discBase.Blue, 0),
                        SkeumoSkia.WithAlpha(discMid, 0.58 * discAlpha),
                        SkeumoSkia.WithAlpha(discBase, 0.48 * discAlpha),
                        SkeumoSkia.WithAlpha(discBase, 0.16 * discAlpha),
                        new SKColor(discBase.Red, discBase.Green, discBase.Blue, 0)
                    },
                    new[] { 0f, 0.3f, 0.4f, 0.75f, 0.95f, 1f }, SKShaderTileMode.Clamp);
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                _paint.Shader = radial;
                canvas.DrawCircle(CenterX, CenterY, 68f, _paint);
                _paint.Shader = null;
            }

            // Motion-blur blade ghosts
            if (s > 0.01)
            {
                var sweep = Math.Max(_lastStep * 1.4, 1.0);
                var bladeColor = pal.IsDark ? new SKColor(0x66, 0x72, 0x89) : new SKColor(0xCB, 0xD3, 0xE0);
                var blurRadius = (float)(4.0 * s * s);
                using var blur = blurRadius > 0.2f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius) : null;
                _ghostPaint.MaskFilter = blur;

                for (var g = 0; g < GhostCount; g++)
                {
                    var ga = 0.25 * Math.Min(1.0, s * 1.6) * (1.0 - 0.5 * g / GhostCount) * opacity;
                    _ghostPaint.Color = SkeumoSkia.WithAlpha(bladeColor, ga);
                    var baseAngle = _angle - sweep * (g + 1) / GhostCount;
                    for (var b = 0; b < BladeCount; b++)
                    {
                        canvas.Save();
                        canvas.Translate(12f, 12f);
                        canvas.RotateDegrees((float)(baseAngle + b * 360.0 / BladeCount), 68f, 68f);
                        canvas.DrawPath(_bladePath, _ghostPaint);
                        canvas.Restore();
                    }
                }
                _ghostPaint.MaskFilter = null;
            }
        }

        private void DrawAir(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            if (_air.Count == 0) return;
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            var s = _shown;
            for (var i = 0; i < _air.Count; i++)
            {
                var p = _air[i];
                var fade = Math.Min(1.0, (p.R - 22.0) / 10.0) * Math.Min(1.0, (66.0 - p.R) / 14.0);
                var a = p.Alpha * fade * Math.Max(0.15, s) * 0.85 * opacity;
                if (a <= 0.01) continue;
                _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, a);
                canvas.DrawCircle((float)(CenterX + p.R * Math.Cos(p.Theta)), (float)(CenterY + p.R * Math.Sin(p.Theta)), (float)(p.Size * 0.5), _paint);
            }
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);
            SkeumoSkia.DrawLabel(canvas, _engraveFont, _paint, "12 V DC Â· 0.3 A", 104f, 149f, 52f, SkeumoSkia.ScaleAlpha(_state.Palette.Engrave, opacity), SKTextAlign.Left);
        }

        private sealed class AirSpeck
        {
            public double Size, R, Theta, Speed, Alpha;
        }
    }
}

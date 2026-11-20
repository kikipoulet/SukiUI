using System;
using System.Collections.Generic;
using Avalonia;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Tank
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float InteriorX = 36f, InteriorY = 36f, InteriorW = 96f, InteriorH = 126f, LiquidMaxH = 112f, FallX = 84f;
        private const int TankParticleCount = 28;

        private static readonly Route OutletRoute = new Route(new Point(96, 160))
            .LineTo(new Point(96, 180)).QuadTo(new Point(96, 192), new Point(108, 192)).LineTo(new Point(288, 192));

        private static readonly Route InletRoute = new Route(new Point(0, 16))
            .LineTo(new Point(72, 16)).QuadTo(new Point(84, 16), new Point(84, 28)).LineTo(new Point(84, 30));

        private readonly Random _rng = new(13);
        private readonly List<Speck> _tankSpecks = new(TankParticleCount);
        private readonly List<PipeSpeck> _outletSpecks = new(16);
        private readonly List<PipeSpeck> _inletSpecks = new(16);
        private readonly List<Speck> _fallSpecks = new(12);

        private HyperLayer _inletFlow = null!, _outletFlow = null!;
        private SKPaint _paint = null!, _auraPaint = null!;
        private SKPath _vesselClip = null!;
        private SKMaskFilter _auraBlur = null!;
        private double _shownIn, _shownOut, _level;
        private SkeumoPalette _palette;

        protected override void OnBind(IHyperBinder ui)
        {
            _inletFlow = ui.Layer("inletFlow");
            _outletFlow = ui.Layer("outletFlow");
            _paint = ui.Own(new SKPaint { IsAntialias = true });
            _auraPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _auraBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14f));
            _auraPaint.MaskFilter = _auraBlur;

            _vesselClip = ui.Own(BuildVesselClip());
            SeedTankParticles();

            ui.Paint("pipeLayer", DrawPipes);
            ui.Paint("interior", DrawInterior);
        }

        private static SKPath BuildVesselClip()
        {
            var path = new SKPath();
            var rr = new SKRoundRect();
            rr.SetRectRadii(
                new SKRect(InteriorX, InteriorY, InteriorX + InteriorW, InteriorY + InteriorH),
                new[] { new SKPoint(32, 32), new SKPoint(32, 32), new SKPoint(14, 14), new SKPoint(14, 14) });
            path.AddRoundRect(rr);
            return path;
        }

        protected override bool OnFrame(in HyperFrame frame, in TankState state)
        {
            _palette = state.Palette;
            _level = state.Level;
            var factor = 1.0 - Math.Exp(-frame.Dt * 8.0);
            _shownIn += (state.Inflow - _shownIn) * factor;
            _shownOut += (state.Outflow - _shownOut) * factor;
            if (Math.Abs(state.Inflow - _shownIn) < 0.002) _shownIn = state.Inflow;
            if (Math.Abs(state.Outflow - _shownOut) < 0.002) _shownOut = state.Outflow;

            _inletFlow.Opacity = 0.85 * _shownIn;
            _outletFlow.Opacity = 0.85 * _shownOut;

            var dt60 = (float)(frame.Dt * 60.0);
            if (dt60 <= 0f) dt60 = 1f;

            UpdatePipe(_outletSpecks, OutletRoute, _shownOut, dt60, onExit: null);
            UpdatePipe(_inletSpecks, InletRoute, _shownIn, dt60, onExit: SpawnFallSpeck);
            UpdateFall(dt60);
            UpdateTank((float)frame.Seconds, dt60);
            return true;
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            var h = (float)(_level * LiquidMaxH);
            if (h > 8f)
            {
                var auraH = Math.Max(0f, h - 4f);
                var top = InteriorY + InteriorH - auraH;
                _auraPaint.Color = SkeumoSkia.ScaleAlpha(_palette.Accent, 0.56 * opacity);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(38, top, 130, top + auraH), 14, 14), _auraPaint);
            }
            base.OnDraw(canvas, opacity);
        }

        private void DrawPipes(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            DrawPipeSpecks(canvas, _inletSpecks, opacity, isInlet: true);
            DrawPipeSpecks(canvas, _outletSpecks, opacity, isInlet: false);
        }

        private void DrawPipeSpecks(SKCanvas canvas, List<PipeSpeck> specks, double opacity, bool isInlet)
        {
            _paint.Style = SKPaintStyle.Fill;
            _paint.Shader = null;
            for (var i = 0; i < specks.Count; i++)
            {
                var p = specks[i];
                var (pos, normal) = (isInlet ? InletRoute : OutletRoute).At(p.S);
                var lateral = p.Offset + 0.9 * Math.Sin(p.S / p.WobbleLength + p.Phase);
                var x = (float)(pos.X + normal.X * lateral);
                var y = (float)(pos.Y + normal.Y * lateral);
                var maskAlpha = isInlet
                    ? Math.Clamp((x - 5.0) / 64.0, 0.0, 1.0)
                    : Math.Clamp((282.0 - x) / 124.0, 0.0, 1.0);
                var a = p.Alpha * Math.Min(1.0, p.S / 8.0) * p.Fade * maskAlpha * opacity;
                if (a <= 0.01) continue;
                _paint.Color = SpeckColor(p.Dark, a);
                canvas.DrawCircle(x, y, (float)(p.Size * 0.5), _paint);
            }
        }

        private void DrawInterior(SKCanvas canvas, Rect windowBounds, double opacity)
        {
            canvas.Save();
            canvas.ClipPath(_vesselClip, SKClipOperation.Intersect, true);

            var h = (float)(_level * LiquidMaxH);
            var surfaceY = InteriorY + InteriorH - h;

            if (_shownIn > 0.01)
            {
                _paint.Style = SKPaintStyle.Stroke;
                _paint.StrokeCap = SKStrokeCap.Round;
                _paint.StrokeWidth = (float)(1.5 + 2.5 * _shownIn);
                _paint.Shader = null;
                _paint.Color = SkeumoSkia.ScaleAlpha(_palette.Accent, 0.8 * _shownIn * opacity);
                canvas.DrawLine(FallX, InteriorY, FallX, surfaceY + 2f, _paint);
            }

            if (h > 0.5f)
            {
                var liqRect = new SKRect(InteriorX, surfaceY, InteriorX + InteriorW, InteriorY + InteriorH);
                _paint.Style = SKPaintStyle.Fill;
                _paint.Color = SKColors.White;
                using (var bg = SKShader.CreateLinearGradient(
                    new SKPoint(InteriorX, 0), new SKPoint(InteriorX + InteriorW, 0),
                    new[] { SkeumoSkia.ScaleAlpha(_palette.Accent75, 0.95 * opacity), SkeumoSkia.ScaleAlpha(_palette.Accent, 0.95 * opacity), SkeumoSkia.ScaleAlpha(_palette.Accent75, 0.95 * opacity) },
                    new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
                {
                    _paint.Shader = bg;
                    canvas.DrawRect(liqRect, _paint);
                }

                // Luminous vertical core
                using (var core = SKShader.CreateLinearGradient(
                    new SKPoint(InteriorX + 28, 0), new SKPoint(InteriorX + 68, 0),
                    new[] { new SKColor(255, 255, 255, 0), SkeumoSkia.WithAlpha(SKColors.White, 0.28 * opacity), new SKColor(255, 255, 255, 0) },
                    new[] { 0f, 0.5f, 1f }, SKShaderTileMode.Clamp))
                {
                    _paint.Shader = core;
                    canvas.DrawRect(new SKRect(InteriorX + 28, surfaceY, InteriorX + 68, InteriorY + InteriorH), _paint);
                }

                // Vertical lighting + bottom caustic
                var shadeBottom = _palette.IsDark ? new SKColor(0, 0, 0, (byte)(0x47 * opacity)) : new SKColor(0x40, 0x50, 0x70, (byte)(0x26 * opacity));
                using (var shade = SKShader.CreateLinearGradient(
                    new SKPoint(0, surfaceY), new SKPoint(0, InteriorY + InteriorH),
                    new[] { SkeumoSkia.WithAlpha(SKColors.White, (_palette.IsDark ? 0.22 : 0.30) * opacity), new SKColor(255, 255, 255, 0), new SKColor(0, 0, 0, 0), shadeBottom },
                    new[] { 0f, 0.12f, 0.8f, 1f }, SKShaderTileMode.Clamp))
                {
                    _paint.Shader = shade;
                    canvas.DrawRect(liqRect, _paint);
                }

                _paint.Shader = null;
                _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.5 * opacity);
                canvas.DrawRect(new SKRect(InteriorX, surfaceY, InteriorX + InteriorW, surfaceY + 1.5f), _paint);
            }

            _paint.Shader = null;
            _paint.Style = SKPaintStyle.Fill;
            for (var i = 0; i < _fallSpecks.Count; i++)
            {
                var p = _fallSpecks[i];
                _paint.Color = SpeckColor(p.Dark, p.Alpha * opacity);
                canvas.DrawCircle((float)(InteriorX + p.X), (float)(InteriorY + p.Y), (float)(p.Size * 0.5), _paint);
            }

            if (h > 8f)
            {
                var relSurf = InteriorH - h;
                for (var i = 0; i < _tankSpecks.Count; i++)
                {
                    var p = _tankSpecks[i];
                    var a = p.Alpha * Math.Clamp((p.Y - relSurf) / 8.0, 0, 1) * opacity;
                    if (a <= 0.01) continue;
                    _paint.Color = SpeckColor(p.Dark, a);
                    canvas.DrawCircle((float)(InteriorX + p.X), (float)(InteriorY + p.Y), (float)(p.Size * 0.5), _paint);
                }
            }

            canvas.Restore();
        }

        private SKColor SpeckColor(bool dark, double alpha)
        {
            var a = (byte)Math.Clamp(Math.Round(alpha * 255), 0, 255);
            if (!dark) return new SKColor(255, 255, 255, (byte)(a * 0.9));
            return _palette.IsDark ? new SKColor(5, 8, 13, (byte)(a * 0.6)) : new SKColor(40, 50, 80, (byte)(a * 0.5));
        }

        private void SeedTankParticles()
        {
            for (var i = 0; i < TankParticleCount; i++)
            {
                var dark = i % 5 == 0;
                _tankSpecks.Add(new Speck
                {
                    Dark = dark,
                    Size = dark ? 1.2 + _rng.NextDouble() * 1.2 : 1.4 + _rng.NextDouble() * 2.2,
                    X = 6 + _rng.NextDouble() * (InteriorW - 12),
                    Y = InteriorH - _rng.NextDouble() * LiquidMaxH,
                    Vx = (_rng.NextDouble() - 0.5) * 0.12,
                    Vy = (_rng.NextDouble() - 0.5) * 0.08,
                    Phase = _rng.NextDouble() * Math.PI * 2,
                    Alpha = dark ? 0.7 : 0.35 + _rng.NextDouble() * 0.5
                });
            }
        }

        private void UpdateTank(float seconds, float dt60)
        {
            var h = _level * LiquidMaxH;
            if (h <= 8) return;
            var surface = InteriorH - h;
            var stir = 1 + 3 * (_shownIn + _shownOut);
            var t = seconds * 60.0;

            for (var i = 0; i < _tankSpecks.Count; i++)
            {
                var p = _tankSpecks[i];
                p.X += ((p.Vx + 0.05 * Math.Sin(t * 0.02 + p.Phase)) * stir + (p.X < 60 ? 0.05 * _shownOut : 0)) * dt60;
                p.Y += (p.Vy + 0.04 * Math.Cos(t * 0.017 + p.Phase * 1.3)) * stir * dt60 + 0.03 * _shownOut * dt60;

                if (p.X < 4) { p.X = 4; p.Vx = Math.Abs(p.Vx); }
                else if (p.X > InteriorW - 4) { p.X = InteriorW - 4; p.Vx = -Math.Abs(p.Vx); }

                var top = surface + 3;
                if (p.Y < top) { p.Y = top; p.Vy = Math.Abs(p.Vy); }
                else if (p.Y > InteriorH - 4) { p.Y = InteriorH - 4; p.Vy = -Math.Abs(p.Vy); }
            }
        }

        private void UpdatePipe(List<PipeSpeck> specks, Route route, double flow, float dt60, Action? onExit)
        {
            if (flow > 0.02)
            {
                var expected = 0.9 * flow * dt60;
                while (_rng.NextDouble() < expected)
                {
                    var dark = _rng.NextDouble() < 0.2;
                    specks.Add(new PipeSpeck
                    {
                        Dark = dark,
                        Size = 1.2 + _rng.NextDouble() * 1.6,
                        S = _rng.NextDouble() * 4,
                        Speed = 0.55 + _rng.NextDouble() * 0.9,
                        Offset = (_rng.NextDouble() * 2 - 1) * 2.4,
                        Phase = _rng.NextDouble() * Math.PI * 2,
                        WobbleLength = 4 + _rng.NextDouble() * 8,
                        Alpha = dark ? 0.75 : 0.45 + _rng.NextDouble() * 0.5
                    });
                    expected -= 1;
                }
            }

            for (var i = specks.Count - 1; i >= 0; i--)
            {
                var p = specks[i];
                p.S += p.Speed * (0.25 + 3.2 * flow) * dt60;
                p.Offset = Math.Clamp(p.Offset + (_rng.NextDouble() - 0.5) * 0.35 * dt60, -2.6, 2.6);

                var exited = p.S >= route.Length;
                if (flow < 0.02) p.Fade -= 0.05 * dt60;
                if (exited || p.Fade <= 0)
                {
                    specks.RemoveAt(i);
                    if (exited) onExit?.Invoke();
                }
            }
        }

        private void SpawnFallSpeck()
        {
            var dark = _rng.NextDouble() < 0.2;
            _fallSpecks.Add(new Speck
            {
                Dark = dark,
                Size = 1.2 + _rng.NextDouble() * 1.4,
                X = (FallX - InteriorX) + (_rng.NextDouble() - 0.5) * 2.4,
                Y = 0,
                Vx = (_rng.NextDouble() - 0.5) * 0.15,
                Vy = 0.8 + _rng.NextDouble() * 0.8,
                Alpha = dark ? 0.75 : 0.5 + _rng.NextDouble() * 0.4
            });
        }

        private void UpdateFall(float dt60)
        {
            var surface = InteriorH - _level * LiquidMaxH;
            for (var i = _fallSpecks.Count - 1; i >= 0; i--)
            {
                var p = _fallSpecks[i];
                p.Vy += 0.12 * dt60;
                p.Y += p.Vy * dt60;
                p.X += p.Vx * dt60;
                if (p.Y >= surface)
                    _fallSpecks.RemoveAt(i);
            }
        }

        private sealed class Speck
        {
            public bool Dark;
            public double Size, X, Y, Vx, Vy, Phase, Alpha;
        }

        private sealed class PipeSpeck
        {
            public bool Dark;
            public double Size, S, Speed, Offset, Phase, WobbleLength, Alpha, Fade = 1;
        }

        private sealed class Route
        {
            private readonly List<Point> _pts = new();
            private readonly List<double> _cum = new() { 0 };

            public Route(Point start) => _pts.Add(start);
            public double Length => _cum[^1];
            public Route LineTo(Point p) => Add(p);

            public Route QuadTo(Point control, Point end)
            {
                var s = _pts[^1];
                for (var i = 1; i <= 12; i++)
                {
                    var t = i / 12.0;
                    var u = 1 - t;
                    Add(new Point(u * u * s.X + 2 * u * t * control.X + t * t * end.X,
                                  u * u * s.Y + 2 * u * t * control.Y + t * t * end.Y));
                }
                return this;
            }

            private Route Add(Point p)
            {
                var last = _pts[^1];
                _cum.Add(_cum[^1] + Math.Sqrt((p.X - last.X) * (p.X - last.X) + (p.Y - last.Y) * (p.Y - last.Y)));
                _pts.Add(p);
                return this;
            }

            public (Point Pos, Vector Normal) At(double s)
            {
                var i = 1;
                while (i < _cum.Count - 1 && _cum[i] < s) i++;
                var a = _pts[i - 1];
                var b = _pts[i];
                var len = _cum[i] - _cum[i - 1];
                var t = len > 0 ? Math.Clamp((s - _cum[i - 1]) / len, 0, 1) : 0;
                var dir = len > 0 ? new Vector((b.X - a.X) / len, (b.Y - a.Y) / len) : new Vector(1, 0);
                return (new Point(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t), new Vector(-dir.Y, dir.X));
            }
        }
    }
}

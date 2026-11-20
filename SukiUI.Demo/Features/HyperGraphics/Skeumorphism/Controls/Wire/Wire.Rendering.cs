using System;
using System.Collections.Generic;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class Wire
{
    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private const float ViewW = 320f, ViewH = 240f, FadeLength = 46f, SparkLength = 13f;
        private static readonly SKColor Hot = new(0xFF, 0xE8, 0x7A);

        private readonly Random _rng = new(53);
        private readonly List<Pulse> _pulses = new(4);
        private readonly List<Glint> _glints = new(16);
        private readonly List<Crackle> _crackles = new(8);

        private SKPaint _paint = null!, _glowPaint = null!, _fadeLayerPaint = null!;
        private SKMaskFilter _pulseBlur = null!;
        private SKPath _wirePath = null!, _fxPath = null!;
        private WireState _state;
        private int _builtGeomVersion = -1;
        private int _lastTriggerSeq;
        private double _shown, _phase;

        protected override void OnBind(IHyperBinder ui)
        {
            _paint = ui.Own(new SKPaint { IsAntialias = true, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round });
            _glowPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round });
            _fadeLayerPaint = ui.Own(new SKPaint());
            _pulseBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2.5f));
            _glowPaint.MaskFilter = _pulseBlur;
            _wirePath = ui.Own(new SKPath());
            _fxPath = ui.Own(new SKPath());
        }

        protected override bool OnFrame(in HyperFrame frame, in WireState state)
        {
            _state = state;
            if (state.GeometryVersion != _builtGeomVersion)
            {
                _builtGeomVersion = state.GeometryVersion;
                RebuildPath(state.Geometry);
            }

            if (state.TriggerSeq != _lastTriggerSeq)
            {
                _lastTriggerSeq = state.TriggerSeq;
                if (state.Geometry != null)
                    _pulses.Add(new Pulse { Speed = 2.4 });
            }

            var dt60 = (float)(frame.Dt * 60.0);
            if (dt60 <= 0f) dt60 = 1f;

            _shown += (state.Current - _shown) * (1.0 - Math.Exp(-frame.Dt * 6.0));
            if (Math.Abs(state.Current - _shown) < 0.002) _shown = state.Current;
            _phase += (0.03 + 0.05 * _shown) * dt60;

            var geom = state.Geometry;
            if (geom != null)
            {
                if (_shown > 0.02 && _rng.NextDouble() < (0.06 + 0.12 * _shown) * dt60)
                    _crackles.Add(new Crackle { S = _rng.NextDouble() * geom.TotalLength, Life = 4 + _rng.Next(4) });

                for (var i = _crackles.Count - 1; i >= 0; i--)
                {
                    _crackles[i].Life -= dt60;
                    if (_crackles[i].Life < 0) _crackles.RemoveAt(i);
                }

                for (var i = _pulses.Count - 1; i >= 0; i--)
                {
                    var p = _pulses[i];
                    p.S += p.Speed * dt60;
                    p.Frame++;
                    if (p.S - SparkLength >= geom.TotalLength)
                    {
                        _pulses.RemoveAt(i);
                        continue;
                    }
                    if (p.Frame % 3 == 1)
                    {
                        for (var k = 0; k < p.Jitter.Length; k++)
                            p.Jitter[k] = (k % 2 == 0 ? 1 : -1) * (0.4 + _rng.NextDouble() * 0.6) * (k == 0 || k == p.Jitter.Length - 1 ? 0 : 1.1);
                    }
                    if (p.Frame % 4 == 0 && p.S < geom.TotalLength)
                    {
                        geom.SampleAt((float)p.S, out var head, out _);
                        _glints.Add(new Glint
                        {
                            X = head.X,
                            Y = head.Y,
                            Vx = (_rng.NextDouble() - 0.5) * 0.9,
                            Vy = (_rng.NextDouble() - 0.5) * 0.9 - 0.15
                        });
                    }
                }

                for (var i = _glints.Count - 1; i >= 0; i--)
                {
                    var g = _glints[i];
                    g.Life -= 0.04 * dt60;
                    if (g.Life <= 0) { _glints.RemoveAt(i); continue; }
                    g.X += g.Vx * dt60;
                    g.Y += g.Vy * dt60;
                }
            }

            return _shown > 0.002 || Math.Abs(state.Current - _shown) > 0.002 || _pulses.Count > 0 || _glints.Count > 0 || _crackles.Count > 0;
        }

        private void RebuildPath(WireGeometrySamples? geom)
        {
            _wirePath.Reset();
            if (geom == null || geom.Points.Length < 2) return;
            _wirePath.MoveTo(geom.Points[0]);
            for (var i = 1; i < geom.Points.Length; i++)
                _wirePath.LineTo(geom.Points[i]);
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            var geom = _state.Geometry;
            if (geom == null || _state.BoundsWidth <= 1 || _state.BoundsHeight <= 1) return;

            canvas.Save();
            canvas.Scale(ViewW / (float)_state.BoundsWidth, ViewH / (float)_state.BoundsHeight);

            if (_state.FadeStart)
            {
                var p0 = geom.Points[0];
                var d = geom.FadeDir;
                using var fadeShader = SKShader.CreateLinearGradient(
                    p0, new SKPoint(p0.X + d.X * FadeLength, p0.Y + d.Y * FadeLength),
                    new[] { new SKColor(255, 255, 255, 0), new SKColor(255, 255, 255, 0x59), SKColors.White },
                    new[] { 0f, 0.4f, 1f }, SKShaderTileMode.Clamp);
                canvas.SaveLayer(_fadeLayerPaint);
                DrawWireCore(canvas, geom, opacity);
                _paint.Style = SKPaintStyle.Fill;
                _paint.BlendMode = SKBlendMode.DstIn;
                _paint.Color = SKColors.White;
                _paint.Shader = fadeShader;
                canvas.DrawRect(-32f, -32f, (float)_state.BoundsWidth + 64f, (float)_state.BoundsHeight + 64f, _paint);
                _paint.Shader = null;
                _paint.BlendMode = SKBlendMode.SrcOver;
                canvas.Restore();
            }
            else
            {
                DrawWireCore(canvas, geom, opacity);
            }

            canvas.Restore();
        }

        private void DrawWireCore(SKCanvas canvas, WireGeometrySamples geom, double opacity)
        {
            var pal = _state.Palette;
            _paint.Style = SKPaintStyle.Stroke;
            _paint.Shader = null;

            // Shadow
            canvas.Save();
            canvas.Translate(0f, 2f);
            _paint.StrokeWidth = 6f;
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.Black, 0.25 * opacity);
            canvas.DrawPath(_wirePath, _paint);
            canvas.Restore();

            // Jacket
            var jacket = _state.Insulation switch
            {
                WireInsulation.Red => pal.IsDark ? new SKColor(0xB8, 0x35, 0x2E) : new SKColor(0xC9, 0x43, 0x3A),
                WireInsulation.Black => pal.IsDark ? new SKColor(0x14, 0x17, 0x1D) : new SKColor(0x3A, 0x3F, 0x4A),
                _ => pal.IsDark ? new SKColor(0x26, 0x2D, 0x3A) : new SKColor(0x8D, 0x99, 0xB1)
            };
            _paint.StrokeWidth = 5f;
            _paint.Color = SkeumoSkia.ScaleAlpha(jacket, opacity);
            canvas.DrawPath(_wirePath, _paint);

            // Gloss
            canvas.Save();
            canvas.Translate(0f, -1.1f);
            _paint.StrokeWidth = 1.4f;
            _paint.Color = SkeumoSkia.WithAlpha(SKColors.White, 0.3 * opacity);
            canvas.DrawPath(_wirePath, _paint);
            canvas.Restore();

            // Continuous faint flow
            if (_shown > 0.005)
            {
                var flowAlpha = _shown * (0.26 + 0.08 * Math.Sin(_phase * 0.5)) * opacity;
                using var dash = SKPathEffect.CreateDash(new[] { 5.5f, 13.2f }, (float)(-_phase * 2.2));
                _paint.PathEffect = dash;
                _paint.StrokeWidth = 2.2f;
                _paint.Color = SkeumoSkia.WithAlpha(new SKColor(0xFF, 0xF0, 0xC0), flowAlpha);
                canvas.DrawPath(_wirePath, _paint);
                _paint.PathEffect = null;
            }

            // Crackles
            for (var i = 0; i < _crackles.Count; i++)
            {
                var c = _crackles[i];
                _fxPath.Reset();
                for (var k = 0; k < 5; k++)
                {
                    geom.SampleAt(Math.Min(geom.TotalLength, (float)(c.S + k * 2.2)), out var pt, out var n);
                    var j = k is 0 or 4 ? 0f : (float)((_rng.NextDouble() - 0.5) * 3.4);
                    var p = new SKPoint(pt.X + n.X * j, pt.Y + n.Y * j);
                    if (k == 0) _fxPath.MoveTo(p);
                    else _fxPath.LineTo(p);
                }
                _paint.StrokeWidth = 1f;
                _paint.Color = SkeumoSkia.WithAlpha(Hot, Math.Min(1.0, c.Life / 3.0) * (0.5 + 0.5 * _shown) * 0.69 * opacity);
                canvas.DrawPath(_fxPath, _paint);
            }

            // Pulses
            for (var i = 0; i < _pulses.Count; i++)
            {
                var p = _pulses[i];
                _fxPath.Reset();
                for (var k = 0; k < p.Jitter.Length; k++)
                {
                    var d = Math.Clamp((float)(p.S - SparkLength * (1 - k / (double)(p.Jitter.Length - 1))), 0f, geom.TotalLength);
                    geom.SampleAt(d, out var pt, out var n);
                    var ptJ = new SKPoint(pt.X + n.X * (float)p.Jitter[k], pt.Y + n.Y * (float)p.Jitter[k]);
                    if (k == 0) _fxPath.MoveTo(ptJ);
                    else _fxPath.LineTo(ptJ);
                }

                var fade = Math.Clamp(Math.Min(1.0, p.S / 30.0) * Math.Min(1.0, (geom.TotalLength - p.S + SparkLength) / 40.0), 0, 1);
                _glowPaint.StrokeWidth = 4f;
                _glowPaint.Color = SkeumoSkia.WithAlpha(new SKColor(0x6E, 0xB8, 0xFF), fade * (0.7 + 0.3 * _rng.NextDouble()) * 0.22 * opacity);
                canvas.DrawPath(_fxPath, _glowPaint);

                _paint.StrokeWidth = 1.2f;
                _paint.Color = SkeumoSkia.WithAlpha(Hot, fade * 0.65 * opacity);
                canvas.DrawPath(_fxPath, _paint);
            }

            // Glints
            _paint.Style = SKPaintStyle.Fill;
            for (var i = 0; i < _glints.Count; i++)
            {
                var g = _glints[i];
                _paint.Color = SkeumoSkia.WithAlpha(Hot, g.Life * 0.48 * opacity);
                canvas.DrawCircle((float)g.X, (float)g.Y, 0.9f, _paint);
            }
        }

        private sealed class Pulse
        {
            public double S, Speed;
            public int Frame;
            public readonly double[] Jitter = new double[6];
        }

        private sealed class Glint
        {
            public double X, Y, Vx, Vy, Life = 1;
        }

        private sealed class Crackle
        {
            public double S, Life;
        }
    }
}

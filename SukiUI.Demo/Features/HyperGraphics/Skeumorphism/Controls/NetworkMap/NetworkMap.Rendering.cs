using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using SkiaSharp;
using SukiUI.HyperGraphics;

namespace SukiUI.Demo.Features.HyperGraphics.Skeumorphism.Controls;

public partial class NetworkMap
{
    private const double PacketSpeed = 150;
    private const double SwitchDwell = 0.16;
    private const double StepPause = 0.6;
    private const double PulseSeconds = 0.9;
    private const double LedSeconds = 0.45;

    private static readonly SKRect RouterBody = SKRect.Create(111, 26, 66, 30);
    private static readonly SKRect SwitchBody = SKRect.Create(100, 99, 88, 26);
    private static readonly SKRect PcABody = SKRect.Create(12, 66, 56, 46);
    private static readonly SKRect PcBBody = SKRect.Create(38, 156, 56, 46);
    private static readonly SKRect PcCBody = SKRect.Create(194, 156, 56, 46);

    protected override HyperStage CreateStage() => new Stage();

    private sealed class Stage : HyperStage
    {
        private HyperLayer _routerPower = null!;
        private HyperLayer _routerLed = null!;
        private HyperLayer _ledA = null!;
        private HyperLayer _ledB = null!;
        private HyperLayer _ledR = null!;
        private HyperLayer _ledC = null!;
        private HyperLayer _screenA = null!;
        private HyperLayer _screenB = null!;
        private HyperLayer _screenC = null!;

        private readonly Dictionary<NetNode, SKPath> _cables = new();
        private readonly Dictionary<NetNode, SKPathMeasure> _cableMeasures = new();
        private SKPath _uplinkPath = null!;

        private readonly List<ActivePacket> _packets = new();
        private readonly List<ActiveRipple> _ripples = new();
        private readonly Dictionary<NetNode, double> _pulses = new();
        private readonly Dictionary<NetNode, double> _leds = new();
        private readonly Dictionary<NetNode, double> _screens = new()
        {
            [NetNode.ComputerA] = 1,
            [NetNode.ComputerB] = 1,
            [NetNode.ComputerC] = 1,
        };

        private SkeumoPalette _palette;

        private SKTypeface _uiTypeface = null!;
        private SKTypeface _monoTypeface = null!;
        private SKFont _deviceFont = null!;
        private SKFont _pcFont = null!;
        private SKFont _packetFont = null!;

        private SKPaint _fillPaint = null!;
        private SKPaint _strokePaint = null!;
        private SKPaint _textPaint = null!;
        private SKMaskFilter _haloBlur = null!;

        protected override void OnBind(IHyperBinder ui)
        {
            _routerPower = ui.Layer("routerPower");
            _routerLed = ui.Layer("routerLed");
            _ledA = ui.Layer("ledA");
            _ledB = ui.Layer("ledB");
            _ledR = ui.Layer("ledR");
            _ledC = ui.Layer("ledC");
            _screenA = ui.Layer("screenA");
            _screenB = ui.Layer("screenB");
            _screenC = ui.Layer("screenC");

            BuildGeometry(ui);

            _uiTypeface = ui.Own(SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default);
            _monoTypeface = ui.Own(SKTypeface.FromFamilyName("Consolas", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default);
            _deviceFont = ui.Own(new SKFont(_uiTypeface, 6f) { Subpixel = true });
            _pcFont = ui.Own(new SKFont(_uiTypeface, 8f) { Subpixel = true });
            _packetFont = ui.Own(new SKFont(_monoTypeface, 8.5f) { Subpixel = true });

            _fillPaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill });
            _strokePaint = ui.Own(new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round });
            _textPaint = ui.Own(new SKPaint { IsAntialias = true });
            _haloBlur = ui.Own(SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8f));

            ui.Paint("networkFx", DrawNetworkFx);
        }

        private void BuildGeometry(IHyperBinder ui)
        {
            var router = new SKPath();
            router.MoveTo(144, 47);
            router.LineTo(144, 112);

            var compA = new SKPath();
            compA.MoveTo(40, 84);
            compA.CubicTo(80, 84, 100, 112, 144, 112);

            var compB = new SKPath();
            compB.MoveTo(66, 174);
            compB.CubicTo(100, 174, 120, 140, 144, 112);

            var compC = new SKPath();
            compC.MoveTo(222, 174);
            compC.CubicTo(188, 174, 168, 140, 144, 112);

            _cables[NetNode.Router] = ui.Own(router);
            _cables[NetNode.ComputerA] = ui.Own(compA);
            _cables[NetNode.ComputerB] = ui.Own(compB);
            _cables[NetNode.ComputerC] = ui.Own(compC);

            foreach (var (node, path) in _cables)
                _cableMeasures[node] = ui.Own(new SKPathMeasure(path, false));

            var uplink = new SKPath();
            uplink.MoveTo(150, 47);
            uplink.CubicTo(200, 47, 220, 34, 304, 34);
            _uplinkPath = ui.Own(uplink);
        }

        protected override bool OnFrame(in HyperFrame frame, in NetworkMapState state)
        {
            _palette = state.Palette;
            bool dirty = false;

            while (state.Channel.TryDequeue(out var cmd))
            {
                dirty = true;
                ApplyCommand(cmd!);
            }

            double dt = frame.Dt;

            if (StepPackets(dt)) dirty = true;
            if (StepRipples(dt)) dirty = true;
            if (DecayTimers(_pulses, PulseSeconds, dt)) dirty = true;
            if (DecayTimers(_leds, LedSeconds, dt)) dirty = true;

            UpdateLayers();

            return dirty || _packets.Count > 0 || _ripples.Count > 0 || _pulses.Count > 0 || _leds.Count > 0;
        }

        private void ApplyCommand(NetCmd cmd)
        {
            switch (cmd)
            {
                case NetCmdReset r:
                    ClearVisuals();
                    foreach (var node in r.Asleep)
                    {
                        if (_screens.ContainsKey(node))
                            _screens[node] = 0;
                    }
                    break;

                case NetCmdClear:
                    ClearVisuals();
                    break;

                case NetCmdPacket p:
                    double inLen = p.From == NetNode.Switch ? 0 : _cableMeasures[p.From].Length;
                    double outLen = p.To == NetNode.Switch ? 0 : _cableMeasures[p.To].Length;
                    double duration = (inLen + outLen) / PacketSpeed + (p.From != NetNode.Switch && p.To != NetNode.Switch ? SwitchDwell : 0);
                    _packets.Add(new ActivePacket(p.From, p.To, p.Label, p.Kind, -p.Delay, Math.Max(0.05, duration), inLen, outLen, p.Completion));
                    break;

                case NetCmdRipple rp:
                    _ripples.Add(new ActiveRipple(NodeCenter(rp.Center), 0, 0.75));
                    break;

                case NetCmdPulse pl:
                    _pulses[pl.Target] = 1;
                    break;

                case NetCmdPower pw:
                    if (_screens.ContainsKey(pw.Target))
                        _screens[pw.Target] = pw.Awake ? 1 : 0;
                    if (pw.Awake)
                        _pulses[pw.Target] = 1;
                    break;
            }
        }

        private void ClearVisuals()
        {
            foreach (var p in _packets)
            {
                var tcs = p.Completion;
                Dispatcher.UIThread.Post(() => tcs.TrySetCanceled());
            }
            _packets.Clear();
            _ripples.Clear();
            _pulses.Clear();
            _leds.Clear();
            _screens[NetNode.ComputerA] = 1;
            _screens[NetNode.ComputerB] = 1;
            _screens[NetNode.ComputerC] = 1;
        }

        private bool StepPackets(double dt)
        {
            if (_packets.Count == 0) return false;

            for (int i = _packets.Count - 1; i >= 0; i--)
            {
                var pkt = _packets[i];
                double prev = pkt.Elapsed;
                pkt.Elapsed += dt;

                if (prev <= 0 && pkt.Elapsed > 0)
                    BlinkLed(pkt.From);

                double t = pkt.Elapsed / pkt.Duration;
                if (t >= 1)
                {
                    BlinkLed(pkt.To);
                    var tcs = pkt.Completion;
                    Dispatcher.UIThread.Post(() => tcs.TrySetResult());
                    _packets.RemoveAt(i);
                }
            }
            return true;
        }

        private bool StepRipples(double dt)
        {
            if (_ripples.Count == 0) return false;
            for (int i = _ripples.Count - 1; i >= 0; i--)
            {
                _ripples[i].Elapsed += dt;
                if (_ripples[i].Elapsed >= _ripples[i].Duration)
                    _ripples.RemoveAt(i);
            }
            return true;
        }

        private static bool DecayTimers(Dictionary<NetNode, double> map, double seconds, double dt)
        {
            if (map.Count == 0) return false;
            Span<NetNode> keys = stackalloc NetNode[map.Count];
            int count = 0;
            foreach (var k in map.Keys)
                keys[count++] = k;

            for (int i = 0; i < count; i++)
            {
                var k = keys[i];
                double next = map[k] - dt / seconds;
                if (next <= 0) map.Remove(k);
                else map[k] = next;
            }
            return true;
        }

        private void BlinkLed(NetNode node)
        {
            if (node == NetNode.Switch) return;
            _leds[node] = 1;
        }

        private void UpdateLayers()
        {
            _routerPower.Opacity = 0.55;
            _routerLed.Opacity = _leds.TryGetValue(NetNode.Router, out var lr) ? lr : 0;
            _ledR.Opacity = _leds.TryGetValue(NetNode.Router, out var lrs) ? lrs : 0;
            _ledA.Opacity = _leds.TryGetValue(NetNode.ComputerA, out var la) ? la : 0;
            _ledB.Opacity = _leds.TryGetValue(NetNode.ComputerB, out var lb) ? lb : 0;
            _ledC.Opacity = _leds.TryGetValue(NetNode.ComputerC, out var lc) ? lc : 0;

            _screenA.Opacity = _screens[NetNode.ComputerA];
            _screenB.Opacity = _screens[NetNode.ComputerB];
            _screenC.Opacity = _screens[NetNode.ComputerC];
        }

        private void DrawNetworkFx(SKCanvas canvas, Rect window, double opacity)
        {
            float op = (float)opacity;
            var cableColor = _palette.IsDark ? new SKColor(0x56, 0x63, 0x7C) : new SKColor(0x8D, 0x99, 0xB1);
            var shadowColor = new SKColor(0x00, 0x00, 0x00, (byte)(0x33 * op));

            DrawUplink(canvas, cableColor, shadowColor, op);
            DrawCables(canvas, cableColor, shadowColor, op);
            DrawRipples(canvas, op);
            DrawHalos(canvas, op);
            DrawPackets(canvas, op);
        }

        private void DrawUplink(SKCanvas canvas, SKColor cableColor, SKColor shadowColor, float op)
        {
            var fadeColors = new[]
            {
                cableColor.WithAlpha((byte)(cableColor.Alpha * op)),
                cableColor.WithAlpha((byte)(0x59 * op)),
                cableColor.WithAlpha(0)
            };
            var shadowFadeColors = new[]
            {
                shadowColor,
                shadowColor.WithAlpha((byte)(shadowColor.Alpha * 0.35f)),
                SKColors.Transparent
            };
            var glossColor = new SKColor(0xFF, 0xFF, 0xFF, (byte)(0x35 * op));
            var glossFade = new[] { glossColor, glossColor.WithAlpha((byte)(0x12 * op)), SKColors.Transparent };
            float[] stops = [0f, 0.4f, 1f];

            _strokePaint.Color = SKColors.White;

            canvas.Save();
            canvas.Translate(0, 2);
            using (var shader = SKShader.CreateLinearGradient(new SKPoint(196, 0), new SKPoint(298, 0), shadowFadeColors, stops, SKShaderTileMode.Clamp))
            {
                _strokePaint.Shader = shader;
                _strokePaint.StrokeWidth = 5f;
                canvas.DrawPath(_uplinkPath, _strokePaint);
            }
            canvas.Restore();

            using (var shader = SKShader.CreateLinearGradient(new SKPoint(196, 0), new SKPoint(298, 0), fadeColors, stops, SKShaderTileMode.Clamp))
            {
                _strokePaint.Shader = shader;
                _strokePaint.StrokeWidth = 4.2f;
                canvas.DrawPath(_uplinkPath, _strokePaint);
            }

            canvas.Save();
            canvas.Translate(0, -1);
            using (var shader = SKShader.CreateLinearGradient(new SKPoint(196, 0), new SKPoint(298, 0), glossFade, stops, SKShaderTileMode.Clamp))
            {
                _strokePaint.Shader = shader;
                _strokePaint.StrokeWidth = 1.4f;
                canvas.DrawPath(_uplinkPath, _strokePaint);
            }
            canvas.Restore();
            _strokePaint.Shader = null;
        }

        private void DrawCables(SKCanvas canvas, SKColor cableColor, SKColor shadowColor, float op)
        {
            canvas.Save();
            canvas.Translate(0, 2);
            _strokePaint.Color = shadowColor;
            _strokePaint.StrokeWidth = 5f;
            foreach (var path in _cables.Values)
                canvas.DrawPath(path, _strokePaint);
            canvas.Restore();

            _strokePaint.Color = SkeumoSkia.WithAlpha(cableColor, op);
            _strokePaint.StrokeWidth = 4.2f;
            foreach (var path in _cables.Values)
                canvas.DrawPath(path, _strokePaint);

            canvas.Save();
            canvas.Translate(0, -1);
            _strokePaint.Color = new SKColor(0xFF, 0xFF, 0xFF, (byte)(0x35 * op));
            _strokePaint.StrokeWidth = 1.4f;
            foreach (var path in _cables.Values)
                canvas.DrawPath(path, _strokePaint);
            canvas.Restore();
        }

        private void DrawRipples(SKCanvas canvas, float op)
        {
            foreach (var ripple in _ripples)
            {
                double t = Math.Clamp(ripple.Elapsed / ripple.Duration, 0, 1);
                double k = SkeumoSkia.EaseOutCubic(t);
                float r = (float)(12 + k * 76);
                double alpha = (1 - t) * 0.7 * op;
                _strokePaint.Color = SkeumoSkia.WithAlpha(_palette.Accent, alpha);
                _strokePaint.StrokeWidth = (float)(2.4 * (1 - t * 0.6));
                canvas.DrawCircle(ripple.Center, r, _strokePaint);
            }
        }

        private void DrawHalos(SKCanvas canvas, float op)
        {
            if (_pulses.Count == 0) return;

            _fillPaint.MaskFilter = _haloBlur;
            foreach (var (node, strength) in _pulses)
            {
                double bell = Math.Sin(strength * Math.PI);
                if (bell <= 0.01) continue;

                var rect = NodeBodyRect(node);
                rect.Inflate(5, 5);
                _fillPaint.Color = SkeumoSkia.WithAlpha(_palette.Accent, 0.63 * bell * op);
                canvas.DrawRoundRect(rect, 13, 13, _fillPaint);
            }
            _fillPaint.MaskFilter = null;
        }

        private void DrawPackets(SKCanvas canvas, float op)
        {
            foreach (var pkt in _packets)
            {
                if (pkt.Elapsed < 0) continue;
                var pos = SamplePacket(pkt);
                DrawPacketBadge(canvas, pos, pkt.Label, pkt.Kind, op);
            }
        }

        private SKPoint SamplePacket(ActivePacket pkt)
        {
            double inTime = pkt.InLen / PacketSpeed;
            double outTime = pkt.OutLen / PacketSpeed;
            double totalTravel = inTime + outTime;
            double dwell = Math.Max(0, pkt.Duration - totalTravel);
            double e = pkt.Elapsed;

            if (e <= inTime && pkt.InLen > 0)
            {
                double f = SkeumoSkia.Smooth(e / inTime);
                _cableMeasures[pkt.From].GetPositionAndTangent((float)(f * pkt.InLen), out var pos, out _);
                return pos;
            }
            if (e <= inTime + dwell || pkt.OutLen <= 0)
            {
                return new SKPoint(144, 112);
            }
            double g = SkeumoSkia.Smooth((e - inTime - dwell) / outTime);
            _cableMeasures[pkt.To].GetPositionAndTangent((float)((1 - g) * pkt.OutLen), out var outPos, out _);
            return outPos;
        }

        private void DrawPacketBadge(SKCanvas canvas, SKPoint pos, string? label, PacketKind kind, float op)
        {
            bool hasLabel = !string.IsNullOrEmpty(label);
            float w = hasLabel ? Math.Max(34f, _packetFont.MeasureText(label) + 12f) : 14f;
            float h = hasLabel ? 16f : 14f;
            var rect = SKRect.Create(pos.X - w / 2f, pos.Y - h / 2f, w, h);
            float r = hasLabel ? 8f : 7f;

            // Subtle drop shadow
            var shadowRect = rect;
            shadowRect.Offset(0, 2);
            _fillPaint.Color = new SKColor(0x00, 0x00, 0x00, (byte)(0x4D * op));
            canvas.DrawRoundRect(shadowRect, r, r, _fillPaint);

            if (kind == PacketKind.Request)
            {
                _fillPaint.Color = SkeumoSkia.WithAlpha(_palette.Accent, op);
                canvas.DrawRoundRect(rect, r, r, _fillPaint);

                _strokePaint.Color = new SKColor(0xFF, 0xFF, 0xFF, (byte)(0x55 * op));
                _strokePaint.StrokeWidth = 1f;
                canvas.DrawRoundRect(rect, r, r, _strokePaint);

                if (hasLabel)
                {
                    _textPaint.Color = new SKColor(0xFF, 0xFF, 0xFF, (byte)(255 * op));
                    DrawCenteredPacketText(canvas, label!, rect);
                }
            }
            else
            {
                _fillPaint.Color = _palette.IsDark
                    ? new SKColor(0x16, 0x1A, 0x24, (byte)(0xEB * op))
                    : new SKColor(0xFF, 0xFF, 0xFF, (byte)(0xF0 * op));
                canvas.DrawRoundRect(rect, r, r, _fillPaint);

                _strokePaint.Color = SkeumoSkia.WithAlpha(_palette.Accent, op);
                _strokePaint.StrokeWidth = 1.6f;
                canvas.DrawRoundRect(rect, r, r, _strokePaint);

                if (hasLabel)
                {
                    _textPaint.Color = SkeumoSkia.WithAlpha(_palette.Accent, op);
                    DrawCenteredPacketText(canvas, label!, rect);
                }
            }
        }

        private void DrawCenteredPacketText(SKCanvas canvas, string text, SKRect rect)
        {
            var metrics = _packetFont.Metrics;
            float baseline = rect.MidY - (metrics.Ascent + metrics.Descent) * 0.5f;
            canvas.DrawText(text, rect.MidX, baseline, SKTextAlign.Center, _packetFont, _textPaint);
        }

        protected override void OnDraw(SKCanvas canvas, double opacity)
        {
            base.OnDraw(canvas, opacity);

            var low = SkeumoSkia.WithAlpha(_palette.LowText, opacity);
            var normal = SkeumoSkia.WithAlpha(_palette.Text, opacity);

            SkeumoSkia.DrawLabel(canvas, _deviceFont, _textPaint, "ROUTER", 120f, 43.4f, 40f, low, SKTextAlign.Left);
            SkeumoSkia.DrawLabel(canvas, _deviceFont, _textPaint, "SWITCH", 107f, 101.6f, 40f, low, SKTextAlign.Left);

            SkeumoSkia.DrawLabel(canvas, _pcFont, _textPaint, "PC A Â· .10", 12f, 114f, 56f, LabelColor(NetNode.ComputerA, low, normal));
            SkeumoSkia.DrawLabel(canvas, _pcFont, _textPaint, "PC B Â· .12", 38f, 204f, 56f, LabelColor(NetNode.ComputerB, low, normal));
            SkeumoSkia.DrawLabel(canvas, _pcFont, _textPaint, "PC C Â· .14", 194f, 204f, 56f, LabelColor(NetNode.ComputerC, low, normal));
        }

        private SKColor LabelColor(NetNode node, SKColor low, SKColor active) =>
            _pulses.TryGetValue(node, out var v) && v > 0.12 ? active : low;

        private static SKPoint NodeCenter(NetNode node) => node switch
        {
            NetNode.Router => new(144, 47),
            NetNode.Switch => new(144, 112),
            NetNode.ComputerA => new(40, 84),
            NetNode.ComputerB => new(66, 174),
            NetNode.ComputerC => new(222, 174),
            _ => new(144, 112),
        };

        private static SKRect NodeBodyRect(NetNode node) => node switch
        {
            NetNode.Router => RouterBody,
            NetNode.Switch => SwitchBody,
            NetNode.ComputerA => PcABody,
            NetNode.ComputerB => PcBBody,
            NetNode.ComputerC => PcCBody,
            _ => SwitchBody,
        };

        private sealed class ActivePacket(
            NetNode from, NetNode to, string? label, PacketKind kind,
            double elapsed, double duration, double inLen, double outLen, TaskCompletionSource completion)
        {
            public NetNode From { get; } = from;
            public NetNode To { get; } = to;
            public string? Label { get; } = label;
            public PacketKind Kind { get; } = kind;
            public double Elapsed { get; set; } = elapsed;
            public double Duration { get; } = duration;
            public double InLen { get; } = inLen;
            public double OutLen { get; } = outLen;
            public TaskCompletionSource Completion { get; } = completion;
        }

        private sealed class ActiveRipple(SKPoint center, double elapsed, double duration)
        {
            public SKPoint Center { get; } = center;
            public double Elapsed { get; set; } = elapsed;
            public double Duration { get; } = duration;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>Délégué de dessin personnalisé Skia dans une fenêtre <c>data-window</c>.</summary>
public delegate void HyperPainter(SKCanvas canvas, Rect windowBounds, double opacity);

internal readonly record struct HyperInput(bool Hover, bool Pressed);

/// <summary>Instantané temporel et d'interaction passé à chaque image dans <see cref="HyperControl{TState}.HyperStage.OnFrame"/>.</summary>
public readonly struct HyperFrame
{
    internal HyperFrame(long now, double dt, HyperInput input)
    {
        Now = now;
        Dt = dt;
        Hover = input.Hover;
        Pressed = input.Pressed;
    }

    /// <summary>Horodatage haute résolution (<see cref="Stopwatch.GetTimestamp"/>).</summary>
    public long Now { get; }

    /// <summary>Temps écoulé depuis l'image précédente (en secondes, borné).</summary>
    public double Dt { get; }

    /// <summary>Indique si le pointeur survole le contrôle.</summary>
    public bool Hover { get; }

    /// <summary>Indique si le pointeur est pressé sur le contrôle.</summary>
    public bool Pressed { get; }

    /// <summary>Temps monotone en secondes.</summary>
    public double Seconds => Now / (double)Stopwatch.Frequency;

    /// <summary>Phase normalisée dans <c>[0, 1)</c> pour une période donnée en millisecondes.</summary>
    public double Phase(double periodMs) => Now * 1000.0 / Stopwatch.Frequency % periodMs / periodMs;
}

/// <summary>Lisseur exponentiel indépendant de la fréquence d'images.</summary>
public struct HyperSmooth
{
    private bool _started;

    /// <summary>Valeur courante lissée.</summary>
    public double Value { get; private set; }

    /// <summary>Fait converger <see cref="Value"/> vers <paramref name="target"/> et retourne <c>true</c> tant que le mouvement continue.</summary>
    public bool Follow(double target, in HyperFrame frame, double seconds = 0.2)
    {
        if (!_started)
        {
            _started = true;
            Value = target;
            return false;
        }
        var gap = target - Value;
        if (Math.Abs(gap) <= 2e-3 * Math.Max(1, Math.Abs(target)))
        {
            Value = target;
            return false;
        }
        Value += gap * (1 - Math.Exp(-frame.Dt / seconds));
        return true;
    }
}

/// <summary>Palette de couleurs Skia courantes prêtes pour la teinte (<see cref="HyperLayer.Tint"/>).</summary>
public static class HyperColors
{
    /// <summary>Blanc opaque.</summary>
    public static readonly SKColor White = SKColors.White;

    /// <summary>Vert émeraude.</summary>
    public static readonly SKColor Emerald = new(52, 211, 153);

    /// <summary>Ambre.</summary>
    public static readonly SKColor Amber = new(251, 191, 36);

    /// <summary>Rouge carmin.</summary>
    public static readonly SKColor Crimson = new(244, 63, 94);

    /// <summary>Cyan.</summary>
    public static readonly SKColor Cyan = new(34, 211, 238);
}

/// <summary>Calque nommé (<c>data-part</c>) manipulable à chaque image sur le thread de rendu.</summary>
public sealed class HyperLayer
{
    internal HyperAtlasSlot Slot;
    internal SKRoundRect? WindowClip;
    internal SKRect WindowClipRect;

    /// <summary>Décalage horizontal en coordonnées SVG.</summary>
    public double X { get; set; }

    /// <summary>Décalage vertical en coordonnées SVG.</summary>
    public double Y { get; set; }

    /// <summary>Rotation en degrés autour du pivot du calque.</summary>
    public double Rotation { get; set; }

    /// <summary>Facteur d'échelle horizontal autour du pivot.</summary>
    public double ScaleX { get; set; } = 1.0;

    /// <summary>Facteur d'échelle vertical autour du pivot.</summary>
    public double ScaleY { get; set; } = 1.0;

    /// <summary>Facteur d'échelle uniforme (lecture de <see cref="ScaleX"/>, écriture sur les deux axes).</summary>
    public double Scale
    {
        get => ScaleX;
        set { ScaleX = value; ScaleY = value; }
    }

    /// <summary>Opacité du calque dans <c>[0, 1]</c>.</summary>
    public double Opacity { get; set; } = 1.0;

    /// <summary>Couleur de modulation (par défaut <see cref="SKColors.White"/>).</summary>
    public SKColor Tint { get; set; } = SKColors.White;

    /// <summary>Visibilité du calque.</summary>
    public bool Visible { get; set; } = true;

    internal HyperParticleField? AttachedParticles;
    internal HyperPainter? AttachedPainter;

    internal HyperLayer(HyperAtlasSlot slot)
    {
        Slot = slot;
        if (slot.Kind == HyperSlotKind.Window)
        {
            WindowClipRect = slot.WindowRect.ToSKRect();
            if (slot.WindowRadius > 0.5)
                WindowClip = new SKRoundRect(WindowClipRect, (float)slot.WindowRadius, (float)slot.WindowRadius);
        }
    }

    internal void SwapSlot(HyperAtlasSlot slot)
    {
        Slot = slot;
        if (slot.Kind == HyperSlotKind.Window)
        {
            WindowClip?.Dispose();
            WindowClip = null;
            WindowClipRect = slot.WindowRect.ToSKRect();
            if (slot.WindowRadius > 0.5)
                WindowClip = new SKRoundRect(WindowClipRect, (float)slot.WindowRadius, (float)slot.WindowRadius);
        }
    }

    /// <summary>Limites logiques du calque dans le repère SVG.</summary>
    public Rect Bounds => Slot.LogicalBounds;

    /// <summary>Point pivot du calque dans le repère SVG.</summary>
    public Point Pivot => Slot.Pivot;

    /// <summary>Positionne le pivot du calque aux coordonnées SVG <paramref name="x"/>, <paramref name="y"/>.</summary>
    public void MoveTo(double x, double y)
    {
        X = x - Pivot.X;
        Y = y - Pivot.Y;
    }

    internal void Dispose() => WindowClip?.Dispose();
}

internal sealed class HyperRig : IDisposable
{
    private const float DegToRad = MathF.PI / 180f;

    private sealed class BatchSection
    {
        public readonly bool IsWindow;
        public readonly int Start;
        public readonly int Count;
        public readonly SKRect[] Src;
        public readonly SKRotationScaleMatrix[] Xf;
        public readonly SKColor[] Col;

        public BatchSection(bool isWindow, int start, int count)
        {
            IsWindow = isWindow;
            Start = start;
            Count = count;
            if (isWindow)
            {
                Src = Array.Empty<SKRect>();
                Xf = Array.Empty<SKRotationScaleMatrix>();
                Col = Array.Empty<SKColor>();
            }
            else
            {
                Src = new SKRect[count];
                Xf = new SKRotationScaleMatrix[count];
                Col = new SKColor[count];
            }
        }

        public void SyncSrc(HyperLayer[] parts)
        {
            if (IsWindow) return;
            for (var i = 0; i < Count; i++)
                Src[i] = parts[Start + i].Slot.SrcRect;
        }
    }

    private HyperAtlas _atlas;
    private readonly HyperLayer[] _parts;
    private readonly Dictionary<string, HyperLayer> _named = new(StringComparer.Ordinal);
    private readonly BatchSection[] _sections;
    private readonly SKPaint _atlasPaint = new();
    private readonly SKSamplingOptions _linearSampling = new(SKFilterMode.Linear);
    private readonly SKSamplingOptions _nearestSampling = new(SKFilterMode.Nearest);

    // Scratch buffers for partial visibility or mixed anisotropic sections (allocated once per rig, zero per-frame alloc)
    private readonly SKRect[] _src;
    private readonly SKRotationScaleMatrix[] _xf;
    private readonly SKColor[] _col;
    private readonly (SKRect[] Src, SKRotationScaleMatrix[] Xf, SKColor[] Col)?[] _exactBatches;
    private readonly SKRect[] _src1 = new SKRect[1];
    private readonly SKRotationScaleMatrix[] _xf1 = new SKRotationScaleMatrix[1];
    private readonly SKColor[] _col1 = new SKColor[1];
    private Dictionary<string, HyperPath>? _paths;
    private bool _disposed;

    public HyperRig(HyperSvgTemplate template, HyperAtlas atlas)
    {
        Template = template;
        _atlas = atlas;

        var slots = atlas.Slots;
        _parts = new HyperLayer[slots.Length];
        for (var i = 0; i < slots.Length; i++)
        {
            var part = new HyperLayer(slots[i]);
            _parts[i] = part;
            if (slots[i].Name != null) _named[slots[i].Name!] = part;
        }

        var sections = new List<BatchSection>();
        var idx = 0;
        while (idx < slots.Length)
        {
            if (slots[idx].Kind == HyperSlotKind.Window)
            {
                sections.Add(new BatchSection(isWindow: true, idx, 1));
                idx++;
                continue;
            }
            var start = idx;
            while (idx < slots.Length && slots[idx].Kind != HyperSlotKind.Window)
                idx++;
            var sec = new BatchSection(isWindow: false, start, idx - start);
            sec.SyncSrc(_parts);
            sections.Add(sec);
        }
        _sections = sections.ToArray();

        var cap = Math.Max(8, slots.Length);
        _src = new SKRect[cap];
        _xf = new SKRotationScaleMatrix[cap];
        _col = new SKColor[cap];
        _exactBatches = new (SKRect[], SKRotationScaleMatrix[], SKColor[])?[cap + 1];
    }

    public void SwapAtlas(HyperAtlas newAtlas)
    {
        if (_disposed)
        {
            newAtlas.Release();
            return;
        }

        var old = _atlas;
        _atlas = newAtlas;
        var slots = newAtlas.Slots;
        var len = Math.Min(_parts.Length, slots.Length);
        for (var i = 0; i < len; i++)
            _parts[i].SwapSlot(slots[i]);

        for (var i = 0; i < _sections.Length; i++)
            _sections[i].SyncSrc(_parts);

        old.Release();
    }

    private void SyncSectionSources()
    {
        for (var i = 0; i < _sections.Length; i++)
            _sections[i].SyncSrc(_parts);
    }

    public HyperSvgTemplate Template { get; }

    public HyperLayer this[string name] => _named.TryGetValue(name, out var layer)
        ? layer
        : throw new KeyNotFoundException($"data-part=\"{name}\" introuvable dans le SVG. Disponibles: [{string.Join(", ", _named.Keys)}]");

    public Rect Zone(string name) => Template.Zone(name);

    public HyperPath Path(string name)
    {
        if (_paths == null)
        {
            var src = Template.Paths;
            _paths = new Dictionary<string, HyperPath>(src.Count, StringComparer.Ordinal);
            foreach (var kv in src)
                _paths[kv.Key] = new HyperPath(kv.Key, kv.Value);
        }
        return _paths.TryGetValue(name, out var path)
            ? path
            : throw new KeyNotFoundException($"data-path=\"{name}\" introuvable dans le SVG. Disponibles: [{string.Join(", ", _paths.Keys)}]");
    }

    public void AttachParticles(string windowName, HyperParticleField field, double brightness = 1.0, double pixelSize = 0.0)
    {
        var part = this[windowName];
        if (part.Slot.Kind != HyperSlotKind.Window)
            throw new InvalidOperationException($"\"{windowName}\" doit porter data-window=\"true\" pour attacher des particules.");
        part.AttachedParticles = field;
        _particleBrightness = (float)brightness;
        _particlePixelSize = pixelSize;
    }

    public void Paint(string windowName, HyperPainter painter)
    {
        var part = this[windowName];
        if (part.Slot.Kind != HyperSlotKind.Window)
            throw new InvalidOperationException($"\"{windowName}\" doit porter data-window=\"true\" pour appeler Paint().");
        part.AttachedPainter = painter;
    }

    public void SetParticleParams(double brightness, double pixelSize)
    {
        _particleBrightness = (float)brightness;
        _particlePixelSize = pixelSize;
    }

    internal GRContext? CurrentGrContext;
    internal bool LastDrawPackPending;
    private float _particleBrightness = 1f;
    private double _particlePixelSize;

    private static SKColor ModulateColor(double opacity, SKColor tint)
    {
        // SKCanvas.DrawAtlas premultiplies each SKColor before applying SKBlendMode.Modulate,
        // so returning premultiplied RGB would square the opacity; always pass straight RGB + modulated alpha.
        var a = (byte)Math.Clamp(Math.Round(Math.Clamp(opacity, 0, 1) * tint.Alpha), 0, 255);
        return new SKColor(tint.Red, tint.Green, tint.Blue, a);
    }

    public void Draw(SKCanvas canvas, double opacity)
    {
        var rasterScale = _atlas.RasterScale;
        var invRasterScale = 1f / rasterScale;
        var atlasImg = _atlas.GetDrawImage(CurrentGrContext, out var srcUpdated, out var packPending);
        LastDrawPackPending = packPending;
        if (srcUpdated) SyncSectionSources();

        for (var sIdx = 0; sIdx < _sections.Length; sIdx++)
        {
            var sec = _sections[sIdx];
            if (sec.IsWindow)
            {
                var p = _parts[sec.Start];
                if (!p.Visible || Math.Abs(p.ScaleX) < 1e-4 || Math.Abs(p.ScaleY) < 1e-4) continue;
                var o = opacity * p.Opacity;
                if (o <= 0.001) continue;

                var pf = p.AttachedParticles;
                var painter = p.AttachedPainter;
                if (pf == null && painter == null) continue;

                canvas.Save();
                if (Math.Abs(p.X) >= 1e-5 || Math.Abs(p.Y) >= 1e-5 || Math.Abs(p.Rotation) >= 1e-5 ||
                    Math.Abs(p.ScaleX - 1.0) >= 1e-5 || Math.Abs(p.ScaleY - 1.0) >= 1e-5)
                {
                    canvas.Translate((float)(p.Slot.Pivot.X + p.X), (float)(p.Slot.Pivot.Y + p.Y));
                    if (Math.Abs(p.Rotation) >= 1e-5)
                        canvas.RotateDegrees((float)p.Rotation);
                    if (Math.Abs(p.ScaleX - 1.0) >= 1e-5 || Math.Abs(p.ScaleY - 1.0) >= 1e-5)
                        canvas.Scale((float)p.ScaleX, (float)p.ScaleY);
                    canvas.Translate((float)(-p.Slot.Pivot.X), (float)(-p.Slot.Pivot.Y));
                }

                if (p.WindowClip != null)
                    canvas.ClipRoundRect(p.WindowClip, SKClipOperation.Intersect, true);
                else
                    canvas.ClipRect(p.WindowClipRect, SKClipOperation.Intersect, false);

                if (pf != null)
                {
                    if (_particlePixelSize > 0.4)
                    {
                        canvas.Save();
                        canvas.Translate((float)p.Slot.WindowRect.X, (float)p.Slot.WindowRect.Y);
                        pf.DrawPixelated(canvas, atlasImg, _atlas.ParticleSpriteRect, _atlasPaint, o, _particleBrightness, _particlePixelSize);
                        canvas.Restore();
                    }
                    else
                    {
                        pf.Draw(canvas, atlasImg, _atlas.ParticleSpriteRect, _atlasPaint, _linearSampling, (float)o, _particleBrightness);
                    }
                }
                painter?.Invoke(canvas, p.Slot.WindowRect, o);

                canvas.Restore();
                continue;
            }

            if (sec.Count == 1)
            {
                var p = _parts[sec.Start];
                if (!p.Visible) continue;
                var o = opacity * p.Opacity;
                if (o <= 0.001) continue;

                var s = p.Slot;
                if (Math.Abs(p.ScaleX - p.ScaleY) >= 1e-5)
                {
                    DrawAnisotropic(canvas, atlasImg, p, o, rasterScale, invRasterScale);
                    continue;
                }

                var totalScale = (float)p.ScaleX * invRasterScale;
                var px = (float)(s.Pivot.X + p.X);
                var py = (float)(s.Pivot.Y + p.Y);
                var ax = (float)(s.Pivot.X - s.LogicalBounds.X) * rasterScale;
                var ay = (float)(s.Pivot.Y - s.LogicalBounds.Y) * rasterScale;

                if (Math.Abs(p.Rotation) < 1e-5 && o >= 0.996 && p.Tint == SKColors.White && totalScale >= 0f)
                {
                    var tx = px - totalScale * ax;
                    var ty = py - totalScale * ay;
                    var dst = new SKRect(tx, ty, tx + s.SrcRect.Width * totalScale, ty + s.SrcRect.Height * totalScale);
                    var samp = Math.Abs(totalScale - 1f) < 1e-4f ? _nearestSampling : _linearSampling;
                    canvas.DrawImage(atlasImg, s.SrcRect, dst, samp, null);
                    continue;
                }

                float scos, ssin;
                if (Math.Abs(p.Rotation) < 1e-5)
                {
                    scos = totalScale;
                    ssin = 0f;
                }
                else
                {
                    var rad = (float)p.Rotation * DegToRad;
                    (ssin, scos) = MathF.SinCos(rad);
                    scos *= totalScale;
                    ssin *= totalScale;
                }

                sec.Xf[0] = new SKRotationScaleMatrix(scos, ssin, px - scos * ax + ssin * ay, py - ssin * ax - scos * ay);
                sec.Col[0] = ModulateColor(o, p.Tint);
                canvas.DrawAtlas(atlasImg, sec.Src, sec.Xf, sec.Col, SKBlendMode.Modulate, _linearSampling, _atlasPaint);
                continue;
            }

            // Multi-slot contiguous section: zero-copy fast path when all slots are visible and isotropic
            var fastPath = true;
            for (var k = 0; k < sec.Count; k++)
            {
                var p = _parts[sec.Start + k];
                var o = opacity * p.Opacity;
                if (!p.Visible || o <= 0.001 || Math.Abs(p.ScaleX - p.ScaleY) >= 1e-5)
                {
                    fastPath = false;
                    break;
                }

                var s = p.Slot;
                var totalScale = (float)p.ScaleX * invRasterScale;
                var px = (float)(s.Pivot.X + p.X);
                var py = (float)(s.Pivot.Y + p.Y);
                var ax = (float)(s.Pivot.X - s.LogicalBounds.X) * rasterScale;
                var ay = (float)(s.Pivot.Y - s.LogicalBounds.Y) * rasterScale;

                float scos, ssin;
                if (Math.Abs(p.Rotation) < 1e-5)
                {
                    scos = totalScale;
                    ssin = 0f;
                }
                else
                {
                    var rad = (float)p.Rotation * DegToRad;
                    (ssin, scos) = MathF.SinCos(rad);
                    scos *= totalScale;
                    ssin *= totalScale;
                }

                sec.Xf[k] = new SKRotationScaleMatrix(scos, ssin, px - scos * ax + ssin * ay, py - ssin * ax - scos * ay);
                sec.Col[k] = ModulateColor(o, p.Tint);
            }

            if (fastPath)
            {
                canvas.DrawAtlas(atlasImg, sec.Src, sec.Xf, sec.Col, SKBlendMode.Modulate, _linearSampling, _atlasPaint);
                continue;
            }

            // Fallback preserving z-order when a subset is hidden or uses anisotropic ScaleX/ScaleY
            var batchCount = 0;
            for (var k = 0; k < sec.Count; k++)
            {
                var p = _parts[sec.Start + k];
                if (!p.Visible) continue;
                var o = opacity * p.Opacity;
                if (o <= 0.001) continue;

                if (Math.Abs(p.ScaleX - p.ScaleY) >= 1e-5)
                {
                    if (batchCount > 0)
                    {
                        FlushBatch(canvas, atlasImg, batchCount);
                        batchCount = 0;
                    }
                    DrawAnisotropic(canvas, atlasImg, p, o, rasterScale, invRasterScale);
                    continue;
                }

                var s = p.Slot;
                var totalScale = (float)p.ScaleX * invRasterScale;
                var px = (float)(s.Pivot.X + p.X);
                var py = (float)(s.Pivot.Y + p.Y);
                var ax = (float)(s.Pivot.X - s.LogicalBounds.X) * rasterScale;
                var ay = (float)(s.Pivot.Y - s.LogicalBounds.Y) * rasterScale;

                float scos, ssin;
                if (Math.Abs(p.Rotation) < 1e-5)
                {
                    scos = totalScale;
                    ssin = 0f;
                }
                else
                {
                    var rad = (float)p.Rotation * DegToRad;
                    (ssin, scos) = MathF.SinCos(rad);
                    scos *= totalScale;
                    ssin *= totalScale;
                }

                _src[batchCount] = s.SrcRect;
                _xf[batchCount] = new SKRotationScaleMatrix(scos, ssin, px - scos * ax + ssin * ay, py - ssin * ax - scos * ay);
                _col[batchCount] = ModulateColor(o, p.Tint);
                batchCount++;
            }

            if (batchCount > 0)
                FlushBatch(canvas, atlasImg, batchCount);
        }
    }

    private void DrawAnisotropic(SKCanvas canvas, SKImage atlasImg, HyperLayer p, double opacity, float rasterScale, float invRasterScale)
    {
        if (Math.Abs(p.ScaleX) < 1e-4 || Math.Abs(p.ScaleY) < 1e-4) return;
        var s = p.Slot;
        var px = (float)(s.Pivot.X + p.X);
        var py = (float)(s.Pivot.Y + p.Y);
        var ax = (float)(s.Pivot.X - s.LogicalBounds.X) * rasterScale;
        var ay = (float)(s.Pivot.Y - s.LogicalBounds.Y) * rasterScale;

        var absSy = (float)Math.Abs(p.ScaleY);
        var sxFactor = (float)(p.ScaleX / absSy);
        var sySign = p.ScaleY < 0 ? -1f : 1f;
        var uniformScale = absSy * invRasterScale;

        canvas.Save();
        canvas.Translate(px, py);
        if (Math.Abs(p.Rotation) >= 1e-5)
            canvas.RotateDegrees((float)p.Rotation);
        canvas.Scale(sxFactor, sySign);
        _src1[0] = s.SrcRect;
        _xf1[0] = new SKRotationScaleMatrix(uniformScale, 0f, -uniformScale * ax, -uniformScale * ay);
        _col1[0] = ModulateColor(opacity, p.Tint);
        canvas.DrawAtlas(atlasImg, _src1, _xf1, _col1, SKBlendMode.Modulate, _linearSampling, _atlasPaint);
        canvas.Restore();
    }

    private void FlushBatch(SKCanvas canvas, SKImage atlasImg, int count)
    {
        if (count == 1 && Math.Abs(_xf[0].SSin) < 1e-6f && _xf[0].SCos >= 0f && _col[0] == SKColors.White)
        {
            var s = _src[0];
            var m = _xf[0];
            var dst = new SKRect(m.TX, m.TY, m.TX + s.Width * m.SCos, m.TY + s.Height * m.SCos);
            canvas.DrawImage(atlasImg, s, dst, Math.Abs(m.SCos - 1f) < 1e-4f ? _nearestSampling : _linearSampling, null);
            return;
        }

        var entry = _exactBatches[count];
        if (entry == null)
        {
            entry = (new SKRect[count], new SKRotationScaleMatrix[count], new SKColor[count]);
            _exactBatches[count] = entry;
        }
        var (bSrc, bXf, bCol) = entry.Value;
        Array.Copy(_src, bSrc, count);
        Array.Copy(_xf, bXf, count);
        Array.Copy(_col, bCol, count);
        canvas.DrawAtlas(atlasImg, bSrc, bXf, bCol, SKBlendMode.Modulate, _linearSampling, _atlasPaint);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_paths != null)
        {
            foreach (var p in _paths.Values) p.Dispose();
            _paths.Clear();
        }
        foreach (var p in _parts) p.Dispose();
        _atlasPaint.Dispose();
        _atlas.Release();
    }
}

internal abstract class HyperSkin<TState> : IDisposable where TState : struct
{
    private readonly List<IDisposable> _owned = new();
    protected HyperRig Rig { get; private set; } = null!;

    protected HyperSkin() { }

    protected HyperSkin(HyperRig rig) => Rig = rig;

    internal void AttachRig(HyperRig rig, out List<IDisposable> ownedList)
    {
        Rig = rig;
        ownedList = _owned;
    }

    public virtual int MaxFps => 0;

    protected HyperLayer Part(string name) => Rig[name];

    protected HyperPath Path(string name) => Rig.Path(name);

    protected Rect Zone(string name) => Rig.Zone(name);

    protected T? Own<T>(T? item) where T : class, IDisposable
    {
        if (item != null) _owned.Add(item);
        return item;
    }

    internal void SetRenderContext(GRContext? grContext)
    {
        if (Rig != null) Rig.CurrentGrContext = grContext;
    }

    internal bool ConsumePendingGpuPack()
    {
        if (Rig == null || !Rig.LastDrawPackPending) return false;
        Rig.LastDrawPackPending = false;
        return true;
    }

    public abstract bool Advance(in HyperFrame frame, in TState state);

    public virtual void SwapAtlas(HyperAtlas atlas) => Rig?.SwapAtlas(atlas);

    public virtual void Draw(SKCanvas canvas, double opacity) => Rig?.Draw(canvas, opacity);

    public virtual void Dispose()
    {
        for (var i = _owned.Count - 1; i >= 0; i--) _owned[i].Dispose();
        _owned.Clear();
        Rig?.Dispose();
    }
}

internal sealed class HyperShared<TState> where TState : struct
{
    private readonly object _gate = new();
    private TState _state;
    private HyperInput _input;
    private int _wake;

    public TState Read(out HyperInput input)
    {
        lock (_gate)
        {
            input = _input;
            return _state;
        }
    }

    public void Publish(TState state)
    {
        lock (_gate) _state = state;
    }

    public bool UpdateInput(bool? hover = null, bool? pressed = null)
    {
        lock (_gate)
        {
            var next = new HyperInput(hover ?? _input.Hover, pressed ?? _input.Pressed);
            if (next == _input) return false;
            _input = next;
            return true;
        }
    }

    public bool ArmWake() => Interlocked.Exchange(ref _wake, 1) == 0;
    public void DisarmWake() => Interlocked.Exchange(ref _wake, 0);
}

internal static class HyperMessages
{
    public static readonly object Wake = new();
    public static readonly object Release = new();
    public static readonly object Hidden = new();
    public static readonly object Visible = new();
}

internal sealed class HyperSwapAtlasMessage
{
    public readonly HyperAtlas Atlas;
    public HyperSwapAtlasMessage(HyperAtlas atlas) => Atlas = atlas;
}

internal sealed class HyperResizeMessage
{
    public readonly Size Size;
    public HyperResizeMessage(Size size) => Size = size;
}

internal sealed class HyperHandler<TState> : CompositionCustomVisualHandler where TState : struct
{
    private readonly HyperShared<TState> _shared;
    private readonly Rect _bounds;
    private Size _renderSize;
    private HyperSkin<TState>? _skin;
    private long _lastFrame;
    private long _lastFadeTick;
    private long _fadeStartLogTick;
    private double _fadeProgress;
    private SKPaint? _fadePaint;
    private bool _registered;
    private bool _hidden;
    private bool _packPending;
    private bool _fadeActive;
    private bool _fadeFirstFrameDone;
    private bool _fadeCompleted;
    private bool _firstRenderLogged;

    public HyperHandler(HyperShared<TState> shared, Rect bounds)
    {
        _shared = shared;
        _bounds = bounds;
        _renderSize = bounds.Size;
    }

    public override Rect GetRenderBounds() =>
        new(0, 0, Math.Max(_bounds.Right, _renderSize.Width), Math.Max(_bounds.Bottom, _renderSize.Height));

    public override void OnMessage(object message)
    {
        if (ReferenceEquals(message, HyperMessages.Wake))
        {
            _shared.DisarmWake();
            if (!_hidden) Step(forced: true);
        }
        else if (ReferenceEquals(message, HyperMessages.Hidden))
        {
            _hidden = true;
        }
        else if (ReferenceEquals(message, HyperMessages.Visible))
        {
            _hidden = false;
            Step(forced: true);
        }
        else if (message is HyperSwapAtlasMessage swap)
        {
            if (_skin != null)
            {
                _skin.SwapAtlas(swap.Atlas);
                Invalidate();
            }
            else
            {
                swap.Atlas.Release();
            }
        }
        else if (message is HyperResizeMessage resize)
        {
            if (resize.Size.Width > 0 && resize.Size.Height > 0 && resize.Size != _renderSize)
            {
                _renderSize = resize.Size;
                Invalidate();
            }
        }
        else if (message is HyperSkin<TState> skin)
        {
            _skin?.Dispose();
            _skin = skin;
            _firstRenderLogged = false;
            if (!_fadeCompleted && HyperSvgOptions.FirstRenderFadeMs > 0)
            {
                _fadeActive = true;
                _fadeFirstFrameDone = false;
                _fadeProgress = 0.0;
                _lastFadeTick = 0;
            }
            Invalidate();
            if (!_hidden) Step(forced: true);
        }
        else if (ReferenceEquals(message, HyperMessages.Release))
        {
            _skin?.Dispose();
            _skin = null;
            _fadeActive = false;
            _fadeFirstFrameDone = false;
            _fadeCompleted = false;
            _fadeProgress = 0.0;
            _lastFadeTick = 0;
            _fadePaint?.Dispose();
            _fadePaint = null;
        }
    }

    public override void OnAnimationFrameUpdate()
    {
        _registered = false;
        if (_packPending)
        {
            _packPending = false;
            Invalidate();
        }
        if (!_hidden) Step(forced: false);
    }

    private void Step(bool forced)
    {
        var skin = _skin;
        if (skin == null || _hidden) return;

        var now = HyperTime.Now();
        var dt = _lastFrame == 0 ? 0.0 : Math.Min(0.1, (now - _lastFrame) / (double)Stopwatch.Frequency);

        var fadeAnimating = false;
        if (_fadeActive && _fadeFirstFrameDone)
        {
            var fadeDt = _lastFadeTick == 0
                ? (1.0 / 60.0)
                : Math.Min(0.033, Math.Max(0.0, (now - _lastFadeTick) / (double)Stopwatch.Frequency));
            _lastFadeTick = now;

            var fadeSec = HyperSvgOptions.FirstRenderFadeMs / 1000.0;
            if (fadeSec <= 0)
            {
                _fadeProgress = 1.0;
                _fadeActive = false;
                _fadeCompleted = true;
            }
            else
            {
                _fadeProgress = Math.Min(1.0, _fadeProgress + fadeDt / fadeSec);
                if (_fadeProgress >= 1.0)
                {
                    _fadeActive = false;
                    _fadeCompleted = true;
                    if (HyperSvgOptions.LogFirstRenderPerf)
                    {
                        var totalFadeMs = (now - _fadeStartLogTick) * 1000.0 / Stopwatch.Frequency;
                        var name = skin.GetType().DeclaringType?.Name ?? skin.GetType().Name;
                        Console.WriteLine($"[HyperPerf] FadeDone {name}: {totalFadeMs:0.0}ms");
                    }
                }
            }
            fadeAnimating = true;
        }

        var skipSkinStep = !forced && skin.MaxFps > 0 && now - _lastFrame < Stopwatch.Frequency / skin.MaxFps * 0.85;
        bool skinAnimating;
        if (skipSkinStep)
        {
            skinAnimating = true;
        }
        else
        {
            var state = _shared.Read(out var input);
            var frame = new HyperFrame(now, dt, input);
            skinAnimating = skin.Advance(in frame, in state);
            _lastFrame = now;
        }

        if (skinAnimating || fadeAnimating || _fadeActive)
        {
            Invalidate();
            if (!_registered)
            {
                _registered = true;
                RegisterForNextAnimationFrameUpdate();
            }
        }
    }

    public override void OnRender(ImmediateDrawingContext context)
    {
        var skin = _skin;
        if (skin == null) return;
        if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not ISkiaSharpApiLeaseFeature feature) return;

        var logFirst = !_firstRenderLogged && HyperSvgOptions.LogFirstRenderPerf;
        var renderT0 = logFirst ? Stopwatch.GetTimestamp() : 0;

        using var lease = feature.Lease();
        HyperAtlasCache.MarkRenderThread(lease.GrContext != null);
        HyperAtlasCache.DrainPendingGpuDisposals(lease.GrContext);
        skin.SetRenderContext(lease.GrContext);

        var warmupOnly = _fadeActive && !_fadeFirstFrameDone;
        var useFadeLayer = _fadeActive;
        if (useFadeLayer)
        {
            byte alpha;
            if (warmupOnly)
            {
                alpha = 2;
            }
            else
            {
                var inv = 1.0 - _fadeProgress;
                var eased = 1.0 - inv * inv * inv;
                alpha = (byte)Math.Clamp((int)Math.Round(eased * 255.0), 0, 255);
            }
            _fadePaint ??= new SKPaint();
            _fadePaint.Color = new SKColor(255, 255, 255, alpha);
        }

        var canvas = lease.SkCanvas;
        var needScale = _renderSize.Width > 0 && _renderSize.Height > 0 &&
                        (Math.Abs(_renderSize.Width - _bounds.Width) > 0.1 || Math.Abs(_renderSize.Height - _bounds.Height) > 0.1);
        if (needScale)
        {
            canvas.Save();
            canvas.Scale((float)(_renderSize.Width / _bounds.Width), (float)(_renderSize.Height / _bounds.Height));
            if (useFadeLayer)
            {
                canvas.SaveLayer(_fadePaint);
                skin.Draw(canvas, lease.CurrentOpacity);
                canvas.Restore();
            }
            else
            {
                skin.Draw(canvas, lease.CurrentOpacity);
            }
            canvas.Restore();
        }
        else if (useFadeLayer)
        {
            canvas.SaveLayer(_fadePaint);
            skin.Draw(canvas, lease.CurrentOpacity);
            canvas.Restore();
        }
        else
        {
            skin.Draw(canvas, lease.CurrentOpacity);
        }

        var packPending = skin.ConsumePendingGpuPack();
        if (logFirst)
        {
            _firstRenderLogged = true;
            var ms = (Stopwatch.GetTimestamp() - renderT0) * 1000.0 / Stopwatch.Frequency;
            var name = skin.GetType().DeclaringType?.Name ?? skin.GetType().Name;
            Console.WriteLine($"[HyperPerf] FirstRender {name}: {ms:0.0}ms (packPending={packPending})");
        }

        if (warmupOnly)
        {
            _fadeFirstFrameDone = true;
            _lastFadeTick = HyperTime.Now();
            _fadeStartLogTick = _lastFadeTick;
        }

        if (packPending)
        {
            _packPending = true;
            if (!_registered)
            {
                _registered = true;
                RegisterForNextAnimationFrameUpdate();
            }
        }
    }
}

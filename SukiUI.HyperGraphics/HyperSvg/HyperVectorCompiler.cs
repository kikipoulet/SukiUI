using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Skia;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

internal enum HyperPaintEntryKind : byte
{
    None,
    Solid,
    LinearGradient,
    RadialGradient,
    Pattern
}

internal sealed class HyperPaintEntry
{
    public HyperPaintEntryKind Kind;
    public SKColor Color;
    public bool IsRelative;
    public Point Start;
    public Point End;
    public Point Center;
    public Point Origin;
    public float RadiusX;
    public float RadiusY;
    public SKShaderTileMode TileMode;
    public SKColor[] Colors = Array.Empty<SKColor>();
    public HyperPaintRef[]? StopRefs;
    public float[] Offsets = Array.Empty<float>();

    public float PatternX;
    public float PatternY;
    public float PatternWidth;
    public float PatternHeight;
    public bool HasPatternTransform;
    public SKMatrix PatternTransform;
    public HyperDrawOp[]? PatternOps;
}

internal enum HyperFilterOpKind : byte
{
    Blur,
    DropShadow,
    InnerShadow
}

internal readonly struct HyperFilterSpec
{
    public readonly HyperFilterOpKind Kind;
    public readonly float Dx;
    public readonly float Dy;
    public readonly float SigmaX;
    public readonly float SigmaY;
    public readonly HyperPaintRef Color;

    public HyperFilterSpec(
        HyperFilterOpKind kind,
        float dx,
        float dy,
        float sigmaX,
        float sigmaY,
        HyperPaintRef color)
    {
        Kind = kind;
        Dx = dx;
        Dy = dy;
        SigmaX = sigmaX;
        SigmaY = sigmaY;
        Color = color;
    }
}

internal sealed class HyperMaskDef
{
    public bool HasBounds;
    public SKRect Bounds;
    public HyperDrawOp[] Ops = Array.Empty<HyperDrawOp>();
}

internal sealed class HyperPaintSnapshot
{
    public readonly HyperPaintEntry[] Entries;

    private HyperPaintSnapshot(HyperPaintEntry[] entries) => Entries = entries;

    public static HyperPaintSnapshot Capture(IReadOnlyList<object?> values)
    {
        var entries = new HyperPaintEntry[values.Count];
        for (var i = 0; i < values.Count; i++)
            entries[i] = Convert(values[i]);
        return new HyperPaintSnapshot(entries);
    }

    private static HyperPaintEntry Convert(object? value)
    {
        switch (value)
        {
            case Color c:
                return new HyperPaintEntry { Kind = HyperPaintEntryKind.Solid, Color = c.ToSKColor() };
            case ISolidColorBrush s:
            {
                var c = s.Color;
                var a = (byte)Math.Clamp((int)Math.Round(c.A * s.Opacity), 0, 255);
                return new HyperPaintEntry { Kind = HyperPaintEntryKind.Solid, Color = new SKColor(c.R, c.G, c.B, a) };
            }
            case ILinearGradientBrush lg:
            {
                ExtractStops(lg, out var colors, out var offsets);
                return new HyperPaintEntry
                {
                    Kind = HyperPaintEntryKind.LinearGradient,
                    IsRelative = lg.StartPoint.Unit == RelativeUnit.Relative,
                    Start = lg.StartPoint.Point,
                    End = lg.EndPoint.Point,
                    TileMode = lg.SpreadMethod.ToSKShaderTileMode(),
                    Colors = colors,
                    Offsets = offsets
                };
            }
            case IRadialGradientBrush rg:
            {
                ExtractStops(rg, out var colors, out var offsets);
                return new HyperPaintEntry
                {
                    Kind = HyperPaintEntryKind.RadialGradient,
                    IsRelative = rg.Center.Unit == RelativeUnit.Relative,
                    Center = rg.Center.Point,
                    Origin = rg.GradientOrigin.Point,
                    RadiusX = (float)rg.RadiusX.Scalar,
                    RadiusY = (float)rg.RadiusY.Scalar,
                    TileMode = rg.SpreadMethod.ToSKShaderTileMode(),
                    Colors = colors,
                    Offsets = offsets
                };
            }
            default:
                return new HyperPaintEntry { Kind = HyperPaintEntryKind.None };
        }
    }

    private static void ExtractStops(IGradientBrush b, out SKColor[] colors, out float[] offsets)
    {
        var stops = b.GradientStops;
        colors = new SKColor[stops.Count];
        offsets = new float[stops.Count];
        var op = b.Opacity;
        for (var i = 0; i < stops.Count; i++)
        {
            var c = stops[i].Color;
            var a = (byte)Math.Clamp((int)Math.Round(c.A * op), 0, 255);
            colors[i] = new SKColor(c.R, c.G, c.B, a);
            offsets[i] = (float)stops[i].Offset;
        }
    }
}

internal enum HyperShapeKind : byte
{
    GroupPush,
    GroupPop,
    Rect,
    Circle,
    Ellipse,
    Line,
    Path
}

internal enum HyperColorSource : byte
{
    None,
    Fixed,
    Token,
    DefPaint
}

internal readonly struct HyperPaintRef
{
    public readonly HyperColorSource Source;
    public readonly SKColor Fixed;
    public readonly int TokenIndex;
    public readonly float Opacity;

    public HyperPaintRef(HyperColorSource source, SKColor fixedColor, int tokenIndex, float opacity)
    {
        Source = source;
        Fixed = fixedColor;
        TokenIndex = tokenIndex;
        Opacity = opacity;
    }

    public static HyperPaintRef Unset => new(HyperColorSource.None, default, -1, 1f);
}

internal sealed class HyperDrawOp
{
    public HyperShapeKind Shape;
    public SKRect Rect;
    public float Rx, Ry;
    public float X1, Y1, X2, Y2;
    public SKPath? Path;
    public SKRect ObjectBounds;

    public HyperPaintRef Fill;
    public HyperPaintRef Stroke;
    public float StrokeWidth;
    public SKStrokeCap StrokeCap;
    public SKStrokeJoin StrokeJoin;
    public float[]? StrokeDashArray;
    public float StrokeDashOffset;
    public float Opacity;
    public bool HasTransform;
    public SKMatrix Transform;
    public int FilterIndex = -1;
    public int ClipPathIndex = -1;
    public int MaskIndex = -1;
}

internal sealed class HyperCompiledSlice : IDisposable
{
    private readonly HyperDrawOp[] _ops;
    private readonly HyperPaintEntry[] _defPaints;
    private readonly HyperFilterSpec[] _filters;
    private readonly SKPath[] _clipPaths;
    private readonly HyperMaskDef[] _masks;

    public HyperCompiledSlice(
        HyperDrawOp[] ops,
        HyperPaintEntry[]? defPaints = null,
        HyperFilterSpec[]? filters = null,
        SKPath[]? clipPaths = null,
        HyperMaskDef[]? masks = null)
    {
        _ops = ops;
        _defPaints = defPaints ?? Array.Empty<HyperPaintEntry>();
        _filters = filters ?? Array.Empty<HyperFilterSpec>();
        _clipPaths = clipPaths ?? Array.Empty<SKPath>();
        _masks = masks ?? Array.Empty<HyperMaskDef>();
    }

    public void Render(SKCanvas canvas, HyperPaintSnapshot palette) =>
        RenderOps(_ops, canvas, palette);

    private void RenderOps(HyperDrawOp[] ops, SKCanvas canvas, HyperPaintSnapshot palette)
    {
        using var paint = new SKPaint { IsAntialias = true };

        for (var i = 0; i < ops.Length; i++)
        {
            var op = ops[i];
            if (op.Shape == HyperShapeKind.GroupPush)
            {
                if (op.HasTransform || op.ClipPathIndex >= 0)
                {
                    canvas.Save();
                    if (op.HasTransform)
                        canvas.Concat(op.Transform);
                    if (op.ClipPathIndex >= 0)
                        canvas.ClipPath(_clipPaths[op.ClipPathIndex], SKClipOperation.Intersect, true);
                }

                if (op.MaskIndex >= 0)
                {
                    BeginMaskLayer(canvas, _masks[op.MaskIndex], op.Opacity, op.FilterIndex, op.ObjectBounds, palette);
                }
                else if (op.FilterIndex >= 0 || op.Opacity < 0.999f)
                {
                    using var filter = CreateFilter(op.FilterIndex, op.ObjectBounds, palette);
                    using var lp = new SKPaint
                    {
                        Color = new SKColor(255, 255, 255, (byte)Math.Clamp((int)Math.Round(op.Opacity * 255f), 0, 255)),
                        ImageFilter = filter
                    };
                    canvas.SaveLayer(lp);
                }
                continue;
            }

            if (op.Shape == HyperShapeKind.GroupPop)
            {
                if (op.MaskIndex >= 0)
                    EndMaskLayer(canvas, _masks[op.MaskIndex], op.Opacity, palette);
                else if (op.FilterIndex >= 0 || op.Opacity < 0.999f)
                    canvas.Restore();

                if (op.HasTransform || op.ClipPathIndex >= 0)
                    canvas.Restore();
                continue;
            }

            var hasFill = op.Fill.Source != HyperColorSource.None;
            var hasStroke = op.Stroke.Source != HyperColorSource.None && op.StrokeWidth > 0f;
            if (!hasFill && !hasStroke) continue;

            var savedTransform = false;
            if (op.HasTransform || op.ClipPathIndex >= 0)
            {
                canvas.Save();
                if (op.HasTransform)
                    canvas.Concat(op.Transform);
                if (op.ClipPathIndex >= 0)
                    canvas.ClipPath(_clipPaths[op.ClipPathIndex], SKClipOperation.Intersect, true);
                savedTransform = true;
            }

            if (op.MaskIndex >= 0)
            {
                BeginMaskLayer(canvas, _masks[op.MaskIndex], op.Opacity, op.FilterIndex, op.ObjectBounds, palette);
            }

            var useLayerForNode = op.MaskIndex < 0 && ((op.FilterIndex >= 0 && hasFill && hasStroke) || (op.Opacity < 0.999f && hasFill && hasStroke));
            SKImageFilter? directFilter = null;
            var savedLayer = false;

            if (useLayerForNode)
            {
                using var filter = CreateFilter(op.FilterIndex, op.ObjectBounds, palette);
                using var lp = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, (byte)Math.Clamp((int)Math.Round(op.Opacity * 255f), 0, 255)),
                    ImageFilter = filter
                };
                canvas.SaveLayer(lp);
                savedLayer = true;
            }
            else if (op.MaskIndex < 0 && op.FilterIndex >= 0)
            {
                directFilter = CreateFilter(op.FilterIndex, op.ObjectBounds, palette);
            }

            var nodeOpacity = (savedLayer || op.MaskIndex >= 0) ? 1f : op.Opacity;

            if (hasFill)
            {
                paint.Reset();
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;
                paint.ImageFilter = directFilter;
                var shader = ConfigurePaint(paint, op.Fill, nodeOpacity, op.ObjectBounds, palette);
                if (paint.Color.Alpha > 0 || shader != null)
                    DrawGeometry(canvas, op, paint);
                shader?.Dispose();
                paint.Shader = null;
                paint.ImageFilter = null;
            }

            if (hasStroke)
            {
                paint.Reset();
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = op.StrokeWidth;
                paint.StrokeCap = op.StrokeCap;
                paint.StrokeJoin = op.StrokeJoin;
                using var dashEffect = op.StrokeDashArray != null
                    ? SKPathEffect.CreateDash(op.StrokeDashArray, op.StrokeDashOffset)
                    : null;
                paint.PathEffect = dashEffect;
                paint.ImageFilter = !hasFill ? directFilter : null;
                var shader = ConfigurePaint(paint, op.Stroke, nodeOpacity, op.ObjectBounds, palette);
                if (paint.Color.Alpha > 0 || shader != null)
                    DrawGeometry(canvas, op, paint);
                shader?.Dispose();
                paint.Shader = null;
                paint.PathEffect = null;
                paint.ImageFilter = null;
            }

            directFilter?.Dispose();
            if (savedLayer) canvas.Restore();
            if (op.MaskIndex >= 0)
                EndMaskLayer(canvas, _masks[op.MaskIndex], op.Opacity, palette);
            if (savedTransform) canvas.Restore();
        }
    }

    private void BeginMaskLayer(
        SKCanvas canvas,
        HyperMaskDef mask,
        float opacity,
        int filterIndex,
        SKRect objBounds,
        HyperPaintSnapshot palette)
    {
        if (opacity < 0.999f)
        {
            using var opPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)Math.Clamp((int)Math.Round(opacity * 255f), 0, 255))
            };
            if (mask.HasBounds)
                canvas.SaveLayer(mask.Bounds, opPaint);
            else
                canvas.SaveLayer(opPaint);
        }

        using var filter = CreateFilter(filterIndex, objBounds, palette);
        using var contentPaint = filter != null ? new SKPaint { ImageFilter = filter } : null;
        if (mask.HasBounds)
            canvas.SaveLayer(mask.Bounds, contentPaint);
        else
            canvas.SaveLayer(contentPaint);
    }

    private void EndMaskLayer(SKCanvas canvas, HyperMaskDef mask, float opacity, HyperPaintSnapshot palette)
    {
        using var dstInPaint = new SKPaint { BlendMode = SKBlendMode.DstIn };
        if (mask.HasBounds)
            canvas.SaveLayer(mask.Bounds, dstInPaint);
        else
            canvas.SaveLayer(dstInPaint);

        using var lumaFilter = SKColorFilter.CreateLumaColor();
        using var lumaPaint = new SKPaint { ColorFilter = lumaFilter };
        if (mask.HasBounds)
            canvas.SaveLayer(mask.Bounds, lumaPaint);
        else
            canvas.SaveLayer(lumaPaint);

        RenderOps(mask.Ops, canvas, palette);

        canvas.Restore(); // lumaPaint -> dstIn layer
        canvas.Restore(); // dstInPaint -> masked content layer
        canvas.Restore(); // masked content layer -> main canvas (or opacity layer)
        if (opacity < 0.999f)
            canvas.Restore();
    }

    private static void DrawGeometry(SKCanvas canvas, HyperDrawOp op, SKPaint paint)
    {
        switch (op.Shape)
        {
            case HyperShapeKind.Rect:
                if (op.Rx > 0f || op.Ry > 0f)
                    canvas.DrawRoundRect(op.Rect, op.Rx, op.Ry, paint);
                else
                    canvas.DrawRect(op.Rect, paint);
                break;
            case HyperShapeKind.Circle:
                canvas.DrawCircle(op.X1, op.Y1, op.Rx, paint);
                break;
            case HyperShapeKind.Ellipse:
                canvas.DrawOval(op.Rect, paint);
                break;
            case HyperShapeKind.Line:
                canvas.DrawLine(op.X1, op.Y1, op.X2, op.Y2, paint);
                break;
            case HyperShapeKind.Path:
                if (op.Path != null)
                    canvas.DrawPath(op.Path, paint);
                break;
        }
    }

    private SKShader? ConfigurePaint(
        SKPaint paint,
        HyperPaintRef pref,
        float nodeOpacity,
        SKRect bounds,
        HyperPaintSnapshot palette)
    {
        var mul = Math.Clamp(nodeOpacity * pref.Opacity, 0f, 1f);
        if (pref.Source == HyperColorSource.Fixed)
        {
            var c = pref.Fixed;
            paint.Color = new SKColor(c.Red, c.Green, c.Blue, (byte)Math.Round(c.Alpha * mul));
            return null;
        }

        HyperPaintEntry entry;
        if (pref.Source == HyperColorSource.DefPaint)
        {
            if (pref.TokenIndex < 0 || pref.TokenIndex >= _defPaints.Length)
            {
                paint.Color = SKColors.Transparent;
                return null;
            }
            entry = _defPaints[pref.TokenIndex];
        }
        else
        {
            if (pref.TokenIndex < 0 || pref.TokenIndex >= palette.Entries.Length)
            {
                paint.Color = SKColors.Transparent;
                return null;
            }
            entry = palette.Entries[pref.TokenIndex];
        }
        switch (entry.Kind)
        {
            case HyperPaintEntryKind.Solid:
            {
                var c = entry.Color;
                paint.Color = new SKColor(c.Red, c.Green, c.Blue, (byte)Math.Round(c.Alpha * mul));
                return null;
            }
            case HyperPaintEntryKind.LinearGradient:
            {
                var colors = ResolveGradientColors(entry, palette);
                if (colors.Length == 0)
                {
                    paint.Color = SKColors.Transparent;
                    return null;
                }
                var shader = CreateLinearShader(entry, colors, bounds);
                paint.Shader = shader;
                paint.Color = new SKColor(255, 255, 255, (byte)Math.Round(255f * mul));
                return shader;
            }
            case HyperPaintEntryKind.RadialGradient:
            {
                var colors = ResolveGradientColors(entry, palette);
                if (colors.Length == 0)
                {
                    paint.Color = SKColors.Transparent;
                    return null;
                }
                var shader = CreateRadialShader(entry, colors, bounds);
                paint.Shader = shader;
                paint.Color = new SKColor(255, 255, 255, (byte)Math.Round(255f * mul));
                return shader;
            }
            case HyperPaintEntryKind.Pattern:
            {
                if (entry.PatternOps == null || entry.PatternWidth <= 0f || entry.PatternHeight <= 0f)
                {
                    paint.Color = SKColors.Transparent;
                    return null;
                }
                var shader = CreatePatternShader(entry, palette);
                paint.Shader = shader;
                paint.Color = new SKColor(255, 255, 255, (byte)Math.Round(255f * mul));
                return shader;
            }
            default:
                paint.Color = SKColors.Transparent;
                return null;
        }
    }

    private SKShader CreatePatternShader(HyperPaintEntry entry, HyperPaintSnapshot palette)
    {
        var tileRect = new SKRect(0, 0, entry.PatternWidth, entry.PatternHeight);
        using var recorder = new SKPictureRecorder();
        var recCanvas = recorder.BeginRecording(tileRect);
        RenderOps(entry.PatternOps!, recCanvas, palette);
        using var pic = recorder.EndRecording();
        var matrix = SKMatrix.CreateTranslation(entry.PatternX, entry.PatternY);
        if (entry.HasPatternTransform)
            matrix = SKMatrix.Concat(matrix, entry.PatternTransform);
        return SKShader.CreatePicture(pic, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix, tileRect);
    }

    private static SKColor[] ResolveGradientColors(HyperPaintEntry entry, HyperPaintSnapshot palette)
    {
        if (entry.StopRefs == null)
            return entry.Colors;

        var colors = new SKColor[entry.StopRefs.Length];
        for (var i = 0; i < entry.StopRefs.Length; i++)
            colors[i] = ResolveColorRef(entry.StopRefs[i], palette);
        return colors;
    }

    private static SKColor ResolveColorRef(HyperPaintRef pref, HyperPaintSnapshot palette)
    {
        if (pref.Source == HyperColorSource.Fixed)
        {
            var c = pref.Fixed;
            return new SKColor(c.Red, c.Green, c.Blue, (byte)Math.Round(c.Alpha * pref.Opacity));
        }
        if (pref.Source == HyperColorSource.Token && pref.TokenIndex >= 0 && pref.TokenIndex < palette.Entries.Length)
        {
            var e = palette.Entries[pref.TokenIndex];
            if (e.Kind == HyperPaintEntryKind.Solid)
            {
                var c = e.Color;
                return new SKColor(c.Red, c.Green, c.Blue, (byte)Math.Round(c.Alpha * pref.Opacity));
            }
        }
        return SKColors.Transparent;
    }

    private static SKShader CreateLinearShader(HyperPaintEntry entry, SKColor[] colors, SKRect bounds)
    {
        if (!entry.IsRelative)
        {
            return SKShader.CreateLinearGradient(
                new SKPoint((float)entry.Start.X, (float)entry.Start.Y),
                new SKPoint((float)entry.End.X, (float)entry.End.Y),
                colors,
                entry.Offsets,
                entry.TileMode);
        }

        var x0 = bounds.Left + (float)entry.Start.X * bounds.Width;
        var y0 = bounds.Top + (float)entry.Start.Y * bounds.Height;
        var x1 = bounds.Left + (float)entry.End.X * bounds.Width;
        var y1 = bounds.Top + (float)entry.End.Y * bounds.Height;
        return SKShader.CreateLinearGradient(new SKPoint(x0, y0), new SKPoint(x1, y1), colors, entry.Offsets, entry.TileMode);
    }

    private static SKShader CreateRadialShader(HyperPaintEntry entry, SKColor[] colors, SKRect bounds)
    {
        var w = Math.Max(1e-3f, bounds.Width);
        var h = Math.Max(1e-3f, bounds.Height);
        var cx = (float)entry.Center.X;
        var cy = (float)entry.Center.Y;
        var fx = (float)entry.Origin.X;
        var fy = (float)entry.Origin.Y;
        var r = entry.RadiusX > 0 ? entry.RadiusX : 0.5f;

        if (entry.IsRelative)
        {
            var local = SKMatrix.CreateScale(w, h).PostConcat(SKMatrix.CreateTranslation(bounds.Left, bounds.Top));
            if (Math.Abs(fx - cx) < 1e-5f && Math.Abs(fy - cy) < 1e-5f)
                return SKShader.CreateRadialGradient(new SKPoint(cx, cy), r, colors, entry.Offsets, entry.TileMode, local);

            if (entry.Offsets[^1] == 1f)
            {
                var n = colors.Length;
                var revC = new SKColor[n];
                var revO = new float[n];
                for (var i = 0; i < n; i++)
                {
                    revC[i] = colors[n - 1 - i];
                    revO[i] = 1f - entry.Offsets[n - 1 - i];
                }
                return SKShader.CreateTwoPointConicalGradient(
                    new SKPoint(cx, cy), r, new SKPoint(fx, fy), 0f, revC, revO, entry.TileMode, local);
            }
            return SKShader.CreateTwoPointConicalGradient(
                new SKPoint(fx, fy), 0f, new SKPoint(cx, cy), r, colors, entry.Offsets, entry.TileMode, local);
        }

        if (Math.Abs(fx - cx) < 1e-5f && Math.Abs(fy - cy) < 1e-5f)
            return SKShader.CreateRadialGradient(new SKPoint(cx, cy), r, colors, entry.Offsets, entry.TileMode);

        return SKShader.CreateTwoPointConicalGradient(
            new SKPoint(fx, fy), 0f, new SKPoint(cx, cy), r, colors, entry.Offsets, entry.TileMode);
    }

    private SKImageFilter? CreateFilter(int filterIndex, SKRect objBounds, HyperPaintSnapshot palette)
    {
        if (filterIndex < 0 || filterIndex >= _filters.Length)
            return null;

        var spec = _filters[filterIndex];
        switch (spec.Kind)
        {
            case HyperFilterOpKind.Blur:
                return SKImageFilter.CreateBlur(spec.SigmaX, spec.SigmaY);
            case HyperFilterOpKind.DropShadow:
                return SKImageFilter.CreateDropShadow(spec.Dx, spec.Dy, spec.SigmaX, spec.SigmaY, ResolveColorRef(spec.Color, palette));
            case HyperFilterOpKind.InnerShadow:
                return CreateInnerShadowFilter(spec, objBounds, palette);
            default:
                return null;
        }
    }

    private static SKImageFilter CreateInnerShadowFilter(HyperFilterSpec spec, SKRect objBounds, HyperPaintSnapshot palette)
    {
        var color = ResolveColorRef(spec.Color, palette);
        var r = color.Red / 255f;
        var g = color.Green / 255f;
        var b = color.Blue / 255f;
        var a = color.Alpha / 255f;
        var padX = (float)Math.Ceiling(spec.SigmaX * 3f + Math.Abs(spec.Dx) + 4f);
        var padY = (float)Math.Ceiling(spec.SigmaY * 3f + Math.Abs(spec.Dy) + 4f);
        var crop = new SKRect(objBounds.Left - padX, objBounds.Top - padY, objBounds.Right + padX, objBounds.Bottom + padY);
        using var invColor = SKColorFilter.CreateColorMatrix(new[]
        {
            0f, 0f, 0f, 0f, r,
            0f, 0f, 0f, 0f, g,
            0f, 0f, 0f, 0f, b,
            0f, 0f, 0f, -a, a
        });
        using var inv = objBounds.IsEmpty
            ? SKImageFilter.CreateColorFilter(invColor)
            : SKImageFilter.CreateColorFilter(invColor, null, crop);
        using var offset = SKImageFilter.CreateOffset(spec.Dx, spec.Dy, inv);
        using var blur = SKImageFilter.CreateBlur(spec.SigmaX, spec.SigmaY, offset);
        using var masked = SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, null!, blur);
        return SKImageFilter.CreateMerge(new SKImageFilter[] { null!, masked });
    }

    public void Dispose()
    {
        for (var i = 0; i < _ops.Length; i++)
        {
            _ops[i].Path?.Dispose();
            _ops[i].Path = null;
        }
        for (var i = 0; i < _clipPaths.Length; i++)
            _clipPaths[i].Dispose();
        for (var i = 0; i < _defPaints.Length; i++)
        {
            var patOps = _defPaints[i].PatternOps;
            if (patOps != null)
            {
                for (var j = 0; j < patOps.Length; j++)
                {
                    patOps[j].Path?.Dispose();
                    patOps[j].Path = null;
                }
            }
        }
        for (var i = 0; i < _masks.Length; i++)
        {
            var mOps = _masks[i].Ops;
            for (var j = 0; j < mOps.Length; j++)
            {
                mOps[j].Path?.Dispose();
                mOps[j].Path = null;
            }
        }
    }
}

internal static class HyperVectorCompiler
{
    private static readonly Regex TransformRx = new(@"([a-zA-Z]+)\s*\(([^)]*)\)", RegexOptions.Compiled);

    private sealed class CompileContext : IDisposable
    {
        public readonly Dictionary<string, int> TokenMap;
        public readonly Dictionary<string, XElement> DefsById;
        public readonly Dictionary<string, int> DefPaintMap = new(StringComparer.Ordinal);
        public readonly List<HyperPaintEntry> DefPaints = new();
        public readonly Dictionary<string, int> FilterMap = new(StringComparer.Ordinal);
        public readonly List<HyperFilterSpec> Filters = new();
        public readonly Dictionary<string, int> ClipPathMap = new(StringComparer.Ordinal);
        public readonly List<SKPath> ClipPaths = new();
        public readonly Dictionary<string, int> MaskMap = new(StringComparer.Ordinal);
        public readonly List<HyperMaskDef> Masks = new();
        public readonly HashSet<string> Visiting = new(StringComparer.Ordinal);

        public CompileContext(Dictionary<string, int> tokenMap, Dictionary<string, XElement> defsById)
        {
            TokenMap = tokenMap;
            DefsById = defsById;
        }

        public void DisposeOwnedResources()
        {
            for (var i = 0; i < ClipPaths.Count; i++)
                ClipPaths[i].Dispose();
            ClipPaths.Clear();
            DisposeOpsPaintsAndMasks();
        }

        private void DisposeOpsPaintsAndMasks()
        {
            for (var i = 0; i < DefPaints.Count; i++)
                DisposeOps(DefPaints[i].PatternOps);
            for (var i = 0; i < Masks.Count; i++)
                DisposeOps(Masks[i].Ops);
        }

        public static void DisposeOps(IReadOnlyList<HyperDrawOp>? ops)
        {
            if (ops == null) return;
            for (var i = 0; i < ops.Count; i++)
                ops[i].Path?.Dispose();
        }

        public void Dispose() { }
    }

    private readonly struct InheritedStyle
    {
        public readonly HyperPaintRef Fill;
        public readonly HyperPaintRef Stroke;
        public readonly float StrokeWidth;
        public readonly SKStrokeCap StrokeCap;
        public readonly SKStrokeJoin StrokeJoin;
        public readonly float FillOpacity;
        public readonly float StrokeOpacity;
        public readonly float[]? StrokeDashArray;
        public readonly float StrokeDashOffset;
        public readonly SKPathFillType FillRule;

        public InheritedStyle(
            HyperPaintRef fill,
            HyperPaintRef stroke,
            float strokeWidth,
            SKStrokeCap strokeCap,
            SKStrokeJoin strokeJoin,
            float fillOpacity,
            float strokeOpacity,
            float[]? strokeDashArray,
            float strokeDashOffset,
            SKPathFillType fillRule)
        {
            Fill = fill;
            Stroke = stroke;
            StrokeWidth = strokeWidth;
            StrokeCap = strokeCap;
            StrokeJoin = strokeJoin;
            FillOpacity = fillOpacity;
            StrokeOpacity = strokeOpacity;
            StrokeDashArray = strokeDashArray;
            StrokeDashOffset = strokeDashOffset;
            FillRule = fillRule;
        }

        public static InheritedStyle Default => new(
            new HyperPaintRef(HyperColorSource.Fixed, SKColors.Black, -1, 1f),
            HyperPaintRef.Unset,
            1f,
            SKStrokeCap.Butt,
            SKStrokeJoin.Miter,
            1f,
            1f,
            null,
            0f,
            SKPathFillType.Winding);
    }

    public static HyperCompiledSlice? TryCompile(
        IReadOnlyList<XElement> elements,
        IReadOnlyList<string> tokens,
        IReadOnlyList<XElement> templateDefs,
        out string? failureReason)
    {
        var tokenMap = new Dictionary<string, int>(tokens.Count, StringComparer.Ordinal);
        for (var i = 0; i < tokens.Count; i++)
            tokenMap[tokens[i]] = i;

        var defsById = new Dictionary<string, XElement>(templateDefs.Count, StringComparer.Ordinal);
        for (var i = 0; i < templateDefs.Count; i++)
        {
            var d = templateDefs[i];
            if (d.Attribute("data-zone") != null || d.Attribute("data-path") != null)
                continue;
            var id = (string?)d.Attribute("id");
            if (!string.IsNullOrEmpty(id))
                defsById[id] = d;
        }

        var ctx = new CompileContext(tokenMap, defsById);
        var referencedIds = CollectReferencedDefIds(elements, defsById);
        foreach (var id in referencedIds)
        {
            if (!TryEnsureDefCompiled(id, ctx, out failureReason))
            {
                ctx.DisposeOwnedResources();
                return null;
            }
        }

        var ops = new List<HyperDrawOp>(elements.Count * 2);
        for (var i = 0; i < elements.Count; i++)
        {
            if (!EmitElement(elements[i], InheritedStyle.Default, ctx, ops, out failureReason))
            {
                CompileContext.DisposeOps(ops);
                ctx.DisposeOwnedResources();
                return null;
            }
        }

        failureReason = null;
        return new HyperCompiledSlice(
            ops.ToArray(),
            ctx.DefPaints.Count == 0 ? null : ctx.DefPaints.ToArray(),
            ctx.Filters.Count == 0 ? null : ctx.Filters.ToArray(),
            ctx.ClipPaths.Count == 0 ? null : ctx.ClipPaths.ToArray(),
            ctx.Masks.Count == 0 ? null : ctx.Masks.ToArray());
    }

    private static List<string> CollectReferencedDefIds(
        IReadOnlyList<XElement> roots,
        Dictionary<string, XElement> defsById)
    {
        var ordered = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < roots.Count; i++)
            Visit(roots[i]);

        return ordered;

        void Visit(XElement el)
        {
            foreach (var node in el.DescendantsAndSelf())
            {
                CheckUrlAttr(node, "fill");
                CheckUrlAttr(node, "stroke");
                CheckUrlAttr(node, "filter");
                CheckUrlAttr(node, "clip-path");
                CheckUrlAttr(node, "mask");
            }
        }

        void CheckUrlAttr(XElement node, string attrName)
        {
            var val = (string?)node.Attribute(attrName);
            if (val != null && TryExtractUrlId(val, out var id) && seen.Add(id))
            {
                if (defsById.TryGetValue(id, out var defEl))
                    Visit(defEl);
                ordered.Add(id);
            }
        }
    }

    private static bool TryEnsureDefCompiled(string id, CompileContext ctx, out string? failureReason)
    {
        if (ctx.DefPaintMap.ContainsKey(id) ||
            ctx.FilterMap.ContainsKey(id) ||
            ctx.ClipPathMap.ContainsKey(id) ||
            ctx.MaskMap.ContainsKey(id))
        {
            failureReason = null;
            return true;
        }

        if (!ctx.DefsById.TryGetValue(id, out var defEl))
        {
            if (TryCompileBuiltInFilter(id, ctx))
            {
                failureReason = null;
                return true;
            }
            failureReason = $"référence url(#{id}) introuvable dans <defs>";
            return false;
        }

        if (!ctx.Visiting.Add(id))
        {
            failureReason = $"cycle détecté dans <defs> autour de '{id}'";
            return false;
        }

        var name = defEl.Name.LocalName;
        bool ok;
        if (name is "linearGradient" or "radialGradient")
        {
            var hasHref = false;
            for (var a = defEl.FirstAttribute; a != null; a = a.NextAttribute)
            {
                if (a.Name.LocalName == "href") { hasHref = true; break; }
            }
            if (hasHref)
            {
                failureReason = $"<{name} id=\"{id}\"> dans <defs> utilise href/xlink:href (héritage non supporté)";
                ok = false;
            }
            else if (defEl.Attribute("gradientTransform") != null)
            {
                failureReason = $"<{name} id=\"{id}\"> dans <defs> utilise gradientTransform (non supporté)";
                ok = false;
            }
            else if (TryParseGradientDef(defEl, name == "linearGradient", ctx.TokenMap, out var entry, out failureReason))
            {
                ctx.DefPaintMap[id] = ctx.DefPaints.Count;
                ctx.DefPaints.Add(entry);
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        else if (name == "filter")
        {
            if (TryParseFilterDef(defEl, id, ctx.TokenMap, out var spec, out failureReason))
            {
                ctx.FilterMap[id] = ctx.Filters.Count;
                ctx.Filters.Add(spec);
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        else if (name == "clipPath")
        {
            if (TryParseClipPathDef(defEl, id, out var clipPath, out failureReason))
            {
                ctx.ClipPathMap[id] = ctx.ClipPaths.Count;
                ctx.ClipPaths.Add(clipPath);
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        else if (name == "pattern")
        {
            if (TryParsePatternDef(defEl, id, ctx, out var patEntry, out failureReason))
            {
                ctx.DefPaintMap[id] = ctx.DefPaints.Count;
                ctx.DefPaints.Add(patEntry);
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        else if (name == "mask")
        {
            if (TryParseMaskDef(defEl, id, ctx, out var maskDef, out failureReason))
            {
                ctx.MaskMap[id] = ctx.Masks.Count;
                ctx.Masks.Add(maskDef);
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        else
        {
            failureReason = $"élément <{name} id=\"{id}\"> dans <defs> non supporté";
            ok = false;
        }

        ctx.Visiting.Remove(id);
        return ok;
    }

    private static bool TryCompileBuiltInFilter(string id, CompileContext ctx)
    {
        if (id == "skeumo-shadow")
        {
            ctx.FilterMap[id] = ctx.Filters.Count;
            ctx.Filters.Add(new HyperFilterSpec(HyperFilterOpKind.DropShadow, 0f, 3f, 2.23f, 2.23f,
                new HyperPaintRef(HyperColorSource.Fixed, new SKColor(0, 0, 0, 128), -1, 1f)));
            return true;
        }
        if (id == "skeumo-glow")
        {
            var tok = ctx.TokenMap.TryGetValue("SukiPrimaryColor", out var gi)
                ? new HyperPaintRef(HyperColorSource.Token, default, gi, 0.6f)
                : new HyperPaintRef(HyperColorSource.Fixed, new SKColor(0x7F, 0xA3, 0xFF, 153), -1, 1f);
            ctx.FilterMap[id] = ctx.Filters.Count;
            ctx.Filters.Add(new HyperFilterSpec(HyperFilterOpKind.DropShadow, 0f, 0f, 2.23f, 2.23f, tok));
            return true;
        }
        if (id == "skeumo-blur")
        {
            ctx.FilterMap[id] = ctx.Filters.Count;
            ctx.Filters.Add(new HyperFilterSpec(HyperFilterOpKind.Blur, 0f, 0f, 2.23f, 2.23f, HyperPaintRef.Unset));
            return true;
        }
        return false;
    }

    private static bool TryParseFilterDef(
        XElement el,
        string id,
        Dictionary<string, int> tokenMap,
        out HyperFilterSpec spec,
        out string? failureReason)
    {
        spec = default;
        if (id == "skeumo-shadow")
        {
            spec = new HyperFilterSpec(HyperFilterOpKind.DropShadow, 0f, 3f, 2.23f, 2.23f,
                new HyperPaintRef(HyperColorSource.Fixed, new SKColor(0, 0, 0, 128), -1, 1f));
            failureReason = null;
            return true;
        }
        if (id == "skeumo-glow")
        {
            var tok = tokenMap.TryGetValue("SukiPrimaryColor", out var gi)
                ? new HyperPaintRef(HyperColorSource.Token, default, gi, 0.6f)
                : new HyperPaintRef(HyperColorSource.Fixed, new SKColor(0x7F, 0xA3, 0xFF, 153), -1, 1f);
            spec = new HyperFilterSpec(HyperFilterOpKind.DropShadow, 0f, 0f, 2.23f, 2.23f, tok);
            failureReason = null;
            return true;
        }
        if (id == "skeumo-blur")
        {
            spec = new HyperFilterSpec(HyperFilterOpKind.Blur, 0f, 0f, 2.23f, 2.23f, HyperPaintRef.Unset);
            failureReason = null;
            return true;
        }

        var children = new List<XElement>();
        foreach (var child in el.Elements())
            children.Add(child);

        if (children.Count == 1 && children[0].Name.LocalName == "feGaussianBlur")
        {
            if (!TryParseStdDeviation(children[0], out var sx, out var sy))
            {
                failureReason = $"<filter id=\"{id}\"> : stdDeviation invalide";
                return false;
            }
            spec = new HyperFilterSpec(HyperFilterOpKind.Blur, 0f, 0f, sx, sy, HyperPaintRef.Unset);
            failureReason = null;
            return true;
        }

        float blurX = 0f, blurY = 0f;
        float dx = 0f, dy = 0f;
        var hasBlur = false;
        var hasFlood = false;
        var floodRef = new HyperPaintRef(HyperColorSource.Fixed, SKColors.Black, -1, 1f);
        var isInnerShadow = false;
        var hasMergeWithSource = false;

        foreach (var child in children)
        {
            var tag = child.Name.LocalName;
            switch (tag)
            {
                case "feGaussianBlur":
                    if (!TryParseStdDeviation(child, out blurX, out blurY))
                    {
                        failureReason = $"<filter id=\"{id}\"> : stdDeviation invalide";
                        return false;
                    }
                    hasBlur = true;
                    break;
                case "feOffset":
                    dx = FloatAttr(child, "dx");
                    dy = FloatAttr(child, "dy");
                    break;
                case "feFlood":
                {
                    var fc = (string?)child.Attribute("flood-color") ?? "#000000";
                    var fo = FloatAttr(child, "flood-opacity", 1f);
                    if (!TryParsePaint(fc, Math.Clamp(fo, 0f, 1f), tokenMap, null, out floodRef) ||
                        floodRef.Source is not (HyperColorSource.Fixed or HyperColorSource.Token))
                    {
                        failureReason = $"<filter id=\"{id}\"> : flood-color '{fc}' non supporté";
                        return false;
                    }
                    hasFlood = true;
                    break;
                }
                case "feComposite":
                {
                    var opAttr = (string?)child.Attribute("operator") ?? "over";
                    var in2Attr = (string?)child.Attribute("in2");
                    if (opAttr == "out" && in2Attr == "SourceAlpha")
                        isInnerShadow = true;
                    else if (opAttr != "in")
                    {
                        failureReason = $"<filter id=\"{id}\"> : feComposite operator=\"{opAttr}\" non supporté";
                        return false;
                    }
                    break;
                }
                case "feMerge":
                {
                    foreach (var mn in child.Elements())
                    {
                        if (mn.Name.LocalName == "feMergeNode" && (string?)mn.Attribute("in") == "SourceGraphic")
                            hasMergeWithSource = true;
                    }
                    break;
                }
                default:
                    failureReason = $"<filter id=\"{id}\"> : primitive <{tag}> non supportée";
                    return false;
            }
        }

        if (hasBlur && hasFlood && hasMergeWithSource)
        {
            spec = new HyperFilterSpec(
                isInnerShadow ? HyperFilterOpKind.InnerShadow : HyperFilterOpKind.DropShadow,
                dx, dy, blurX, blurY, floodRef);
            failureReason = null;
            return true;
        }

        failureReason = $"<filter id=\"{id}\"> : combinaison de primitives non supportée";
        return false;
    }

    private static bool TryParseStdDeviation(XElement blurEl, out float sx, out float sy)
    {
        var raw = (string?)blurEl.Attribute("stdDeviation");
        if (string.IsNullOrWhiteSpace(raw))
        {
            sx = sy = 0f;
            return true;
        }
        var parts = raw.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1 && TryFloat(parts[0], out sx))
        {
            sy = sx;
            return true;
        }
        if (parts.Length >= 2 && TryFloat(parts[0], out sx) && TryFloat(parts[1], out sy))
            return true;
        sx = sy = 0f;
        return false;
    }

    private static bool TryParseClipPathDef(XElement el, string id, out SKPath combined, out string? failureReason)
    {
        var units = (string?)el.Attribute("clipPathUnits");
        if (units != null && !units.Equals("userSpaceOnUse", StringComparison.OrdinalIgnoreCase))
        {
            combined = null!;
            failureReason = $"<clipPath id=\"{id}\"> : clipPathUnits='{units}' non supporté";
            return false;
        }

#pragma warning disable CS0618
        combined = new SKPath();
        if (((string?)el.Attribute("clip-rule"))?.Equals("evenodd", StringComparison.OrdinalIgnoreCase) == true)
            combined.FillType = SKPathFillType.EvenOdd;

        foreach (var child in el.Elements())
        {
            SKPath? childPath = null;
            var tag = child.Name.LocalName;
            switch (tag)
            {
                case "rect":
                {
                    var x = FloatAttr(child, "x");
                    var y = FloatAttr(child, "y");
                    var w = FloatAttr(child, "width");
                    var h = FloatAttr(child, "height");
                    var rx = FloatAttr(child, "rx", float.NaN);
                    var ry = FloatAttr(child, "ry", float.NaN);
                    if (float.IsNaN(rx) && float.IsNaN(ry)) { rx = 0f; ry = 0f; }
                    else if (float.IsNaN(rx)) rx = ry;
                    else if (float.IsNaN(ry)) ry = rx;
                    childPath = new SKPath();
                    var rect = new SKRect(x, y, x + w, y + h);
                    if (rx > 0f || ry > 0f)
                        childPath.AddRoundRect(rect, rx, ry);
                    else
                        childPath.AddRect(rect);
                    break;
                }
                case "circle":
                {
                    var cx = FloatAttr(child, "cx");
                    var cy = FloatAttr(child, "cy");
                    var r = FloatAttr(child, "r");
                    childPath = new SKPath();
                    childPath.AddCircle(cx, cy, r);
                    break;
                }
                case "ellipse":
                {
                    var cx = FloatAttr(child, "cx");
                    var cy = FloatAttr(child, "cy");
                    var rx = FloatAttr(child, "rx");
                    var ry = FloatAttr(child, "ry");
                    childPath = new SKPath();
                    childPath.AddOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry));
                    break;
                }
                case "path":
                {
                    var d = (string?)child.Attribute("d");
                    if (!string.IsNullOrWhiteSpace(d))
                    {
                        childPath = SKPath.ParseSvgPathData(d);
                        if (childPath == null)
                        {
                            combined.Dispose();
                            failureReason = $"<clipPath id=\"{id}\"> : <path> d illisible";
                            return false;
                        }
                    }
                    break;
                }
                case "polygon":
                {
                    var pts = (string?)child.Attribute("points");
                    if (!string.IsNullOrWhiteSpace(pts) && !TryParsePoints(pts, true, out childPath))
                    {
                        combined.Dispose();
                        failureReason = $"<clipPath id=\"{id}\"> : <polygon> points invalide";
                        return false;
                    }
                    break;
                }
                default:
                    combined.Dispose();
                    failureReason = $"<clipPath id=\"{id}\"> : enfant <{tag}> non supporté";
                    return false;
            }

            if (childPath != null)
            {
                var tf = (string?)child.Attribute("transform");
                if (tf != null)
                {
                    if (!TryParseTransform(tf, out var m))
                    {
                        childPath.Dispose();
                        combined.Dispose();
                        failureReason = $"<clipPath id=\"{id}\"> : transform '{tf}' non supporté";
                        return false;
                    }
                    childPath.Transform(m);
                }
                combined.AddPath(childPath);
                childPath.Dispose();
            }
        }
#pragma warning restore CS0618

        failureReason = null;
        return true;
    }

    private static bool TryParsePatternDef(
        XElement el,
        string id,
        CompileContext ctx,
        out HyperPaintEntry entry,
        out string? failureReason)
    {
        entry = null!;
        var units = (string?)el.Attribute("patternUnits");
        if (units == null || !units.Equals("userSpaceOnUse", StringComparison.OrdinalIgnoreCase))
        {
            failureReason = $"<pattern id=\"{id}\"> : patternUnits='{units ?? "(default)"}' non supporté (userSpaceOnUse requis)";
            return false;
        }
        var contentUnits = (string?)el.Attribute("patternContentUnits");
        if (contentUnits != null && !contentUnits.Equals("userSpaceOnUse", StringComparison.OrdinalIgnoreCase))
        {
            failureReason = $"<pattern id=\"{id}\"> : patternContentUnits='{contentUnits}' non supporté";
            return false;
        }

        var x = FloatAttr(el, "x");
        var y = FloatAttr(el, "y");
        var w = FloatAttr(el, "width");
        var h = FloatAttr(el, "height");
        if (w <= 0f || h <= 0f)
        {
            failureReason = $"<pattern id=\"{id}\"> : width/height invalides ({w}x{h})";
            return false;
        }

        var hasTf = false;
        var tf = SKMatrix.Identity;
        var tfAttr = (string?)el.Attribute("patternTransform");
        if (tfAttr != null)
        {
            if (!TryParseTransform(tfAttr, out tf))
            {
                failureReason = $"<pattern id=\"{id}\"> : patternTransform '{tfAttr}' non supporté";
                return false;
            }
            hasTf = true;
        }

        var patOps = new List<HyperDrawOp>();
        foreach (var child in el.Elements())
        {
            if (!EmitElement(child, InheritedStyle.Default, ctx, patOps, out failureReason))
            {
                CompileContext.DisposeOps(patOps);
                return false;
            }
        }

        entry = new HyperPaintEntry
        {
            Kind = HyperPaintEntryKind.Pattern,
            PatternX = x,
            PatternY = y,
            PatternWidth = w,
            PatternHeight = h,
            HasPatternTransform = hasTf,
            PatternTransform = tf,
            PatternOps = patOps.ToArray()
        };
        failureReason = null;
        return true;
    }

    private static bool TryParseMaskDef(
        XElement el,
        string id,
        CompileContext ctx,
        out HyperMaskDef maskDef,
        out string? failureReason)
    {
        maskDef = null!;
        var units = (string?)el.Attribute("maskUnits");
        if (units != null && !units.Equals("userSpaceOnUse", StringComparison.OrdinalIgnoreCase))
        {
            failureReason = $"<mask id=\"{id}\"> : maskUnits='{units}' non supporté (userSpaceOnUse requis)";
            return false;
        }
        var contentUnits = (string?)el.Attribute("maskContentUnits");
        if (contentUnits != null && !contentUnits.Equals("userSpaceOnUse", StringComparison.OrdinalIgnoreCase))
        {
            failureReason = $"<mask id=\"{id}\"> : maskContentUnits='{contentUnits}' non supporté";
            return false;
        }

        var hasBounds = el.Attribute("width") != null && el.Attribute("height") != null;
        var bounds = SKRect.Empty;
        if (hasBounds)
        {
            var x = FloatAttr(el, "x");
            var y = FloatAttr(el, "y");
            var w = FloatAttr(el, "width");
            var h = FloatAttr(el, "height");
            if (w <= 0f || h <= 0f)
            {
                failureReason = $"<mask id=\"{id}\"> : width/height invalides ({w}x{h})";
                return false;
            }
            bounds = new SKRect(x, y, x + w, y + h);
        }

        var maskOps = new List<HyperDrawOp>();
        foreach (var child in el.Elements())
        {
            if (!EmitElement(child, InheritedStyle.Default, ctx, maskOps, out failureReason))
            {
                CompileContext.DisposeOps(maskOps);
                return false;
            }
        }

        maskDef = new HyperMaskDef
        {
            HasBounds = hasBounds,
            Bounds = bounds,
            Ops = maskOps.ToArray()
        };
        failureReason = null;
        return true;
    }

    private static bool TryParseGradientDef(
        XElement el,
        bool isLinear,
        Dictionary<string, int> tokenMap,
        out HyperPaintEntry entry,
        out string? failureReason)
    {
        entry = null!;
        var gradTag = el.Name.LocalName;
        var gradId = (string?)el.Attribute("id") ?? "?";
        var units = ((string?)el.Attribute("gradientUnits"))?.Trim();
        var isRelative = units == null || units.Equals("objectBoundingBox", StringComparison.OrdinalIgnoreCase);
        var spread = ((string?)el.Attribute("spreadMethod"))?.Trim()?.ToLowerInvariant() switch
        {
            "reflect" => SKShaderTileMode.Mirror,
            "repeat" => SKShaderTileMode.Repeat,
            _ => SKShaderTileMode.Clamp
        };

        var stops = new List<(float Offset, HyperPaintRef Paint)>();
        var hasDynamicStops = false;
        foreach (var child in el.Elements())
        {
            if (child.Name.LocalName != "stop") continue;
            var offAttr = (string?)child.Attribute("offset");
            float off = 0f;
            if (offAttr != null)
            {
                var os = offAttr.Trim();
                if (os.EndsWith("%", StringComparison.Ordinal))
                {
                    if (!TryFloat(os.Substring(0, os.Length - 1), out off))
                    {
                        failureReason = $"<{gradTag} id=\"{gradId}\"> : offset '{offAttr}' invalide";
                        return false;
                    }
                    off *= 0.01f;
                }
                else if (!TryFloat(os, out off))
                {
                    failureReason = $"<{gradTag} id=\"{gradId}\"> : offset '{offAttr}' invalide";
                    return false;
                }
            }

            var colAttr = (string?)child.Attribute("stop-color") ?? "#000000";
            if (!TryParsePaint(colAttr, 1f, tokenMap, null, out var stopPaint) ||
                stopPaint.Source is not (HyperColorSource.Fixed or HyperColorSource.Token))
            {
                failureReason = $"<{gradTag} id=\"{gradId}\"> : stop-color '{colAttr}' non supporté";
                return false;
            }
            var sopAttr = (string?)child.Attribute("stop-opacity");
            if (sopAttr != null)
            {
                if (!TryFloat(sopAttr, out var sop))
                {
                    failureReason = $"<{gradTag} id=\"{gradId}\"> : stop-opacity '{sopAttr}' invalide";
                    return false;
                }
                stopPaint = ApplyOpacity(stopPaint, Math.Clamp(sop, 0f, 1f));
            }
            if (stopPaint.Source == HyperColorSource.Token)
                hasDynamicStops = true;
            stops.Add((Math.Clamp(off, 0f, 1f), stopPaint));
        }

        if (stops.Count == 0)
        {
            failureReason = $"<{gradTag} id=\"{gradId}\"> : aucun <stop>";
            return false;
        }

        var offsets = new float[stops.Count];
        SKColor[] fixedColors;
        HyperPaintRef[]? stopRefs = null;
        if (hasDynamicStops)
        {
            fixedColors = Array.Empty<SKColor>();
            stopRefs = new HyperPaintRef[stops.Count];
            for (var i = 0; i < stops.Count; i++)
            {
                offsets[i] = stops[i].Offset;
                stopRefs[i] = stops[i].Paint;
            }
        }
        else
        {
            fixedColors = new SKColor[stops.Count];
            for (var i = 0; i < stops.Count; i++)
            {
                offsets[i] = stops[i].Offset;
                var sp = stops[i].Paint;
                var c = sp.Fixed;
                fixedColors[i] = new SKColor(c.Red, c.Green, c.Blue, (byte)Math.Round(c.Alpha * sp.Opacity));
            }
        }

        if (isLinear)
        {
            if (!TryGradientCoord((string?)el.Attribute("x1"), 0f, out var x1) ||
                !TryGradientCoord((string?)el.Attribute("y1"), 0f, out var y1) ||
                !TryGradientCoord((string?)el.Attribute("x2"), isRelative ? 1f : 0f, out var x2) ||
                !TryGradientCoord((string?)el.Attribute("y2"), 0f, out var y2))
            {
                failureReason = $"<{gradTag} id=\"{gradId}\"> : coordonnées x1/y1/x2/y2 invalides";
                return false;
            }

            entry = new HyperPaintEntry
            {
                Kind = HyperPaintEntryKind.LinearGradient,
                IsRelative = isRelative,
                Start = new Point(x1, y1),
                End = new Point(x2, y2),
                TileMode = spread,
                Colors = fixedColors,
                StopRefs = stopRefs,
                Offsets = offsets
            };
            failureReason = null;
            return true;
        }

        if (!TryGradientCoord((string?)el.Attribute("cx"), 0.5f, out var cx) ||
            !TryGradientCoord((string?)el.Attribute("cy"), 0.5f, out var cy) ||
            !TryGradientCoord((string?)el.Attribute("r"), 0.5f, out var r) ||
            !TryGradientCoord((string?)el.Attribute("fx"), cx, out var fx) ||
            !TryGradientCoord((string?)el.Attribute("fy"), cy, out var fy))
        {
            failureReason = $"<{gradTag} id=\"{gradId}\"> : coordonnées cx/cy/r/fx/fy invalides";
            return false;
        }

        entry = new HyperPaintEntry
        {
            Kind = HyperPaintEntryKind.RadialGradient,
            IsRelative = isRelative,
            Center = new Point(cx, cy),
            Origin = new Point(fx, fy),
            RadiusX = r,
            RadiusY = r,
            TileMode = spread,
            Colors = fixedColors,
            StopRefs = stopRefs,
            Offsets = offsets
        };
        failureReason = null;
        return true;
    }

    private static bool TryGradientCoord(string? raw, float fallback, out float value)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            value = fallback;
            return true;
        }
        var s = raw.Trim();
        if (s.EndsWith("%", StringComparison.Ordinal))
        {
            if (!TryFloat(s.Substring(0, s.Length - 1), out value)) return false;
            value *= 0.01f;
            return true;
        }
        return TryFloat(s, out value);
    }

    private static bool EmitElement(
        XElement el,
        in InheritedStyle parent,
        CompileContext ctx,
        List<HyperDrawOp> ops,
        out string? failureReason)
    {
        var tag = el.Name.LocalName;
        if (tag is "defs" or "title" or "desc" or "metadata")
        {
            failureReason = null;
            return true;
        }

        var fillOpacity = parent.FillOpacity;
        var fopAttr = (string?)el.Attribute("fill-opacity");
        if (fopAttr != null && !TryFloat(fopAttr, out fillOpacity))
        {
            failureReason = $"<{tag}> : fill-opacity '{fopAttr}' invalide";
            return false;
        }

        var strokeOpacity = parent.StrokeOpacity;
        var sopAttr = (string?)el.Attribute("stroke-opacity");
        if (sopAttr != null && !TryFloat(sopAttr, out strokeOpacity))
        {
            failureReason = $"<{tag}> : stroke-opacity '{sopAttr}' invalide";
            return false;
        }

        var fill = parent.Fill;
        var fillAttr = (string?)el.Attribute("fill");
        if (fillAttr != null)
        {
            if (!TryParsePaint(fillAttr, 1f, ctx.TokenMap, ctx.DefPaintMap, out fill))
            {
                failureReason = $"<{tag}> : fill '{fillAttr}' non supporté";
                return false;
            }
        }

        var stroke = parent.Stroke;
        var strokeAttr = (string?)el.Attribute("stroke");
        if (strokeAttr != null)
        {
            if (!TryParsePaint(strokeAttr, 1f, ctx.TokenMap, ctx.DefPaintMap, out stroke))
            {
                failureReason = $"<{tag}> : stroke '{strokeAttr}' non supporté";
                return false;
            }
        }

        var strokeWidth = parent.StrokeWidth;
        var swAttr = (string?)el.Attribute("stroke-width");
        if (swAttr != null && !TryFloat(swAttr, out strokeWidth))
        {
            failureReason = $"<{tag}> : stroke-width '{swAttr}' invalide";
            return false;
        }

        var strokeCap = parent.StrokeCap;
        var capAttr = (string?)el.Attribute("stroke-linecap");
        if (capAttr != null)
        {
            strokeCap = capAttr switch
            {
                "butt" => SKStrokeCap.Butt,
                "round" => SKStrokeCap.Round,
                "square" => SKStrokeCap.Square,
                _ => SKStrokeCap.Butt
            };
        }

        var strokeJoin = parent.StrokeJoin;
        var joinAttr = (string?)el.Attribute("stroke-linejoin");
        if (joinAttr != null)
        {
            strokeJoin = joinAttr switch
            {
                "miter" => SKStrokeJoin.Miter,
                "round" => SKStrokeJoin.Round,
                "bevel" => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Miter
            };
        }

        var opacity = 1f;
        var opAttr = (string?)el.Attribute("opacity");
        if (opAttr != null && !TryFloat(opAttr, out opacity))
        {
            failureReason = $"<{tag}> : opacity '{opAttr}' invalide";
            return false;
        }

        var filterIndex = -1;
        var filterAttr = (string?)el.Attribute("filter");
        if (filterAttr != null)
        {
            if (!TryExtractUrlId(filterAttr, out var filterId) || !ctx.FilterMap.TryGetValue(filterId, out filterIndex))
            {
                failureReason = $"<{tag}> : filter '{filterAttr}' non supporté";
                return false;
            }
        }

        var hasTransform = false;
        var matrix = SKMatrix.Identity;
        var tfAttr = (string?)el.Attribute("transform");
        if (tfAttr != null)
        {
            if (!TryParseTransform(tfAttr, out matrix))
            {
                failureReason = $"<{tag}> : transform '{tfAttr}' non supporté";
                return false;
            }
            hasTransform = true;
        }

        var clipPathIndex = -1;
        var clipAttr = (string?)el.Attribute("clip-path");
        if (clipAttr != null)
        {
            if (!TryExtractUrlId(clipAttr, out var clipId) || !ctx.ClipPathMap.TryGetValue(clipId, out clipPathIndex))
            {
                failureReason = $"<{tag}> : clip-path '{clipAttr}' non supporté";
                return false;
            }
        }

        var maskIndex = -1;
        var maskAttr = (string?)el.Attribute("mask");
        if (maskAttr != null)
        {
            if (!TryExtractUrlId(maskAttr, out var maskId) || !ctx.MaskMap.TryGetValue(maskId, out maskIndex))
            {
                failureReason = $"<{tag}> : mask '{maskAttr}' non supporté";
                return false;
            }
        }

        if (el.Attribute("style") is { } styleAttr)
        {
            failureReason = $"<{tag}> : attribut style=\"{styleAttr.Value}\" non supporté";
            return false;
        }

        var strokeDashArray = parent.StrokeDashArray;
        var dashAttr = (string?)el.Attribute("stroke-dasharray");
        if (dashAttr != null)
        {
            if (!TryParseDashArray(dashAttr, out strokeDashArray))
            {
                failureReason = $"<{tag}> : stroke-dasharray '{dashAttr}' invalide";
                return false;
            }
        }

        var strokeDashOffset = parent.StrokeDashOffset;
        var dashOffAttr = (string?)el.Attribute("stroke-dashoffset");
        if (dashOffAttr != null && !TryFloat(dashOffAttr, out strokeDashOffset))
        {
            failureReason = $"<{tag}> : stroke-dashoffset '{dashOffAttr}' invalide";
            return false;
        }

        var fillRule = parent.FillRule;
        var fillRuleAttr = (string?)el.Attribute("fill-rule");
        if (fillRuleAttr != null)
        {
            fillRule = fillRuleAttr.Trim().ToLowerInvariant() switch
            {
                "evenodd" => SKPathFillType.EvenOdd,
                _ => SKPathFillType.Winding
            };
        }

        if (tag == "g")
        {
            var needsPush = hasTransform || opacity < 0.999f || filterIndex >= 0 || clipPathIndex >= 0 || maskIndex >= 0;
            var pushIdx = -1;
            if (needsPush)
            {
                pushIdx = ops.Count;
                ops.Add(new HyperDrawOp
                {
                    Shape = HyperShapeKind.GroupPush,
                    Opacity = opacity,
                    HasTransform = hasTransform,
                    Transform = matrix,
                    FilterIndex = filterIndex,
                    ClipPathIndex = clipPathIndex,
                    MaskIndex = maskIndex
                });
            }

            var childStyle = new InheritedStyle(fill, stroke, strokeWidth, strokeCap, strokeJoin, fillOpacity, strokeOpacity, strokeDashArray, strokeDashOffset, fillRule);
            foreach (var child in el.Elements())
            {
                if (!EmitElement(child, childStyle, ctx, ops, out failureReason))
                    return false;
            }

            if (needsPush)
            {
                var union = SKRect.Empty;
                var hasBounds = false;
                for (var j = pushIdx + 1; j < ops.Count; j++)
                {
                    var b = ops[j].ObjectBounds;
                    if (b.IsEmpty) continue;
                    union = hasBounds ? SKRect.Union(union, b) : b;
                    hasBounds = true;
                }
                ops[pushIdx].ObjectBounds = union;

                ops.Add(new HyperDrawOp
                {
                    Shape = HyperShapeKind.GroupPop,
                    Opacity = opacity,
                    HasTransform = hasTransform,
                    FilterIndex = filterIndex,
                    ClipPathIndex = clipPathIndex,
                    MaskIndex = maskIndex
                });
            }
            failureReason = null;
            return true;
        }

        var effectiveFill = ApplyOpacity(fill, fillOpacity);
        var effectiveStroke = ApplyOpacity(stroke, strokeOpacity);

        var op = new HyperDrawOp
        {
            Fill = effectiveFill,
            Stroke = effectiveStroke,
            StrokeWidth = strokeWidth,
            StrokeCap = strokeCap,
            StrokeJoin = strokeJoin,
            StrokeDashArray = strokeDashArray,
            StrokeDashOffset = strokeDashOffset,
            Opacity = opacity,
            HasTransform = hasTransform,
            Transform = matrix,
            FilterIndex = filterIndex,
            ClipPathIndex = clipPathIndex,
            MaskIndex = maskIndex
        };

        switch (tag)
        {
            case "rect":
            {
                var x = FloatAttr(el, "x");
                var y = FloatAttr(el, "y");
                var w = FloatAttr(el, "width");
                var h = FloatAttr(el, "height");
                var rx = FloatAttr(el, "rx", float.NaN);
                var ry = FloatAttr(el, "ry", float.NaN);
                if (float.IsNaN(rx) && float.IsNaN(ry)) { rx = 0f; ry = 0f; }
                else if (float.IsNaN(rx)) rx = ry;
                else if (float.IsNaN(ry)) ry = rx;
                op.Shape = HyperShapeKind.Rect;
                op.Rect = new SKRect(x, y, x + w, y + h);
                op.ObjectBounds = op.Rect;
                op.Rx = rx;
                op.Ry = ry;
                ops.Add(op);
                failureReason = null;
                return true;
            }
            case "circle":
            {
                var cx = FloatAttr(el, "cx");
                var cy = FloatAttr(el, "cy");
                var r = FloatAttr(el, "r");
                op.Shape = HyperShapeKind.Circle;
                op.X1 = cx;
                op.Y1 = cy;
                op.Rx = r;
                op.ObjectBounds = new SKRect(cx - r, cy - r, cx + r, cy + r);
                ops.Add(op);
                failureReason = null;
                return true;
            }
            case "ellipse":
            {
                var cx = FloatAttr(el, "cx");
                var cy = FloatAttr(el, "cy");
                var rx = FloatAttr(el, "rx");
                var ry = FloatAttr(el, "ry");
                op.Shape = HyperShapeKind.Ellipse;
                op.Rect = new SKRect(cx - rx, cy - ry, cx + rx, cy + ry);
                op.ObjectBounds = op.Rect;
                ops.Add(op);
                failureReason = null;
                return true;
            }
            case "line":
            {
                var x1 = FloatAttr(el, "x1");
                var y1 = FloatAttr(el, "y1");
                var x2 = FloatAttr(el, "x2");
                var y2 = FloatAttr(el, "y2");
                op.Fill = HyperPaintRef.Unset;
                op.Shape = HyperShapeKind.Line;
                op.X1 = x1;
                op.Y1 = y1;
                op.X2 = x2;
                op.Y2 = y2;
                op.ObjectBounds = new SKRect(Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(x1, x2), Math.Max(y1, y2));
                ops.Add(op);
                failureReason = null;
                return true;
            }
            case "path":
            {
                var d = (string?)el.Attribute("d");
                if (string.IsNullOrWhiteSpace(d)) { failureReason = null; return true; }
                var path = SKPath.ParseSvgPathData(d);
                if (path == null)
                {
                    failureReason = "<path> : attribut d illisible";
                    return false;
                }
                path.FillType = fillRule;
                op.Shape = HyperShapeKind.Path;
                op.Path = path;
                op.ObjectBounds = path.TightBounds;
                ops.Add(op);
                failureReason = null;
                return true;
            }
            case "polygon":
            case "polyline":
            {
                var ptsText = (string?)el.Attribute("points");
                if (string.IsNullOrWhiteSpace(ptsText)) { failureReason = null; return true; }
                if (!TryParsePoints(ptsText, tag == "polygon", out var path))
                {
                    failureReason = $"<{tag}> : attribut points invalide";
                    return false;
                }
                if (tag == "polygon")
                    path.FillType = fillRule;
                op.Shape = HyperShapeKind.Path;
                op.Path = path;
                op.ObjectBounds = path.TightBounds;
                ops.Add(op);
                failureReason = null;
                return true;
            }
            default:
                failureReason = $"balise <{tag}> non supportée par HyperVectorCompiler";
                return false;
        }
    }

    private static bool TryExtractUrlId(string raw, out string id)
    {
        var s = raw.AsSpan().Trim();
        if (s.StartsWith("url(".AsSpan(), StringComparison.Ordinal) && s.EndsWith(")".AsSpan(), StringComparison.Ordinal))
        {
            var inner = s.Slice(4, s.Length - 5).Trim();
            if (inner.Length >= 2 && ((inner[0] == '\'' && inner[^1] == '\'') || (inner[0] == '"' && inner[^1] == '"')))
                inner = inner.Slice(1, inner.Length - 2).Trim();
            if (inner.Length > 1 && inner[0] == '#')
            {
                id = inner.Slice(1).ToString();
                return true;
            }
        }
        id = string.Empty;
        return false;
    }

    private static HyperPaintRef ApplyOpacity(HyperPaintRef p, float opacity) =>
        p.Source == HyperColorSource.None ? p : new HyperPaintRef(p.Source, p.Fixed, p.TokenIndex, p.Opacity * opacity);

    private static bool TryParsePaint(
        string raw,
        float opacity,
        Dictionary<string, int> tokenMap,
        Dictionary<string, int>? defPaintMap,
        out HyperPaintRef result)
    {
        var s = raw.Trim();
        if (s.Equals("none", StringComparison.OrdinalIgnoreCase) || s.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            result = HyperPaintRef.Unset;
            return true;
        }

        if (s.StartsWith("var(--", StringComparison.Ordinal) && s.EndsWith(")", StringComparison.Ordinal) && s.Length > 7)
        {
            var tokenName = s.Substring(6, s.Length - 7);
            if (!tokenMap.TryGetValue(tokenName, out var idx))
            {
                result = HyperPaintRef.Unset;
                return false;
            }
            result = new HyperPaintRef(HyperColorSource.Token, default, idx, opacity);
            return true;
        }

        if (s.StartsWith("url(#", StringComparison.Ordinal) && s.EndsWith(")", StringComparison.Ordinal) && s.Length > 6)
        {
            var defId = s.Substring(5, s.Length - 6);
            if (defPaintMap != null && defPaintMap.TryGetValue(defId, out var defIdx))
            {
                result = new HyperPaintRef(HyperColorSource.DefPaint, default, defIdx, opacity);
                return true;
            }
            result = HyperPaintRef.Unset;
            return false;
        }

        if (TryParseFixedColor(s, out var fixedColor))
        {
            result = new HyperPaintRef(HyperColorSource.Fixed, fixedColor, -1, opacity);
            return true;
        }

        result = HyperPaintRef.Unset;
        return false;
    }

    private static bool TryParseFixedColor(string raw, out SKColor skColor)
    {
        var s = raw.Trim();
        if (SKColor.TryParse(s, out skColor))
            return true;
        if (Color.TryParse(s, out var avColor))
        {
            skColor = avColor.ToSKColor();
            return true;
        }
        return false;
    }

    private static bool TryParsePoints(string text, bool close, out SKPath path)
    {
        var nums = text.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (nums.Length < 4 || (nums.Length & 1) != 0)
        {
            path = null!;
            return false;
        }

#pragma warning disable CS0618 // Compatibilité SkiaSharp 2.88 (Avalonia.Skia par défaut) et SkiaSharp 3.x
        var p = new SKPath();
        for (var i = 0; i < nums.Length; i += 2)
        {
            if (!TryFloat(nums[i], out var x) || !TryFloat(nums[i + 1], out var y))
            {
                p.Dispose();
                path = null!;
                return false;
            }
            if (i == 0) p.MoveTo(x, y);
            else p.LineTo(x, y);
        }
        if (close) p.Close();
#pragma warning restore CS0618
        path = p;
        return true;
    }

    internal static bool TryParseTransform(string text, out SKMatrix matrix)
    {
        matrix = SKMatrix.Identity;
        var matches = TransformRx.Matches(text);
        if (matches.Count == 0) return false;

        foreach (Match m in matches)
        {
            var fn = m.Groups[1].Value;
            var argsRaw = m.Groups[2].Value.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var args = new float[argsRaw.Length];
            for (var i = 0; i < argsRaw.Length; i++)
                if (!TryFloat(argsRaw[i], out args[i])) return false;

            SKMatrix step;
            switch (fn)
            {
                case "translate":
                    step = SKMatrix.CreateTranslation(args.Length >= 1 ? args[0] : 0f, args.Length >= 2 ? args[1] : 0f);
                    break;
                case "scale":
                    if (args.Length < 1) return false;
                    step = SKMatrix.CreateScale(args[0], args.Length >= 2 ? args[1] : args[0]);
                    break;
                case "rotate":
                    if (args.Length == 1)
                        step = SKMatrix.CreateRotationDegrees(args[0]);
                    else if (args.Length == 3)
                        step = SKMatrix.CreateRotationDegrees(args[0], args[1], args[2]);
                    else
                        return false;
                    break;
                case "matrix":
                    if (args.Length != 6) return false;
                    step = new SKMatrix(args[0], args[2], args[4], args[1], args[3], args[5], 0f, 0f, 1f);
                    break;
                default:
                    return false;
            }

            matrix = SKMatrix.Concat(matrix, step);
        }

        return true;
    }

    private static bool TryParseDashArray(string raw, out float[]? intervals)
    {
        var s = raw.Trim();
        if (s.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            intervals = null;
            return true;
        }

        var parts = s.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            intervals = null;
            return false;
        }

        var parsed = new float[parts.Length];
        var sum = 0f;
        for (var i = 0; i < parts.Length; i++)
        {
            if (!TryFloat(parts[i], out var v) || v < 0f)
            {
                intervals = null;
                return false;
            }
            parsed[i] = v;
            sum += v;
        }

        if (sum <= 0f)
        {
            intervals = null;
            return true;
        }

        // SVG spec: an odd-length dasharray is repeated twice to form an even-length pattern (required by SKPathEffect.CreateDash)
        if ((parsed.Length & 1) != 0)
        {
            var doubled = new float[parsed.Length * 2];
            parsed.CopyTo(doubled, 0);
            parsed.CopyTo(doubled, parsed.Length);
            intervals = doubled;
        }
        else
        {
            intervals = parsed;
        }
        return true;
    }

    private static float FloatAttr(XElement el, string name, float fallback = 0f)
    {
        var s = (string?)el.Attribute(name);
        return s != null && TryFloat(s, out var v) ? v : fallback;
    }

    private static bool TryFloat(string s, out float v) =>
        float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
}

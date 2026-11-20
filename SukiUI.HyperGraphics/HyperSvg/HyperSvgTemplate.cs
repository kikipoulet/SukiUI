using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia;

namespace SukiUI.HyperGraphics;

internal enum HyperSlotKind
{
    Static,
    Part,
    Window
}

internal sealed class HyperSliceSpec
{
    public HyperSlotKind Kind;
    public string? Name;
    public string Svg = "";
    public Rect EstimateBounds;
    public Point? Pivot;
    public Rect Window;
    public double Radius;
}

internal sealed class HyperTemplateSlot
{
    public HyperSlotKind Kind;
    public string? Name;
    public Rect EstimateBounds;
    public Point? Pivot;
    public Rect Window;
    public double Radius;
    public HyperCompiledSlice? Compiled;
    public string? FallbackReason;
}

/// <summary>
///     Parseur SVG déclaratif pour HyperSvg : même syntaxe LLM-friendly que SukiDrawingSVG
///     (var(--Cle), data-part, data-pivot, data-window, data-zone, data-path, filter="url(#skeumo-...)").
///     Boot quasi-nul sur le thread UI (ViewBox + Tokens par span scan), compilation XML + vectorielle
///     déportée en parallèle sur les threads workers lors du premier build.
/// </summary>
internal sealed class HyperSvgTemplate : IDisposable
{
    private static readonly Regex TokenRx = new(@"var\(--([A-Za-z0-9_-]+)\)", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> OpacityOf = new()
    {
        ["fill"] = "fill-opacity",
        ["stroke"] = "stroke-opacity",
        ["stop-color"] = "stop-opacity",
        ["flood-color"] = "flood-opacity"
    };

    private static int _nextId;

    private readonly object _compileGate = new();
    private string? _rawSvg;
    private readonly List<XElement> _defs = new();
    private readonly List<Slot> _slots = new();
    private readonly Dictionary<string, Rect> _zones = new();
    private readonly Dictionary<string, string> _paths = new();
    private HyperTemplateSlot[]? _compiledSlots;
    private bool _allSlotsCompiled;
    private bool _compiled;
    private bool _disposed;

    private sealed class Slot
    {
        public HyperSlotKind Kind;
        public string? Name;
        public string? PivotExpr;
        public readonly List<XElement> Elements = new();
    }

    public int Id { get; }
    public Rect ViewBox { get; }
    public Size NaturalSize
    {
        get
        {
            var w = ViewBox.Width + 2 * ViewBox.X;
            var h = ViewBox.Height + 2 * ViewBox.Y;
            return w > 0 && h > 0 ? new Size(w, h) : ViewBox.Size;
        }
    }

    public IReadOnlyList<string> Tokens { get; }

    internal IReadOnlyDictionary<string, string> Paths
    {
        get { EnsureCompiled(); return _paths; }
    }

    internal IReadOnlyList<HyperTemplateSlot> CompiledSlots
    {
        get { EnsureCompiled(); return _compiledSlots!; }
    }

    internal bool AllSlotsCompiled
    {
        get { EnsureCompiled(); return _allSlotsCompiled; }
    }

    public Rect Zone(string name)
    {
        EnsureCompiled();
        return _zones.TryGetValue(name, out var zone)
            ? zone
            : throw new KeyNotFoundException($"Pas de data-zone=\"{name}\" dans le SVG.");
    }

    public HyperSvgTemplate(string svg)
    {
        Id = System.Threading.Interlocked.Increment(ref _nextId);
        _rawSvg = svg ?? throw new ArgumentNullException(nameof(svg));
        ViewBox = ExtractViewBoxFast(svg);
        Tokens = ExtractTokensFast(svg);
    }

    internal void EnsureCompiled()
    {
        if (System.Threading.Volatile.Read(ref _compiled)) return;
        lock (_compileGate)
        {
            if (_compiled || _disposed) return;
            var svg = _rawSvg ?? throw new ObjectDisposedException(nameof(HyperSvgTemplate));
            var library = HyperFilters.Used(svg);

            var doc = XDocument.Parse(svg.Replace("xmlns=\"http://www.w3.org/2000/svg\"", ""));
            var root = doc.Root ?? throw new FormatException("SVG sans racine");
            if (root.Name.LocalName != "svg") throw new FormatException("La racine doit etre <svg>");

            foreach (var element in root.Descendants())
            {
                var zone = (string?)element.Attribute("data-zone");
                if (zone != null) _zones[zone] = ZoneRect(element, zone);
                var path = (string?)element.Attribute("data-path");
                if (path == null) continue;
                if (element.Name.LocalName != "path") throw new FormatException($"data-path=\"{path}\" doit etre un <path>");
                _paths[path] = (string?)element.Attribute("d") ?? throw new FormatException($"data-path=\"{path}\" : attribut d manquant");
            }

            Slot? run = null;
            foreach (var child in root.Elements())
            {
                if (child.Name.LocalName == "defs")
                {
                    _defs.AddRange(child.Elements());
                    continue;
                }

                var part = (string?)child.Attribute("data-part");
                if (part == null)
                {
                    if (run == null) _slots.Add(run = new Slot { Kind = HyperSlotKind.Static });
                    run.Elements.Add(child);
                    continue;
                }

                run = null;
                var slot = new Slot
                {
                    Kind = child.Attribute("data-window") != null ? HyperSlotKind.Window : HyperSlotKind.Part,
                    Name = part,
                    PivotExpr = (string?)child.Attribute("data-pivot")
                };
                slot.Elements.Add(child);
                _slots.Add(slot);
            }

            foreach (var (id, xml) in library)
                if (_defs.All(d => (string?)d.Attribute("id") != id))
                    _defs.Add(XElement.Parse(xml));

            var compiledSlots = new HyperTemplateSlot[_slots.Count];
            var allCompiled = true;
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.Kind == HyperSlotKind.Window)
                {
                    var el = slot.Elements[0];
                    var wRect = new Rect(Num(el, "x"), Num(el, "y"), Num(el, "width"), Num(el, "height"));
                    var pivot = slot.PivotExpr != null
                        ? ResolvePivot(slot.PivotExpr, wRect, slot.Name ?? "window")
                        : wRect.Center;
                    compiledSlots[i] = new HyperTemplateSlot
                    {
                        Kind = HyperSlotKind.Window,
                        Name = slot.Name,
                        Pivot = pivot,
                        Window = wRect,
                        Radius = Num(el, "rx"),
                        EstimateBounds = wRect
                    };
                    continue;
                }

                var compiled = HyperVectorCompiler.TryCompile(slot.Elements, Tokens, _defs, out var fallbackReason);
                if (compiled == null) allCompiled = false;
                var est = ComputeSlotEstimateBounds(slot.Elements, out var rawBounds);
                var partPivot = slot.PivotExpr != null
                    ? ResolvePivot(slot.PivotExpr, rawBounds, slot.Name ?? "part")
                    : (Point?)null;
                compiledSlots[i] = new HyperTemplateSlot
                {
                    Kind = slot.Kind,
                    Name = slot.Name,
                    Pivot = partPivot,
                    EstimateBounds = est,
                    Compiled = compiled,
                    FallbackReason = fallbackReason
                };
            }

            _compiledSlots = compiledSlots;
            _allSlotsCompiled = allCompiled;
            _rawSvg = null;
            if (allCompiled)
            {
                _defs.Clear();
                _slots.Clear();
            }
            System.Threading.Volatile.Write(ref _compiled, true);
        }
    }

    public void Dispose()
    {
        lock (_compileGate)
        {
            if (_disposed) return;
            _disposed = true;
            _rawSvg = null;
            _defs.Clear();
            _slots.Clear();
            if (_compiledSlots != null)
            {
                for (var i = 0; i < _compiledSlots.Length; i++)
                    _compiledSlots[i].Compiled?.Dispose();
            }
        }
    }

    private static Rect ExtractViewBoxFast(string svg)
    {
        var span = svg.AsSpan();
        var svgStart = span.IndexOf("<svg".AsSpan(), StringComparison.Ordinal);
        if (svgStart < 0) throw new FormatException("La racine doit etre <svg>");
        var tagEnd = span.Slice(svgStart).IndexOf('>');
        if (tagEnd < 0) throw new FormatException("Balise <svg> non fermee");
        var head = span.Slice(svgStart, tagEnd);

        var vbIdx = head.IndexOf("viewBox".AsSpan(), StringComparison.Ordinal);
        if (vbIdx < 0) throw new FormatException("viewBox obligatoire");
        var afterVb = head.Slice(vbIdx + 7);
        var qIdx = afterVb.IndexOfAny('"', '\'');
        if (qIdx < 0) throw new FormatException("viewBox invalide");
        var quote = afterVb[qIdx];
        var valStart = afterVb.Slice(qIdx + 1);
        var qEnd = valStart.IndexOf(quote);
        if (qEnd < 0) throw new FormatException("viewBox invalide");
        var vbSpan = valStart.Slice(0, qEnd);

        Span<double> nums = stackalloc double[4];
        var count = 0;
        var pos = 0;
        while (pos < vbSpan.Length && count < 4)
        {
            while (pos < vbSpan.Length && (vbSpan[pos] == ' ' || vbSpan[pos] == ',' || vbSpan[pos] == '\t' || vbSpan[pos] == '\r' || vbSpan[pos] == '\n'))
                pos++;
            if (pos >= vbSpan.Length) break;
            var start = pos;
            while (pos < vbSpan.Length && vbSpan[pos] != ' ' && vbSpan[pos] != ',' && vbSpan[pos] != '\t' && vbSpan[pos] != '\r' && vbSpan[pos] != '\n')
                pos++;
            if (!double.TryParse(vbSpan.Slice(start, pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out nums[count]))
                throw new FormatException("viewBox invalide");
            count++;
        }
        if (count != 4) throw new FormatException("viewBox invalide");
        return new Rect(nums[0], nums[1], nums[2], nums[3]);
    }

    private static string[] ExtractTokensFast(string svg)
    {
        var list = new List<string>(8);
        ScanTokens(svg.AsSpan(), list);
        foreach (var (_, xml) in HyperFilters.Used(svg))
            ScanTokens(xml.AsSpan(), list);
        return list.Count == 0 ? Array.Empty<string>() : list.ToArray();
    }

    private static void ScanTokens(ReadOnlySpan<char> span, List<string> list)
    {
        const string prefix = "var(--";
        var idx = 0;
        while (idx < span.Length)
        {
            var rel = span.Slice(idx).IndexOf(prefix.AsSpan(), StringComparison.Ordinal);
            if (rel < 0) break;
            var start = idx + rel + prefix.Length;
            var end = start;
            while (end < span.Length)
            {
                var ch = span[end];
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-')
                    end++;
                else
                    break;
            }
            if (end > start && end < span.Length && span[end] == ')')
            {
                var token = span.Slice(start, end - start).ToString();
                if (!list.Contains(token)) list.Add(token);
            }
            idx = end + 1;
        }
    }

    private Rect ComputeSlotEstimateBounds(List<XElement> elements, out Rect rawBounds)
    {
        Rect? bounds = null;
        var hasFilter = false;
        foreach (var element in elements)
            AccumulateBounds(element, SkiaSharp.SKMatrix.Identity, ref bounds, ref hasFilter);
        if (bounds.HasValue)
        {
            rawBounds = bounds.Value;
            var pad = hasFilter ? 10.0 : 2.0;
            return bounds.Value.Inflate(pad).Intersect(ViewBox);
        }
        rawBounds = ViewBox;
        return ViewBox;
    }

    public List<HyperSliceSpec> Compile(HyperTheme theme)
    {
        EnsureCompiled();
        var defs = new StringBuilder();
        foreach (var paint in theme.Paints.Values)
            if (paint.GradientXml != null) defs.Append(paint.GradientXml);
        foreach (var def in _defs) defs.Append(Themed(def, theme));

        var head = string.Create(CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{ViewBox.X} {ViewBox.Y} {ViewBox.Width} {ViewBox.Height}\" width=\"{ViewBox.Width}\" height=\"{ViewBox.Height}\"><defs>{defs}</defs>");

        var result = new List<HyperSliceSpec>(_slots.Count);
        foreach (var slot in _slots)
        {
            var body = new StringBuilder(head);
            var slice = new HyperSliceSpec { Kind = slot.Kind, Name = slot.Name };
            Rect? bounds = null;
            var hasFilter = false;

            foreach (var element in slot.Elements)
            {
                if (slot.Kind == HyperSlotKind.Window)
                {
                    slice.Window = new Rect(Num(element, "x"), Num(element, "y"), Num(element, "width"), Num(element, "height"));
                    slice.Radius = Num(element, "rx");
                    continue;
                }

                AccumulateBounds(element, SkiaSharp.SKMatrix.Identity, ref bounds, ref hasFilter);

                body.Append(Themed(element, theme));
            }

            if (slot.Kind == HyperSlotKind.Window)
            {
                slice.Pivot = slot.PivotExpr != null
                    ? ResolvePivot(slot.PivotExpr, slice.Window, slot.Name ?? "window")
                    : slice.Window.Center;
            }
            else if (bounds.HasValue)
            {
                var pad = hasFilter ? 10.0 : 2.0;
                slice.EstimateBounds = bounds.Value.Inflate(pad).Intersect(ViewBox);
                if (slot.PivotExpr != null)
                    slice.Pivot = ResolvePivot(slot.PivotExpr, bounds.Value, slot.Name ?? "part");
            }
            else
            {
                slice.EstimateBounds = ViewBox;
                if (slot.PivotExpr != null)
                    slice.Pivot = ResolvePivot(slot.PivotExpr, ViewBox, slot.Name ?? "part");
            }

            slice.Svg = body.Append("</svg>").ToString();
            result.Add(slice);
        }

        return result;
    }

    private static void AccumulateBounds(
        XElement element,
        SkiaSharp.SKMatrix parentTransform,
        ref Rect? bounds,
        ref bool hasFilter)
    {
        if (element.Attribute("filter") != null)
            hasFilter = true;

        var current = parentTransform;
        var tf = (string?)element.Attribute("transform");
        if (tf != null && HyperVectorCompiler.TryParseTransform(tf, out var localTf))
            current = SkiaSharp.SKMatrix.Concat(parentTransform, localTf);

        if (TryLocalElementBounds(element, out var localRect))
        {
            var sw = Num(element, "stroke-width");
            if (sw > 0)
                localRect = localRect.Inflate(sw * 0.5);
            if (!current.IsIdentity)
            {
                var mapped = current.MapRect(new SkiaSharp.SKRect(
                    (float)localRect.X, (float)localRect.Y, (float)localRect.Right, (float)localRect.Bottom));
                localRect = new Rect(mapped.Left, mapped.Top, Math.Max(1, mapped.Width), Math.Max(1, mapped.Height));
            }
            bounds = bounds.HasValue ? bounds.Value.Union(localRect) : localRect;
        }

        foreach (var child in element.Elements())
            AccumulateBounds(child, current, ref bounds, ref hasFilter);
    }

    private static bool TryElementBounds(XElement e, out Rect rect)
    {
        if (!TryLocalElementBounds(e, out rect)) return false;
        var tf = (string?)e.Attribute("transform");
        if (tf != null && HyperVectorCompiler.TryParseTransform(tf, out var m))
        {
            var mapped = m.MapRect(new SkiaSharp.SKRect((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom));
            rect = new Rect(mapped.Left, mapped.Top, Math.Max(1, mapped.Width), Math.Max(1, mapped.Height));
        }
        return true;
    }

    private static bool TryLocalElementBounds(XElement e, out Rect rect)
    {
        bool ok;
        switch (e.Name.LocalName)
        {
            case "rect":
                rect = new Rect(Num(e, "x"), Num(e, "y"), Num(e, "width"), Num(e, "height"));
                ok = rect.Width > 0 && rect.Height > 0;
                break;
            case "circle":
            {
                var r = Num(e, "r");
                rect = new Rect(Num(e, "cx") - r, Num(e, "cy") - r, 2 * r, 2 * r);
                ok = r > 0;
                break;
            }
            case "ellipse":
            {
                var rx = Num(e, "rx");
                var ry = Num(e, "ry");
                rect = new Rect(Num(e, "cx") - rx, Num(e, "cy") - ry, 2 * rx, 2 * ry);
                ok = rx > 0 && ry > 0;
                break;
            }
            case "line":
            {
                var x1 = Num(e, "x1");
                var y1 = Num(e, "y1");
                var x2 = Num(e, "x2");
                var y2 = Num(e, "y2");
                var minX = Math.Min(x1, x2);
                var minY = Math.Min(y1, y2);
                rect = new Rect(minX, minY, Math.Max(1, Math.Abs(x2 - x1)), Math.Max(1, Math.Abs(y2 - y1)));
                ok = true;
                break;
            }
            case "polygon":
            case "polyline":
                ok = TryPointsBounds((string?)e.Attribute("points"), out rect);
                break;
            case "path":
            {
                var d = (string?)e.Attribute("d");
                if (string.IsNullOrWhiteSpace(d)) { rect = default; return false; }
                using var path = SkiaSharp.SKPath.ParseSvgPathData(d);
                if (path == null) { rect = default; return false; }
                var b = path.TightBounds;
                rect = new Rect(b.Left, b.Top, Math.Max(1, b.Width), Math.Max(1, b.Height));
                ok = true;
                break;
            }
            default:
                rect = default;
                return false;
        }

        if (!ok) return false;
        var tf = (string?)e.Attribute("transform");
        if (tf != null && HyperVectorCompiler.TryParseTransform(tf, out var m))
        {
            var mapped = m.MapRect(new SkiaSharp.SKRect((float)rect.X, (float)rect.Y, (float)rect.Right, (float)rect.Bottom));
            rect = new Rect(mapped.Left, mapped.Top, Math.Max(1, mapped.Width), Math.Max(1, mapped.Height));
        }
        return true;
    }

    private static bool TryPointsBounds(string? text, out Rect rect)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            rect = default;
            return false;
        }

        var parts = text.Split(new[] { ' ', ',', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4 || (parts.Length & 1) != 0)
        {
            rect = default;
            return false;
        }

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        for (var i = 0; i < parts.Length; i += 2)
        {
            if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                !double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
            {
                rect = default;
                return false;
            }
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }

        rect = new Rect(minX, minY, Math.Max(1, maxX - minX), Math.Max(1, maxY - minY));
        return true;
    }

    private static Point ResolvePivot(string text, Rect bounds, string name)
    {
        var key = text.Trim().ToLowerInvariant();
        switch (key)
        {
            case "center": return bounds.Center;
            case "left": return new Point(bounds.X, bounds.Y + bounds.Height * 0.5);
            case "right": return new Point(bounds.Right, bounds.Y + bounds.Height * 0.5);
            case "top": return new Point(bounds.X + bounds.Width * 0.5, bounds.Y);
            case "bottom": return new Point(bounds.X + bounds.Width * 0.5, bounds.Bottom);
            case "top-left":
            case "left-top": return new Point(bounds.X, bounds.Y);
            case "top-right":
            case "right-top": return new Point(bounds.Right, bounds.Y);
            case "bottom-left":
            case "left-bottom": return new Point(bounds.X, bounds.Bottom);
            case "bottom-right":
            case "right-bottom": return new Point(bounds.Right, bounds.Bottom);
        }

        var n = text.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (n.Length == 2 &&
            TryParsePivotCoord(n[0], bounds.X, bounds.Width, out var px) &&
            TryParsePivotCoord(n[1], bounds.Y, bounds.Height, out var py))
        {
            return new Point(px, py);
        }

        throw new FormatException($"data-pivot=\"{text}\" sur {name} invalide (attendu: center|left|right|top|bottom|x y|x% y%)");
    }

    private static bool TryParsePivotCoord(string token, double origin, double size, out double value)
    {
        if (token.EndsWith('%'))
        {
            if (double.TryParse(token.AsSpan(0, token.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
            {
                value = origin + pct * 0.01 * size;
                return true;
            }
            value = 0;
            return false;
        }
        return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static Rect ZoneRect(XElement e, string name)
    {
        if (e.Attribute("transform") != null)
            throw new FormatException($"data-zone=\"{name}\" : pas de transform");
        return e.Name.LocalName switch
        {
            "rect" => new Rect(Num(e, "x"), Num(e, "y"), Num(e, "width"), Num(e, "height")),
            "circle" => new Rect(Num(e, "cx") - Num(e, "r"), Num(e, "cy") - Num(e, "r"), 2 * Num(e, "r"), 2 * Num(e, "r")),
            "ellipse" => new Rect(Num(e, "cx") - Num(e, "rx"), Num(e, "cy") - Num(e, "ry"), 2 * Num(e, "rx"), 2 * Num(e, "ry")),
            "polygon" or "polyline" when TryPointsBounds((string?)e.Attribute("points"), out var pr) => pr,
            _ => throw new FormatException($"data-zone=\"{name}\" : type non supporte")
        };
    }

    private static double Num(XElement e, string name) =>
        double.TryParse((string?)e.Attribute(name), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static string Themed(XElement source, HyperTheme theme)
    {
        var copy = new XElement(source);
        foreach (var element in copy.DescendantsAndSelf())
        {
            foreach (var attribute in element.Attributes().ToList())
            {
                var name = attribute.Name.LocalName;
                if (name.StartsWith("data-", StringComparison.Ordinal))
                {
                    attribute.Remove();
                    continue;
                }

                var match = TokenRx.Match(attribute.Value);
                if (!match.Success) continue;
                var key = match.Groups[1].Value;
                if (!theme.Paints.TryGetValue(key, out var paint))
                {
                    attribute.Value = "none";
                    continue;
                }

                if (paint.GradientXml != null)
                {
                    attribute.Value = $"url(#{key})";
                    continue;
                }

                attribute.Value = paint.Hex!;
                if (paint.Alpha < 0.999 && OpacityOf.TryGetValue(name, out var opacityName))
                {
                    var existing = double.TryParse((string?)element.Attribute(opacityName), NumberStyles.Float, CultureInfo.InvariantCulture, out var e) ? e : 1;
                    element.SetAttributeValue(opacityName, (paint.Alpha * existing).ToString("0.####", CultureInfo.InvariantCulture));
                }
            }
        }
        return copy.ToString(SaveOptions.DisableFormatting);
    }
}

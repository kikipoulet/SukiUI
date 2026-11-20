using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>Résout une ressource thématique par clé et variante de thème.</summary>
public delegate bool HyperResourceLookup(string key, ThemeVariant theme, out object? value);

/// <summary>Moteur de rendu SVG de secours lorsqu'un fragment n'est pas compilé nativement par <c>HyperVectorCompiler</c>.</summary>
public interface IHyperFallbackSvg : IDisposable
{
    /// <summary>Dessine le SVG compilé sur le canvas Skia fourni.</summary>
    void Draw(SKCanvas canvas);
}

/// <summary>Compile une chaîne SVG en instance <see cref="IHyperFallbackSvg"/> de secours.</summary>
public delegate IHyperFallbackSvg? HyperFallbackSvgCompiler(string svg);

/// <summary>Options globales de rendu, de mise en cache GPU et de résolution de ressources pour HyperSvg.</summary>
public static class HyperSvgOptions
{
    /// <summary>Fournisseur de ressources de repli (par défaut <see cref="SkeumoMaterials.TryFind"/>).</summary>
    public static HyperResourceLookup? ExternalResourceLookup { get; set; } = SkeumoMaterials.TryFind;

    /// <summary>Compilateur SVG de secours optionnel (par défaut <c>null</c> ; ex. adaptateur <c>Svg.Skia</c>).</summary>
    public static HyperFallbackSvgCompiler? FallbackSvgCompiler { get; set; }

    /// <summary>Délai initial (ms) avant de démarrer une nouvelle vague de premiers montages (laisse la transition de page finir).</summary>
    public static int FirstRenderInitialDelayMs { get; set; } = 140;

    /// <summary>Espacement minimal (ms) entre deux ticks de montage UI dans la vague.</summary>
    public static int FirstRenderStepMs { get; set; } = 8;

    /// <summary>Nombre max d'actions de montage par tick UI (0 = dynamique selon la charge UI et FirstRenderBudgetMs).</summary>
    public static int MaxMountsPerTick { get; set; } = 0;

    /// <summary>Plafond du budget CPU UI-thread (ms) par tick Background quand le thread UI est libre (réduit automatiquement s'il est chargé).</summary>
    public static double FirstRenderBudgetMs { get; set; } = 3.0;

    /// <summary>Durée du fondu d'apparition (Opacity 0 -> 1) lors du premier rendu (0 = instantané).</summary>
    public static int FirstRenderFadeMs { get; set; } = 450;

    /// <summary>Plafond de packs texture VRAM (HyperGpuPage) par fenêtre de frame sur le render thread (0 = illimité).</summary>
    public static int MaxGpuPacksPerFrame { get; set; } = 4;

    /// <summary>Budget temps GPU (ms) par fenêtre de frame pour le packing HyperGpuPage.</summary>
    public static double GpuPackBudgetMs { get; set; } = 2.0;

    /// <summary>Active les logs console [HyperPerf] pour diagnostiquer les temps de build, bind et premier rendu.</summary>
    public static bool LogFirstRenderPerf { get; set; } = false;
}

/// <summary>
///     Journalisation lorsqu'un compilateur SVG de secours remplace la compilation vectorielle native.
/// </summary>
public static class HyperFallbackLog
{
    /// <summary>Affiche un avertissement détaillé indiquant la source et la raison du repli.</summary>
    public static void Report(string source, string reason)
    {
        const string bar = "################################################################################";
        Console.WriteLine();
        Console.WriteLine(bar);
        Console.WriteLine("[WARNING !!!] HYPERSVG FALLBACK -> FallbackSvgCompiler");
        Console.WriteLine($"  Source  : {source}");
        Console.WriteLine($"  Raison  : {reason}");
        Console.WriteLine("  A faire : ajouter cette fonction SVG a HyperVectorCompiler.cs (ou simplifier le SVG).");
        Console.WriteLine("            Tant que ce message existe, le rendu passe par le compilateur de secours.");
        Console.WriteLine(bar);
        Console.WriteLine();
    }
}

internal static class HyperTime
{
    public static Func<long> Source { get; set; } = Stopwatch.GetTimestamp;

    public static int Seed { get; set; } = 1;

    internal static long Now() => Math.Max(1, Source());
}

/// <summary>Chemin vectoriel SVG pré-mesuré permettant d'échantillonner une position normalisée <c>t ∈ [0, 1]</c>.</summary>
public sealed class HyperPath : IDisposable
{
    private readonly SKPath _path;
    private readonly SKPathMeasure _measure;
    private readonly float _length;

    internal HyperPath(string name, string d)
    {
        _path = SKPath.ParseSvgPathData(d) ?? throw new FormatException($"data-path=\"{name}\" : attribut d illisible");
        _measure = new SKPathMeasure(_path, false, 1);
        _length = _measure.Length;
    }

    /// <summary>Échantillonne la position sur le chemin pour <paramref name="t"/> dans <c>[0, 1]</c>.</summary>
    public Point PointAt(double t)
    {
        _measure.GetPosition((float)(Math.Clamp(t, 0, 1) * _length), out var point);
        return new Point(point.X, point.Y);
    }

    /// <summary>Libère les ressources natives Skia du chemin.</summary>
    public void Dispose()
    {
        _measure.Dispose();
        _path.Dispose();
    }
}

internal static class HyperFilters
{
    private const string Region = "filterUnits=\"userSpaceOnUse\" x=\"-100\" y=\"-100\" width=\"1400\" height=\"1400\"";

    private static readonly Regex UseRx = new(@"url\(#(skeumo-[A-Za-z0-9-]+)\)", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> Library = new()
    {
        ["skeumo-shadow"] =
            $"<filter id=\"skeumo-shadow\" {Region}><feGaussianBlur in=\"SourceAlpha\" stdDeviation=\"2.23\"/><feOffset dy=\"3\" result=\"o\"/>" +
            "<feFlood flood-color=\"#000\" flood-opacity=\".5\"/><feComposite in2=\"o\" operator=\"in\"/>" +
            "<feMerge><feMergeNode/><feMergeNode in=\"SourceGraphic\"/></feMerge></filter>",

        ["skeumo-glow"] =
            $"<filter id=\"skeumo-glow\" {Region}><feGaussianBlur in=\"SourceAlpha\" stdDeviation=\"2.23\" result=\"b\"/>" +
            "<feFlood flood-color=\"var(--SukiPrimaryColor)\" flood-opacity=\".6\"/><feComposite in2=\"b\" operator=\"in\" result=\"g\"/>" +
            "<feMerge><feMergeNode in=\"g\"/><feMergeNode in=\"SourceGraphic\"/></feMerge></filter>",

        ["skeumo-blur"] = $"<filter id=\"skeumo-blur\" {Region}><feGaussianBlur stdDeviation=\"2.23\"/></filter>"
    };

    public static IEnumerable<string> Names => Library.Keys;

    public static List<(string Id, string Xml)> Used(string svg)
    {
        var result = new List<(string, string)>();
        foreach (var id in UseRx.Matches(svg).Select(m => m.Groups[1].Value).Distinct())
        {
            if (!Library.TryGetValue(id, out var xml))
                throw new FormatException($"Filtre '{id}' inconnu (disponibles : {string.Join(", ", Library.Keys)})");
            result.Add((id, xml));
        }
        return result;
    }
}

internal sealed class HyperPaint
{
    public string? Hex;
    public double Alpha = 1;
    public string? GradientXml;
}

internal sealed class HyperTheme
{
    public readonly Dictionary<string, HyperPaint> Paints = new();

    public static HyperTheme Create(IReadOnlyList<string> keys, IReadOnlyList<object?> values)
    {
        var theme = new HyperTheme();
        for (var i = 0; i < keys.Count; i++)
        {
            var paint = From(keys[i], values[i]);
            if (paint != null) theme.Paints[keys[i]] = paint;
        }
        return theme;
    }

    public static HyperTheme FromSnapshot(IReadOnlyList<string> keys, HyperPaintSnapshot snapshot)
    {
        var theme = new HyperTheme();
        var count = Math.Min(keys.Count, snapshot.Entries.Length);
        for (var i = 0; i < count; i++)
        {
            var paint = FromEntry(keys[i], snapshot.Entries[i]);
            if (paint != null) theme.Paints[keys[i]] = paint;
        }
        return theme;
    }

    private static HyperPaint? From(string key, object? value) => value switch
    {
        Color color => Solid(color, 1),
        ISolidColorBrush solid => Solid(solid.Color, solid.Opacity),
        ILinearGradientBrush linear => new HyperPaint { GradientXml = Linear(key, linear) },
        IRadialGradientBrush radial => new HyperPaint { GradientXml = Radial(key, radial) },
        _ => null
    };

    private static HyperPaint? FromEntry(string key, HyperPaintEntry entry) => entry.Kind switch
    {
        HyperPaintEntryKind.Solid => new HyperPaint
        {
            Hex = $"#{entry.Color.Red:X2}{entry.Color.Green:X2}{entry.Color.Blue:X2}",
            Alpha = entry.Color.Alpha / 255.0
        },
        HyperPaintEntryKind.LinearGradient => new HyperPaint { GradientXml = Linear(key, entry) },
        HyperPaintEntryKind.RadialGradient => new HyperPaint { GradientXml = Radial(key, entry) },
        _ => null
    };

    private static HyperPaint Solid(Color color, double opacity) =>
        new() { Hex = Hex(color), Alpha = color.A / 255.0 * opacity };

    private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static string N(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);

    private static string Linear(string key, HyperPaintEntry e)
    {
        var sb = new StringBuilder();
        sb.Append($"<linearGradient id=\"{key}\" x1=\"{N(e.Start.X)}\" y1=\"{N(e.Start.Y)}\" x2=\"{N(e.End.X)}\" y2=\"{N(e.End.Y)}\"");
        if (!e.IsRelative) sb.Append(" gradientUnits=\"userSpaceOnUse\"");
        Spread(sb, e.TileMode);
        sb.Append('>');
        Stops(sb, e.Colors, e.Offsets);
        return sb.Append("</linearGradient>").ToString();
    }

    private static string Radial(string key, HyperPaintEntry e)
    {
        var sb = new StringBuilder();
        sb.Append($"<radialGradient id=\"{key}\" cx=\"{N(e.Center.X)}\" cy=\"{N(e.Center.Y)}\" r=\"{N(e.RadiusX)}\" fx=\"{N(e.Origin.X)}\" fy=\"{N(e.Origin.Y)}\"");
        if (!e.IsRelative) sb.Append(" gradientUnits=\"userSpaceOnUse\"");
        Spread(sb, e.TileMode);
        sb.Append('>');
        Stops(sb, e.Colors, e.Offsets);
        return sb.Append("</radialGradient>").ToString();
    }

    private static void Spread(StringBuilder sb, SKShaderTileMode mode)
    {
        if (mode == SKShaderTileMode.Mirror) sb.Append(" spreadMethod=\"reflect\"");
        else if (mode == SKShaderTileMode.Repeat) sb.Append(" spreadMethod=\"repeat\"");
    }

    private static void Stops(StringBuilder sb, SKColor[] colors, float[] offsets)
    {
        for (var i = 0; i < colors.Length; i++)
        {
            var c = colors[i];
            var alpha = c.Alpha / 255.0;
            sb.Append($"<stop offset=\"{N(offsets[i])}\" stop-color=\"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}\"");
            if (alpha < 0.999) sb.Append($" stop-opacity=\"{N(alpha)}\"");
            sb.Append("/>");
        }
    }

    private static string Linear(string key, ILinearGradientBrush b)
    {
        var sb = new StringBuilder();
        var relative = b.StartPoint.Unit == RelativeUnit.Relative;
        sb.Append($"<linearGradient id=\"{key}\" x1=\"{N(b.StartPoint.Point.X)}\" y1=\"{N(b.StartPoint.Point.Y)}\" x2=\"{N(b.EndPoint.Point.X)}\" y2=\"{N(b.EndPoint.Point.Y)}\"");
        if (!relative) sb.Append(" gradientUnits=\"userSpaceOnUse\"");
        Spread(sb, b);
        sb.Append('>');
        Stops(sb, b);
        return sb.Append("</linearGradient>").ToString();
    }

    private static string Radial(string key, IRadialGradientBrush b)
    {
        var sb = new StringBuilder();
        sb.Append($"<radialGradient id=\"{key}\" cx=\"{N(b.Center.Point.X)}\" cy=\"{N(b.Center.Point.Y)}\" r=\"{N(b.RadiusX.Scalar)}\" fx=\"{N(b.GradientOrigin.Point.X)}\" fy=\"{N(b.GradientOrigin.Point.Y)}\"");
        if (b.Center.Unit != RelativeUnit.Relative) sb.Append(" gradientUnits=\"userSpaceOnUse\"");
        Spread(sb, b);
        sb.Append('>');
        Stops(sb, b);
        return sb.Append("</radialGradient>").ToString();
    }

    private static void Spread(StringBuilder sb, IGradientBrush b)
    {
        if (b.SpreadMethod == GradientSpreadMethod.Reflect) sb.Append(" spreadMethod=\"reflect\"");
        else if (b.SpreadMethod == GradientSpreadMethod.Repeat) sb.Append(" spreadMethod=\"repeat\"");
    }

    private static void Stops(StringBuilder sb, IGradientBrush b)
    {
        foreach (var stop in b.GradientStops)
        {
            var alpha = stop.Color.A / 255.0 * b.Opacity;
            sb.Append($"<stop offset=\"{N(stop.Offset)}\" stop-color=\"{Hex(stop.Color)}\"");
            if (alpha < 0.999) sb.Append($" stop-opacity=\"{N(alpha)}\"");
            sb.Append("/>");
        }
    }
}

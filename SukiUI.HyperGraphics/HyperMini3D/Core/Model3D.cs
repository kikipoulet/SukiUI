using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Modele pour Mini3D : une classe, une methode Build qui decrit la scene avec l'API Scene3D (voir GUIDE.md).
///     Repere : X = longueur (gauche -> droite), Y = profondeur (0 = facade, vers l'arriere), Z = hauteur (vers le haut).
///     Build doit etre pure (aucun etat modifie) : le moteur la rappelle pour reconstruire la scene apres un repos.
/// </summary>
public abstract class Model3D
{
    /// <summary>Degres -> radians, pour les calculs de points dans Build (les API Mini3D prennent des degres).</summary>
    protected const double Deg = Math.PI / 180;

    private ModelInfo? _info;
    private List<(int Small, int Host, int Face)>? _mountPlan;
    private int? _mountPlanQuality;
    private int _quality = 50;

    /// <summary>Qualite de rendu par defaut de 0 a 100 (50 par defaut ; peut etre surchargee par la vue).</summary>
    public int Quality
    {
        get => _quality;
        set => _quality = Math.Clamp(value, 0, 100);
    }

    protected internal abstract void Build(Scene3D s);

    /// <summary>Canaux, camera, cadrage : compile une fois (thread UI), la geometrie produite est jetee.</summary>
    internal ModelInfo Info
    {
        get
        {
            if (_info != null) return _info;
            using var geometry = Compile();
            return _info = geometry.Info;
        }
    }

    internal Geometry Compile(int? quality = null)
    {
        var effective = Math.Clamp(quality ?? Quality, 0, 100);
        if (_mountPlanQuality != effective)
        {
            _mountPlan = null;
            _mountPlanQuality = effective;
        }

        var scene = new Scene3D(effective);
        Build(scene);
        var geometry = scene.Finish(_mountPlan);
        _mountPlan ??= scene.MountPlan;
        return geometry;
    }

    /// <summary>Noms des entrees (s.Input) que la vue peut piloter avec SetChannel.</summary>
    public IReadOnlyList<string> InputNames => Info.Inputs.ConvertAll(i => i.Name);

    protected static SKColor Rgb(byte r, byte g, byte b) => new(r, g, b);

    protected static SKColor Hex(string hex) => SKColor.Parse(hex);
}

/// <summary>Faces d'une boite, nommees par la direction de leur normale.</summary>
[Flags]
public enum Side
{
    None = 0,
    /// <summary>-Y (facade, vers l'observateur par defaut).</summary>
    Front = 1,
    /// <summary>+Y.</summary>
    Back = 2,
    /// <summary>-X.</summary>
    Left = 4,
    /// <summary>+X.</summary>
    Right = 8,
    /// <summary>+Z.</summary>
    Top = 16,
    /// <summary>-Z.</summary>
    Bottom = 32,
    All = 63,
}

public enum Axis { X, Y, Z }

/// <summary>Valeurs courantes des canaux (entrees animees et canaux derives), lues par nom.</summary>
public sealed class Channels
{
    internal readonly double[] Values;
    internal readonly Dictionary<string, int> Index;

    internal Channels(double[] values, Dictionary<string, int> index)
    {
        Values = values;
        Index = index;
    }

    public double this[string name] =>
        Index.TryGetValue(name, out var i) ? Values[i] : throw new KeyNotFoundException($"Mini3D : canal '{name}' inconnu.");
}

/// <summary>
///     Suite de points 2D (contour d'extrusion, polygone de decalque) faite de segments, d'arcs, de cercles et de courbes de Bezier.
///     Lorsque <c>segments</c> est omis, la tessellation s'adapte automatiquement a la taille et a <see cref="Model3D.Quality"/> (0..100).
/// </summary>
public sealed class Path2D
{
    private enum CmdKind { Line, Arc, Quadratic, Cubic }

    private readonly record struct Command(
        CmdKind Kind,
        double A, double B,
        double C1A, double C1B,
        double C2A, double C2B,
        double R, double FromDeg, double ToDeg,
        int? Segments, bool ClosedLoop);

    private readonly List<Command> _commands = new();

    /// <summary>Ajoute un point.</summary>
    public Path2D Line(double a, double b)
    {
        _commands.Add(new Command(CmdKind.Line, a, b, 0, 0, 0, 0, 0, 0, 0, null, false));
        return this;
    }

    /// <summary>
    ///     Ajoute un cercle ferme centre en (ca, cb) de rayon r.
    ///     Sans <paramref name="segments"/>, le nombre de faces suit automatiquement le rayon et la qualite (0..100).
    /// </summary>
    public Path2D Circle(double ca, double cb, double r, int? segments = null)
    {
        _commands.Add(new Command(CmdKind.Arc, ca, cb, 0, 0, 0, 0, r, 0, 360, segments, ClosedLoop: true));
        return this;
    }

    /// <summary>
    ///     Ajoute un arc de cercle (degres, sens trigonometrique si toDeg &gt; fromDeg).
    ///     Sans <paramref name="segments"/>, le decoupage est calcule automatiquement selon le rayon, l'angle et la qualite (0..100).
    /// </summary>
    public Path2D Arc(double ca, double cb, double r, double fromDeg, double toDeg, int? segments = null)
    {
        var fullCircle = Math.Abs(Math.Abs(toDeg - fromDeg) - 360.0) < 1e-3;
        _commands.Add(new Command(CmdKind.Arc, ca, cb, 0, 0, 0, 0, r, fromDeg, toDeg, segments, ClosedLoop: fullCircle));
        return this;
    }

    /// <summary>
    ///     Ajoute une courbe de Bezier quadratique depuis le dernier point vers (a, b) via le point de controle (ca, cb).
    ///     Sans <paramref name="segments"/>, le decoupage s'adapte a la longueur et a <see cref="Model3D.Quality"/>.
    /// </summary>
    public Path2D Quadratic(double ca, double cb, double a, double b, int? segments = null)
    {
        _commands.Add(new Command(CmdKind.Quadratic, a, b, ca, cb, 0, 0, 0, 0, 0, segments, false));
        return this;
    }

    /// <summary>
    ///     Ajoute une courbe de Bezier cubique depuis le dernier point vers (a, b) via (c1a, c1b) et (c2a, c2b).
    ///     Sans <paramref name="segments"/>, le decoupage s'adapte a la longueur et a <see cref="Model3D.Quality"/>.
    /// </summary>
    public Path2D Cubic(double c1a, double c1b, double c2a, double c2b, double a, double b, int? segments = null)
    {
        _commands.Add(new Command(CmdKind.Cubic, a, b, c1a, c1b, c2a, c2b, 0, 0, 0, segments, false));
        return this;
    }

    internal int CommandCount => _commands.Count;

    internal bool HasSameStructure(Path2D other)
    {
        if (_commands.Count != other._commands.Count) return false;
        for (var i = 0; i < _commands.Count; i++)
            if (_commands[i].Kind != other._commands[i].Kind || _commands[i].ClosedLoop != other._commands[i].ClosedLoop)
                return false;
        return true;
    }

    internal int[] ResolveCommandSegments(int quality)
    {
        var counts = new int[_commands.Count];
        var pts = new List<(double A, double B)>();
        for (var c = 0; c < _commands.Count; c++)
        {
            var cmd = _commands[c];
            var segs = CommandSegments(cmd, pts, quality);
            counts[c] = segs;
            AppendCommand(cmd, segs, pts);
        }
        return counts;
    }

    internal List<(double A, double B)> Resolve(int quality, int[]? fixedCounts = null)
    {
        var pts = new List<(double A, double B)>();
        for (var c = 0; c < _commands.Count; c++)
        {
            var cmd = _commands[c];
            var segs = fixedCounts != null ? fixedCounts[c] : CommandSegments(cmd, pts, quality);
            AppendCommand(cmd, segs, pts);
        }
        return pts;
    }

    private static int CommandSegments(Command cmd, List<(double A, double B)> pts, int quality)
    {
        switch (cmd.Kind)
        {
            case CmdKind.Line:
                return 0;
            case CmdKind.Arc:
            {
                var sweep = Math.Abs(cmd.ToDeg - cmd.FromDeg);
                var segs = cmd.Segments ?? ResolveSegments(cmd.R, sweep, quality);
                return Math.Max(cmd.ClosedLoop ? 3 : 1, segs);
            }
            case CmdKind.Quadratic:
            {
                if (pts.Count == 0) throw new InvalidOperationException("Mini3D : Path2D.Quadratic demande un point de depart (Line ou Arc).");
                var p0 = pts[^1];
                var len = Dist(p0, (cmd.C1A, cmd.C1B)) + Dist((cmd.C1A, cmd.C1B), (cmd.A, cmd.B));
                return Math.Max(1, cmd.Segments ?? ResolveCurveSegments(len, quality));
            }
            case CmdKind.Cubic:
            {
                if (pts.Count == 0) throw new InvalidOperationException("Mini3D : Path2D.Cubic demande un point de depart (Line ou Arc).");
                var p0 = pts[^1];
                var len = Dist(p0, (cmd.C1A, cmd.C1B)) + Dist((cmd.C1A, cmd.C1B), (cmd.C2A, cmd.C2B)) + Dist((cmd.C2A, cmd.C2B), (cmd.A, cmd.B));
                return Math.Max(1, cmd.Segments ?? ResolveCurveSegments(len, quality));
            }
            default:
                return 0;
        }
    }

    private static void AppendCommand(Command cmd, int segs, List<(double A, double B)> pts)
    {
        switch (cmd.Kind)
        {
            case CmdKind.Line:
                pts.Add((cmd.A, cmd.B));
                break;
            case CmdKind.Arc:
                if (cmd.ClosedLoop)
                {
                    for (var i = 0; i < segs; i++)
                    {
                        var t = (cmd.FromDeg + (cmd.ToDeg - cmd.FromDeg) * i / segs) * M.Deg;
                        pts.Add((cmd.A + cmd.R * Math.Cos(t), cmd.B + cmd.R * Math.Sin(t)));
                    }
                }
                else
                {
                    for (var i = 0; i <= segs; i++)
                    {
                        var t = (cmd.FromDeg + (cmd.ToDeg - cmd.FromDeg) * i / segs) * M.Deg;
                        pts.Add((cmd.A + cmd.R * Math.Cos(t), cmd.B + cmd.R * Math.Sin(t)));
                    }
                }
                break;
            case CmdKind.Quadratic:
            {
                var p0 = pts[^1];
                for (var i = 1; i <= segs; i++)
                {
                    var s = i / (double)segs;
                    var u = 1.0 - s;
                    pts.Add((
                        u * u * p0.A + 2 * u * s * cmd.C1A + s * s * cmd.A,
                        u * u * p0.B + 2 * u * s * cmd.C1B + s * s * cmd.B));
                }
                break;
            }
            case CmdKind.Cubic:
            {
                var p0 = pts[^1];
                for (var i = 1; i <= segs; i++)
                {
                    var s = i / (double)segs;
                    var u = 1.0 - s;
                    pts.Add((
                        u * u * u * p0.A + 3 * u * u * s * cmd.C1A + 3 * u * s * s * cmd.C2A + s * s * s * cmd.A,
                        u * u * u * p0.B + 3 * u * u * s * cmd.C1B + 3 * u * s * s * cmd.C2B + s * s * s * cmd.B));
                }
                break;
            }
        }
    }

    private static double Dist((double A, double B) p, (double A, double B) q)
        => Math.Sqrt((q.A - p.A) * (q.A - p.A) + (q.B - p.B) * (q.B - p.B));

    internal static int ResolveCurveSegments(double length, int quality)
    {
        var l = Math.Max(0, length);
        var t = Math.Clamp(quality, 0, 100) / 100.0;
        double minSeg, maxSeg, baseSeg, slope;
        if (t <= 0.5)
        {
            var u = t * 2.0;
            minSeg = 2 + 1 * u;
            maxSeg = 7 + 7 * u;
            baseSeg = 2 + 2 * u;
            slope = 0.04 + 0.06 * u;
        }
        else
        {
            var u = (t - 0.5) * 2.0;
            minSeg = 3 + 2 * u;
            maxSeg = 14 + 12 * u;
            baseSeg = 4 + 4 * u;
            slope = 0.10 + 0.12 * u;
        }
        return (int)Math.Round(Math.Clamp(baseSeg + l * slope, minSeg, maxSeg));
    }

    internal static int ResolveSegments(double radius, double sweepDeg, int quality)
    {
        var r = Math.Max(0, radius);
        var t = Math.Clamp(quality, 0, 100) / 100.0;
        double minSeg, maxSeg, baseSeg, slope;
        if (t <= 0.5)
        {
            var u = t * 2.0;
            minSeg = 8 + 4 * u;
            maxSeg = 14 + 8 * u;
            baseSeg = 6 + 4 * u;
            slope = 0.22 + 0.24 * u;
        }
        else
        {
            var u = (t - 0.5) * 2.0;
            minSeg = 12 + 4 * u;
            maxSeg = 22 + 10 * u;
            baseSeg = 10 + 4 * u;
            slope = 0.46 + 0.34 * u;
        }
        var full = (int)Math.Round(Math.Clamp(baseSeg + r * slope, minSeg, maxSeg));
        if (sweepDeg >= 360 - 1e-3) return full;
        return Math.Max(2, (int)Math.Ceiling(full * (sweepDeg / 360.0)));
    }
}

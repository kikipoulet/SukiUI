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

    private List<(int Small, int Host, int Face)>? _mountPlan;

    internal Geometry Compile()
    {
        var scene = new Scene3D();
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

/// <summary>Suite de points 2D (contour d'extrusion, polygone de decalque) faite de segments et d'arcs.</summary>
public sealed class Path2D
{
    internal readonly List<(double A, double B)> Points = new();

    /// <summary>Ajoute un point.</summary>
    public Path2D Line(double a, double b)
    {
        Points.Add((a, b));
        return this;
    }

    /// <summary>Ajoute un arc de cercle (degres, sens trigonometrique si toDeg &gt; fromDeg), segments + 1 points.</summary>
    public Path2D Arc(double ca, double cb, double r, double fromDeg, double toDeg, int segments = 8)
    {
        for (var i = 0; i <= segments; i++)
        {
            var t = (fromDeg + (toDeg - fromDeg) * i / segments) * M.Deg;
            Points.Add((ca + r * Math.Cos(t), cb + r * Math.Sin(t)));
        }
        return this;
    }
}

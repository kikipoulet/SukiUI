using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Dessin 2D sur une face. Coordonnees (a, b) = les deux axes du monde dans le plan de la face :
///     (X, Z) pour Front/Back, (Y, Z) pour Left/Right, (X, Y) pour Top/Bottom. Chaque forme se peint dans l'ordre
///     des appels ; sans Fill ni Stroke elle ne se voit pas.
/// </summary>
public sealed class Canvas2D
{
    private static readonly SKTypeface DefaultBoldTypeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold);

    private readonly Face _face;
    private readonly ModelInfo _info;
    private readonly Geometry? _geometry;
    private readonly Channels? _live;
    private readonly Stack<SKPath>? _pool;

    internal Canvas2D(Face face, ModelInfo info, Geometry? geometry, Channels? live, Stack<SKPath>? pool = null)
    {
        _face = face;
        _info = info;
        _geometry = geometry;
        _live = live;
        _pool = pool;
    }

    private Vector2 L(double a, double b) => _face.Local(_face.PlanePoint(a, b));

    private Mark Add(Vector2[][] figures, bool line = false)
    {
        SKPath? pooled = null;
        if (_pool != null && _pool.Count > 0) pooled = _pool.Pop();
        var decal = new Decal { Local = figures, Line = line, Path = pooled, PathDirty = pooled != null };
        _face.Dec.Add(decal);
        return new Mark(decal, _info, _geometry, _live);
    }

    private Vector2[] MapPoints((double A, double B)[] points)
    {
        var pts = new Vector2[points.Length];
        for (var i = 0; i < points.Length; i++) pts[i] = L(points[i].A, points[i].B);
        return pts;
    }

    public Mark Polygon(params (double A, double B)[] points) => Add(new[] { MapPoints(points) });

    public Mark Polygon(Path2D path)
    {
        var src = path.Points;
        var pts = new Vector2[src.Count];
        for (var i = 0; i < src.Count; i++) pts[i] = L(src[i].A, src[i].B);
        return Add(new[] { pts });
    }

    /// <summary>Ligne brisee ouverte, bouts ronds ; couleur par defaut : EdgeColor.</summary>
    public Mark Line(params (double A, double B)[] points) => Add(new[] { MapPoints(points) }, true);

    public Mark Rect((double A, double B) from, (double A, double B) to) =>
        Add(new[] { new[] { L(from.A, from.B), L(to.A, from.B), L(to.A, to.B), L(from.A, to.B) } });

    /// <summary>Rectangle a coins arrondis entre deux coins opposes, segments par coin.</summary>
    public Mark RoundRect((double A, double B) from, (double A, double B) to, double radius, int segments = 4) =>
        Add(new[] { RoundRectPoints(Math.Min(from.A, to.A), Math.Max(from.A, to.A), Math.Min(from.B, to.B), Math.Max(from.B, to.B), radius, segments) });

    public Mark Circle(double a, double b, double r, int segments = 16) => Add(new[] { EllipsePoints(a, b, r, r, segments) });

    public Mark Ellipse(double a, double b, double ra, double rb, int segments = 24) => Add(new[] { EllipsePoints(a, b, ra, rb, segments) });

    /// <summary>
    ///     Texte sur une ligne, (a, b) = debut de la ligne de base, size = hauteur de police en unites modele
    ///     (police par defaut : celle du systeme, en gras). Il se lit a l'endroit vu de l'exterieur de la face.
    /// </summary>
    public Mark Text(string text, (double A, double B) at, double size, SKTypeface? typeface = null)
    {
        using var font = new SKFont(typeface ?? DefaultBoldTypeface, (float)size);
        var path = font.GetTextPath(text, SKPoint.Empty);
        // Axe a vers la droite de l'observateur ? Sinon (Back, Left, Bottom), le texte avance vers les a decroissants.
        var p0 = _face.PlanePoint(0, 0);
        var right = Vector3.Dot(Vector3.Cross(_face.PlanePoint(1, 0) - p0, _face.PlanePoint(0, 1) - p0), _face.RestN) > 0 ? 1 : -1;
        var o = L(at.A, at.B);
        var x = L(at.A + right, at.B) - o; // une unite de texte vers la droite
        var y = L(at.A, at.B - 1) - o;     // une unite de texte vers le bas (Y de Skia)
        path.Transform(new SKMatrix(x.X, y.X, o.X, x.Y, y.Y, o.Y, 0, 0, 1));
        // Chemin deja local : Local vide pour que le rendu le prenne tel quel
        var decal = new Decal { Local = Array.Empty<Vector2[]>(), Path = path };
        _face.Dec.Add(decal);
        return new Mark(decal, _info, _geometry, _live);
    }

    /// <summary>Vis a fleur (tete + empreinte hexagonale), rayon 3.2 par defaut.</summary>
    public void Screws(params (double A, double B)[] at) => Screws(3.2, at);

    public void Screws(double radius, params (double A, double B)[] at)
    {
        var heads = new Vector2[at.Length][];
        var sockets = new Vector2[at.Length][];
        for (var i = 0; i < at.Length; i++)
        {
            heads[i] = EllipsePoints(at[i].A, at[i].B, radius, radius, 8);
            sockets[i] = EllipsePoints(at[i].A, at[i].B, radius * .5, radius * .5, 6);
        }
        Add(heads).Fill(new SKColor(206, 213, 228)).Stroke(_info.Edge, .4);
        Add(sockets).Fill(new SKColor(138, 150, 180));
    }

    private Vector2[] EllipsePoints(double a, double b, double ra, double rb, int n)
    {
        var pts = new Vector2[n];
        for (var i = 0; i < n; i++)
        {
            var t = i / (double)n * 2 * Math.PI;
            pts[i] = L(a + ra * Math.Cos(t), b + rb * Math.Sin(t));
        }
        return pts;
    }

    private Vector2[] RoundRectPoints(double a0, double a1, double b0, double b1, double r, int n)
    {
        var pts = new Vector2[4 * (n + 1)];
        var idx = 0;
        void Corner(double ca, double cb, double start)
        {
            for (var i = 0; i <= n; i++)
            {
                var t = (start + 90.0 * i / n) * M.Deg;
                pts[idx++] = L(ca + r * Math.Cos(t), cb + r * Math.Sin(t));
            }
        }
        Corner(a1 - r, b0 + r, -90.0);
        Corner(a1 - r, b1 - r, 0.0);
        Corner(a0 + r, b1 - r, 90.0);
        Corner(a0 + r, b0 + r, 180.0);
        return pts;
    }
}

/// <summary>Dessin 3D libre dans un groupe de profondeur (coordonnees monde), projete a chaque image.</summary>
public sealed class Canvas3D
{
    private readonly Part _part;
    private readonly ModelInfo _info;
    private readonly Geometry _geometry;

    internal Canvas3D(Part part, ModelInfo info, Geometry geometry)
    {
        _part = part;
        _info = info;
        _geometry = geometry;
    }

    private Mark Add(Vector3[] points, bool line)
    {
        var decal = new Decal { F = new[] { points }, Line = line };
        _part.Free.Add(decal);
        return new Mark(decal, _info, _geometry, null);
    }

    private static Vector3[] MapPoints((double X, double Y, double Z)[] points)
    {
        var pts = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++) pts[i] = M.V(points[i].X, points[i].Y, points[i].Z);
        return pts;
    }

    public Mark Polygon(params (double X, double Y, double Z)[] points) => Add(MapPoints(points), false);

    public Mark Line(params (double X, double Y, double Z)[] points) => Add(MapPoints(points), true);

    /// <summary>Courbe de Bezier quadratique de a a b, tiree vers control (cable, tuyau).</summary>
    public Mark Bezier((double X, double Y, double Z) a, (double X, double Y, double Z) control, (double X, double Y, double Z) b, int segments = 10)
    {
        Vector3 pa = M.V(a.X, a.Y, a.Z), pm = M.V(control.X, control.Y, control.Z), pb = M.V(b.X, b.Y, b.Z);
        var pts = new Vector3[segments + 1];
        for (var i = 0; i <= segments; i++)
        {
            var t = i / (double)segments;
            var u = (float)(1 - t);
            var tt = (float)t;
            pts[i] = u * u * pa + 2 * u * tt * pm + tt * tt * pb;
        }
        return Add(pts, true);
    }
}

/// <summary>Une forme dessinee : son style. Chaque methode renvoie la forme pour chainer.</summary>
public readonly struct Mark
{
    private readonly Decal _decal;
    private readonly ModelInfo _info;
    private readonly Geometry? _geometry;
    private readonly Channels? _live;

    internal Mark(Decal decal, ModelInfo info, Geometry? geometry, Channels? live)
    {
        _decal = decal;
        _info = info;
        _geometry = geometry;
        _live = live;
    }

    public Mark Fill(SKColor color)
    {
        _decal.Fill = color;
        return this;
    }

    /// <summary>Contour (ou trait pour Line), largeur en unites modele.</summary>
    public Mark Stroke(SKColor color, double width = .5)
    {
        _decal.Stroke = color;
        _decal.W = width;
        return this;
    }

    public Mark Opacity(double opacity)
    {
        _decal.Op = opacity;
        return this;
    }

    /// <summary>Lueur : opacite = opacity * canal (invisible a 0, ni teintee en sombre ni dessinee en filaire).</summary>
    public Mark Glow(string channel, double opacity = 1)
    {
        _decal.Glow = _info.Channel(channel);
        _decal.Op = opacity;
        return this;
    }

    /// <summary>Remplissage interpole de from (canal = 0) a to (canal = 1).</summary>
    public Mark FillWith(string channel, SKColor from, SKColor to)
    {
        var index = _info.Channel(channel);
        if (_live != null)
            _decal.Fill = M.Lerp(from, to, _live.Values[index]);
        else
        {
            _decal.Fill = from;
            _geometry?.Colors.Add(new ColorBinding { Decal = _decal, Channel = index, From = from, To = to });
        }
        return this;
    }
}

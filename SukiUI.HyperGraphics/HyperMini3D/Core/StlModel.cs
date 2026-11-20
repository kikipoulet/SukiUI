using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Fichier STL (binaire ou ASCII) affiche en best effort : maillage simplifie par grille jusqu'a maxTriangles,
///     mis a l'echelle (plus grande dimension = size), pose sur le sol. Seules les aretes vives (angle &gt; sharpAngle
///     ou bord) sont tracees. Le fichier est lu une fois dans le constructeur ; Build ne fait qu'emettre les faces.
///     Pas de validateur : il est quadratique sur le nombre de faces.
/// </summary>
public sealed class StlModel : Model3D
{
    private readonly Vector3[] _points;
    private readonly int[] _triangles;
    private readonly bool[][] _sharp;
    private readonly SKColor _color;

    /// <summary>Triangles du fichier.</summary>
    public int SourceTriangles { get; }

    /// <summary>Triangles affiches apres simplification.</summary>
    public int Triangles => _triangles.Length / 3;

    public StlModel(string path, int maxTriangles = 2000, double sharpAngle = 30, double size = 300, SKColor? color = null)
        : this(File.ReadAllBytes(path), maxTriangles, sharpAngle, size, color)
    {
    }

    public StlModel(byte[] data, int maxTriangles = 2000, double sharpAngle = 30, double size = 300, SKColor? color = null)
    {
        _color = color ?? Rgb(226, 232, 243);
        var raw = Read(data);
        SourceTriangles = raw.Count / 3;
        if (SourceTriangles == 0) throw new InvalidDataException("Mini3D : STL vide ou illisible.");
        var (points, triangles) = Weld(DropOutliers(raw));
        Normalize(points, size);
        triangles = Simplify(points, triangles, maxTriangles, out points);
        _points = points;
        _triangles = triangles;
        _sharp = SharpEdges(points, triangles, sharpAngle);
    }

    protected internal override void Build(Scene3D s)
    {
        s.Camera(yaw: 28, pitch: 24);
        s.Ground(z: 0).Shadow();
        for (var t = 0; t < _sharp.Length; t++)
        {
            Vector3 a = _points[_triangles[3 * t]], b = _points[_triangles[3 * t + 1]], c = _points[_triangles[3 * t + 2]];
            s.Wall(_color, (a.X, a.Y, a.Z), (b.X, b.Y, b.Z), (c.X, c.Y, c.Z)).Face.SharpEdges = _sharp[t];
        }
    }

    // ---------------------------------------------------------------- lecture

    /// <summary>Sommets bruts, 3 par triangle, dans l'ordre du fichier (remis dans le sens de la normale du fichier).</summary>
    private static List<Vector3> Read(byte[] data)
    {
        var points = new List<Vector3>();
        if (data.Length >= 84 && 84 + 50L * BitConverter.ToUInt32(data, 80) == data.Length)
        {
            var count = BitConverter.ToUInt32(data, 80);
            for (var i = 0; i < count; i++)
            {
                var o = 84 + 50 * i;
                Vector3 F(int k) => new(BitConverter.ToSingle(data, o + k), BitConverter.ToSingle(data, o + k + 4), BitConverter.ToSingle(data, o + k + 8));
                AddTriangle(points, F(0), F(12), F(24), F(36));
            }
            return points;
        }
        // ASCII : "facet normal nx ny nz" puis trois "vertex x y z"
        var normal = Vector3.Zero;
        var corners = new Vector3[3];
        var n = 0;
        foreach (var line in Encoding.ASCII.GetString(data).Split('\n'))
        {
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5 && parts[0] == "facet") normal = Parse(parts, 2);
            else if (parts.Length >= 4 && parts[0] == "vertex")
            {
                if (n < 3) corners[n++] = Parse(parts, 1);
                if (n == 3)
                {
                    AddTriangle(points, normal, corners[0], corners[1], corners[2]);
                    n = 0;
                }
            }
        }
        return points;

        static Vector3 Parse(string[] p, int i) => new(
            float.Parse(p[i], CultureInfo.InvariantCulture), float.Parse(p[i + 1], CultureInfo.InvariantCulture), float.Parse(p[i + 2], CultureInfo.InvariantCulture));
    }

    private static void AddTriangle(List<Vector3> points, Vector3 normal, Vector3 a, Vector3 b, Vector3 c)
    {
        // Certains exports ont un sens faux mais une normale juste : la normale du fichier decide
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0) (b, c) = (c, b);
        points.Add(a);
        points.Add(b);
        points.Add(c);
    }

    // ---------------------------------------------------------------- maillage

    /// <summary>
    ///     Retire les triangles qui sortent loin du gros des points (plan de sol oublie dans un export Blender, par
    ///     exemple) : boite des centiles 0,5 a 99,5 par axe, elargie de son etendue de chaque cote.
    /// </summary>
    private static List<Vector3> DropOutliers(List<Vector3> raw)
    {
        var lo = new float[3];
        var hi = new float[3];
        var values = new float[raw.Count];
        for (var axis = 0; axis < 3; axis++)
        {
            for (var i = 0; i < raw.Count; i++) values[i] = raw[i][axis];
            Array.Sort(values);
            float p0 = values[(int)(values.Length * .005)], p1 = values[(int)((values.Length - 1) * .995)];
            var margin = Math.Max(p1 - p0, 1e-6f);
            (lo[axis], hi[axis]) = (p0 - margin, p1 + margin);
        }
        var kept = new List<Vector3>(raw.Count);
        for (var t = 0; t < raw.Count; t += 3)
        {
            var inside = true;
            for (var k = 0; k < 3 && inside; k++)
                for (var axis = 0; axis < 3 && inside; axis++)
                    inside = raw[t + k][axis] >= lo[axis] && raw[t + k][axis] <= hi[axis];
            if (!inside) continue;
            kept.Add(raw[t]);
            kept.Add(raw[t + 1]);
            kept.Add(raw[t + 2]);
        }
        return kept;
    }

    /// <summary>Fusionne les sommets identiques : indices partages, necessaires au voisinage des aretes.</summary>
    private static (Vector3[] Points, int[] Triangles) Weld(List<Vector3> raw)
    {
        var index = new Dictionary<Vector3, int>();
        var points = new List<Vector3>();
        var triangles = new int[raw.Count];
        for (var i = 0; i < raw.Count; i++)
        {
            if (!index.TryGetValue(raw[i], out var k))
            {
                k = points.Count;
                index[raw[i]] = k;
                points.Add(raw[i]);
            }
            triangles[i] = k;
        }
        return (points.ToArray(), triangles);
    }

    /// <summary>Plus grande dimension = size, coin minimal en (0, 0, 0) : le modele est pose sur le sol.</summary>
    private static void Normalize(Vector3[] points, double size)
    {
        var (min, max) = Bounds(points);
        var extent = Math.Max(max.X - min.X, Math.Max(max.Y - min.Y, max.Z - min.Z));
        var scale = extent > 0 ? (float)(size / extent) : 1f;
        for (var i = 0; i < points.Length; i++) points[i] = (points[i] - min) * scale;
    }

    /// <summary>
    ///     Regroupement de sommets sur une grille : les sommets d'une case fusionnent (moyenne), les triangles
    ///     aplatis disparaissent. La grille la plus fine qui tient dans maxTriangles est cherchee par dichotomie.
    /// </summary>
    private static int[] Simplify(Vector3[] points, int[] triangles, int maxTriangles, out Vector3[] result)
    {
        result = points;
        if (triangles.Length / 3 <= maxTriangles) return Clean(points, triangles);
        var (min, max) = Bounds(points);
        var extent = Math.Max(max.X - min.X, Math.Max(max.Y - min.Y, max.Z - min.Z));
        int lo = 2, hi = 1024;
        int[]? best = null;
        Vector3[]? bestPoints = null;
        while (lo <= hi)
        {
            var cells = (lo + hi) / 2;
            var (p, t) = Cluster(points, triangles, min, extent / cells);
            if (t.Length / 3 <= maxTriangles)
            {
                (best, bestPoints) = (t, p);
                lo = cells + 1;
            }
            else hi = cells - 1;
        }
        if (best == null) (bestPoints, best) = Cluster(points, triangles, min, extent / 2);
        result = bestPoints!;
        return best;
    }

    private static (Vector3[] Points, int[] Triangles) Cluster(Vector3[] points, int[] triangles, Vector3 min, float cell)
    {
        var cellOf = new Dictionary<(int, int, int), int>();
        var remap = new int[points.Length];
        var sums = new List<Vector3>();
        var counts = new List<int>();
        for (var i = 0; i < points.Length; i++)
        {
            var q = (points[i] - min) / cell;
            var key = ((int)q.X, (int)q.Y, (int)q.Z);
            if (!cellOf.TryGetValue(key, out var k))
            {
                k = sums.Count;
                cellOf[key] = k;
                sums.Add(Vector3.Zero);
                counts.Add(0);
            }
            sums[k] += points[i];
            counts[k]++;
            remap[i] = k;
        }
        var merged = new Vector3[sums.Count];
        for (var i = 0; i < merged.Length; i++) merged[i] = sums[i] / counts[i];
        var mapped = new int[triangles.Length];
        for (var i = 0; i < triangles.Length; i++) mapped[i] = remap[triangles[i]];
        return (merged, Clean(merged, mapped));
    }

    /// <summary>Retire les triangles a deux sommets confondus ou d'aire quasi nulle (Wall les refuse).</summary>
    private static int[] Clean(Vector3[] points, int[] triangles)
    {
        var kept = new List<int>(triangles.Length);
        for (var t = 0; t < triangles.Length; t += 3)
        {
            int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
            if (a == b || b == c || a == c) continue;
            if (Vector3.Cross(points[b] - points[a], points[c] - points[a]).LengthSquared() < 1e-8f) continue;
            kept.Add(a);
            kept.Add(b);
            kept.Add(c);
        }
        return kept.ToArray();
    }

    /// <summary>Arete vive : aucun triangle voisin dont l'angle avec celui-ci est sous sharpAngle (bord compris).</summary>
    private static bool[][] SharpEdges(Vector3[] points, int[] triangles, double sharpAngle)
    {
        var count = triangles.Length / 3;
        var normals = new Vector3[count];
        var edges = new Dictionary<(int, int), List<int>>();
        for (var t = 0; t < count; t++)
        {
            Vector3 a = points[triangles[3 * t]], b = points[triangles[3 * t + 1]], c = points[triangles[3 * t + 2]];
            normals[t] = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            for (var e = 0; e < 3; e++)
            {
                var key = EdgeKey(triangles, t, e);
                if (!edges.TryGetValue(key, out var list)) edges[key] = list = new List<int>(2);
                list.Add(t);
            }
        }
        var cos = (float)Math.Cos(sharpAngle * M.Deg);
        var sharp = new bool[count][];
        for (var t = 0; t < count; t++)
        {
            sharp[t] = new bool[3];
            for (var e = 0; e < 3; e++)
            {
                // Paroi mince ecrasee par la grille : 4 triangles par arete (dehors + dedans). Le voisin le plus proche decide.
                var smooth = false;
                foreach (var other in edges[EdgeKey(triangles, t, e)])
                    smooth |= other != t && Vector3.Dot(normals[t], normals[other]) >= cos;
                sharp[t][e] = !smooth;
            }
        }
        return sharp;

        static (int, int) EdgeKey(int[] tri, int t, int e)
        {
            int i = tri[3 * t + e], j = tri[3 * t + (e + 1) % 3];
            return i < j ? (i, j) : (j, i);
        }
    }

    private static (Vector3 Min, Vector3 Max) Bounds(Vector3[] points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var p in points)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        return (min, max);
    }
}

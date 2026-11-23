using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>Ce qui peut recevoir des volumes : la scene, un groupe mobile, un groupe de profondeur.</summary>
public abstract class Container3D
{
    internal readonly Scene3D Scene;

    private protected Container3D(Scene3D? scene) => Scene = scene ?? (Scene3D)this;

    private protected abstract void Add(List<Face> faces, bool wall);

    /// <summary>Boite alignee sur les axes, de min a max. open : faces non dessinees (cachees ou ouvertes) ; only : seules faces dessinees.</summary>
    public Body3D Box((double X, double Y, double Z) min, (double X, double Y, double Z) max, SKColor color,
        Side open = Side.None, Side only = Side.All, double opacity = 1)
    {
        var faces = Scene3D.BoxFaces(min, max, color, only & ~open, opacity);
        Add(faces, false);
        return Scene.Register(new Body3D(Scene, faces, min, max, this));
    }

    /// <summary>
    ///     Face plane libre (paroi, panneau). Points dans le sens trigonometrique vus du cote visible
    ///     (la normale sort vers l'observateur, regle de la main droite).
    /// </summary>
    public Face3D Wall(SKColor color, params (double X, double Y, double Z)[] points)
    {
        if (points.Length < 3) throw new ArgumentException("Mini3D : Wall demande au moins 3 points.");
        double nx = 0, ny = 0, nz = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Length];
            nx += (a.Y - b.Y) * (a.Z + b.Z);
            ny += (a.Z - b.Z) * (a.X + b.X);
            nz += (a.X - b.X) * (a.Y + b.Y);
        }
        var len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        if (len < 1e-9) throw new ArgumentException("Mini3D : Wall degeneree (points alignes).");
        var pts3 = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++) pts3[i] = M.V(points[i].X, points[i].Y, points[i].Z);
        var face = new Face { P = pts3, N = M.V(nx / len, ny / len, nz / len), C = color };
        face.Rest();
        var faces = new List<Face> { face };
        Add(faces, true);
        Scene.Register(new Body3D(Scene, faces, Min(points), Max(points), this) { IsWall = true });
        return new Face3D(Scene, face);
    }

    /// <summary>
    ///     Profil 2D extrude le long d'un axe, de from a to. Coordonnees du profil : (Y, Z) le long de X, (X, Z) le long
    ///     de Y, (X, Y) le long de Z. rim : couleur de la tranche (par defaut celle des faces).
    /// </summary>
    public Body3D Extrude(Path2D profile, Axis along, double from, double to, SKColor color, SKColor? rim = null, double opacity = 1)
    {
        var pts = profile.Resolve(Scene.Info.Quality);
        if (pts.Count < 3) throw new ArgumentException("Mini3D : Extrude demande un profil d'au moins 3 points.");
        double area = 0;
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            area += a.A * b.B - b.A * a.B;
        }
        var clockwise = area < 0;
        var sg = Math.Sign(to - from);

        // Point du profil (a, b) a la position t le long de l'axe ; sert aussi pour les normales
        Vector3 Map(double a, double b, double t) => along switch
        {
            Axis.X => M.V(t, a, b),
            Axis.Y => M.V(a, t, b),
            _ => M.V(a, b, t),
        };
        var capTo = new Vector3[pts.Count];
        var capFrom = new Vector3[pts.Count];
        var minV = new Vector3(float.MaxValue);
        var maxV = new Vector3(float.MinValue);
        for (var i = 0; i < pts.Count; i++)
        {
            var p = pts[i];
            var vTo = Map(p.A, p.B, to);
            var vFrom = Map(p.A, p.B, from);
            capTo[i] = vTo;
            capFrom[pts.Count - 1 - i] = vFrom;
            minV = Vector3.Min(minV, Vector3.Min(vTo, vFrom));
            maxV = Vector3.Max(maxV, Vector3.Max(vTo, vFrom));
        }
        var faces = new List<Face>(pts.Count + 2)
        {
            new() { P = capTo, N = Map(0, 0, sg), C = color, Op = opacity },
            new() { P = capFrom, N = Map(0, 0, -sg), C = color, Op = opacity },
        };
        var rimFaces = new List<(Face Face, double NA, double NB)>(pts.Count);
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            double da = b.A - a.A, db = b.B - a.B, len = Math.Sqrt(da * da + db * db);
            if (len < .01) continue;
            var n = clockwise ? (A: -db / len, B: da / len) : (A: db / len, B: -da / len);
            var rimFace = new Face
            {
                P = new[] { Map(a.A, a.B, to), Map(a.A, a.B, from), Map(b.A, b.B, from), Map(b.A, b.B, to) },
                N = Map(n.A, n.B, 0), C = rim ?? color, Op = opacity,
            };
            faces.Add(rimFace);
            rimFaces.Add((rimFace, n.A, n.B));
        }
        const double smoothCos = 0.848; // cos(32 deg) : cercles (>=12 cotes) lisses, hexagones/octogones vifs
        if (rimFaces.Count >= 3)
        {
            for (var k = 0; k < rimFaces.Count; k++)
            {
                var prev = (k - 1 + rimFaces.Count) % rimFaces.Count;
                var next = (k + 1) % rimFaces.Count;
                var cur = rimFaces[k];
                var pPrev = rimFaces[prev];
                var pNext = rimFaces[next];
                var smooth0 = cur.NA * pPrev.NA + cur.NB * pPrev.NB >= smoothCos;
                var smooth2 = cur.NA * pNext.NA + cur.NB * pNext.NB >= smoothCos;
                if (smooth0 || smooth2)
                {
                    cur.Face.SharpEdges = new[] { !smooth0, true, !smooth2, true };
                    cur.Face.SeamPeers = new Face?[] { smooth0 ? pPrev.Face : null, null, smooth2 ? pNext.Face : null, null };
                }
            }
        }
        foreach (var face in faces) face.Rest();
        Add(faces, false);
        return Scene.Register(new Body3D(Scene, faces, (minV.X, minV.Y, minV.Z), (maxV.X, maxV.Y, maxV.Z), this));
    }

    /// <summary>
    ///     Extrusion multi-sections (loft) le long d'un axe : relie une suite de coupes <c>(At, Profile)</c> par une
    ///     surface lissee (aretes douces automatiques + silhouette). Les coordonnees de chaque profil suivent la meme
    ///     convention que <see cref="Extrude"/> : (Y, Z) le long de X, (X, Z) le long de Y, (X, Y) le long de Z.
    /// </summary>
    public Body3D Loft(Axis along, SKColor color, params (double At, Path2D Profile)[] sections)
        => Loft(along, color, rim: null, opacity: 1, capStart: true, capEnd: true, sections);

    /// <summary>Version complete de <c>Loft</c> avec couleur de tranche, opacite et option de fermeture des bouts.</summary>
    public Body3D Loft(Axis along, SKColor color, SKColor? rim, double opacity, bool capStart, bool capEnd,
        params (double At, Path2D Profile)[] sections)
        => BuildLoft(along, color, rim, opacity, capStart, capEnd, smooth: false, stepsPerSpan: null, sections);

    /// <summary>
    ///     Loft lisse le long de l'axe par spline cubique monotone (PCHIP) entre les coupes : produit des surfaces
    ///     a double courbure (capots, toits, coques) dont la finesse longitudinale suit <see cref="Model3D.Quality"/>.
    /// </summary>
    public Body3D SmoothLoft(Axis along, SKColor color, params (double At, Path2D Profile)[] sections)
        => BuildLoft(along, color, rim: null, opacity: 1, capStart: true, capEnd: true, smooth: true, stepsPerSpan: null, sections);

    /// <summary>Version complete de <c>SmoothLoft</c> avec tranche, opacite, bouts et pas par intervalle.</summary>
    public Body3D SmoothLoft(Axis along, SKColor color, SKColor? rim, double opacity, bool capStart, bool capEnd,
        int? stepsPerSpan, params (double At, Path2D Profile)[] sections)
        => BuildLoft(along, color, rim, opacity, capStart, capEnd, smooth: true, stepsPerSpan, sections);

    private Body3D BuildLoft(Axis along, SKColor color, SKColor? rim, double opacity, bool capStart, bool capEnd,
        bool smooth, int? stepsPerSpan, params (double At, Path2D Profile)[] sections)
    {
        if (sections.Length < 2) throw new ArgumentException("Mini3D : Loft demande au moins 2 sections.");

        var quality = Scene.Info.Quality;
        int[]? sharedCounts = null;
        var firstProf = sections[0].Profile;
        if (sections.All(s => s.Profile.HasSameStructure(firstProf)))
        {
            sharedCounts = firstProf.ResolveCommandSegments(quality);
            for (var i = 1; i < sections.Length; i++)
            {
                var c = sections[i].Profile.ResolveCommandSegments(quality);
                for (var k = 0; k < sharedCounts.Length; k++)
                    if (c[k] > sharedCounts[k]) sharedCounts[k] = c[k];
            }
        }

        var wantNegArea = along == Axis.Y;
        var resolved = new List<(double At, List<(double A, double B)> Pts)>(sections.Length);
        var maxPts = 0;
        foreach (var (at, prof) in sections)
        {
            var pts = prof.Resolve(quality, sharedCounts);
            if (pts.Count < 3) throw new ArgumentException("Mini3D : chaque section de Loft doit avoir au moins 3 points.");
            var area = SignedArea(pts);
            if (wantNegArea ? area > 0 : area < 0) pts.Reverse();
            resolved.Add((at, pts));
            if (pts.Count > maxPts) maxPts = pts.Count;
        }
        if (resolved[0].At > resolved[^1].At) resolved.Reverse();

        var sameCount = resolved.All(s => s.Pts.Count == maxPts);
        var cols = maxPts;
        var ctrlRows = resolved.Count;
        var ctrlPts = new List<(double A, double B)>[ctrlRows];
        var ctrlAt = new double[ctrlRows];
        for (var r = 0; r < ctrlRows; r++)
        {
            ctrlPts[r] = sameCount ? resolved[r].Pts : ResampleClosed(resolved[r].Pts, cols);
            ctrlAt[r] = resolved[r].At;
        }

        var subSteps = smooth && ctrlRows >= 2
            ? Math.Max(1, stepsPerSpan ?? ResolveLoftSteps(quality))
            : 1;
        var rows = (ctrlRows - 1) * subSteps + 1;
        var grid = new Vector3[rows, cols];
        var minV = new Vector3(float.MaxValue);
        var maxV = new Vector3(float.MinValue);

        if (subSteps == 1)
        {
            for (var r = 0; r < ctrlRows; r++)
            {
                var t = ctrlAt[r];
                var pts = ctrlPts[r];
                for (var c = 0; c < cols; c++)
                {
                    var v = MapAlong(along, pts[c].A, pts[c].B, t);
                    grid[r, c] = v;
                    minV = Vector3.Min(minV, v);
                    maxV = Vector3.Max(maxV, v);
                }
            }
        }
        else
        {
            var slopesA = new double[ctrlRows, cols];
            var slopesB = new double[ctrlRows, cols];
            ComputePchipSlopes(ctrlAt, ctrlPts, cols, slopesA, slopesB);

            var outRow = 0;
            for (var seg = 0; seg < ctrlRows - 1; seg++)
            {
                var t0 = ctrlAt[seg];
                var dt = ctrlAt[seg + 1] - t0;
                var limit = seg == ctrlRows - 2 ? subSteps + 1 : subSteps;
                for (var s = 0; s < limit; s++)
                {
                    var u = (double)s / subSteps;
                    var u2 = u * u;
                    var u3 = u2 * u;
                    var h00 = 2 * u3 - 3 * u2 + 1;
                    var h10 = u3 - 2 * u2 + u;
                    var h01 = -2 * u3 + 3 * u2;
                    var h11 = u3 - u2;
                    var t = t0 + u * dt;

                    for (var c = 0; c < cols; c++)
                    {
                        var a0 = ctrlPts[seg][c].A;
                        var a1 = ctrlPts[seg + 1][c].A;
                        var b0 = ctrlPts[seg][c].B;
                        var b1 = ctrlPts[seg + 1][c].B;
                        var a = h00 * a0 + h10 * dt * slopesA[seg, c] + h01 * a1 + h11 * dt * slopesA[seg + 1, c];
                        var b = h00 * b0 + h10 * dt * slopesB[seg, c] + h01 * b1 + h11 * dt * slopesB[seg + 1, c];
                        var v = MapAlong(along, a, b, t);
                        grid[outRow, c] = v;
                        minV = Vector3.Min(minV, v);
                        maxV = Vector3.Max(maxV, v);
                    }
                    outRow++;
                }
            }
        }

        var sideColor = rim ?? color;
        var sideFaces = BuildGridTriangles(grid, rows, cols, closedRows: false, closedCols: true, sideColor, opacity);
        // Align side triangle winding with outward cap winding ((e_A x e_B) . e_t > 0 => side normals point outward)
        FlipTriangles(sideFaces);
        var allFaces = new List<Face>(sideFaces.Count + 2);

        var startP = new Vector3[cols];
        for (var c = 0; c < cols; c++) startP[c] = grid[0, cols - 1 - c];
        var endP = new Vector3[cols];
        for (var c = 0; c < cols; c++) endP[c] = grid[rows - 1, c];

        Face? startCap = null, endCap = null;
        if (capStart)
        {
            startCap = new Face { P = startP, N = MapAlong(along, 0, 0, -1), C = color, Op = opacity };
            allFaces.Add(startCap);
        }
        if (capEnd)
        {
            endCap = new Face { P = endP, N = MapAlong(along, 0, 0, 1), C = color, Op = opacity };
            allFaces.Add(endCap);
        }
        allFaces.AddRange(sideFaces);

        // Compute signed volume on the closed boundary even when caps are omitted so open lofts keep outward normals
        EnsureOutwardVolume(sideFaces, startCap, endCap, capStart ? null : startP, capEnd ? null : endP);
        WireSmoothSeams(sideFaces);
        foreach (var f in allFaces) f.Rest();

        Add(allFaces, false);
        return Scene.Register(new Body3D(Scene, allFaces, (minV.X, minV.Y, minV.Z), (maxV.X, maxV.Y, maxV.Z), this));
    }

    private static int ResolveLoftSteps(int quality)
    {
        var q = Math.Clamp(quality, 0, 100);
        if (q <= 20) return 1;
        return 2;
    }

    private static void ComputePchipSlopes(double[] at, List<(double A, double B)>[] pts, int cols,
        double[,] slopesA, double[,] slopesB)
    {
        var n = at.Length;
        for (var c = 0; c < cols; c++)
        {
            if (n == 2)
            {
                var dt = at[1] - at[0];
                var sa = Math.Abs(dt) > 1e-6 ? (pts[1][c].A - pts[0][c].A) / dt : 0;
                var sb = Math.Abs(dt) > 1e-6 ? (pts[1][c].B - pts[0][c].B) / dt : 0;
                slopesA[0, c] = slopesA[1, c] = sa;
                slopesB[0, c] = slopesB[1, c] = sb;
                continue;
            }

            for (var k = 0; k < n; k++)
            {
                if (k == 0)
                {
                    slopesA[0, c] = PchipEndSlope(at[1] - at[0], at[2] - at[1], pts[1][c].A - pts[0][c].A, pts[2][c].A - pts[1][c].A);
                    slopesB[0, c] = PchipEndSlope(at[1] - at[0], at[2] - at[1], pts[1][c].B - pts[0][c].B, pts[2][c].B - pts[1][c].B);
                }
                else if (k == n - 1)
                {
                    slopesA[k, c] = PchipEndSlope(at[k] - at[k - 1], at[k - 1] - at[k - 2], pts[k][c].A - pts[k - 1][c].A, pts[k - 1][c].A - pts[k - 2][c].A);
                    slopesB[k, c] = PchipEndSlope(at[k] - at[k - 1], at[k - 1] - at[k - 2], pts[k][c].B - pts[k - 1][c].B, pts[k - 1][c].B - pts[k - 2][c].B);
                }
                else
                {
                    slopesA[k, c] = PchipInteriorSlope(at[k] - at[k - 1], at[k + 1] - at[k], pts[k][c].A - pts[k - 1][c].A, pts[k + 1][c].A - pts[k][c].A);
                    slopesB[k, c] = PchipInteriorSlope(at[k] - at[k - 1], at[k + 1] - at[k], pts[k][c].B - pts[k - 1][c].B, pts[k + 1][c].B - pts[k][c].B);
                }
            }
        }
    }

    private static double PchipInteriorSlope(double h0, double h1, double d0, double d1)
    {
        if (Math.Abs(h0) < 1e-6 || Math.Abs(h1) < 1e-6) return 0;
        var s0 = d0 / h0;
        var s1 = d1 / h1;
        if (s0 * s1 <= 0) return 0;
        var w0 = 2 * h1 + h0;
        var w1 = h1 + 2 * h0;
        return (w0 + w1) / (w0 / s0 + w1 / s1);
    }

    private static double PchipEndSlope(double h0, double h1, double d0, double d1)
    {
        if (Math.Abs(h0) < 1e-6) return 0;
        var s0 = d0 / h0;
        if (Math.Abs(h1) < 1e-6) return s0;
        var s1 = d1 / h1;
        var d = ((2 * h0 + h1) * s0 - h0 * s1) / (h0 + h1);
        if (d * s0 <= 0) return 0;
        if (s0 * s1 <= 0 && Math.Abs(d) > Math.Abs(3 * s0)) return 3 * s0;
        return d;
    }

    /// <summary>
    ///     Surface de revolution (lathe) obtenue en faisant tourner un profil 2D <c>(Rayon, PositionAxe)</c>
    ///     autour d'un axe passant par <paramref name="center"/> :
    ///     <c>Axis.X</c> -> centre <c>(Y, Z)</c> ; <c>Axis.Y</c> -> centre <c>(X, Z)</c> ; <c>Axis.Z</c> -> centre <c>(X, Y)</c>.
    ///     Le decoupage angulaire s'adapte automatiquement au rayon maximal et a <see cref="Model3D.Quality"/>.
    /// </summary>
    public Body3D Lathe(Path2D profile, Axis around, (double A, double B) center, SKColor color,
        double fromDeg = 0, double toDeg = 360, int? segments = null, double opacity = 1)
    {
        var pts = profile.Resolve(Scene.Info.Quality);
        if (pts.Count < 2) throw new ArgumentException("Mini3D : Lathe demande un profil d'au moins 2 points.");

        var maxR = 0.0;
        foreach (var p in pts) maxR = Math.Max(maxR, Math.Abs(p.A));
        var sweep = Math.Abs(toDeg - fromDeg);
        var full = Math.Abs(sweep - 360.0) < 1e-3;
        var segs = Math.Max(full ? 3 : 1, segments ?? Path2D.ResolveSegments(maxR, sweep, Scene.Info.Quality));
        var closedProfile = Math.Abs(pts[0].A) > 1e-3 && Math.Abs(pts[^1].A) > 1e-3;

        var rows = full ? segs : segs + 1;
        var cols = pts.Count;
        var grid = new Vector3[rows, cols];
        var minV = new Vector3(float.MaxValue);
        var maxV = new Vector3(float.MinValue);

        for (var r = 0; r < rows; r++)
        {
            var deg = fromDeg + (toDeg - fromDeg) * r / segs;
            var rad = deg * M.Deg;
            double cs = Math.Cos(rad), sn = Math.Sin(rad);
            for (var c = 0; c < cols; c++)
            {
                var radR = Math.Max(0, pts[c].A);
                var axisT = pts[c].B;
                var a = center.A + radR * cs;
                var b = center.B + radR * sn;
                var v = MapAlong(around, a, b, axisT);
                grid[r, c] = v;
                minV = Vector3.Min(minV, v);
                maxV = Vector3.Max(maxV, v);
            }
        }

        var sideFaces = BuildGridTriangles(grid, rows, cols, closedRows: full, closedCols: closedProfile, color, opacity);
        if (sideFaces.Count == 0) throw new ArgumentException("Mini3D : Lathe degeneree (aucune face).");

        if (full)
        {
            EnsureOutwardVolume(sideFaces, null, null, null, null);
        }
        else
        {
            var area = SignedArea(pts);
            if (area * Math.Sign(toDeg - fromDeg) < 0) FlipTriangles(sideFaces);
        }

        WireSmoothSeams(sideFaces);
        foreach (var f in sideFaces) f.Rest();
        Add(sideFaces, false);
        return Scene.Register(new Body3D(Scene, sideFaces, (minV.X, minV.Y, minV.Z), (maxV.X, maxV.Y, maxV.Z), this));
    }

    /// <summary>
    ///     Boite aux coins et bords arrondis de <paramref name="min"/> a <paramref name="max"/>,
    ///     sensible a <see cref="Model3D.Quality"/>.
    /// </summary>
    public Body3D RoundedBox((double X, double Y, double Z) min, (double X, double Y, double Z) max,
        double radius, SKColor color, double opacity = 1)
    {
        var dx = max.X - min.X;
        var dy = max.Y - min.Y;
        var dz = max.Z - min.Z;
        if (dx <= 0 || dy <= 0 || dz <= 0)
            throw new ArgumentException($"Mini3D : RoundedBox vide ou inversee, min {min} doit etre < max {max}.");

        var r = Math.Min(radius, Math.Min(dx, Math.Min(dy, dz)) * 0.45);
        if (r <= 0.1) return Box(min, max, color, opacity: opacity);

        var t = Math.Clamp(Scene.Info.Quality, 0, 100) / 100.0;
        var arcSegs = Math.Max(2, (int)Math.Round(2 + t * 3));
        var bevelSegs = Math.Max(1, (int)Math.Round(1 + t));

        Path2D MakeRectProfile(double insetXY)
        {
            var x0 = min.X + insetXY;
            var x1 = max.X - insetXY;
            var y0 = min.Y + insetXY;
            var y1 = max.Y - insetXY;
            var cr = Math.Max(0.2, r - insetXY);
            return new Path2D()
                .Arc(x1 - cr, y0 + cr, cr, -90, 0, arcSegs)
                .Arc(x1 - cr, y1 - cr, cr, 0, 90, arcSegs)
                .Arc(x0 + cr, y1 - cr, cr, 90, 180, arcSegs)
                .Arc(x0 + cr, y0 + cr, cr, 180, 270, arcSegs);
        }

        var sections = new List<(double At, Path2D Profile)>(2 * bevelSegs + 2);
        for (var i = 0; i <= bevelSegs; i++)
        {
            var a = (90.0 * i / bevelSegs) * M.Deg;
            var z = min.Z + r * (1.0 - Math.Cos(a));
            var inset = r * (1.0 - Math.Sin(a));
            sections.Add((z, MakeRectProfile(inset)));
        }
        for (var i = 0; i <= bevelSegs; i++)
        {
            var a = (90.0 * i / bevelSegs) * M.Deg;
            var z = max.Z - r + r * Math.Sin(a);
            if (i == 0 && Math.Abs(sections[^1].At - z) < 1e-4) continue;
            var inset = r * (1.0 - Math.Cos(a));
            sections.Add((z, MakeRectProfile(inset)));
        }
        return Loft(Axis.Z, color, sections.ToArray());
    }

    private static Vector3 MapAlong(Axis along, double a, double b, double t) => along switch
    {
        Axis.X => M.V(t, a, b),
        Axis.Y => M.V(a, t, b),
        _ => M.V(a, b, t),
    };

    private static double SignedArea(List<(double A, double B)> pts)
    {
        double area = 0;
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            area += a.A * b.B - b.A * a.B;
        }
        return area;
    }

    private static List<(double A, double B)> ResampleClosed(List<(double A, double B)> pts, int targetCount)
    {
        var cum = new double[pts.Count + 1];
        for (var i = 0; i < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % pts.Count];
            cum[i + 1] = cum[i] + Math.Sqrt((b.A - a.A) * (b.A - a.A) + (b.B - a.B) * (b.B - a.B));
        }
        var total = cum[^1];
        if (total < 1e-6) return pts;

        var res = new List<(double A, double B)>(targetCount);
        var seg = 0;
        for (var k = 0; k < targetCount; k++)
        {
            var d = total * k / targetCount;
            while (seg + 1 < cum.Length - 1 && cum[seg + 1] < d) seg++;
            var len = cum[seg + 1] - cum[seg];
            var u = len > 1e-9 ? (d - cum[seg]) / len : 0;
            var a = pts[seg];
            var b = pts[(seg + 1) % pts.Count];
            res.Add((a.A + (b.A - a.A) * u, a.B + (b.B - a.B) * u));
        }
        return res;
    }

    private static List<Face> BuildGridTriangles(Vector3[,] grid, int rows, int cols,
        bool closedRows, bool closedCols, SKColor color, double opacity)
    {
        var rMax = closedRows ? rows : rows - 1;
        var cMax = closedCols ? cols : cols - 1;
        var faces = new List<Face>(rMax * cMax * 2);

        void TryAdd(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var n = Vector3.Cross(p1 - p0, p2 - p0);
            var len = n.Length();
            if (len < 1e-4f) return;
            faces.Add(new Face { P = new[] { p0, p1, p2 }, N = n / len, C = color, Op = opacity });
        }

        for (var r = 0; r < rMax; r++)
        {
            var r1 = (r + 1) % rows;
            for (var c = 0; c < cMax; c++)
            {
                var c1 = (c + 1) % cols;
                var p00 = grid[r, c];
                var p10 = grid[r1, c];
                var p11 = grid[r1, c1];
                var p01 = grid[r, c1];

                // Choix de la diagonale la plus courte pour eviter les triangles etires
                if ((p00 - p11).LengthSquared() <= (p10 - p01).LengthSquared())
                {
                    TryAdd(p00, p10, p11);
                    TryAdd(p00, p11, p01);
                }
                else
                {
                    TryAdd(p00, p10, p01);
                    TryAdd(p10, p11, p01);
                }
            }
        }
        return faces;
    }

    private static void EnsureOutwardVolume(List<Face> sideFaces, Face? startCap, Face? endCap,
        Vector3[]? virtualStartCap, Vector3[]? virtualEndCap)
    {
        if (sideFaces.Count == 0) return;
        var origin = sideFaces[0].P[0];
        double vol = 0;

        void Accum(Vector3[] p)
        {
            var p0 = p[0] - origin;
            for (var i = 1; i + 1 < p.Length; i++)
                vol += Vector3.Dot(p0, Vector3.Cross(p[i] - origin, p[i + 1] - origin));
        }

        foreach (var f in sideFaces) Accum(f.P);
        if (startCap != null) Accum(startCap.P);
        else if (virtualStartCap != null) Accum(virtualStartCap);
        if (endCap != null) Accum(endCap.P);
        else if (virtualEndCap != null) Accum(virtualEndCap);
        if (vol >= 0) return;

        FlipTriangles(sideFaces);
        if (startCap != null)
        {
            Array.Reverse(startCap.P);
            startCap.N = -startCap.N;
        }
        if (endCap != null)
        {
            Array.Reverse(endCap.P);
            endCap.N = -endCap.N;
        }
    }

    private static void FlipTriangles(List<Face> faces)
    {
        foreach (var f in faces)
        {
            (f.P[1], f.P[2]) = (f.P[2], f.P[1]);
            f.N = -f.N;
        }
    }

    private static void WireSmoothSeams(List<Face> faces)
    {
        const float smoothCos = 0.848f; // cos(32 deg)
        var edges = new Dictionary<(long, long, long, long, long, long), (Face F, int E)>(faces.Count * 2);

        static (long, long, long) Key(Vector3 v) =>
            ((long)Math.Round(v.X * 100), (long)Math.Round(v.Y * 100), (long)Math.Round(v.Z * 100));

        static (long, long, long, long, long, long) EdgeKey((long X, long Y, long Z) a, (long X, long Y, long Z) b)
            => a.CompareTo(b) <= 0 ? (a.X, a.Y, a.Z, b.X, b.Y, b.Z) : (b.X, b.Y, b.Z, a.X, a.Y, a.Z);

        foreach (var f in faces)
        {
            f.SharpEdges = new[] { true, true, true };
            f.SeamPeers = new Face?[3];
            for (var e = 0; e < 3; e++)
            {
                var k = EdgeKey(Key(f.P[e]), Key(f.P[(e + 1) % 3]));
                if (edges.TryGetValue(k, out var peer))
                {
                    if (Vector3.Dot(f.N, peer.F.N) >= smoothCos)
                    {
                        f.SharpEdges[e] = false;
                        peer.F.SharpEdges![peer.E] = false;
                        f.SeamPeers[e] = peer.F;
                        peer.F.SeamPeers![peer.E] = f;
                    }
                }
                else
                {
                    edges[k] = (f, e);
                }
            }
        }
    }

    private static (double, double, double) Min((double X, double Y, double Z)[] p) => (p.Min(q => q.X), p.Min(q => q.Y), p.Min(q => q.Z));
    private static (double, double, double) Max((double X, double Y, double Z)[] p) => (p.Max(q => q.X), p.Max(q => q.Y), p.Max(q => q.Z));
}

/// <summary>
///     Description d'une scene, passee a Model3D.Build. Declarer d'abord camera, cadrage, sol et canaux, puis les volumes.
///     L'ordre des volumes compte peu (tri automatique), l'ordre des dessins sur une face est l'ordre de peinture.
/// </summary>
public sealed class Scene3D : Container3D
{
    internal readonly ModelInfo Info = new();
    internal readonly Geometry Geometry;
    internal readonly List<Body3D> Bodies = new();
    internal readonly List<Group3D> Groups = new();
    internal List<(int Small, int Host, int Face)>? MountPlan;
    private (double Z, double? Step, (double, double)? X, (double, double)? Y, bool Shadow, (double, double)? From, (double, double)? To)? _ground;

    internal Scene3D(int quality = 50) : base(null)
    {
        Info.Quality = Math.Clamp(quality, 0, 100);
        Geometry = new Geometry(Info);
    }

    private protected override void Add(List<Face> faces, bool wall) => Geometry.Faces.AddRange(faces);

    internal Body3D Register(Body3D body)
    {
        Bodies.Add(body);
        return body;
    }

    /// <summary>Vue par defaut (degres). intro : lacet de depart, anime vers yaw a l'apparition. Tangage limite a [minPitch, maxPitch].</summary>
    public Scene3D Camera(double yaw = 28, double pitch = 24, double? intro = null, double minPitch = 6, double maxPitch = 80)
    {
        (Info.Yaw, Info.Pitch, Info.Intro, Info.MinPitch, Info.MaxPitch) = (yaw, pitch, intro, minPitch, maxPitch);
        return this;
    }

    /// <summary>
    ///     Cadrage : le point center du modele tombe en screen dans une vue de size unites (1 unite modele = 1 unite vue),
    ///     mise a l'echelle dans le controle et calee en haut. Sans appel, cadrage automatique sur la boite englobante.
    /// </summary>
    public Scene3D Frame((double X, double Y, double Z) center, (double Width, double Height) size, (double X, double Y)? screen = null)
    {
        Info.HasFrame = true;
        Info.Center = M.V(center.X, center.Y, center.Z);
        (Info.Width, Info.Height) = size;
        (Info.ScreenX, Info.ScreenY) = screen ?? (size.Width / 2, size.Height / 2);
        return this;
    }

    /// <summary>Sol en Z = z : quadrillage de pas grid (estompe vers les bords) sur les plages x et y (auto si absents).</summary>
    public Ground3D Ground(double z, double? grid = null, (double From, double To)? x = null, (double From, double To)? y = null)
    {
        _ground = (z, grid, x, y, false, null, null);
        return new Ground3D(this);
    }

    internal void SetShadow((double X, double Y)? from, (double X, double Y)? to)
    {
        if (_ground is { } g) _ground = g with { Shadow = true, From = from, To = to };
    }

    /// <summary>Entree pilotee par la vue (SetChannel), animee en douceur sur ms millisecondes. Valeur conseillee : 0..1.</summary>
    public Scene3D Input(string name, double ms = 600, double initial = 0)
    {
        if (Info.Index.ContainsKey(name)) throw new InvalidOperationException($"Mini3D : canal '{name}' declare deux fois.");
        var index = Info.Index.Count;
        Info.Index[name] = index;
        Info.Inputs.Add(new InputSpec(name, index, ms, initial));
        return this;
    }

    /// <summary>Canal derive, recalcule a chaque image a partir des canaux declares avant lui.</summary>
    public Scene3D Channel(string name, Func<Channels, double> value)
    {
        if (Info.Index.ContainsKey(name)) throw new InvalidOperationException($"Mini3D : canal '{name}' declare deux fois.");
        var index = Info.Index.Count;
        Info.Index[name] = index;
        Info.Derived.Add((index, value));
        return this;
    }

    /// <summary>Couleur des aretes, des traits sans couleur et du liserÃ© des vis.</summary>
    public Scene3D EdgeColor(SKColor color)
    {
        Info.Edge = color;
        return this;
    }

    /// <summary>Groupe rigide : ses volumes tournent et/ou glissent ensemble selon des canaux.</summary>
    public Group3D Group()
    {
        var group = new Group3D(this);
        Groups.Add(group);
        return group;
    }

    /// <summary>
    ///     Groupe trie comme un seul bloc a l'ancre (une cavite et son contenu) : ses parois sont peintes d'abord, dans
    ///     l'ordre, puis ses pieces, triees entre elles. A utiliser quand des volumes s'emboitent.
    /// </summary>
    public DepthGroup3D DepthGroup((double X, double Y, double Z) anchor)
    {
        var data = new DepthGroup { Anchor = M.V(anchor.X, anchor.Y, anchor.Z) };
        Geometry.Groups.Add(data);
        return new DepthGroup3D(this, data);
    }

    internal static List<Face> BoxFaces((double X, double Y, double Z) min, (double X, double Y, double Z) max, SKColor c, Side sides, double op)
    {
        var (x0, y0, z0) = min;
        var (x1, y1, z1) = max;
        if (x0 >= x1 || y0 >= y1 || z0 >= z1)
            throw new ArgumentException($"Mini3D : Box vide ou inversee, min {min} doit etre < max {max} sur chaque axe.");
        var faces = new List<Face>();

        void Add(Side side, Vector3 n, params Vector3[] p)
        {
            if ((sides & side) == 0) return;
            var face = new Face { P = p, N = n, C = c, Op = op };
            face.Rest();
            faces.Add(face);
        }

        Add(Side.Front, M.V(0, -1, 0), M.V(x0, y0, z0), M.V(x1, y0, z0), M.V(x1, y0, z1), M.V(x0, y0, z1));
        Add(Side.Back, M.V(0, 1, 0), M.V(x1, y1, z0), M.V(x0, y1, z0), M.V(x0, y1, z1), M.V(x1, y1, z1));
        Add(Side.Left, M.V(-1, 0, 0), M.V(x0, y1, z0), M.V(x0, y0, z0), M.V(x0, y0, z1), M.V(x0, y1, z1));
        Add(Side.Right, M.V(1, 0, 0), M.V(x1, y0, z0), M.V(x1, y1, z0), M.V(x1, y1, z1), M.V(x1, y0, z1));
        Add(Side.Top, M.V(0, 0, 1), M.V(x0, y0, z1), M.V(x1, y0, z1), M.V(x1, y1, z1), M.V(x0, y1, z1));
        Add(Side.Bottom, M.V(0, 0, -1), M.V(x0, y0, z0), M.V(x0, y1, z0), M.V(x1, y1, z0), M.V(x1, y0, z0));
        return faces;
    }

    /// <summary>Fin de Build : cibles de clic, pieces mobiles, decalques dynamiques, cadrage et sol automatiques.</summary>
    internal Geometry Finish(List<(int Small, int Host, int Face)>? mountPlan = null)
    {
        // Les cibles les plus precises gagnent : face, puis volume (le plus recent d'abord : un Mount avant son parent), puis groupe
        for (var i = Bodies.Count - 1; i >= 0; i--)
            if (Bodies[i].Target is { } id) SetTarget(Bodies[i].Faces, id);
        foreach (var group in Groups)
        {
            if (group.Target is { } id) SetTarget(group.Faces, id);
            if (group.Motion is { } motion)
            {
                motion.Parent = group.Parent?.Motion;
                motion.HostFace = group.HostFace;
                motion.Capture(group.Faces);
                Geometry.Motions.Add(motion);
            }
        }
        foreach (var face in Geometry.Drawings) face.Dynamic!.StaticCount = face.Dec.Count;
        foreach (var face in AllFaces())
            if (face.Target != null) { Info.HasTargets = true; break; }
        AutoMount(mountPlan);

        // Boite englobante : seulement si le cadrage ou le sol n'ont pas ete donnes explicitement
        var g = _ground;
        var min = Vector3.Zero;
        var max = Vector3.Zero;
        var (restMin, restMax) = (min, max);
        if (!Info.HasFrame || g is { Step: null } or { X: null } or { Y: null } || g is { Shadow: true } && (g.Value.From == null || g.Value.To == null))
        {
            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            Bounds(ref min, ref max);
            (restMin, restMax) = (min, max);
            if (Geometry.Motions.Count > 0)
            {
                // Pieces mobiles : la boite couvre aussi la pose "tout a 1" (tiroir sorti, capot ouvert)
                var open = Info.CreateChannels();
                foreach (var input in Info.Inputs) open.Values[input.Index] = 1;
                Info.Evaluate(open);
                foreach (var motion in Geometry.Motions) motion.Pose(open);
                Bounds(ref min, ref max);
                var rest = Info.CreateChannels();
                foreach (var motion in Geometry.Motions) motion.Pose(rest);
            }
            if (min.X > max.X) (min, max) = (Vector3.Zero, Vector3.One);
        }
        var radius = (max - min).Length() / 2;
        if (!Info.HasFrame)
        {
            Info.Center = (min + max) / 2;
            Info.Width = Math.Ceiling(radius * 2 * 1.1);
            Info.Height = Math.Ceiling(Info.Width * .6);
            (Info.ScreenX, Info.ScreenY) = (Info.Width / 2, Info.Height / 2);
        }
        if (g is { } ground)
        {
            // Quadrillage jusqu'au bord du degrade qui l'estompe (62 % de la largeur de vue autour du centre), cale sur le pas
            var reach = Info.Width * .62;
            var step = ground.Step ?? Nice(radius / 7);
            double Snap(double v) => Math.Round(v / step) * step;
            var pad = radius * .03;
            var (gx0, gx1) = ground.X ?? (Snap(Info.Center.X - reach), Snap(Info.Center.X + reach));
            var (gy0, gy1) = ground.Y ?? (Snap(Info.Center.Y - reach), Snap(Info.Center.Y + reach));
            // Ombre sous l'appareil au repos
            var (sx0, sy0) = ground.From ?? (restMin.X - pad, restMin.Y - pad);
            var (sx1, sy1) = ground.To ?? (restMax.X + pad, restMax.Y + pad);
            Info.Ground = new GroundSpec(ground.Z, step, gx0, gx1, gy0, gy1, ground.Shadow, sx0, sy0, sx1, sy1);
        }
        return Geometry;
    }

    /// <summary>Le volume touche la face par l'exterieur, la couvre en partie seulement, et est mince devant elle.</summary>
    internal static bool Pressed(Body3D body, Face face)
    {
        var n = Vector3.Normalize(face.N);
        var axis = Math.Abs(n.X) > .99f ? 0 : Math.Abs(n.Y) > .99f ? 1 : Math.Abs(n.Z) > .99f ? 2 : -1;
        if (axis < 0) return false;
        double Get((double X, double Y, double Z) p, int a) => a == 0 ? p.X : a == 1 ? p.Y : p.Z;
        double FaceMin(int a) => face.P.Min(p => a == 0 ? p.X : a == 1 ? p.Y : p.Z);
        double FaceMax(int a) => face.P.Max(p => a == 0 ? p.X : a == 1 ? p.Y : p.Z);
        var plane = FaceMin(axis);
        var outward = (axis == 0 ? n.X : axis == 1 ? n.Y : n.Z) > 0;
        var near = outward ? Get(body.Min, axis) : Get(body.Max, axis);
        if (Math.Abs(near - plane) > .5) return false;
        double faceArea = 1, bodyArea = 1, faceSize = 0;
        for (var a = 0; a < 3; a++)
        {
            if (a == axis) continue;
            var overlap = Math.Min(FaceMax(a), Get(body.Max, a)) - Math.Max(FaceMin(a), Get(body.Min, a));
            if (overlap <= .5) return false;
            faceArea *= FaceMax(a) - FaceMin(a);
            bodyArea *= Get(body.Max, a) - Get(body.Min, a);
            faceSize = Math.Max(faceSize, FaceMax(a) - FaceMin(a));
        }
        var thickness = Get(body.Max, axis) - Get(body.Min, axis);
        return bodyArea < faceArea / 2 && thickness < faceSize / 4;
    }

    /// <summary>Pas "rond" (1, 2 ou 5 x 10^n) proche de v.</summary>
    private static double Nice(double v)
    {
        if (v <= 0) return 1;
        var p = Math.Pow(10, Math.Floor(Math.Log10(v)));
        var f = v / p;
        return (f < 1.5 ? 1 : f < 3.5 ? 2 : f < 7.5 ? 5 : 10) * p;
    }

    private void Bounds(ref Vector3 min, ref Vector3 max)
    {
        foreach (var face in AllFaces())
            foreach (var p in face.P)
            {
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
    }

    /// <summary>
    ///     Un volume fixe plaque contre une face d'un autre volume fixe (bouton, poignee, plaque) devient un Mount de cette
    ///     face : peint juste apres elle, il ne peut plus passer dessous au tri du peintre.
    /// </summary>
    private void AutoMount(List<(int Small, int Host, int Face)>? plan)
    {
        // Le plan ne depend que de la geometrie : calcule a la premiere compilation, rejoue ensuite (Build est pure)
        if (plan == null)
        {
            plan = new();
            for (var s = 0; s < Bodies.Count; s++)
            {
                var small = Bodies[s];
                if (small.Owner != this || small.IsWall || small.IsMount) continue;
                (int Host, int Face, double Area)? best = null;
                for (var b = 0; b < Bodies.Count; b++)
                {
                    var big = Bodies[b];
                    if (b == s || big.Owner != this || big.IsMount) continue;
                    for (var f = 0; f < big.Faces.Count; f++)
                        if (Pressed(small, big.Faces[f]) && (best == null || Area(big.Faces[f]) > best.Value.Area))
                            best = (b, f, Area(big.Faces[f]));
                }
                if (best is { } host) plan.Add((s, host.Host, host.Face));
            }
            MountPlan = plan;
        }
        foreach (var (s, b, f) in plan)
        {
            var small = Bodies[s];
            var host = Bodies[b].Faces[f];
            foreach (var face in small.Faces) Geometry.Faces.Remove(face);
            (host.Kids ??= new()).AddRange(small.Faces);
            small.IsMount = true;
        }

        static double Area(Face f)
        {
            var sum = Vector3.Zero;
            for (var i = 0; i < f.P.Length; i++) sum += Vector3.Cross(f.P[i], f.P[(i + 1) % f.P.Length]);
            return sum.Length();
        }
    }

    internal IEnumerable<Face> AllFaces()
    {
        IEnumerable<Face> Walk(IEnumerable<Face> faces) => faces.SelectMany(f => f.Kids == null ? new[] { f } : new[] { f }.Concat(Walk(f.Kids)));
        return Walk(Geometry.Faces).Concat(Geometry.Groups.SelectMany(g => Walk(g.Walls).Concat(g.Parts.SelectMany(p => Walk(p.Faces)))));
    }

    private static void SetTarget(List<Face> faces, string id)
    {
        foreach (var face in faces)
        {
            face.Target ??= id;
            if (face.Kids != null) SetTarget(face.Kids, id);
        }
    }
}

public sealed class Ground3D
{
    private readonly Scene3D _scene;

    internal Ground3D(Scene3D scene) => _scene = scene;

    /// <summary>Ombre portee rectangulaire sur le sol, de from a to (X, Y) ; sans argument, sous la boite englobante.</summary>
    public Ground3D Shadow((double X, double Y)? from = null, (double X, double Y)? to = null)
    {
        _scene.SetShadow(from, to);
        return this;
    }
}

/// <summary>Volume construit (boite, extrusion, paroi) : acces a ses faces pour y dessiner.</summary>
public sealed class Body3D
{
    private readonly Scene3D _scene;
    internal readonly List<Face> Faces;
    internal readonly (double X, double Y, double Z) Min, Max;
    internal readonly Container3D Owner;
    internal string? Target;
    internal bool IsWall, IsMount;

    internal Body3D(Scene3D scene, List<Face> faces, (double, double, double) min, (double, double, double) max, Container3D owner)
    {
        _scene = scene;
        Faces = faces;
        Min = min;
        Max = max;
        Owner = owner;
    }

    /// <summary>La face dont la normale pointe vers side (Front = -Y, Back = +Y, Left = -X, Right = +X, Top = +Z, Bottom = -Z).</summary>
    public Face3D Face(Side side)
    {
        var dir = side switch
        {
            Side.Front => new Vector3(0, -1, 0),
            Side.Back => new Vector3(0, 1, 0),
            Side.Left => new Vector3(-1, 0, 0),
            Side.Right => new Vector3(1, 0, 0),
            Side.Top => new Vector3(0, 0, 1),
            Side.Bottom => new Vector3(0, 0, -1),
            _ => throw new ArgumentException("Mini3D : Face attend un seul cote (Front, Back, Left, Right, Top ou Bottom)."),
        };
        foreach (var face in Faces)
            if (Vector3.Dot(Vector3.Normalize(face.N), dir) > .99f) return new Face3D(_scene, face);
        throw new InvalidOperationException($"Mini3D : ce volume n'a pas de face {side} (retiree par open/only ?).");
    }

    /// <summary>Clic sur une face du volume : la vue leve Clicked(id). Survol : curseur main.</summary>
    public Body3D Clickable(string id)
    {
        Target = id;
        return this;
    }
}

/// <summary>Une face : on y dessine en 2D, dans les deux axes du monde de son plan (voir Canvas2D).</summary>
public sealed class Face3D
{
    private readonly Scene3D _scene;
    internal readonly Face Face;

    internal Face3D(Scene3D scene, Face face)
    {
        _scene = scene;
        Face = face;
    }

    /// <summary>Details fixes colles sur la face, peints dans l'ordre des appels.</summary>
    public Face3D Draw(Action<Canvas2D> draw)
    {
        draw(new Canvas2D(Face, _scene.Info, _scene.Geometry, null));
        return this;
    }

    /// <summary>Details redessines quand le canal change (la valeur est passee au dessin), peints apres les details fixes.</summary>
    public Face3D DrawWith(string channel, Action<Canvas2D, double> draw)
    {
        if (Face.Dynamic != null) throw new InvalidOperationException("Mini3D : une seule DrawWith par face.");
        Face.Dynamic = new FaceDrawing { Channel = _scene.Info.Channel(channel), Draw = draw };
        _scene.Geometry.Drawings.Add(Face);
        return this;
    }

    public Face3D Clickable(string id)
    {
        Face.Target = id;
        return this;
    }

    /// <summary>Cache les details de la face (pas la face) quand la regle est vraie : ex. masques par une piece fermee. Ignore en filaire.</summary>
    public Face3D HideDetailsWhen(Func<Channels, bool> rule)
    {
        Face.HideDecals = rule;
        return this;
    }

    /// <summary>Petite boite fixee sur la face (poignee, bouton) : triee avec elle, suit ses mouvements.</summary>
    public Body3D Mount((double X, double Y, double Z) min, (double X, double Y, double Z) max, SKColor color,
        Side open = Side.None, Side only = Side.All, double opacity = 1)
    {
        var faces = Scene3D.BoxFaces(min, max, color, only & ~open, opacity);
        (Face.Kids ??= new()).AddRange(faces);
        return _scene.Register(new Body3D(_scene, faces, min, max, _scene) { IsMount = true });
    }
}

/// <summary>Groupe rigide : pivot + rotation liee a un canal, et/ou translation liee a un canal.</summary>
public sealed class Group3D : Container3D
{
    internal readonly List<Face> Faces = new();
    internal readonly Group3D? Parent;
    internal Face? HostFace;
    internal Motion? Motion;
    internal string? Target;

    internal Group3D(Scene3D scene, Group3D? parent = null) : base(scene) => Parent = parent;

    /// <summary>Sous-groupe rigide enfant qui herite de la transformation du parent.</summary>
    public Group3D Group()
    {
        var child = new Group3D(Scene, this);
        Scene.Groups.Add(child);
        return child;
    }

    /// <summary>Attache les volumes du groupe directement aux Kids d'une face (peints apres elle, meme s'ils animent).</summary>
    public Group3D MountTo(Face3D host)
    {
        HostFace = host.Face;
        return this;
    }

    private protected override void Add(List<Face> faces, bool wall)
    {
        if (HostFace != null) (HostFace.Kids ??= new()).AddRange(faces);
        else Scene.Geometry.Faces.AddRange(faces);
        Faces.AddRange(faces);
    }

    /// <summary>Axe de rotation : parallele a axis, passant par at.</summary>
    public Group3D Pivot((double X, double Y, double Z) at, Axis axis)
    {
        Motion ??= new Motion();
        Motion.Pivot = M.V(at.X, at.Y, at.Z);
        Motion.Axis = axis;
        return this;
    }

    /// <summary>Angle (degres, sens trigonometrique autour de l'axe) = from + (to - from) * canal.</summary>
    public Group3D RotateWith(string channel, double from, double to)
    {
        Motion ??= new Motion();
        Motion.RotateChannel = Scene.Info.Channel(channel);
        Motion.From = from * M.Deg;
        Motion.To = to * M.Deg;
        return this;
    }

    /// <summary>Translation = by * canal (appliquee apres la rotation).</summary>
    public Group3D MoveWith(string channel, (double X, double Y, double Z) by)
    {
        Motion ??= new Motion();
        Motion.MoveChannel = Scene.Info.Channel(channel);
        Motion.Move = M.V(by.X, by.Y, by.Z);
        return this;
    }

    public Group3D Clickable(string id)
    {
        Target = id;
        return this;
    }
}

/// <summary>Groupe de profondeur (cavite) : Wall = parois peintes en premier, Box/Extrude/Free = pieces triees entre elles.</summary>
public sealed class DepthGroup3D : Container3D
{
    private readonly DepthGroup _data;

    internal DepthGroup3D(Scene3D scene, DepthGroup data) : base(scene) => _data = data;

    private protected override void Add(List<Face> faces, bool wall)
    {
        if (wall) _data.Walls.AddRange(faces);
        else _data.Parts.Add(new Part { Faces = faces });
    }

    /// <summary>Dessins 3D libres (lueur, cable) traites comme une piece placee a anchor pour le tri.</summary>
    public DepthGroup3D Free((double X, double Y, double Z) anchor, Action<Canvas3D> draw)
    {
        var part = new Part { Anchor = M.V(anchor.X, anchor.Y, anchor.Z) };
        _data.Parts.Add(part);
        draw(new Canvas3D(part, Scene.Info, Scene.Geometry));
        return this;
    }
}

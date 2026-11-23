using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Controles d'un modele avant affichage : Build qui echoue, budget de faces, faces degenerees ou non planes,
///     details qui debordent de leur face, volumes qui s'interpenetrent. Renvoie des messages lisibles (vide = rien a signaler).
/// </summary>
public static class Model3DValidator
{
    public static IReadOnlyList<string> Check(Model3D model, int? faceBudget = null)
    {
        var budget = faceBudget ?? (int)Math.Round(800 + model.Quality / 100.0 * 3200);
        var warnings = new List<string>();
        var scene = new Scene3D(model.Quality);
        Geometry geometry;
        try
        {
            model.Build(scene);
            geometry = scene.Finish();
            // Une pose : les details dynamiques (DrawWith) existent et sont controles aussi
            geometry.Pose(scene.Info.CreateChannels());
        }
        catch (Exception e)
        {
            warnings.Add($"Build echoue : {e.Message}");
            return warnings;
        }

        using (geometry)
        {
            var faces = scene.AllFaces().ToList();
            if (faces.Count > budget)
                warnings.Add($"{faces.Count} faces : au-dela du budget de {budget} (rotation plus lente). Simplifier les profils ou les arcs.");

            foreach (var face in faces)
            {
                var n = Vector3.Normalize(face.N);
                if (Area(face) < 1e-3) warnings.Add($"{Describe(face)} : surface nulle.");
                var off = face.P.Max(p => Math.Abs(Vector3.Dot(p - face.P[0], n)));
                if (off > .5) warnings.Add($"{Describe(face)} : non plane (ecart {off:F1}).");
                if (face.Dec.Count == 0) continue;
                var outline = face.P.Select(face.Local).ToArray();
                float minU = outline.Min(p => p.X) - 1, maxU = outline.Max(p => p.X) + 1;
                float minV = outline.Min(p => p.Y) - 1, maxV = outline.Max(p => p.Y) + 1;
                var outside = face.Dec.Count(d => d.Local != null && d.Local.Any(f => f.Any(p => p.X < minU || p.X > maxU || p.Y < minV || p.Y > maxV)));
                if (outside > 0) warnings.Add($"{Describe(face)} : {outside} detail(s) deborde(nt) de la face (coordonnees du plan : voir Canvas2D).");
            }

            // Interpenetrations : volumes pleins d'un meme niveau (premier plan + groupes mobiles, ou un meme groupe de profondeur)
            foreach (var level in scene.Bodies.Where(b => !b.IsWall && !b.IsMount).GroupBy(b => b.Owner is DepthGroup3D ? b.Owner : scene))
            {
                var bodies = level.ToList();
                for (var i = 0; i < bodies.Count; i++)
                    for (var j = i + 1; j < bodies.Count; j++)
                        if (Overlap(bodies[i], bodies[j]) is { } depth)
                            warnings.Add($"Volumes {Box(bodies[i])} et {Box(bodies[j])} s'interpenetrent ({depth:F1}) : le tri peut se tromper. Les faire se toucher, ou utiliser DepthGroup.");
            }

            // Premier plan, trie face par face au centroide : un volume mince plaque sur une grande face peut passer dessous.
            // Les volumes fixes sont rattaches automatiquement (Finish) ; restent ceux qui bougent.
            var top = scene.Bodies.Where(b => b.Owner is not DepthGroup3D && !b.IsMount).ToList();
            foreach (var big in top)
                foreach (var face in big.Faces)
                    foreach (var small in top)
                        if (small != big && !small.IsWall && Scene3D.Pressed(small, face))
                            warnings.Add($"Volume {Box(small)} plaque contre une grande face ({Describe(face)}) : le peintre peut le cacher. " +
                                "Ouvrir cette face (open:) et mettre une Wall en retrait.");
        }
        return warnings;
    }

    private static double? Overlap(Body3D a, Body3D b)
    {
        var x = Math.Min(a.Max.X, b.Max.X) - Math.Max(a.Min.X, b.Min.X);
        var y = Math.Min(a.Max.Y, b.Max.Y) - Math.Max(a.Min.Y, b.Min.Y);
        var z = Math.Min(a.Max.Z, b.Max.Z) - Math.Max(a.Min.Z, b.Min.Z);
        var depth = Math.Min(x, Math.Min(y, z));
        return depth > .5 ? depth : null;
    }

    private static string Box(Body3D b) => $"({b.Min.X:0.#}, {b.Min.Y:0.#}, {b.Min.Z:0.#})-({b.Max.X:0.#}, {b.Max.Y:0.#}, {b.Max.Z:0.#})";

    private static double Area(Face face)
    {
        var sum = Vector3.Zero;
        for (var i = 0; i < face.P.Length; i++) sum += Vector3.Cross(face.P[i], face.P[(i + 1) % face.P.Length]);
        return sum.Length() / 2;
    }

    private static string Describe(Face face)
    {
        var c = face.P.Aggregate(Vector3.Zero, (s, p) => s + p) / face.P.Length;
        var n = face.N;
        return $"Face de normale ({n.X:0.##}, {n.Y:0.##}, {n.Z:0.##}) centree en ({c.X:0}, {c.Y:0}, {c.Z:0})";
    }
}

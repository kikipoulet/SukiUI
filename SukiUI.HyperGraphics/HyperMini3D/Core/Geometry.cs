using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

// Forme compilee d'un modele (sortie de Scene3D) : faces planes, decalques, pieces mobiles et liaisons aux canaux.
// Tout vit sur le thread de rendu ; les chemins Skia natifs sont crees a la premiere image et liberes par Dispose.

internal static class M
{
    public const double Deg = Math.PI / 180;

    public static Vector3 V(double x, double y, double z) => new((float)x, (float)y, (float)z);

    public static SKColor Lerp(SKColor a, SKColor b, double t) => new(
        (byte)(a.Red + (b.Red - a.Red) * t), (byte)(a.Green + (b.Green - a.Green) * t), (byte)(a.Blue + (b.Blue - a.Blue) * t));
}

internal readonly record struct DepthItem<T>(double Depth, int Index, T? Item) : IComparable<DepthItem<T>> where T : class
{
    public int CompareTo(DepthItem<T> other)
    {
        var result = Depth.CompareTo(other.Depth);
        return result != 0 ? result : Index.CompareTo(other.Index);
    }
}

internal readonly record struct OrderEntry(double Depth, int Index, Face? Face, DepthGroup? Group) : IComparable<OrderEntry>
{
    public int CompareTo(OrderEntry other)
    {
        var result = Depth.CompareTo(other.Depth);
        return result != 0 ? result : Index.CompareTo(other.Index);
    }
}

internal sealed class Face
{
    public Vector3[] P = Array.Empty<Vector3>();
    public Vector3 N;
    public SKColor C;
    public double Op = 1;
    public List<Decal> Dec = new();
    public List<Face>? Kids;
    public string? Target;
    /// <summary>Decalques caches (pas la face) quand la regle est vraie, sauf en filaire.</summary>
    public Func<Channels, bool>? HideDecals;
    public FaceDrawing? Dynamic;
    /// <summary>
    ///     Maillage importe (STL) : SharpEdges[i] = arete P[i] -> P[i + 1] vive. Les autres aretes prennent la couleur
    ///     de la face (cache les jours entre triangles). Null : contour complet, comme avant.
    /// </summary>
    public bool[]? SharpEdges;

    // Repere de la pose de repos : les decalques y sont exprimes une fois pour toutes
    public Vector3 RestO, RestU, RestV, RestN;
    public Vector3 RestP0;
    public Axis Dominant;

    // Repere courant (mis a jour seulement si la face appartient a un Motion qui bouge)
    public Vector3 CurO, CurU, CurV;
    public bool BasisDirty;

    // Caches par image / camera
    public Vector3? Center;
    public DepthItem<Face>[] KidsOrder = Array.Empty<DepthItem<Face>>();
    public long OrderVersion = -1;
    public SKMatrix Transform;
    public long TransformCamera = -1;
    /// <summary>Contour projete de la derniere image (repere vue), pour le hit-test.</summary>
    public Vector2[] Screen = Array.Empty<Vector2>();

    /// <summary>Fige le repere de repos ; a appeler une fois les points et la normale connus.</summary>
    public void Rest()
    {
        (RestO, RestU, RestV, RestN) = Basis(this);
        CurO = RestO;
        CurU = RestU;
        CurV = RestV;
        BasisDirty = false;
        RestP0 = P[0];
        var n = N;
        Dominant = Math.Abs(n.X) >= Math.Abs(n.Y) && Math.Abs(n.X) >= Math.Abs(n.Z) ? Axis.X
            : Math.Abs(n.Y) >= Math.Abs(n.Z) ? Axis.Y : Axis.Z;
    }

    public void EnsureBasis()
    {
        if (!BasisDirty) return;
        (CurO, CurU, CurV, _) = Basis(this);
        BasisDirty = false;
    }

    /// <summary>
    ///     Point 3D de la face (au repos) a partir de ses coordonnees 2D : les deux axes du monde dans son plan,
    ///     (Y, Z) si la face regarde vers X, (X, Z) vers Y, (X, Y) vers Z.
    /// </summary>
    public Vector3 PlanePoint(double a, double b)
    {
        double nx = RestN.X, ny = RestN.Y, nz = RestN.Z;
        double x0 = RestP0.X, y0 = RestP0.Y, z0 = RestP0.Z;
        return Dominant switch
        {
            Axis.X => M.V(x0 - (ny * (a - y0) + nz * (b - z0)) / nx, a, b),
            Axis.Y => M.V(a, y0 - (nx * (a - x0) + nz * (b - z0)) / ny, b),
            _ => M.V(a, b, z0 - (nx * (a - x0) + ny * (b - y0)) / nz),
        };
    }

    public Vector2 Local(Vector3 p) => new(Vector3.Dot(p - RestO, RestU), Vector3.Dot(p - RestO, RestV));

    /// <summary>Repere d'une face : origine P[0], U le long de sa premiere arete, V = N x U. Suit la face quand elle pivote.</summary>
    public static (Vector3 O, Vector3 U, Vector3 V, Vector3 N) Basis(Face face)
    {
        var o = face.P[0];
        var n = Vector3.Normalize(face.N);
        var u = Vector3.Zero;
        for (var i = 1; i < face.P.Length; i++)
        {
            var edge = face.P[i] - o;
            if (edge.LengthSquared() < 1e-6f) continue;
            u = Vector3.Normalize(edge - n * Vector3.Dot(edge, n));
            break;
        }
        return (o, u, Vector3.Cross(n, u), n);
    }
}

/// <summary>
///     Dessin plan. Colle sur une face, il est exprime dans le repere 2D de la face (Local) et son chemin Skia (Path)
///     est construit une seule fois : seule la matrice de la face bouge avec la camera.
///     Libre (groupe de profondeur), il reste en 3D (F) et se projette a chaque image.
/// </summary>
internal sealed class Decal
{
    public Vector3[][] F = Array.Empty<Vector3[]>();
    public Vector2[][]? Local;
    public SKColor? Fill;
    public SKColor? Stroke;
    public double W = .5;
    public double Op = 1;
    public bool Line;
    /// <summary>Canal qui module l'opacite (lueur, non teintee en theme sombre), -1 : aucun.</summary>
    public int Glow = -1;
    public SKPath? Path;
    public bool PathDirty;
}

internal sealed class ColorBinding
{
    public Decal Decal = null!;
    public int Channel;
    public SKColor From, To;
    public double Last = double.NaN;
}

/// <summary>Decalques redessines quand un canal change (ex. piston) ; ils suivent les decalques fixes de la face.</summary>
internal sealed class FaceDrawing : IDisposable
{
    public int Channel;
    public Action<Canvas2D, double> Draw = null!;
    public int StaticCount;
    public double Last = double.NaN;
    public readonly Stack<SKPath> Pool = new();

    public void Dispose()
    {
        while (Pool.Count > 0) Pool.Pop().Dispose();
    }
}

/// <summary>Objet d'un groupe de profondeur : faces triees entre elles, ou decalques libres places a Anchor.</summary>
internal sealed class Part
{
    public List<Face> Faces = new();
    public List<Decal> Free = new();
    public Vector3? Anchor;
    public Vector3? Center;
    public DepthItem<Face>[] Order = Array.Empty<DepthItem<Face>>();
}

/// <summary>Ensemble trie comme un seul bloc (a son ancre), puis trie a l'interieur : evite les erreurs du peintre.</summary>
internal sealed class DepthGroup
{
    public List<Face> Walls = new();
    public List<Part> Parts = new();
    public Vector3 Anchor;
    public DepthItem<Part>[] Order = Array.Empty<DepthItem<Part>>();
    public long OrderVersion = -1;
}

/// <summary>Piece rigide : rotation autour d'un axe passant par Pivot, puis translation, pilotees par des canaux.</summary>
internal sealed class Motion
{
    private FacePose[] _faces = Array.Empty<FacePose>();
    public Motion? Parent;
    public Face? HostFace;
    public Vector3 Pivot;
    public Axis Axis;
    public int RotateChannel = -1;
    public double From, To;
    public int MoveChannel = -1;
    public Vector3 Move;
    private (double Angle, double Shift, long ParentVer) _last = (double.NaN, double.NaN, -1);
    private Matrix4x4 _world = Matrix4x4.Identity;
    private long _version;

    public void Capture(List<Face> faces) => _faces = faces.ConvertAll(CaptureFace).ToArray();

    public bool Pose(Channels channels)
    {
        var angle = RotateChannel >= 0 ? From + (To - From) * channels.Values[RotateChannel] : 0;
        var shift = MoveChannel >= 0 ? channels.Values[MoveChannel] : 0;
        var parentVer = Parent?._version ?? 0;
        if (angle == _last.Angle && shift == _last.Shift && parentVer == _last.ParentVer) return false;
        _last = (angle, shift, parentVer);
        _version++;

        var local = Matrix4x4.Identity;
        if (RotateChannel >= 0 && Math.Abs(angle) > 1e-7)
        {
            var a = (float)angle;
            var rot = Axis switch
            {
                Axis.X => Matrix4x4.CreateRotationX(a),
                Axis.Y => Matrix4x4.CreateRotationY(a),
                _ => Matrix4x4.CreateRotationZ(a),
            };
            local = Matrix4x4.CreateTranslation(-Pivot) * rot * Matrix4x4.CreateTranslation(Pivot);
        }
        if (MoveChannel >= 0 && shift != 0)
            local *= Matrix4x4.CreateTranslation(Move * (float)shift);

        _world = Parent != null ? local * Parent._world : local;
        foreach (var face in _faces) Apply(face);
        if (HostFace != null) HostFace.OrderVersion = -1;
        return true;
    }

    /// <summary>Seules les faces bougent : leurs decalques sont en coordonnees locales et suivent la face.</summary>
    private void Apply(FacePose pose)
    {
        var face = pose.Face;
        for (var i = 0; i < face.P.Length; i++)
            face.P[i] = Vector3.Transform(pose.P0[i], _world);
        face.N = Vector3.Normalize(Vector3.TransformNormal(pose.N0, _world));
        face.Center = null;
        face.OrderVersion = -1;
        face.TransformCamera = -1;
        face.BasisDirty = true;
        foreach (var child in pose.Kids) Apply(child);
    }

    private static FacePose CaptureFace(Face face) => new(
        face,
        (Vector3[])face.P.Clone(),
        face.N,
        face.Kids?.ConvertAll(CaptureFace).ToArray() ?? Array.Empty<FacePose>());

    private sealed record FacePose(Face Face, Vector3[] P0, Vector3 N0, FacePose[] Kids);
}

internal sealed record InputSpec(string Name, int Index, double Milliseconds, double Initial);

internal sealed record GroundSpec(double Z, double Step, double X0, double X1, double Y0, double Y1, bool Shadow,
    double ShadowX0, double ShadowY0, double ShadowX1, double ShadowY1);

/// <summary>Ce qui ne depend pas de la geometrie : canaux, camera, cadrage, sol. Garde meme quand la scene est liberee.</summary>
internal sealed class ModelInfo
{
    public readonly Dictionary<string, int> Index = new();
    public readonly List<InputSpec> Inputs = new();
    public readonly List<(int Index, Func<Channels, double> Value)> Derived = new();
    public double Yaw = 28, Pitch = 24, MinPitch = 6, MaxPitch = 80;
    public double? Intro;
    public bool HasFrame;
    public Vector3 Center;
    public double Width = 680, Height = 400, ScreenX = 340, ScreenY = 200;
    public GroundSpec? Ground;
    public SKColor Edge = new(61, 74, 107);
    public bool HasTargets;

    public int Channel(string name) =>
        Index.TryGetValue(name, out var i)
            ? i
            : throw new InvalidOperationException($"Mini3D : canal '{name}' inconnu. Declare-le avant usage avec s.Input(\"{name}\") ou s.Channel(\"{name}\", c => ...).");

    public int InputSlot(string name)
    {
        for (var i = 0; i < Inputs.Count; i++)
            if (Inputs[i].Name == name) return i;
        throw new InvalidOperationException($"Mini3D : '{name}' n'est pas une entree du modele (s.Input). Entrees : {string.Join(", ", Inputs.ConvertAll(x => x.Name))}.");
    }

    public Channels CreateChannels()
    {
        var channels = new Channels(new double[Index.Count], Index);
        foreach (var input in Inputs) channels.Values[input.Index] = input.Initial;
        Evaluate(channels);
        return channels;
    }

    /// <summary>Recalcule les canaux derives, dans l'ordre de declaration.</summary>
    public void Evaluate(Channels channels)
    {
        foreach (var (index, value) in Derived) channels.Values[index] = value(channels);
    }
}

/// <summary>Scene compilee : faces du premier plan, groupes de profondeur, pieces mobiles, decalques dynamiques.</summary>
internal sealed class Geometry : IDisposable
{
    public readonly ModelInfo Info;
    public readonly List<Face> Faces = new();
    public readonly List<DepthGroup> Groups = new();
    public readonly List<Motion> Motions = new();
    public readonly List<Face> Drawings = new();
    public readonly List<ColorBinding> Colors = new();
    public OrderEntry[] Order = Array.Empty<OrderEntry>();
    public long MotionVersion;

    public Geometry(ModelInfo info) => Info = info;

    /// <summary>Met les pieces mobiles et les decalques lies dans l'etat des canaux ; ne touche que ce qui a change.</summary>
    public void Pose(Channels channels)
    {
        var moved = false;
        foreach (var motion in Motions) moved |= motion.Pose(channels);
        if (moved) MotionVersion++;
        foreach (var face in Drawings)
        {
            var drawing = face.Dynamic!;
            var value = channels.Values[drawing.Channel];
            if (value == drawing.Last) continue;
            drawing.Last = value;
            var decals = face.Dec;
            for (var i = drawing.StaticCount; i < decals.Count; i++)
            {
                if (decals[i].Path is { } path)
                {
                    path.Rewind();
                    drawing.Pool.Push(path);
                    decals[i].Path = null;
                }
            }
            if (decals.Count > drawing.StaticCount) decals.RemoveRange(drawing.StaticCount, decals.Count - drawing.StaticCount);
            drawing.Draw(new Canvas2D(face, Info, null, channels, drawing.Pool), value);
        }
        foreach (var binding in Colors)
        {
            var value = channels.Values[binding.Channel];
            if (value == binding.Last) continue;
            binding.Last = value;
            binding.Decal.Fill = M.Lerp(binding.From, binding.To, value);
        }
    }

    public void Dispose()
    {
        foreach (var face in Drawings) face.Dynamic?.Dispose();
        DisposeFaces(Faces);
        foreach (var group in Groups)
        {
            DisposeFaces(group.Walls);
            foreach (var part in group.Parts)
            {
                DisposeFaces(part.Faces);
                foreach (var decal in part.Free) decal.Path?.Dispose();
            }
        }

        static void DisposeFaces(List<Face> faces)
        {
            foreach (var face in faces)
            {
                foreach (var decal in face.Dec) decal.Path?.Dispose();
                if (face.Kids != null) DisposeFaces(face.Kids);
            }
        }
    }
}

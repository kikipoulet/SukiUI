using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Rendu sans moteur 3D : projection orthographique (camera = 3 vecteurs), faces dos a l'oeil ignorees, tri du plus
///     lointain au plus proche (algorithme du peintre), ombrage par la normale. Ne depend que de SkiaSharp.
///     Les decalques d'une face sont des chemins construits une fois dans son repere 2D, dessines sous la matrice de la
///     face : en orthographique, projeter un plan est une simple application affine.
///     La scene se libere (DropScene) et se reconstruit a la demande ; seuls les contours cliquables sont gardes.
/// </summary>
internal sealed class Renderer : IDisposable
{
    private static readonly SKColor WireColor = new(0x37, 0x8A, 0xDD);
    private static readonly Comparer<(float MinX, int Idx)> ByMinXComparer =
        Comparer<(float MinX, int Idx)>.Create(static (x, y) => x.MinX.CompareTo(y.MinX));

    private readonly Model3D _model;
    public readonly ModelInfo Info;
    public readonly Channels Channels;
    public double Yaw, Pitch;
    public bool Wire, Dark;
    public int? Quality;
    public double Scale = 1;
    public Vector2 Offset;

    private readonly List<Face> _hitFaces = new();
    private readonly SKPaint _fill = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _stroke = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeJoin = SKStrokeJoin.Round, StrokeMiter = 10 };
    private readonly SKPath _path = new() { FillType = SKPathFillType.EvenOdd };
    private readonly SKPath _edges = new();
    private readonly Vector3 _center;
    private (Vector2[] Shape, string Target)[] _restHits = Array.Empty<(Vector2[], string)>();
    private Dictionary<uint, SKColor> _darkTones = new();
    private Geometry? _geometry;
    private SKPath? _grid, _shadow;
    private SKShader? _lightGrid, _darkGrid;
    private Vector3 _eye, _right, _up, _light;
    private (double Yaw, double Pitch)? _cameraKey;
    private long _cameraVersion, _groundVersion = -1;
    private int _orderCount = -1;
    private long _orderCameraVersion = -1, _orderMotionVersion = -1;
    private bool _orderWire;
    private (float MinX, float MaxX, float MinY, float MaxY)[] _rects = Array.Empty<(float, float, float, float)>();
    private OrderEntry[] _sorted = Array.Empty<OrderEntry>();
    private int[] _inDeg = Array.Empty<int>();
    private List<int>[] _out = Array.Empty<List<int>>();
    private (float MinX, int Idx)[] _byMinX = Array.Empty<(float, int)>();
    private int[] _tIndex = Array.Empty<int>();
    private int[] _tLow = Array.Empty<int>();
    private int[] _tStack = Array.Empty<int>();
    private int[] _dfsV = Array.Empty<int>();
    private int[] _dfsEdge = Array.Empty<int>();
    private int[] _sccId = Array.Empty<int>();
    private int[] _sccInDeg = Array.Empty<int>();
    private bool[] _tOnStack = Array.Empty<bool>();
    private List<int>[] _sccNodes = Array.Empty<List<int>>();
    private readonly PriorityQueue<int, int> _pq = new();

    public Renderer(Model3D model)
    {
        _model = model;
        Info = model.Info;
        Channels = Info.CreateChannels();
        Yaw = Info.Yaw * M.Deg;
        Pitch = Info.Pitch * M.Deg;
        _center = Info.Center;
    }

    public bool HasScene => _geometry != null;

    /// <summary>Mise a l'echelle de la vue dans un controle : calee en haut, l'espace libre reste dessous.</summary>
    public void Layout(double width, double height)
    {
        Scale = Math.Min(width / Info.Width, height / Info.Height);
        Offset = new Vector2((float)((width - Info.Width * Scale) / 2), 0);
    }

    /// <summary>Dessine l'image, en unites de vue (Info.Width x Info.Height) : le canvas porte deja Offset et Scale.</summary>
    public void DrawFrame(SKCanvas canvas)
    {
        _hitFaces.Clear();
        SetCamera();
        Info.Evaluate(Channels);

        var geometry = _geometry ??= _model.Compile(Quality);
        geometry.Pose(Channels);
        DrawGround(canvas);
        var faces = geometry.Faces;
        var maxCount = faces.Count + geometry.Groups.Count;
        if (geometry.Order.Length < maxCount) geometry.Order = new OrderEntry[maxCount];
        var order = geometry.Order;
        if (_orderCount < 0 || _orderWire != Wire || _orderCameraVersion != _cameraVersion || _orderMotionVersion != geometry.MotionVersion)
        {
            var count = 0;
            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                if (!Wire && Vector3.Dot(face.N, _eye) <= 1e-4f) continue;
                order[count++] = new OrderEntry(Depth(face.Center ??= Centroid(face.P)), i, face, null);
            }
            for (var i = 0; i < geometry.Groups.Count; i++)
            {
                var group = geometry.Groups[i];
                order[count++] = new OrderEntry(Depth(group.Anchor), faces.Count + i, null, group);
            }
            Array.Sort(order, 0, count);
            if (!Wire && count > 1 && count <= 8000) RefineOrder(order, count);
            _orderCount = count;
            _orderWire = Wire;
            _orderCameraVersion = _cameraVersion;
            _orderMotionVersion = geometry.MotionVersion;
        }
        for (var i = 0; i < _orderCount; i++)
        {
            var item = order[i];
            if (item.Face is { } face) DrawFace(canvas, face, Wire || face.HideDecals == null || !face.HideDecals(Channels));
            else DrawGroup(canvas, item.Group!);
        }
    }

    /// <summary>Libere la scene et les caches ; seuls les contours cliquables restent, pour le survol.</summary>
    public void DropScene()
    {
        if (_hitFaces.Count > 0)
        {
            _restHits = new (Vector2[], string)[_hitFaces.Count];
            for (var i = 0; i < _hitFaces.Count; i++) _restHits[i] = ((Vector2[])_hitFaces[i].Screen.Clone(), _hitFaces[i].Target!);
            _hitFaces.Clear();
        }
        _hitFaces.TrimExcess();
        _geometry?.Dispose();
        _geometry = null;
        _cameraKey = null;
        _orderCount = -1;
        _orderCameraVersion = -1;
        _orderMotionVersion = -1;
        _rects = Array.Empty<(float, float, float, float)>();
        _sorted = Array.Empty<OrderEntry>();
        _inDeg = Array.Empty<int>();
        _out = Array.Empty<List<int>>();
        _byMinX = Array.Empty<(float, int)>();
        _tIndex = Array.Empty<int>();
        _tLow = Array.Empty<int>();
        _tStack = Array.Empty<int>();
        _dfsV = Array.Empty<int>();
        _dfsEdge = Array.Empty<int>();
        _sccId = Array.Empty<int>();
        _sccInDeg = Array.Empty<int>();
        _tOnStack = Array.Empty<bool>();
        _sccNodes = Array.Empty<List<int>>();
        _pq.Clear();
        _grid?.Dispose();
        _grid = null;
        _shadow?.Dispose();
        _shadow = null;
        _groundVersion = -1;
        _lightGrid?.Dispose();
        _darkGrid?.Dispose();
        _lightGrid = _darkGrid = null;
        _darkTones = new();
    }

    public void Dispose()
    {
        DropScene();
        _fill.Dispose();
        _stroke.Dispose();
        _path.Dispose();
        _edges.Dispose();
    }

    // ---------------------------------------------------------------- camera

    private void SetCamera()
    {
        var key = (Yaw, Pitch);
        if (_cameraKey == key) return;
        _cameraKey = key;
        _cameraVersion++;
        double sy = Math.Sin(Yaw), cy = Math.Cos(Yaw);
        double sp = Math.Sin(Pitch), cp = Math.Cos(Pitch);
        _eye = M.V(-sy * cp, -cy * cp, sp);
        _right = M.V(cy, -sy, 0);
        _up = M.V(sy * sp, cy * sp, cp);
        _light = Vector3.Normalize(_eye * .55f + _up * .7f - _right * .45f);
    }

    private Vector2 Project(Vector3 p)
    {
        var q = p - _center;
        return new Vector2((float)(Info.ScreenX + Vector3.Dot(q, _right)), (float)(Info.ScreenY - Vector3.Dot(q, _up)));
    }

    private double Depth(Vector3 p) => Vector3.Dot(p - _center, _eye);

    private static Vector3 Centroid(Vector3[] p)
    {
        var s = Vector3.Zero;
        foreach (var v in p) s += v;
        return s / p.Length;
    }

    /// <summary>Projection d'un plan = application affine : (u, v) du repere de la face -> point de vue. Une fois par camera / mouvement.</summary>
    private SKMatrix FaceTransform(Face face)
    {
        if (face.TransformCamera == _cameraVersion) return face.Transform;
        face.EnsureBasis();
        var origin = Project(face.CurO);
        face.Transform = new SKMatrix(
            Vector3.Dot(face.CurU, _right), Vector3.Dot(face.CurV, _right), origin.X,
            -Vector3.Dot(face.CurU, _up), -Vector3.Dot(face.CurV, _up), origin.Y,
            0, 0, 1);
        face.TransformCamera = _cameraVersion;
        return face.Transform;
    }

    // ---------------------------------------------------------------- peintre

    /// <summary>
    ///     Affinage topologique (Newell) apres le tri par centroide : lorsqu'une face B recouvre a l'ecran une face A
    ///     mais que A est entierement du cote avant du plan de B (ou B du cote arriere du plan de A), B doit etre
    ///     peinte avant A meme si son centroide etait plus proche de la camera (ex. petit bouton sur grand chassis).
    /// </summary>
    private void RefineOrder(OrderEntry[] order, int count)
    {
        if (_rects.Length < count) _rects = new (float, float, float, float)[count];
        if (_byMinX.Length < count) _byMinX = new (float, int)[count];
        var rectCount = 0;
        for (var i = 0; i < count; i++)
        {
            if (order[i].Face is not { } face || face.P.Length == 0) continue;
            if (face.Screen.Length != face.P.Length) face.Screen = new Vector2[face.P.Length];
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (var k = 0; k < face.P.Length; k++)
            {
                var s = face.Screen[k] = Project(face.P[k]);
                if (s.X < minX) minX = s.X;
                if (s.X > maxX) maxX = s.X;
                if (s.Y < minY) minY = s.Y;
                if (s.Y > maxY) maxY = s.Y;
            }
            _rects[i] = (minX, maxX, minY, maxY);
            _byMinX[rectCount++] = (minX, i);
        }
        if (rectCount < 2) return;
        Array.Sort(_byMinX, 0, rectCount, ByMinXComparer);

        if (_out.Length < count)
        {
            var old = _out.Length;
            Array.Resize(ref _out, count);
            Array.Resize(ref _sccNodes, count);
            for (var i = old; i < count; i++)
            {
                _out[i] = new List<int>();
                _sccNodes[i] = new List<int>();
            }
            _inDeg = new int[count];
            _sorted = new OrderEntry[count];
            _tIndex = new int[count];
            _tLow = new int[count];
            _tStack = new int[count];
            _dfsV = new int[count];
            _dfsEdge = new int[count];
            _sccId = new int[count];
            _sccInDeg = new int[count];
            _tOnStack = new bool[count];
        }
        else
        {
            Array.Clear(_inDeg, 0, count);
            for (var i = 0; i < count; i++) _out[i].Clear();
        }

        var hasBackEdge = false;
        for (var p = 0; p < rectCount; p++)
        {
            var u = _byMinX[p].Idx;
            var ru = _rects[u];
            var fu = order[u].Face!;
            for (var q = p + 1; q < rectCount; q++)
            {
                if (_byMinX[q].MinX >= ru.MaxX) break;
                var v = _byMinX[q].Idx;
                var rv = _rects[v];
                if (ru.MaxY <= rv.MinY || rv.MaxY <= ru.MinY) continue;
                var fv = order[v].Face!;
                if (fu.Screen.Length == 3 && fv.Screen.Length == 3 && !TrianglesOverlap2D(fu.Screen, fv.Screen))
                    continue;

                int i, j;
                Face a, b;
                if (u < v) { i = u; j = v; a = fu; b = fv; }
                else { i = v; j = u; a = fv; b = fu; }

                var ab = MustDrawBefore(a, b);
                var ba = MustDrawBefore(b, a);
                if (ab && !ba)
                {
                    _out[i].Add(j);
                    _inDeg[j]++;
                }
                else if (ba && !ab)
                {
                    _out[j].Add(i);
                    _inDeg[i]++;
                    hasBackEdge = true;
                }
            }
        }
        if (!hasBackEdge) return;

        // Kahn fast-path; fall back to Tarjan SCC condensation DAG only if a topological cycle exists so
        // cycles in one mesh region never bypass valid ordering constraints elsewhere.
        _pq.Clear();
        for (var i = 0; i < count; i++)
            if (_inDeg[i] == 0) _pq.Enqueue(i, i);

        var written = 0;
        while (_pq.Count > 0)
        {
            var u = _pq.Dequeue();
            _sorted[written++] = order[u];
            foreach (var v in _out[u])
                if (--_inDeg[v] == 0) _pq.Enqueue(v, v);
        }

        if (written == count)
        {
            Array.Copy(_sorted, order, count);
            return;
        }

        RefineOrderWithScc(order, count);
    }

    private void RefineOrderWithScc(OrderEntry[] order, int count)
    {
        Array.Fill(_tIndex, -1, 0, count);
        Array.Clear(_tOnStack, 0, count);

        var nextIndex = 0;
        var stackTop = 0;
        var sccCount = 0;
        for (var start = 0; start < count; start++)
        {
            if (_tIndex[start] != -1) continue;
            var dfsTop = 1;
            _dfsV[0] = start;
            _dfsEdge[0] = 0;
            _tIndex[start] = _tLow[start] = nextIndex++;
            _tStack[stackTop++] = start;
            _tOnStack[start] = true;

            while (dfsTop > 0)
            {
                var top = dfsTop - 1;
                var u = _dfsV[top];
                var ei = _dfsEdge[top];
                var adj = _out[u];
                if (ei < adj.Count)
                {
                    _dfsEdge[top] = ei + 1;
                    var w = adj[ei];
                    if (_tIndex[w] == -1)
                    {
                        _tIndex[w] = _tLow[w] = nextIndex++;
                        _tStack[stackTop++] = w;
                        _tOnStack[w] = true;
                        _dfsV[dfsTop] = w;
                        _dfsEdge[dfsTop] = 0;
                        dfsTop++;
                    }
                    else if (_tOnStack[w] && _tIndex[w] < _tLow[u])
                    {
                        _tLow[u] = _tIndex[w];
                    }
                }
                else
                {
                    dfsTop--;
                    if (_tLow[u] == _tIndex[u])
                    {
                        _sccNodes[sccCount].Clear();
                        while (true)
                        {
                            var w = _tStack[--stackTop];
                            _tOnStack[w] = false;
                            _sccId[w] = sccCount;
                            if (w == u) break;
                        }
                        sccCount++;
                    }
                    if (dfsTop > 0)
                    {
                        var parent = _dfsV[dfsTop - 1];
                        if (_tLow[u] < _tLow[parent]) _tLow[parent] = _tLow[u];
                    }
                }
            }
        }

        // Traversing 0..count-1 preserves ascending initial centroid-depth order inside each SCC.
        for (var i = 0; i < count; i++) _sccNodes[_sccId[i]].Add(i);

        Array.Clear(_sccInDeg, 0, sccCount);
        for (var u = 0; u < count; u++)
        {
            var cu = _sccId[u];
            foreach (var v in _out[u])
            {
                var cv = _sccId[v];
                if (cu != cv) _sccInDeg[cv]++;
            }
        }

        _pq.Clear();
        for (var c = 0; c < sccCount; c++)
            if (_sccInDeg[c] == 0) _pq.Enqueue(c, _sccNodes[c][0]);

        var written = 0;
        while (_pq.Count > 0)
        {
            var c = _pq.Dequeue();
            var nodes = _sccNodes[c];
            for (var i = 0; i < nodes.Count; i++)
            {
                var u = nodes[i];
                _sorted[written++] = order[u];
                foreach (var v in _out[u])
                {
                    var cv = _sccId[v];
                    if (cv != c && --_sccInDeg[cv] == 0)
                        _pq.Enqueue(cv, _sccNodes[cv][0]);
                }
            }
        }
        Array.Copy(_sorted, order, count);
    }

    private static bool TrianglesOverlap2D(Vector2[] a, Vector2[] b)
    {
        return !SeparatedByEdges(a, b) && !SeparatedByEdges(b, a);
    }

    private static bool SeparatedByEdges(Vector2[] edgesFrom, Vector2[] other)
    {
        for (var i = 0; i < 3; i++)
        {
            var p = edgesFrom[i];
            var q = edgesFrom[(i + 1) % 3];
            var nx = -(q.Y - p.Y);
            var ny = q.X - p.X;
            var len = MathF.Sqrt(nx * nx + ny * ny);
            if (len < 1e-5f) continue;

            float minA = float.MaxValue, maxA = float.MinValue;
            for (var k = 0; k < 3; k++)
            {
                var d = edgesFrom[k].X * nx + edgesFrom[k].Y * ny;
                if (d < minA) minA = d;
                if (d > maxA) maxA = d;
            }
            float minB = float.MaxValue, maxB = float.MinValue;
            for (var k = 0; k < 3; k++)
            {
                var d = other[k].X * nx + other[k].Y * ny;
                if (d < minB) minB = d;
                if (d > maxB) maxB = d;
            }
            var eps = 1e-3f * len;
            if (maxA <= minB + eps || maxB <= minA + eps) return true;
        }
        return false;
    }

    private static bool MustDrawBefore(Face back, Face front)
    {
        const float eps = 1e-3f;
        const float minSep = 1e-2f;

        // Tous les sommets de front sont du cote avant (+normal) du plan de back
        var bn = back.N;
        var bp0 = back.P[0];
        var maxFront = float.MinValue;
        var allFront = true;
        foreach (var p in front.P)
        {
            var d = Vector3.Dot(p - bp0, bn);
            if (d < -eps) { allFront = false; break; }
            if (d > maxFront) maxFront = d;
        }
        if (allFront && maxFront > minSep) return true;

        // Symetrique : tous les sommets de back sont du cote arriere (-normal) du plan de front
        var fn = front.N;
        var fp0 = front.P[0];
        var minBack = float.MaxValue;
        foreach (var p in back.P)
        {
            var d = Vector3.Dot(p - fp0, fn);
            if (d > eps) return false;
            if (d < minBack) minBack = d;
        }
        return minBack < -minSep;
    }

    private void SortFaces(List<Face> faces, ref DepthItem<Face>[] order)
    {
        if (order.Length != faces.Count) order = new DepthItem<Face>[faces.Count];
        for (var i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            order[i] = new DepthItem<Face>(Depth(face.Center ??= Centroid(face.P)), i, face);
        }
        Array.Sort(order);
    }

    /// <summary>Quadrillage au sol (un seul chemin, estompe par un degrade radial) et ombre portee.</summary>
    private void DrawGround(SKCanvas canvas)
    {
        if (Info.Ground is not { } g) return;
        if (_groundVersion != _cameraVersion)
        {
            _grid ??= new SKPath();
            _grid.Rewind();
            for (var x = g.X0; x <= g.X1 + 1e-9; x += g.Step) Segment(_grid, M.V(x, g.Y0, g.Z), M.V(x, g.Y1, g.Z));
            for (var y = g.Y0; y <= g.Y1 + 1e-9; y += g.Step) Segment(_grid, M.V(g.X0, y, g.Z), M.V(g.X1, y, g.Z));
            if (g.Shadow)
            {
                _shadow ??= new SKPath { FillType = SKPathFillType.EvenOdd };
                _shadow.Rewind();
                AddPolygon(_shadow, new[] { M.V(g.ShadowX0, g.ShadowY0, g.Z), M.V(g.ShadowX1, g.ShadowY0, g.Z), M.V(g.ShadowX1, g.ShadowY1, g.Z), M.V(g.ShadowX0, g.ShadowY1, g.Z) }, true);
            }
            _groundVersion = _cameraVersion;
        }
        _stroke.Shader = GridShader();
        _stroke.Color = SKColors.Black;
        _stroke.StrokeWidth = .6f;
        _stroke.StrokeCap = SKStrokeCap.Butt;
        canvas.DrawPath(_grid!, _stroke);
        _stroke.Shader = null;
        if (!Wire && _shadow != null) Fill(canvas, _shadow, Dark ? SKColors.Black : new SKColor(91, 107, 149), Dark ? .3 : .12);
    }

    private SKShader GridShader()
    {
        if (Dark) return _darkGrid ??= CreateGridShader(new SKColor(120, 138, 180));
        return _lightGrid ??= CreateGridShader(new SKColor(143, 160, 196));
    }

    /// <summary>Degrade radial (ellipse sous le modele) qui estompe la grille vers les bords de la vue.</summary>
    private SKShader CreateGridShader(SKColor color)
    {
        var faded = ToSk(color, .3);
        float cx = (float)(Info.Width / 2), cy = (float)(Info.Height * .66), rx = (float)(Info.Width * .62), ry = (float)(Info.Height * .62);
        return SKShader.CreateRadialGradient(new SKPoint(cx, cy), rx, new[] { faded, faded, new SKColor(255, 255, 255, 0) },
            new[] { 0f, .55f, 1f }, SKShaderTileMode.Clamp, SKMatrix.CreateScale(1, ry / rx, cx, cy));
    }

    private void Segment(SKPath path, Vector3 a, Vector3 b)
    {
        var p = Project(a);
        var q = Project(b);
        path.MoveTo(p.X, p.Y);
        path.LineTo(q.X, q.Y);
    }

    private void DrawFace(SKCanvas canvas, Face f, bool decals = true)
    {
        var visible = Vector3.Dot(f.N, _eye) > 1e-4f;
        if (!visible && !Wire) return;
        if (f.Screen.Length != f.P.Length) f.Screen = new Vector2[f.P.Length];
        _path.Rewind();
        for (var i = 0; i < f.P.Length; i++)
        {
            var p = f.Screen[i] = Project(f.P[i]);
            if (i == 0) _path.MoveTo(p.X, p.Y);
            else _path.LineTo(p.X, p.Y);
        }
        _path.Close();
        if (Wire)
            Stroke(canvas, _path, WireColor, visible ? .95 : .25, .6, false);
        else
        {
            // k arrondi a 1/48 : un jeu de teintes limite
            var k = Math.Round((.82 + .18 * Math.Max(0, Vector3.Dot(f.N, _light))) * 48) / 48;
            var color = Shade(Tone(f.C), k);
            Fill(canvas, _path, color, f.Op);
            if (f.SharpEdges is { } sharp)
            {
                // Maillage : contour couleur de face (pas de jour entre triangles), puis seulement les aretes vives
                Stroke(canvas, _path, color, f.Op, .5, false);
                _edges.Rewind();
                for (var i = 0; i < sharp.Length; i++)
                {
                    if (!sharp[i] && (f.SeamPeers?[i] is not { } peer || Vector3.Dot(peer.N, _eye) > 1e-4f)) continue;
                    var a = f.Screen[i];
                    var b = f.Screen[(i + 1) % f.Screen.Length];
                    _edges.MoveTo(a.X, a.Y);
                    _edges.LineTo(b.X, b.Y);
                }
                if (!_edges.IsEmpty) Stroke(canvas, _edges, Tone(Info.Edge), .55, .6, true);
            }
            else Stroke(canvas, _path, Tone(Info.Edge), .55, .6, false);
        }
        if (!visible) return;
        if (f.Target != null) _hitFaces.Add(f);
        if (decals && f.Dec.Count > 0) DrawDecals(canvas, f);
        if (f.Kids == null) return;
        if (f.OrderVersion != _cameraVersion)
        {
            SortFaces(f.Kids, ref f.KidsOrder);
            f.OrderVersion = _cameraVersion;
        }
        foreach (var kid in f.KidsOrder) DrawFace(canvas, kid.Item!);
    }

    /// <summary>Les decalques d'une face, sous une seule matrice : leurs chemins ne changent jamais.</summary>
    private void DrawDecals(SKCanvas canvas, Face f)
    {
        canvas.Save();
        var transform = FaceTransform(f);
        canvas.Concat(in transform);
        foreach (var d in f.Dec) DrawDecal(canvas, d);
        canvas.Restore();
    }

    private void DrawDecal(SKCanvas canvas, Decal d)
    {
        var op = d.Op;
        if (d.Glow >= 0)
        {
            var glow = Channels.Values[d.Glow];
            if (glow < .01 || Wire) return;
            op *= glow;
        }
        SKPath path;
        if (d.Local != null)
        {
            if (d.Path == null) d.Path = LocalPath(d.Local, !d.Line);
            else if (d.PathDirty)
            {
                d.Path.Rewind();
                FillLocalPath(d.Path, d.Local, !d.Line);
                d.PathDirty = false;
            }
            path = d.Path;
        }
        else
        {
            _path.Rewind();
            foreach (var figure in d.F) AddPolygon(_path, figure, !d.Line);
            path = _path;
        }
        if (d.Line)
        {
            Stroke(canvas, path, Wire ? WireColor : Tone(d.Stroke ?? Info.Edge), op, d.W, true);
            return;
        }
        if (Wire)
        {
            Stroke(canvas, path, WireColor, .7, .4, false);
            return;
        }
        if (d.Fill is { } fill) Fill(canvas, path, d.Glow >= 0 ? fill : Tone(fill), op);
        if (d.Stroke is { } stroke) Stroke(canvas, path, Tone(stroke), op, d.W, false);
    }

    /// <summary>Groupe de profondeur : parois dans l'ordre, puis pieces triees, chacune triee a l'interieur.</summary>
    private void DrawGroup(SKCanvas canvas, DepthGroup group)
    {
        if (group.OrderVersion != _cameraVersion)
        {
            if (group.Order.Length != group.Parts.Count) group.Order = new DepthItem<Part>[group.Parts.Count];
            for (var i = 0; i < group.Parts.Count; i++)
            {
                var part = group.Parts[i];
                group.Order[i] = new DepthItem<Part>(PartDepth(part), i, part);
                SortFaces(part.Faces, ref part.Order);
            }
            Array.Sort(group.Order);
            group.OrderVersion = _cameraVersion;
        }
        foreach (var wall in group.Walls) DrawFace(canvas, wall);
        foreach (var item in group.Order)
        {
            var part = item.Item!;
            foreach (var f in part.Order) DrawFace(canvas, f.Item!);
            foreach (var d in part.Free) DrawDecal(canvas, d);
        }
    }

    private double PartDepth(Part part)
    {
        if (part.Anchor is { } anchor) return Depth(anchor);
        if (part.Center is not { } center)
        {
            var min = part.Faces[0].P[0];
            var max = min;
            foreach (var face in part.Faces)
                foreach (var point in face.P)
                {
                    min = Vector3.Min(min, point);
                    max = Vector3.Max(max, point);
                }
            part.Center = center = (min + max) / 2;
        }
        return Depth(center);
    }

    // ---------------------------------------------------------------- hit-test

    /// <summary>Cible cliquable la plus proche sous le point du controle (la derniere face peinte qui le contient).</summary>
    public string? HitTest(double x, double y)
    {
        var p = new Vector2((float)((x - Offset.X) / Scale), (float)((y - Offset.Y) / Scale));
        if (_geometry != null && _hitFaces.Count > 0)
        {
            for (var i = _hitFaces.Count - 1; i >= 0; i--)
                if (Contains(_hitFaces[i].Screen, p)) return _hitFaces[i].Target;
            return null;
        }
        for (var i = _restHits.Length - 1; i >= 0; i--)
            if (Contains(_restHits[i].Shape, p)) return _restHits[i].Target;
        return null;
    }

    private static bool Contains(Vector2[] polygon, Vector2 p)
    {
        var inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            var a = polygon[i];
            var b = polygon[j];
            if (a.Y > p.Y != b.Y > p.Y && p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X) inside = !inside;
        }
        return inside;
    }

    // ---------------------------------------------------------------- outils

    /// <summary>
    ///     Theme sombre : memes couleurs, a peine attenuees (-5 % de luminosite et de saturation, en HSL) pour ne pas
    ///     eblouir sur fond fonce. Les lueurs ne passent pas par ici.
    /// </summary>
    private SKColor Tone(SKColor c)
    {
        if (!Dark) return c;
        var key = (uint)c;
        if (_darkTones.TryGetValue(key, out var toned)) return toned;
        toned = Hsl.Dim(c, .95, .95);
        _darkTones[key] = toned;
        return toned;
    }

    private void Fill(SKCanvas canvas, SKPath path, SKColor color, double opacity)
    {
        _fill.Color = ToSk(color, opacity);
        canvas.DrawPath(path, _fill);
    }

    private void Stroke(SKCanvas canvas, SKPath path, SKColor color, double opacity, double thickness, bool round)
    {
        _stroke.Color = ToSk(color, opacity);
        _stroke.StrokeWidth = (float)thickness;
        _stroke.StrokeCap = round ? SKStrokeCap.Round : SKStrokeCap.Butt;
        canvas.DrawPath(path, _stroke);
    }

    private void AddPolygon(SKPath path, Vector3[] points, bool closed)
    {
        if (points.Length == 0) return;
        var p = Project(points[0]);
        path.MoveTo(p.X, p.Y);
        for (var i = 1; i < points.Length; i++)
        {
            p = Project(points[i]);
            path.LineTo(p.X, p.Y);
        }
        if (closed) path.Close();
    }

    private static SKPath LocalPath(Vector2[][] figures, bool closed)
    {
        var path = new SKPath { FillType = SKPathFillType.EvenOdd };
        FillLocalPath(path, figures, closed);
        return path;
    }

    private static void FillLocalPath(SKPath path, Vector2[][] figures, bool closed)
    {
        foreach (var points in figures)
        {
            if (points.Length == 0) continue;
            path.MoveTo(points[0].X, points[0].Y);
            for (var i = 1; i < points.Length; i++) path.LineTo(points[i].X, points[i].Y);
            if (closed) path.Close();
        }
    }

    private static SKColor ToSk(SKColor c, double opacity) =>
        new(c.Red, c.Green, c.Blue, (byte)Math.Round(Math.Clamp(opacity, 0, 1) * 255));

    private static SKColor Shade(SKColor c, double k) => new(
        (byte)Math.Min(255, c.Red * k), (byte)Math.Min(255, c.Green * k), (byte)Math.Min(255, c.Blue * k));
}

/// <summary>Conversion HSL identique a celle d'Avalonia (Color.ToHsl / HslColor.ToRgb), pour des teintes sombres au pixel pres.</summary>
internal static class Hsl
{
    public static SKColor Dim(SKColor c, double saturation, double lightness)
    {
        double r = 1.0 / 255.0 * c.Red, g = 1.0 / 255.0 * c.Green, b = 1.0 / 255.0 * c.Blue, a = 1.0 / 255.0 * c.Alpha;
        var max = !(r >= g) ? (g >= b ? g : b) : (r >= b ? r : b);
        var min = !(r <= g) ? (g <= b ? g : b) : (r <= b ? r : b);
        var delta = max - min;
        var h = delta == 0.0 ? 0.0 : max == r ? ((g - b) / delta + 6.0) % 6.0 : max != g ? 4.0 + (r - g) / delta : 2.0 + (b - r) / delta;
        var l = .5 * (max + min);
        var s = delta == 0.0 ? 0.0 : delta / (1.0 - Math.Abs(2.0 * l - 1.0));
        // HslColor(a, h, s, l) borne ses composantes
        var hue = Math.Clamp(60.0 * h, 0.0, 360.0);
        hue = hue == 360.0 ? 0.0 : hue;
        return ToRgb(hue, Math.Clamp(s * saturation, 0.0, 1.0), Math.Clamp(l * lightness, 0.0, 1.0), Math.Clamp(a, 0.0, 1.0));
    }

    private static SKColor ToRgb(double hue, double saturation, double lightness, double alpha)
    {
        while (hue >= 360.0) hue -= 360.0;
        while (hue < 0.0) hue += 360.0;
        var chroma = (1.0 - Math.Abs(2.0 * lightness - 1.0)) * saturation;
        var h1 = hue / 60.0;
        var x = chroma * (1.0 - Math.Abs(h1 % 2.0 - 1.0));
        var m = lightness - 0.5 * chroma;
        double r, g, b;
        if (h1 < 1.0) (r, g, b) = (chroma, x, 0.0);
        else if (h1 < 2.0) (r, g, b) = (x, chroma, 0.0);
        else if (h1 < 3.0) (r, g, b) = (0.0, chroma, x);
        else if (h1 < 4.0) (r, g, b) = (0.0, x, chroma);
        else if (h1 < 5.0) (r, g, b) = (x, 0.0, chroma);
        else (r, g, b) = (chroma, 0.0, x);
        return new SKColor((byte)Math.Round(255.0 * (r + m)), (byte)Math.Round(255.0 * (g + m)), (byte)Math.Round(255.0 * (b + m)),
            (byte)Math.Round(255.0 * alpha));
    }
}

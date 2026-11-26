using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

internal sealed class HyperAtlasSlot
{
    public HyperSlotKind Kind;
    public string? Name;
    public SKRect SrcRect;        // Source rect inside shared SKImage atlas (pixels)
    public Rect LogicalBounds;    // Bounds in SVG coordinate space
    public Point Pivot;           // Rotation/scale pivot in SVG coordinate space
    public Rect WindowRect;
    public double WindowRadius;
}

internal sealed class HyperGpuPage
{
    public const int PageSize = 2048;
    private static readonly SKSamplingOptions NearestSampling = new(SKFilterMode.Nearest);

    private readonly GRContext _grContext;
    private readonly SKSurface _surface;
    private SKImage? _snapshot;
    private int _cursorX;
    private int _cursorY;
    private int _rowHeight;

    public int LiveSlots { get; internal set; }
    public bool NeedsReset { get; internal set; }
    public bool Disposed { get; private set; }

    public HyperGpuPage(GRContext grContext)
    {
        _grContext = grContext;
        var info = new SKImageInfo(PageSize, PageSize, SKColorType.Bgra8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(grContext, true, info)
            ?? throw new InvalidOperationException("Unable to allocate GPU atlas page.");
        _surface.Canvas.Clear(SKColors.Transparent);
    }

    public SKImage? CurrentImage => _snapshot;
    public GRContext Context => _grContext;

    public bool TryPack(SKImage src, int w, int h, out float offsetX, out float offsetY)
    {
        const int pad = 2;
        var needW = w + pad;
        var needH = h + pad;
        if (Disposed || needW > PageSize || needH > PageSize)
        {
            offsetX = 0;
            offsetY = 0;
            return false;
        }

        if (NeedsReset && LiveSlots == 0)
        {
            NeedsReset = false;
            _cursorX = 0;
            _cursorY = 0;
            _rowHeight = 0;
            _snapshot?.Dispose();
            _snapshot = null;
            _surface.Canvas.Clear(SKColors.Transparent);
        }

        var nextX = _cursorX;
        var nextY = _cursorY;
        var nextRowH = _rowHeight;

        if (nextX + needW > PageSize)
        {
            nextX = 0;
            nextY += nextRowH;
            nextRowH = 0;
        }

        if (nextY + needH > PageSize)
        {
            offsetX = 0;
            offsetY = 0;
            return false;
        }

        var px = nextX;
        var py = nextY;
        _cursorX = nextX + needW;
        _cursorY = nextY;
        _rowHeight = needH > nextRowH ? needH : nextRowH;

        // Release active snapshot before drawing so surface texture refcount == 1 (zero copy-on-write!)
        _snapshot?.Dispose();
        _snapshot = null;

        _surface.Canvas.DrawImage(src, px, py, NearestSampling);
        _surface.Flush();
        _snapshot = _surface.Snapshot();

        LiveSlots++;
        offsetX = px;
        offsetY = py;
        return true;
    }

    public void DisposeWithActiveContext()
    {
        if (Disposed) return;
        Disposed = true;
        _snapshot?.Dispose();
        _snapshot = null;
        _surface.Dispose();
    }
}

/// <summary>
///     Texture atlas unique (1 seule SKImage) contenant TOUTES les tranches statiques ET animées du SVG
///     plus le sprite 3-en-1 des particules. Sur GPU (GrContext), les atlas uniques sont packés dans
///     des pages VRAM partagées 2048x2048 afin de supprimer les changements de texture entre contrôles.
/// </summary>
internal sealed class HyperAtlas
{
    public const int ParticleSpriteSize = 24;

    private readonly HyperAtlasKey _key;
    internal int RefCount;
    internal bool Disposed;
    private readonly int _pixelW;
    private readonly int _pixelH;
    private HyperGpuPage? _gpuPage;
    private SKImage? _gpuFallbackImage;
    private GRContext? _gpuContext;
    private bool _srcCoordsShifted;

    public SKImage Image { get; }
    public float RasterScale { get; }
    public SKRect ParticleSpriteRect { get; private set; }
    public HyperAtlasSlot[] Slots { get; }
    public Dictionary<string, int> NamedPartIndices { get; }

    internal HyperAtlasKey Key => _key;

    private HyperAtlas(
        HyperAtlasKey key,
        SKImage image,
        int pixelW,
        int pixelH,
        float rasterScale,
        SKRect particleSpriteRect,
        HyperAtlasSlot[] slots,
        Dictionary<string, int> namedPartIndices)
    {
        _key = key;
        Image = image;
        _pixelW = pixelW;
        _pixelH = pixelH;
        RasterScale = rasterScale;
        ParticleSpriteRect = particleSpriteRect;
        Slots = slots;
        NamedPartIndices = namedPartIndices;
        RefCount = 1;
    }

    public SKImage GetDrawImage(GRContext? grContext, out bool srcCoordsUpdated) =>
        GetDrawImage(grContext, out srcCoordsUpdated, out _);

    public SKImage GetDrawImage(GRContext? grContext, out bool srcCoordsUpdated, out bool packPending)
    {
        srcCoordsUpdated = false;
        packPending = false;
        if (grContext == null) return Image;
        HyperAtlasCache.MarkRenderThread(hasGpu: true);

        if (_gpuPage != null && !_gpuPage.Disposed && ReferenceEquals(_gpuContext, grContext))
            return _gpuPage.CurrentImage ?? Image;

        if (_gpuFallbackImage != null && ReferenceEquals(_gpuContext, grContext))
            return _gpuFallbackImage;

        if (!HyperAtlasCache.CanStartGpuPack())
        {
            packPending = true;
            return Image;
        }

        var packStart = Stopwatch.GetTimestamp();
        if (_gpuPage != null)
        {
            HyperAtlasCache.ReleaseGpuPageSlot(_gpuPage);
            _gpuPage = null;
        }
        if (_gpuFallbackImage != null)
        {
            _gpuFallbackImage.Dispose();
            _gpuFallbackImage = null;
        }
        _gpuContext = grContext;

        if (!_srcCoordsShifted && HyperAtlasCache.TryPackIntoGpuPage(grContext, Image, _pixelW, _pixelH, out var page, out var ox, out var oy))
        {
            _gpuPage = page;
            if (ox != 0f || oy != 0f)
            {
                ParticleSpriteRect = OffsetRect(ParticleSpriteRect, ox, oy);
                for (var i = 0; i < Slots.Length; i++)
                {
                    if (Slots[i].Kind != HyperSlotKind.Window)
                        Slots[i].SrcRect = OffsetRect(Slots[i].SrcRect, ox, oy);
                }
                srcCoordsUpdated = true;
            }
            _srcCoordsShifted = true;
            HyperAtlasCache.RecordGpuPack(Stopwatch.GetTimestamp() - packStart);
            return page.CurrentImage ?? Image;
        }

        _gpuFallbackImage = Image.ToTextureImage(grContext);
        HyperAtlasCache.RecordGpuPack(Stopwatch.GetTimestamp() - packStart);
        return _gpuFallbackImage ?? Image;
    }

    private static SKRect OffsetRect(SKRect r, float ox, float oy) =>
        new(r.Left + ox, r.Top + oy, r.Right + ox, r.Bottom + oy);

    public bool TryAddRef() => HyperAtlasCache.TryAddRef(this);

    public bool Release() => HyperAtlasCache.Release(this);

    internal void DisposeResources()
    {
        var page = _gpuPage;
        _gpuPage = null;
        _gpuContext = null;
        if (page != null)
            HyperAtlasCache.ReleaseGpuPageSlot(page);

        var gpu = _gpuFallbackImage;
        _gpuFallbackImage = null;
        if (gpu != null)
            HyperAtlasCache.DisposeOrQueueGpuImage(gpu);
        Image.Dispose();
    }

    public static HyperAtlas Build(
        HyperAtlasKey key,
        HyperSvgTemplate template,
        HyperPaintSnapshot palette,
        Func<HyperTheme>? fallbackThemeFactory,
        float rasterScale)
    {
        var sw = Stopwatch.StartNew();
        var compileStart = Stopwatch.GetTimestamp();
        var compiledSlots = template.CompiledSlots;
        var compileMs = (Stopwatch.GetTimestamp() - compileStart) * 1000.0 / Stopwatch.Frequency;
        var vb = template.ViewBox;
        var count = compiledSlots.Count;

        // Only compile fallback XML strings if any slot could not compile to native Skia IR and a fallback compiler is wired
        List<HyperSliceSpec>? fallbackSlices = null;
        IHyperFallbackSvg?[]? fallbackSvgs = null;
        var fallbackCompiler = HyperSvgOptions.FallbackSvgCompiler;
        if (!template.AllSlotsCompiled && fallbackCompiler != null)
        {
            var theme = fallbackThemeFactory != null ? fallbackThemeFactory() : new HyperTheme();
            fallbackSlices = template.Compile(theme);
            fallbackSvgs = new IHyperFallbackSvg?[count];
        }

        var boundsList = new Rect[count];
        var cellSizes = new (int W, int H)[count];
        var cellPos = new (int X, int Y)[count];

        const int pad = 2;
        var maxCellW = ParticleSpriteSize;

        for (var i = 0; i < count; i++)
        {
            var slot = compiledSlots[i];
            if (slot.Kind == HyperSlotKind.Window)
            {
                boundsList[i] = slot.Window;
                continue;
            }

            if (slot.Compiled == null)
            {
                var slotName = slot.Name ?? "static";
                var reason = slot.FallbackReason ?? "raison inconnue";
                if (fallbackSlices != null && fallbackSvgs != null && fallbackCompiler != null)
                {
                    HyperFallbackLog.Report($"modele HyperSvg #{template.Id}, slot '{slotName}'", reason);
                    fallbackSvgs[i] = fallbackCompiler(fallbackSlices[i].Svg);
                }
                else
                {
                    Console.WriteLine($"[WARNING !!!] HyperSvg fallback requis mais FallbackSvgCompiler absent (template #{template.Id}, slot '{slotName}') : {reason}");
                }
            }

            var est = slot.EstimateBounds;
            var left = Math.Max(vb.X, Math.Floor(est.X));
            var top = Math.Max(vb.Y, Math.Floor(est.Y));
            var right = Math.Min(vb.Right, Math.Ceiling(est.Right));
            var bottom = Math.Min(vb.Bottom, Math.Ceiling(est.Bottom));
            if (right <= left || bottom <= top)
            {
                left = vb.X;
                top = vb.Y;
                right = vb.Right;
                bottom = vb.Bottom;
            }

            var lb = new Rect(left, top, right - left, bottom - top);
            boundsList[i] = lb;

            var pw = Math.Max(1, (int)Math.Ceiling(lb.Width * rasterScale));
            var ph = Math.Max(1, (int)Math.Ceiling(lb.Height * rasterScale));
            cellSizes[i] = (pw, ph);
            if (pw > maxCellW) maxCellW = pw;
        }

        // 2D shelf packing inside a compact square-ish block so many unique controls pack cleanly into each 2048x2048 GPU page
        var targetMaxW = Math.Min(1024, Math.Max(maxCellW + pad * 2, Math.Max(384, (int)Math.Ceiling(vb.Width * rasterScale) + pad * 2 + 64)));
        var curX = pad;
        var curY = pad;
        var shelfH = 0;
        var usedMaxW = 0;

        var partX = curX;
        var partY = curY;
        curX += ParticleSpriteSize + pad * 2;
        shelfH = ParticleSpriteSize + pad;
        usedMaxW = curX;

        for (var i = 0; i < count; i++)
        {
            if (compiledSlots[i].Kind == HyperSlotKind.Window) continue;
            var (pw, ph) = cellSizes[i];
            var boxW = pw + pad * 2;
            var boxH = ph + pad * 2;

            if (curX + boxW > targetMaxW && curX > pad)
            {
                curX = pad;
                curY += shelfH;
                shelfH = 0;
            }

            cellPos[i] = (curX, curY);
            curX += boxW;
            if (curX > usedMaxW) usedMaxW = curX;
            if (boxH > shelfH) shelfH = boxH;
        }

        var atlasW = Math.Max(64, usedMaxW);
        var atlasH = Math.Max(64, curY + shelfH + pad);
        using var bitmap = new SKBitmap(new SKImageInfo(atlasW, atlasH, SKColorType.Bgra8888, SKAlphaType.Premul));
        SKRect particleRect;
        var slots = new HyperAtlasSlot[count];
        var named = new Dictionary<string, int>(StringComparer.Ordinal);

        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);

            particleRect = new SKRect(partX, partY, partX + ParticleSpriteSize, partY + ParticleSpriteSize);
            DrawCombinedParticleSprite(canvas, partX, partY);

            for (var i = 0; i < count; i++)
            {
                var s = compiledSlots[i];
                if (s.Kind == HyperSlotKind.Window)
                {
                    slots[i] = new HyperAtlasSlot
                    {
                        Kind = HyperSlotKind.Window,
                        Name = s.Name,
                        LogicalBounds = s.Window,
                        Pivot = s.Window.Center,
                        WindowRect = s.Window,
                        WindowRadius = s.Radius
                    };
                    if (s.Name != null) named[s.Name] = i;
                    continue;
                }

                var lb = boundsList[i];
                var (pw, ph) = cellSizes[i];
                var (cx, cy) = cellPos[i];
                var dst = new SKRect(cx, cy, cx + pw, cy + ph);

                if (pw > 0 && ph > 0)
                {
                    if (s.Compiled != null)
                    {
                        canvas.Save();
                        canvas.ClipRect(dst);
                        canvas.Translate(cx, cy);
                        canvas.Scale(rasterScale, rasterScale);
                        canvas.Translate((float)-lb.X, (float)-lb.Y);
                        s.Compiled.Render(canvas, palette);
                        canvas.Restore();
                    }
                    else if (fallbackSvgs?[i] is { } fb)
                    {
                        canvas.Save();
                        canvas.ClipRect(dst);
                        canvas.Translate(cx, cy);
                        canvas.Scale(rasterScale, rasterScale);
                        canvas.Translate((float)(vb.X - lb.X), (float)(vb.Y - lb.Y));
                        fb.Draw(canvas);
                        canvas.Restore();
                    }
                }

                fallbackSvgs?[i]?.Dispose();

                slots[i] = new HyperAtlasSlot
                {
                    Kind = s.Kind,
                    Name = s.Name,
                    SrcRect = dst,
                    LogicalBounds = lb,
                    Pivot = s.Pivot ?? lb.Center
                };
                if (s.Name != null) named[s.Name] = i;
            }
        }

        bitmap.SetImmutable();
        var image = SKImage.FromBitmap(bitmap);

        sw.Stop();
        HyperAtlasCache.OnBuildTicks?.Invoke(sw.ElapsedTicks);
        if (HyperSvgOptions.LogFirstRenderPerf)
        {
            var totalMs = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"[HyperPerf] AtlasBuild #{template.Id} ({atlasW}x{atlasH} @ {rasterScale:0.##}x): total={totalMs:0.0}ms (compile={compileMs:0.0}ms)");
        }

        return new HyperAtlas(key, image, atlasW, atlasH, rasterScale, particleRect, slots, named);
    }

    private static void DrawCombinedParticleSprite(SKCanvas canvas, float ox, float oy)
    {
        var cx = ox + ParticleSpriteSize * 0.5f;
        var cy = oy + ParticleSpriteSize * 0.5f;
        var haloR = ParticleSpriteSize * 0.5f - 0.5f;
        var bodyR = haloR / 2.4f;
        var hiR = bodyR * 0.55f;

        using var paint = new SKPaint { IsAntialias = true };

        // Halo (150, 185, 255, alpha 36/255)
        paint.Color = new SKColor(150, 185, 255, 36);
        canvas.DrawCircle(cx, cy, haloR, paint);

        // Body (200, 222, 255, alpha 230/255)
        paint.Color = new SKColor(200, 222, 255, 230);
        canvas.DrawCircle(cx, cy, bodyR, paint);

        // Specular highlight (255, 255, 255, alpha 255/255)
        paint.Color = SKColors.White;
        canvas.DrawCircle(cx - bodyR * 0.2f, cy - bodyR * 0.2f, hiR, paint);
    }
}

internal readonly record struct HyperAtlasKey(int TemplateId, long ThemeSignature, int ScaleMilli);

internal static class HyperAtlasCache
{
    private const int MaxStandbyPerTemplate = 2;
    private const int MaxPooledEmptyPages = 2;

    private static readonly object Gate = new();
    private static readonly object GpuPageGate = new();
    private static readonly Dictionary<HyperAtlasKey, HyperAtlas> Cache = new();
    private static readonly Dictionary<int, int> TemplateUsers = new();
    private static readonly Dictionary<int, List<HyperAtlas>> Standby = new();
    private static readonly ConcurrentQueue<SKImage> PendingGpuDisposals = new();
    private static readonly List<HyperGpuPage> GpuPages = new();
    private static readonly ConcurrentQueue<HyperGpuPage> PendingPageDisposals = new();
    private static int _renderThreadId;
    private static int _gpuContextSeen;
    private static long _gpuPackWindowStart;
    private static int _gpuPacksInWindow;
    private static long _gpuPackElapsedTicks;

    public static Action<long>? OnBuildTicks { get; set; }
    public static bool GpuContextSeen => Volatile.Read(ref _gpuContextSeen) != 0;
    public static int PendingGpuDisposalCount => PendingGpuDisposals.Count + PendingPageDisposals.Count;
    public static int ActiveGpuPageCount => Volatile.Read(ref _activeGpuPageCount);
    private static int _activeGpuPageCount;

    public static void MarkRenderThread(bool hasGpu)
    {
        Volatile.Write(ref _renderThreadId, Environment.CurrentManagedThreadId);
        if (hasGpu) Volatile.Write(ref _gpuContextSeen, 1);
    }

    internal static bool CanStartGpuPack()
    {
        var maxCount = HyperSvgOptions.MaxGpuPacksPerFrame;
        var budgetMs = HyperSvgOptions.GpuPackBudgetMs;
        if (maxCount <= 0 && budgetMs <= 0) return true;

        var now = Stopwatch.GetTimestamp();
        var windowTicks = (long)(Stopwatch.Frequency * 0.010); // ~10 ms: même pass de rendu compositor
        if (now - _gpuPackWindowStart > windowTicks)
        {
            _gpuPackWindowStart = now;
            _gpuPacksInWindow = 0;
            _gpuPackElapsedTicks = 0;
            return true; // Garantit au moins 1 pack par fenêtre de frame
        }

        if (_gpuPacksInWindow == 0) return true;
        if (maxCount > 0 && _gpuPacksInWindow >= maxCount) return false;
        if (budgetMs > 0 && _gpuPackElapsedTicks >= (long)(budgetMs * Stopwatch.Frequency / 1000.0)) return false;
        return true;
    }

    internal static void RecordGpuPack(long elapsedTicks)
    {
        _gpuPacksInWindow++;
        _gpuPackElapsedTicks += Math.Max(0, elapsedTicks);
    }

    public static bool TryPackIntoGpuPage(
        GRContext grContext,
        SKImage src,
        int w,
        int h,
        out HyperGpuPage page,
        out float offsetX,
        out float offsetY)
    {
        // Called strictly on the render thread inside an active ISkiaSharpApiLease with grContext
        lock (GpuPageGate)
        {
            // First try pages that already have active slots (tightest packing), then empty pooled pages
            for (var pass = 0; pass < 2; pass++)
            {
                for (var i = GpuPages.Count - 1; i >= 0; i--)
                {
                    var candidate = GpuPages[i];
                    if (candidate.Disposed || !ReferenceEquals(candidate.Context, grContext)) continue;
                    var isEmpty = candidate.LiveSlots == 0;
                    if ((pass == 0 && isEmpty) || (pass == 1 && !isEmpty)) continue;

                    if (candidate.TryPack(src, w, h, out offsetX, out offsetY))
                    {
                        if (isEmpty) Interlocked.Increment(ref _activeGpuPageCount);
                        page = candidate;
                        return true;
                    }
                }
            }

            if (w + 2 <= HyperGpuPage.PageSize && h + 2 <= HyperGpuPage.PageSize)
            {
                var created = new HyperGpuPage(grContext);
                if (created.TryPack(src, w, h, out offsetX, out offsetY))
                {
                    GpuPages.Add(created);
                    Interlocked.Increment(ref _activeGpuPageCount);
                    page = created;
                    return true;
                }
                created.DisposeWithActiveContext();
            }
        }

        page = null!;
        offsetX = 0;
        offsetY = 0;
        return false;
    }

    public static void ReleaseGpuPageSlot(HyperGpuPage page)
    {
        lock (GpuPageGate)
        {
            if (page.Disposed || page.LiveSlots <= 0) return;
            page.LiveSlots--;
            if (page.LiveSlots > 0) return;

            Interlocked.Decrement(ref _activeGpuPageCount);
            var emptyCount = 0;
            for (var i = 0; i < GpuPages.Count; i++)
            {
                var p = GpuPages[i];
                if (!p.Disposed && p.LiveSlots == 0 && ReferenceEquals(p.Context, page.Context))
                    emptyCount++;
            }

            if (emptyCount <= MaxPooledEmptyPages)
            {
                page.NeedsReset = true;
            }
            else
            {
                GpuPages.Remove(page);
                PendingPageDisposals.Enqueue(page);
            }
        }
    }

    public static void DisposeOrQueueGpuImage(SKImage img)
    {
        PendingGpuDisposals.Enqueue(img);
    }

    public static void DrainPendingGpuDisposals(GRContext? grContext)
    {
        if (grContext == null) return;
        if (PendingPageDisposals.IsEmpty && PendingGpuDisposals.IsEmpty) return;

        var disposedAny = false;
        while (PendingPageDisposals.TryDequeue(out var page))
        {
            page.DisposeWithActiveContext();
            disposedAny = true;
        }
        while (PendingGpuDisposals.TryDequeue(out var img))
        {
            img.Dispose();
            disposedAny = true;
        }

        if (disposedAny)
        {
            grContext.Flush();
        }
    }

    public static int CachedCount
    {
        get { lock (Gate) return Cache.Count; }
    }

    public static void RegisterTemplateUser(int templateId)
    {
        lock (Gate)
        {
            TemplateUsers.TryGetValue(templateId, out var c);
            TemplateUsers[templateId] = c + 1;
        }
    }

    public static void UnregisterTemplateUser(int templateId)
    {
        List<HyperAtlas>? toDispose = null;
        lock (Gate)
        {
            if (!TemplateUsers.TryGetValue(templateId, out var c)) return;
            c--;
            if (c > 0)
            {
                TemplateUsers[templateId] = c;
                return;
            }

            TemplateUsers.Remove(templateId);
            if (Standby.TryGetValue(templateId, out var list))
            {
                Standby.Remove(templateId);
                for (var i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (!a.Disposed && a.RefCount <= 0)
                    {
                        a.Disposed = true;
                        if (Cache.TryGetValue(a.Key, out var cur) && ReferenceEquals(cur, a))
                            Cache.Remove(a.Key);
                        (toDispose ??= new List<HyperAtlas>()).Add(a);
                    }
                }
            }
        }

        if (toDispose != null)
        {
            for (var i = 0; i < toDispose.Count; i++)
                toDispose[i].DisposeResources();
        }
    }

    private static void ReviveLocked(HyperAtlas atlas)
    {
        atlas.RefCount++;
        if (Standby.TryGetValue(atlas.Key.TemplateId, out var list))
            list.Remove(atlas);
    }

    public static HyperAtlas Acquire(
        HyperSvgTemplate template,
        long themeSignature,
        HyperPaintSnapshot palette,
        Func<HyperTheme>? fallbackThemeFactory,
        float rasterScale)
    {
        var key = new HyperAtlasKey(template.Id, themeSignature, (int)Math.Round(rasterScale * 1000));
        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var existing) && !existing.Disposed)
            {
                ReviveLocked(existing);
                return existing;
            }
        }

        var built = HyperAtlas.Build(key, template, palette, fallbackThemeFactory, rasterScale);
        var disposeBuilt = false;
        HyperAtlas result;

        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var raceWinner) && !raceWinner.Disposed)
            {
                ReviveLocked(raceWinner);
                built.Disposed = true;
                disposeBuilt = true;
                result = raceWinner;
            }
            else
            {
                Cache[key] = built;
                result = built;
            }
        }

        if (disposeBuilt)
            built.DisposeResources();
        return result;
    }

    public static bool TryGetCached(HyperSvgTemplate template, long themeSignature, float rasterScale, out HyperAtlas? atlas)
    {
        var key = new HyperAtlasKey(template.Id, themeSignature, (int)Math.Round(rasterScale * 1000));
        lock (Gate)
        {
            if (Cache.TryGetValue(key, out var existing) && !existing.Disposed)
            {
                ReviveLocked(existing);
                atlas = existing;
                return true;
            }
        }
        atlas = null;
        return false;
    }

    internal static bool TryAddRef(HyperAtlas atlas)
    {
        lock (Gate)
        {
            if (atlas.Disposed) return false;
            ReviveLocked(atlas);
            return true;
        }
    }

    internal static bool Release(HyperAtlas atlas)
    {
        HyperAtlas? disposeTarget = null;
        var disposed = false;

        lock (Gate)
        {
            if (atlas.Disposed) return false;
            atlas.RefCount--;
            if (atlas.RefCount > 0) return false;

            var tid = atlas.Key.TemplateId;
            if (TemplateUsers.TryGetValue(tid, out var users) && users > 0)
            {
                if (!Standby.TryGetValue(tid, out var list))
                {
                    list = new List<HyperAtlas>(MaxStandbyPerTemplate + 1);
                    Standby[tid] = list;
                }
                list.Remove(atlas);
                list.Insert(0, atlas);

                if (list.Count > MaxStandbyPerTemplate)
                {
                    var evicted = list[^1];
                    list.RemoveAt(list.Count - 1);
                    if (!evicted.Disposed && evicted.RefCount <= 0)
                    {
                        evicted.Disposed = true;
                        if (Cache.TryGetValue(evicted.Key, out var cur) && ReferenceEquals(cur, evicted))
                            Cache.Remove(evicted.Key);
                        disposeTarget = evicted;
                    }
                }
            }
            else
            {
                atlas.Disposed = true;
                disposed = true;
                if (Cache.TryGetValue(atlas.Key, out var cur) && ReferenceEquals(cur, atlas))
                    Cache.Remove(atlas.Key);
                disposeTarget = atlas;
            }
        }

        disposeTarget?.DisposeResources();
        return disposed;
    }

    public static long ComputeThemeSignature(IReadOnlyList<object?> values)
    {
        var hash = new HashCode();
        for (var i = 0; i < values.Count; i++)
        {
            switch (values[i])
            {
                case Color c:
                    hash.Add(1);
                    hash.Add(c.ToUInt32());
                    break;
                case ISolidColorBrush s:
                    hash.Add(2);
                    hash.Add(s.Color.ToUInt32());
                    hash.Add(s.Opacity);
                    break;
                case ILinearGradientBrush lg:
                    hash.Add(3);
                    hash.Add(lg.StartPoint);
                    hash.Add(lg.EndPoint);
                    foreach (var st in lg.GradientStops) { hash.Add(st.Offset); hash.Add(st.Color.ToUInt32()); }
                    break;
                case IRadialGradientBrush rg:
                    hash.Add(4);
                    hash.Add(rg.Center);
                    hash.Add(rg.GradientOrigin);
                    foreach (var st in rg.GradientStops) { hash.Add(st.Offset); hash.Add(st.Color.ToUInt32()); }
                    break;
                default:
                    hash.Add(0);
                    break;
            }
        }
        return hash.ToHashCode();
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Déclaration explicite du mode de vie SVG :
///     - <see cref="Shared"/> : gabarit fixe réutilisé et dédupliqué en cache (compilé une seule fois).
///     - <see cref="Unique"/> : illustration générée à la volée (0 % réutilisation), libérée dès le détachement.
/// </summary>
public sealed class HyperSource
{
    private static readonly ConcurrentDictionary<string, HyperSvgTemplate> SharedLiterals =
        new(ReferenceEqualityComparer.Instance);

    private readonly string _svg;
    private HyperSvgTemplate? _sharedTemplate;

    /// <summary>Indique si la source est unique à cette instance (libérée au détachement).</summary>
    public bool IsUnique { get; }

    private HyperSource(string svg, bool isUnique)
    {
        _svg = svg ?? throw new ArgumentNullException(nameof(svg));
        IsUnique = isUnique;
    }

    /// <summary>Déclare un SVG statique partagé et mis en cache entre toutes les instances.</summary>
    public static HyperSource Shared(string svg) => new(svg, isUnique: false);

    /// <summary>Déclare un SVG généré pour une seule instance et libéré dès le détachement.</summary>
    public static HyperSource Unique(string svg) => new(svg, isUnique: true);

    internal HyperSvgTemplate AcquireTemplate(out bool ownsTemplate)
    {
        if (IsUnique)
        {
            ownsTemplate = true;
            return new HyperSvgTemplate(_svg);
        }

        ownsTemplate = false;
        if (_sharedTemplate != null) return _sharedTemplate;

        if (string.IsInterned(_svg) != null)
            return _sharedTemplate = SharedLiterals.GetOrAdd(_svg, static s => new HyperSvgTemplate(s));

        return _sharedTemplate = new HyperSvgTemplate(_svg);
    }
}

/// <summary>
///     Surface de liaison sécurisée fournie dans <see cref="HyperControl{TState}.HyperStage.OnBind"/>.
///     Tout objet créé ou enregistré via ce binder est automatiquement possédé et libéré au détachement.
/// </summary>
public interface IHyperBinder
{
    /// <summary>Récupère un calque SVG nommé via <c>data-part</c>.</summary>
    HyperLayer Layer(string name);

    /// <summary>Attache un champ de particules dans une fenêtre <c>data-window</c>.</summary>
    HyperParticleField Particles(
        string windowName,
        int count = 60,
        int seed = 42,
        double brightness = 1.0,
        double pixelSize = 0.0);

    /// <summary>Enregistre un peintre Skia personnalisé sur une fenêtre <c>data-window</c>.</summary>
    void Paint(string windowName, HyperPainter painter);

    /// <summary>Récupère un chemin SVG nommé via <c>data-path</c>.</summary>
    HyperPath Path(string name);

    /// <summary>Récupère une zone SVG nommée via <c>data-zone</c>.</summary>
    Rect Zone(string name);

    /// <summary>Transfère la propriété d'un objet <see cref="IDisposable"/> pour libération automatique au détachement.</summary>
    T Own<T>(T item) where T : class, IDisposable;
}

internal sealed class HyperAllocGuard
{
#if DEBUG
    private const int WarmUp = 10;
    private readonly string _owner;
    private long _before;
    private int _calls;
    private int _reports;

    public HyperAllocGuard(string owner) => _owner = owner;
#else
    public HyperAllocGuard(string owner) { }
#endif

    [Conditional("DEBUG")]
    public void Begin()
    {
#if DEBUG
        _before = GC.GetAllocatedBytesForCurrentThread();
#endif
    }

    [Conditional("DEBUG")]
    public void End(string phase)
    {
#if DEBUG
        var allocated = GC.GetAllocatedBytesForCurrentThread() - _before;
        if (++_calls <= WarmUp || allocated <= 0 || _reports >= 3) return;
        _reports++;
        Debug.WriteLine($"[HyperControl] {_owner}.{phase} a alloué {allocated} octets sur le thread de rendu : pré-allouer dans OnBind().");
#endif
    }
}

/// <summary>
///     Classe de base unifiée pour les contrôles HyperSvg :
///     - Fichier public (<c>Control.cs</c>) : propriétés Avalonia + <see cref="AffectsRenderState"/> + <see cref="CaptureState"/>.
///     - Fichier rendu (<c>Control.Rendering.cs</c>) : <see cref="Source"/> + <see cref="HyperStage"/> encapsulé.
///     Toute la plomberie GPU (atlas 2D, pool de pages 2048×2048, purge GL sûre, thread rendu, pause si caché,
///     libération des illustrations uniques, suivi pointeur) est gérée automatiquement.
/// </summary>
public abstract class HyperControl<TState> : Control, ICustomHitTest where TState : struct
{
    private sealed class TrackedUniqueTemplate
    {
        public readonly HyperSvgTemplate Template;
        public int ActiveBuilds;
        public bool ReleasedByControl;

        public TrackedUniqueTemplate(HyperSvgTemplate template) => Template = template;

        public void BeginBuild() => Interlocked.Increment(ref ActiveBuilds);

        public void EndBuild()
        {
            if (Interlocked.Decrement(ref ActiveBuilds) == 0 && ReleasedByControl)
                Template.Dispose();
        }

        public void ReleaseFromControl()
        {
            ReleasedByControl = true;
            if (Volatile.Read(ref ActiveBuilds) == 0)
                Template.Dispose();
        }
    }

    private sealed class HyperBinder : IHyperBinder
    {
        private readonly HyperRig _rig;
        private readonly List<IDisposable> _owned;

        public HyperBinder(HyperRig rig, List<IDisposable> owned)
        {
            _rig = rig;
            _owned = owned;
        }

        public HyperLayer Layer(string name) => _rig[name];

        public HyperParticleField Particles(
            string windowName,
            int count = 60,
            int seed = 42,
            double brightness = 1.0,
            double pixelSize = 0.0)
        {
            var win = _rig[windowName].Bounds;
            var field = Own(new HyperParticleField(count, win.X, win.Y, win.Width, win.Height, seed));
            _rig.AttachParticles(windowName, field, brightness, pixelSize);
            return field;
        }

        public void Paint(string windowName, HyperPainter painter) => _rig.Paint(windowName, painter);

        public HyperPath Path(string name) => _rig.Path(name);

        public Rect Zone(string name) => _rig.Zone(name);

        public T Own<T>(T item) where T : class, IDisposable
        {
            ArgumentNullException.ThrowIfNull(item);
            _owned.Add(item);
            return item;
        }
    }

    /// <summary>Scène d'animation et de dessin exécutée sur le thread de rendu pour ce contrôle.</summary>
    protected abstract class HyperStage
    {
        private readonly HyperAllocGuard _guard;
        private readonly List<IDisposable> _owned = new();
        private HyperRig? _rig;
        private int _fps;

        /// <summary>Initialise une nouvelle scène.</summary>
        protected HyperStage()
        {
            _guard = new HyperAllocGuard(GetType().DeclaringType?.Name ?? GetType().Name);
        }

        internal int MaxFpsInternal => _fps;
        internal List<IDisposable> OwnedInternal => _owned;

        /// <summary>Cadence maximale souhaitée en images par seconde (0 = cadence native).</summary>
        protected int TargetFps { get => _fps; set => _fps = value; }

        internal void BindInternal(HyperRig rig)
        {
            _rig = rig;
            OnBind(new HyperBinder(rig, _owned));
        }

        /// <summary>Appelé une fois lors du montage pour lier calques, chemins, zones et ressources natives.</summary>
        protected virtual void OnBind(IHyperBinder ui) { }

        /// <summary>Met à jour les calques pour l'image courante et retourne <c>true</c> si une autre image est nécessaire.</summary>
        protected abstract bool OnFrame(in HyperFrame frame, in TState state);

        /// <summary>Dessine la scène sur le canvas Skia (à surcharger seulement pour ajouter un dessin direct).</summary>
        protected virtual void OnDraw(SKCanvas canvas, double opacity) => _rig?.Draw(canvas, opacity);

        /// <summary>Ajuste la luminosité et la pixelisation des champs de particules attachés.</summary>
        protected void SetParticleParams(double brightness, double pixelSize) =>
            _rig?.SetParticleParams(brightness, pixelSize);

        internal bool AdvanceInternal(in HyperFrame frame, in TState state)
        {
            _guard.Begin();
            var keepAnimating = OnFrame(in frame, in state);
            _guard.End(nameof(OnFrame));
            return keepAnimating;
        }

        internal void DrawInternal(SKCanvas canvas, double opacity) => OnDraw(canvas, opacity);
    }

    private sealed class StageSkinAdapter : HyperSkin<TState>
    {
        private readonly HyperStage _stage;

        public StageSkinAdapter(HyperStage stage, HyperRig rig) : base(rig) => _stage = stage;

        public override int MaxFps => _stage.MaxFpsInternal;

        public override bool Advance(in HyperFrame frame, in TState state) =>
            _stage.AdvanceInternal(in frame, in state);

        public override void Draw(SKCanvas canvas, double opacity) =>
            _stage.DrawInternal(canvas, opacity);

        public override void Dispose()
        {
            var owned = _stage.OwnedInternal;
            for (var i = owned.Count - 1; i >= 0; i--) owned[i].Dispose();
            owned.Clear();
            base.Dispose();
        }
    }

    private static readonly Dictionary<HyperAtlasKey, Task<HyperAtlas>> InFlight = new();
    private static readonly object InFlightGate = new();
    private static readonly SemaphoreSlim BuildConcurrencyGate = new(Math.Clamp(Environment.ProcessorCount / 2, 1, 4));

    private readonly HyperShared<TState> _shared = new();
    private HyperSource? _source;
    private HyperSvgTemplate? _template;
    private TrackedUniqueTemplate? _uniqueTracker;
    private object?[] _values = Array.Empty<object?>();
    private CompositionCustomVisual? _visual;
    private Size _visualSize;
    private long _lastThemeSig = long.MinValue;
    private double _lastScale = double.NaN;
    private int _generation;
    private bool _templateUserRegistered;
    private bool _hasVisualStage;
    private bool _awaitingFirstMount;
    private bool _mountQueued;
    private bool _hover;
    private bool _pressed;

    static HyperControl()
    {
        IsVisibleProperty.Changed.AddClassHandler<HyperControl<TState>>((c, e) =>
            c.OnVisibilityChanged(e.GetNewValue<bool>()));
    }

    /// <summary>Initialise les abonnements au thème, aux ressources et aux événements de pointeur.</summary>
    protected HyperControl()
    {
        ActualThemeVariantChanged += (_, _) => { Refresh(); Rebuild(false); };
        ResourcesChanged += (_, _) => { Refresh(); Rebuild(false); };

        PointerEntered += (_, _) => SetInput(true, _pressed);
        PointerExited += (_, _) => SetInput(false, _pressed);
        AddHandler(PointerPressedEvent, (_, _) => SetInput(_hover, true), handledEventsToo: true);
        AddHandler(PointerReleasedEvent, (_, _) => SetInput(_hover, false), handledEventsToo: true);
        PointerCaptureLost += (_, _) => SetInput(_hover, false);
        EffectiveViewportChanged += (_, _) => CheckScaleRebuild();
    }

    /// <summary>Source SVG du contrôle (<see cref="HyperSource.Shared"/> ou <see cref="HyperSource.Unique"/>).</summary>
    protected abstract HyperSource Source { get; }

    /// <summary>Capture l'état immuable courant pour le transmettre au thread de rendu.</summary>
    protected abstract TState CaptureState();

    /// <summary>Instancie la scène responsable de l'animation et du dessin.</summary>
    protected abstract HyperStage CreateStage();

    /// <summary>Déclare les propriétés Avalonia dont les changements doivent republier l'état de rendu.</summary>
    protected static void AffectsRenderState(params AvaloniaProperty[] properties)
    {
        foreach (var property in properties)
            property.Changed.AddClassHandler<HyperControl<TState>>((c, _) => c.Refresh());
    }

    /// <summary>Retourne le rectangle d'une zone <c>data-zone</c> mis à l'échelle du contrôle.</summary>
    protected Rect Zone(string name)
    {
        var template = EnsureTemplate();
        var zone = template.Zone(name);
        var vb = template.ViewBox;
        var b = Bounds.Size;
        if (b.Width <= 0 || b.Height <= 0 || vb.Width <= 0 || vb.Height <= 0)
            return zone;

        var sx = b.Width / vb.Width;
        var sy = b.Height / vb.Height;
        if (Math.Abs(sx - 1.0) < 1e-4 && Math.Abs(sy - 1.0) < 1e-4)
            return zone;

        return new Rect((zone.X - vb.X) * sx, (zone.Y - vb.Y) * sy, zone.Width * sx, zone.Height * sy);
    }

    /// <summary>Teste si <paramref name="point"/> (en coordonnées du contrôle) appartient à la zone <paramref name="zone"/>.</summary>
    protected bool Hit(string zone, Point point) => Zone(zone).Contains(point);

    /// <summary>Republie l'état courant et réveille la scène sur le thread de rendu.</summary>
    protected void Refresh()
    {
        _shared.Publish(CaptureState());
        Wake();
    }

    private HyperSvgTemplate EnsureTemplate()
    {
        if (_template != null) return _template;
        _source ??= Source;
        var template = _source.AcquireTemplate(out var ownsTemplate);
        _template = template;
        _uniqueTracker = ownsTemplate ? new TrackedUniqueTemplate(template) : null;
        if (_values.Length != template.Tokens.Count)
            _values = template.Tokens.Count == 0 ? Array.Empty<object?>() : new object?[template.Tokens.Count];
        return template;
    }

    private void SetInput(bool hover, bool pressed)
    {
        if (hover == _hover && pressed == _pressed) return;
        _hover = hover;
        _pressed = pressed;
        _shared.UpdateInput(hover, pressed);
        Wake();
    }

    private void OnVisibilityChanged(bool visible)
    {
        if (_visual == null) return;
        _visual.SendHandlerMessage(visible ? HyperMessages.Visible : HyperMessages.Hidden);
        if (visible)
        {
            if (_awaitingFirstMount && Bounds.Width > 0 && Bounds.Height > 0)
                ScheduleFirstMount();
            else
                Wake();
        }
    }

    private void Wake()
    {
        if (_visual != null && IsVisible && _shared.ArmWake())
            _visual.SendHandlerMessage(HyperMessages.Wake);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var template = EnsureTemplate();
        if (!_templateUserRegistered)
        {
            _templateUserRegistered = true;
            HyperAtlasCache.RegisterTemplateUser(template.Id);
        }

        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor == null) return;

        if (_visual == null || _visual.Compositor != compositor)
        {
            _visual = compositor.CreateCustomVisual(new HyperHandler<TState>(_shared, template.ViewBox));
            _visualSize = default;
            _hasVisualStage = false;
            ElementComposition.SetElementChildVisual(this, _visual);
        }

        _visual.Opacity = 1f;
        SyncVisualSize(Bounds.Size);
        if (!IsVisible) _visual.SendHandlerMessage(HyperMessages.Hidden);
        _shared.DisarmWake();
        _awaitingFirstMount = true;
        _mountQueued = false;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _generation++;
        _awaitingFirstMount = false;
        _mountQueued = false;
        _hasVisualStage = false;
        Array.Clear(_values);
        _lastThemeSig = long.MinValue;
        _lastScale = double.NaN;

        var template = _template;
        if (_templateUserRegistered && template != null)
        {
            _templateUserRegistered = false;
            HyperAtlasCache.UnregisterTemplateUser(template.Id);
        }

        _visual?.SendHandlerMessage(HyperMessages.Release);

        if (_uniqueTracker != null)
        {
            _uniqueTracker.ReleaseFromControl();
            _uniqueTracker = null;
            _template = null;
        }
    }

    protected override Size MeasureOverride(Size availableSize) => EnsureTemplate().NaturalSize;

    protected override Size ArrangeOverride(Size finalSize)
    {
        SyncVisualSize(finalSize);
        if (VisualRoot != null)
        {
            if (_awaitingFirstMount)
            {
                if (IsVisible && finalSize.Width > 0 && finalSize.Height > 0)
                    ScheduleFirstMount();
            }
            else if (Math.Abs(ComputeEffectiveScale(EnsureTemplate(), finalSize) - _lastScale) > 0.05)
            {
                Rebuild(false);
            }
        }
        return finalSize;
    }

    private void CheckScaleRebuild()
    {
        if (VisualRoot == null || _visual == null || _awaitingFirstMount) return;
        if (Math.Abs(ComputeEffectiveScale(EnsureTemplate(), Bounds.Size) - _lastScale) > 0.05)
            Rebuild(false);
    }

    private void ScheduleFirstMount()
    {
        if (!_awaitingFirstMount || _mountQueued || VisualRoot == null || _visual == null || !IsVisible)
            return;

        _mountQueued = true;
        var generation = _generation;
        HyperMountQueue.Enqueue(() => PerformFirstMount(generation));
    }

    private void PerformFirstMount(int expectedGeneration)
    {
        if (expectedGeneration != _generation) return;
        _mountQueued = false;
        if (!_awaitingFirstMount || VisualRoot == null || _visual == null || !IsVisible) return;
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

        _awaitingFirstMount = false;
        _shared.Publish(CaptureState());
        Rebuild(true);
        Wake();
    }

    private double ComputeEffectiveScale(HyperSvgTemplate template, Size boundsSize)
    {
        var dpiScale = LayoutHelper.GetLayoutScale(this);
        var fitScale = 1.0;
        if (boundsSize.Width > 0 && boundsSize.Height > 0 && template.ViewBox.Width > 0 && template.ViewBox.Height > 0)
            fitScale = Math.Max(boundsSize.Width / template.ViewBox.Width, boundsSize.Height / template.ViewBox.Height);
        if (VisualRoot is Visual root && this.TransformToVisual(root) is { } m)
        {
            var vx = Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12);
            var vy = Math.Sqrt(m.M21 * m.M21 + m.M22 * m.M22);
            var vScale = Math.Max(vx, vy);
            if (vScale > fitScale) fitScale = vScale;
        }
        return Math.Clamp(dpiScale * Math.Max(1.0, fitScale), 1.0, 8.0);
    }

    private void SyncVisualSize(Size size)
    {
        if (_visual == null || size == _visualSize) return;
        _visualSize = size;
        _visual.Size = new Vector(size.Width, size.Height);
        if (size.Width > 0 && size.Height > 0)
            _visual.SendHandlerMessage(new HyperResizeMessage(size));
    }

    bool ICustomHitTest.HitTest(Point point) =>
        point.X >= 0 && point.Y >= 0 && point.X <= Bounds.Width && point.Y <= Bounds.Height;

    private void SendAtlasToVisual(CompositionCustomVisual visual, HyperSvgTemplate template, HyperAtlas atlas)
    {
        if (_hasVisualStage)
        {
            visual.SendHandlerMessage(new HyperSwapAtlasMessage(atlas));
            return;
        }

        _hasVisualStage = true;
        var t0 = HyperSvgOptions.LogFirstRenderPerf ? Stopwatch.GetTimestamp() : 0;
        var rig = new HyperRig(template, atlas);
        var stage = CreateStage();
        stage.BindInternal(rig);
        var skin = new StageSkinAdapter(stage, rig);
        if (HyperSvgOptions.LogFirstRenderPerf)
        {
            var ms = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
            Console.WriteLine($"[HyperPerf] Bind {GetType().Name} (#{template.Id}): {ms:0.0}ms");
        }
        visual.SendHandlerMessage(skin);
    }

    private void Rebuild(bool force)
    {
        if (_awaitingFirstMount || VisualRoot == null || _visual == null || ActualThemeVariant is null) return;
        var template = EnsureTemplate();

        for (var i = 0; i < _values.Length; i++)
        {
            if (!this.TryFindResource(template.Tokens[i], ActualThemeVariant, out var value))
                HyperSvgOptions.ExternalResourceLookup?.Invoke(template.Tokens[i], ActualThemeVariant, out value);
            _values[i] = value;
        }

        var scale = ComputeEffectiveScale(template, Bounds.Size);
        var sig = HyperAtlasCache.ComputeThemeSignature(_values);
        if (!force && sig == _lastThemeSig && Math.Abs(scale - _lastScale) <= 0.05) return;

        _lastThemeSig = sig;
        _lastScale = scale;
        var rasterScale = (float)Math.Max(1.0, scale);

        if (HyperAtlasCache.TryGetCached(template, sig, rasterScale, out var cached))
        {
            SendAtlasToVisual(_visual, template, cached!);
            return;
        }

        var generation = ++_generation;
        var visual = _visual;
        var palette = HyperPaintSnapshot.Capture(_values);
        Func<HyperTheme>? fallbackFactory = _values.Length > 0
            ? () => HyperTheme.FromSnapshot(template.Tokens, palette)
            : null;
        var key = new HyperAtlasKey(template.Id, sig, (int)Math.Round(rasterScale * 1000));

        _ = AcquireAsync(key, template, _uniqueTracker, sig, palette, fallbackFactory, rasterScale, generation, visual);
    }

    private async Task AcquireAsync(
        HyperAtlasKey key,
        HyperSvgTemplate template,
        TrackedUniqueTemplate? uniqueTracker,
        long sig,
        HyperPaintSnapshot palette,
        Func<HyperTheme>? fallbackFactory,
        float rasterScale,
        int generation,
        CompositionCustomVisual visual)
    {
        uniqueTracker?.BeginBuild();
        Task<HyperAtlas> task;
        var isCreator = false;
        try
        {
            lock (InFlightGate)
            {
                if (!InFlight.TryGetValue(key, out task!))
                {
                    isCreator = true;
                    task = Task.Run(async () =>
                    {
                        await BuildConcurrencyGate.WaitAsync().ConfigureAwait(false);
                        try
                        {
                            return HyperAtlasCache.Acquire(template, sig, palette, fallbackFactory, rasterScale);
                        }
                        finally
                        {
                            BuildConcurrencyGate.Release();
                        }
                    });
                    InFlight[key] = task;
                }
            }

            HyperAtlas atlas;
            try
            {
                atlas = await task;
            }
            finally
            {
                if (isCreator)
                {
                    lock (InFlightGate)
                    {
                        if (InFlight.TryGetValue(key, out var cur) && ReferenceEquals(cur, task))
                            InFlight.Remove(key);
                    }
                }
            }

            await HyperMountQueue.EnqueueAsync(() =>
            {
                if (generation == _generation && atlas.TryAddRef())
                    SendAtlasToVisual(visual, template, atlas);
            });

            if (isCreator)
                atlas.Release();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[HyperControl] AcquireAsync failed: {ex}");
        }
        finally
        {
            uniqueTracker?.EndBuild();
        }
    }

    public sealed override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
    }
}

/// <summary>
///     File UI thread unique partagée entre tous les contrôles HyperSvg afin d'échelonner les premiers montages
///     (TryFindResource, CreateStage, OnBind, callbacks AcquireAsync) selon un budget adaptatif à la charge UI réelle.
/// </summary>
internal static class HyperMountQueue
{
    private readonly record struct MountItem(Action Work, bool YieldAfter);

    private static readonly object Gate = new();
    private static readonly Queue<MountItem> Pending = new();
    private static bool _scheduled;
    private static long _scheduledAtTicks;
    private static int _plannedDelayMs;

    public static void Enqueue(Action work) => EnqueueCore(work, honorInitialDelay: true, yieldAfter: false);

    public static void EnqueueStaggered(Action work) => EnqueueCore(work, honorInitialDelay: true, yieldAfter: true);

    public static Task EnqueueAsync(Action work)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        EnqueueCore(() =>
        {
            try
            {
                work();
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, honorInitialDelay: false, yieldAfter: false);
        return tcs.Task;
    }

    private static void EnqueueCore(Action work, bool honorInitialDelay, bool yieldAfter)
    {
        bool start;
        lock (Gate)
        {
            Pending.Enqueue(new MountItem(work, yieldAfter));
            start = !_scheduled;
            if (start) _scheduled = true;
        }
        if (start)
            ScheduleNext(honorInitialDelay ? HyperSvgOptions.FirstRenderInitialDelayMs : 0);
    }

    private static void ScheduleNext(int delayMs)
    {
        Volatile.Write(ref _plannedDelayMs, delayMs);
        Volatile.Write(ref _scheduledAtTicks, Stopwatch.GetTimestamp());
        if (delayMs > 0)
        {
            _ = Task.Delay(delayMs).ContinueWith(
                static _ => Dispatcher.UIThread.Post(Drain, DispatcherPriority.Background),
                TaskScheduler.Default);
        }
        else
        {
            Dispatcher.UIThread.Post(Drain, DispatcherPriority.Background);
        }
    }

    private static double ComputeAdaptiveBudgetMs(long nowTicks)
    {
        var maxBudget = Math.Max(0.2, HyperSvgOptions.FirstRenderBudgetMs);
        var scheduledAt = Volatile.Read(ref _scheduledAtTicks);
        var plannedDelay = Volatile.Read(ref _plannedDelayMs);
        var totalWaitMs = (nowTicks - scheduledAt) * 1000.0 / Stopwatch.Frequency;
        var uiLagMs = Math.Max(0.0, totalWaitMs - plannedDelay);

        if (uiLagMs >= 12.0) return Math.Min(maxBudget, 0.3);
        if (uiLagMs >= 4.0) return Math.Min(maxBudget, 1.2);
        return maxBudget;
    }

    private static void Drain()
    {
        var start = Stopwatch.GetTimestamp();
        var budgetMs = ComputeAdaptiveBudgetMs(start);
        var budgetTicks = (long)(budgetMs * Stopwatch.Frequency / 1000.0);
        var maxItems = HyperSvgOptions.MaxMountsPerTick;
        var executed = 0;
        var yieldAfterLast = false;

        while (true)
        {
            MountItem item;
            lock (Gate)
            {
                if (Pending.Count == 0)
                {
                    _scheduled = false;
                    return;
                }
                if (executed > 0 && (yieldAfterLast || (maxItems > 0 && executed >= maxItems) || Stopwatch.GetTimestamp() - start >= budgetTicks))
                {
                    var elapsedMs = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
                    var minStep = yieldAfterLast ? Math.Max(HyperSvgOptions.FirstRenderStepMs, 16) : HyperSvgOptions.FirstRenderStepMs;
                    var nextDelayMs = Math.Clamp((int)Math.Ceiling(elapsedMs * 2.0), minStep, 32);
                    ScheduleNext(nextDelayMs);
                    return;
                }
                item = Pending.Dequeue();
            }

            executed++;
            yieldAfterLast = item.YieldAfter;
            try
            {
                item.Work();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[HyperMountQueue] Work failed: {ex}");
            }
        }
    }
}

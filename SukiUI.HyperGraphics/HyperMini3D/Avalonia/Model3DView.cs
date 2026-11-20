using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.HyperGraphics;

/// <summary>
///     Affiche un Model3D, orientable a la souris (glisser = tourner, double-clic dans le vide = recentrer, clic sur une
///     piece Clickable = evenement Clicked). Les entrees du modele se pilotent par SetChannel et s'animent en douceur.
///     Cote thread UI, cette vue ne calcule ni ne dessine rien : elle envoie des messages au moteur (thread de rendu).
///     Au repos, l'image est figee en bitmap : meme cout qu'une Image. Cachee ou detachee, elle ne garde rien.
/// </summary>
public sealed class Model3DView : Control, ICustomHitTest
{
    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<Model3DView, bool>(nameof(IsInteractive), true);

    public static readonly StyledProperty<bool> IsWireframeProperty =
        AvaloniaProperty.Register<Model3DView, bool>(nameof(IsWireframe));

    private const double ClickTolerance = 4, YawPerPixel = .008, PitchPerPixel = .006;
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(500);

    private readonly List<Visual> _visibilityAncestors = new();
    private Model3D? _model;
    private double[] _inputs = Array.Empty<double>();
    private CompositionCustomVisual? _visual;
    private bool _attached, _active;
    private Point? _dragFrom;
    private Point _pressedAt;
    private bool _dragged;
    private int _clickCount;
    private bool _hovering;
    private Cursor? _handCursor;
    private IPointer? _capturedPointer;
    private IDisposable? _settleTimer;

    public Model3DView()
    {
        ClipToBounds = true;
        AttachedToVisualTree += (_, _) => Attach();
        DetachedFromVisualTree += (_, _) => Detach();
        ActualThemeVariantChanged += (_, _) => SendFlags();
    }

    /// <summary>Clic sur une piece declaree Clickable(id).</summary>
    public event EventHandler<Model3DClickedEventArgs>? Clicked;

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public bool IsWireframe
    {
        get => GetValue(IsWireframeProperty);
        set => SetValue(IsWireframeProperty, value);
    }

    /// <summary>Modele affiche. Le changer reconstruit le moteur (les entrees reprennent leurs valeurs initiales).</summary>
    public Model3D? Model
    {
        get => _model;
        set
        {
            if (ReferenceEquals(value, _model)) return;
            if (_attached) Detach();
            _model = value;
            _inputs = new double[value?.Info.Inputs.Count ?? 0];
            if (value != null)
                for (var i = 0; i < _inputs.Length; i++) _inputs[i] = value.Info.Inputs[i].Initial;
            if (VisualRoot != null) Attach();
        }
    }

    /// <summary>Change une entree du modele (s.Input) ; animee sur la duree declaree, ou immediate si animate = false.</summary>
    public void SetChannel(string name, double value, bool animate = true)
    {
        if (_model == null) throw new InvalidOperationException("Mini3D : Model doit etre affecte avant SetChannel.");
        var slot = _model.Info.InputSlot(name);
        if (_inputs[slot] == value) return;
        _inputs[slot] = value;
        Send(new AnimateMessage(slot, value, animate));
    }

    public double GetChannel(string name) =>
        _model == null ? throw new InvalidOperationException("Mini3D : aucun Model.") : _inputs[_model.Info.InputSlot(name)];

    bool ICustomHitTest.HitTest(Point point) => new Rect(Bounds.Size).Contains(point);

    private void Attach()
    {
        _attached = true;
        WatchAncestors();
        var compositor = ElementComposition.GetElementVisual(this)?.Compositor;
        if (compositor == null || _model == null) return;
        _visual = compositor.CreateCustomVisual(new Model3DEngine(_model, OnEngineReply));
        _visual.ClipToBounds = true;
        _visual.Size = new Vector(Bounds.Width, Bounds.Height);
        ElementComposition.SetElementChildVisual(this, _visual);
        _active = ComputeActive();
        Send(new ActiveMessage(_active));
        Send(new InitMessage((double[])_inputs.Clone(), IsWireframe, IsDark, Intro: true));
    }

    /// <summary>Hors de l'arbre visuel, ne garde rien : le moteur libere tout, le visuel est retire.</summary>
    private void Detach()
    {
        Send(new ReleaseMessage());
        ElementComposition.SetElementChildVisual(this, null);
        _visual = null;
        _attached = _active = false;
        foreach (var ancestor in _visibilityAncestors) ancestor.PropertyChanged -= OnAncestorPropertyChanged;
        _visibilityAncestors.Clear();
        _visibilityAncestors.TrimExcess();
        CancelSettle();
        EndInteraction();
        _handCursor?.Dispose();
        _handCursor = null;
    }

    private void Send(object message) => _visual?.SendHandlerMessage(message);

    private bool IsDark => ActualThemeVariant == ThemeVariant.Dark;

    private void SendFlags() => Send(new FlagsMessage(IsWireframe, IsDark));

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsWireframeProperty) SendFlags();
        else if (change.Property == IsInteractiveProperty && !IsInteractive) EndInteraction();
        else if (change.Property == BoundsProperty && _visual != null) _visual.Size = new Vector(Bounds.Width, Bounds.Height);
        else if (change.Property == IsVisibleProperty || change.Property == OpacityProperty) UpdateActivity();
    }

    // ---------------------------------------------------------------- visibilite : cache = rien en memoire, rien d'anime

    private void WatchAncestors()
    {
        foreach (var ancestor in _visibilityAncestors) ancestor.PropertyChanged -= OnAncestorPropertyChanged;
        _visibilityAncestors.Clear();
        foreach (var ancestor in this.GetVisualAncestors())
        {
            _visibilityAncestors.Add(ancestor);
            ancestor.PropertyChanged += OnAncestorPropertyChanged;
        }
    }

    private void OnAncestorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsVisibleProperty || change.Property == OpacityProperty || change.Property == Window.WindowStateProperty)
            UpdateActivity();
    }

    private bool ComputeActive()
    {
        if (!_attached || !IsVisible || Opacity <= 0) return false;
        foreach (var ancestor in _visibilityAncestors)
            if (!ancestor.IsVisible || ancestor.Opacity <= 0 || ancestor is Window { WindowState: WindowState.Minimized })
                return false;
        return true;
    }

    private void UpdateActivity()
    {
        var active = ComputeActive();
        if (active == _active) return;
        _active = active;
        Send(new ActiveMessage(active));
        if (active) return;
        CancelSettle();
        EndInteraction();
    }

    // ---------------------------------------------------------------- reponses du moteur

    private void OnEngineReply(object reply)
    {
        if (_visual == null) return;
        switch (reply)
        {
            case IdleReply idle when _active:
                // Rien ne bouge : apres un court delai, le moteur fige l'image en bitmap et libere la scene
                CancelSettle();
                _settleTimer = DispatcherTimer.RunOnce(() =>
                {
                    _settleTimer = null;
                    Send(new SettleMessage(idle.Generation));
                }, SettleDelay);
                break;
            case HitReply { Click: false } hover when _dragFrom == null && IsInteractive:
                var over = hover.Target != null;
                if (over == _hovering) break;
                _hovering = over;
                Cursor = over ? _handCursor ??= new Cursor(StandardCursorType.Hand) : null;
                break;
            case HitReply { Click: true, Target: { } id }:
                Clicked?.Invoke(this, new Model3DClickedEventArgs(id));
                break;
        }
    }

    private void CancelSettle()
    {
        _settleTimer?.Dispose();
        _settleTimer = null;
    }

    // ---------------------------------------------------------------- souris : glisser = tourner, clic = animer

    private void EndInteraction()
    {
        if (_dragged) Send(new DragMessage(false));
        _dragFrom = null;
        _dragged = false;
        _capturedPointer?.Capture(null);
        _capturedPointer = null;
        _hovering = false;
        Cursor = null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsInteractive) return;
        _pressedAt = e.GetPosition(this);
        _dragFrom = _pressedAt;
        _dragged = false;
        _clickCount = e.ClickCount;
        _capturedPointer = e.Pointer;
        e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!IsInteractive || !_active) return;
        var p = e.GetPosition(this);
        if (_dragFrom is not { } from)
        {
            if (_model?.Info.HasTargets == true) Send(new HitMessage(p, false, 0));
            return;
        }
        if (!_dragged && Distance(p, _pressedAt) < ClickTolerance) return;
        if (!_dragged)
        {
            _dragged = true;
            Send(new DragMessage(true));
        }
        Send(new RotateMessage((p.X - from.X) * YawPerPixel, (p.Y - from.Y) * PitchPerPixel));
        _dragFrom = p;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_dragFrom == null) return;
        _dragFrom = null;
        _capturedPointer = null;
        e.Pointer.Capture(null);
        if (_dragged)
        {
            _dragged = false;
            Send(new DragMessage(false));
            return;
        }
        Send(new HitMessage(e.GetPosition(this), true, _clickCount));
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _dragFrom = null;
        _capturedPointer = null;
        if (!_dragged) return;
        _dragged = false;
        Send(new DragMessage(false));
    }

    private static double Distance(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
}

public sealed class Model3DClickedEventArgs : EventArgs
{
    public Model3DClickedEventArgs(string id) => Id = id;

    /// <summary>Identifiant passe a Clickable(id).</summary>
    public string Id { get; }
}

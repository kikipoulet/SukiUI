using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace SukiUI.HyperGraphics;

// ---------------------------------------------------------------- protocole UI -> moteur (messages immuables)

/// <summary>Etat complet (attachement) : valeurs des entrees (sautees), filaire, theme, intro.</summary>
internal sealed record InitMessage(double[] Inputs, bool Wire, bool Dark, bool Intro);

/// <summary>Entree (indice dans ModelInfo.Inputs) vers To, animee sur la duree declaree par le modele, ou sautee.</summary>
internal sealed record AnimateMessage(int Input, double To, bool Animate);

/// <summary>Increment de rotation pendant un glisser (radians).</summary>
internal sealed record RotateMessage(double Yaw, double Pitch);

internal sealed record DragMessage(bool Dragging);

internal sealed record FlagsMessage(bool Wire, bool Dark);

internal sealed record ActiveMessage(bool Active);

/// <summary>Le repos annonce par IdleReply a dure : figer l'image si rien n'a change depuis (meme generation).</summary>
internal sealed record SettleMessage(int Generation);

/// <summary>Qu'y a-t-il sous ce point ? Click : clic relache (double-clic dans le vide = recentrer).</summary>
internal sealed record HitMessage(Point Point, bool Click, int ClickCount);

internal sealed record ReleaseMessage;

// ---------------------------------------------------------------- reponses moteur -> UI (postees sur le thread UI)

internal sealed record IdleReply(int Generation);

internal sealed record HitReply(string? Target, bool Click);

/// <summary>
///     Moteur Mini3D sur le thread de rendu d'Avalonia (visuel de composition) : animations cadencees par le compositeur,
///     dessin SkiaSharp direct (Renderer). Le thread UI n'envoie que des messages.
///     Au repos l'image devient un bitmap et la scene est liberee (elle se reconstruit en une fraction de milliseconde).
/// </summary>
internal sealed class Model3DEngine : CompositionCustomVisualHandler
{
    private const int Yaw = 0, Pitch = 1, FirstInput = 2;
    private const double IntroMs = 1800, RecenterMs = 1200;

    private readonly record struct Tween(double From, double To, TimeSpan Start, double Milliseconds);

    private readonly Action<object> _reply;
    private readonly Renderer _renderer;
    private readonly ModelInfo _info;
    private readonly double[] _values;
    private readonly Tween?[] _tweens;
    private readonly double _defaultYaw, _defaultPitch, _minPitch, _maxPitch;
    private readonly SKPaint _opacityPaint = new();
    private SKImage? _snapshot;
    private (Vector Size, float Device) _snapshotKey;
    private bool _active, _dragging, _settle, _idleSent, _released, _introPending, _lastHover;
    private int _generation;

    public Model3DEngine(Model3D model, Action<object> reply)
    {
        _reply = reply;
        _renderer = new Renderer(model);
        _info = _renderer.Info;
        _values = new double[FirstInput + _info.Inputs.Count];
        _tweens = new Tween?[_values.Length];
        _defaultYaw = _info.Yaw * M.Deg;
        _defaultPitch = _info.Pitch * M.Deg;
        _minPitch = _info.MinPitch * M.Deg;
        _maxPitch = _info.MaxPitch * M.Deg;
        _values[Yaw] = _defaultYaw;
        _values[Pitch] = _defaultPitch;
        for (var i = 0; i < _info.Inputs.Count; i++) _values[FirstInput + i] = _info.Inputs[i].Initial;
    }

    // ---------------------------------------------------------------- messages

    public override void OnMessage(object message)
    {
        if (_released) return;
        switch (message)
        {
            case InitMessage m:
                Array.Clear(_tweens);
                _values[Yaw] = m.Intro && _info.Intro is { } intro ? intro * M.Deg : _defaultYaw;
                _values[Pitch] = _defaultPitch;
                Array.Copy(m.Inputs, 0, _values, FirstInput, Math.Min(m.Inputs.Length, _info.Inputs.Count));
                _renderer.Wire = m.Wire;
                _renderer.Dark = m.Dark;
                _lastHover = false;
                // Vue cachee a l'attachement (fondu de page) : l'intro attend la premiere activation
                _introPending = m.Intro && _info.Intro != null;
                if (_introPending && _active) StartIntro();
                Changed();
                break;
            case AnimateMessage m:
                Animate(FirstInput + m.Input, m.To, m.Animate ? _info.Inputs[m.Input].Milliseconds : 0);
                Changed();
                break;
            case RotateMessage m:
                _tweens[Yaw] = null;
                _tweens[Pitch] = null;
                _values[Yaw] += m.Yaw;
                _values[Pitch] = Math.Clamp(_values[Pitch] + m.Pitch, _minPitch, _maxPitch);
                Changed();
                break;
            case DragMessage m:
                _dragging = m.Dragging;
                _lastHover = false;
                _idleSent = false;
                NotifyIdle();
                break;
            case FlagsMessage m when m.Wire != _renderer.Wire || m.Dark != _renderer.Dark:
                _renderer.Wire = m.Wire;
                _renderer.Dark = m.Dark;
                Changed();
                break;
            case ActiveMessage m when m.Active != _active:
                _active = m.Active;
                if (m.Active)
                {
                    if (_introPending) StartIntro();
                    Changed();
                }
                else
                {
                    // Cache : les animations sautent a leur fin, plus rien n'est garde en memoire
                    for (var i = 0; i < _tweens.Length; i++)
                        if (_tweens[i] is { } tween) _values[i] = tween.To;
                    Array.Clear(_tweens);
                    _dragging = false;
                    _lastHover = false;
                    DropSnapshot();
                    _renderer.DropScene();
                }
                break;
            case SettleMessage m when m.Generation == _generation && _snapshot == null && !_dragging && !HasTweens():
                _settle = true;
                Invalidate();
                break;
            case HitMessage m:
                var target = _renderer.HitTest(m.Point.X, m.Point.Y);
                if (m.Click)
                {
                    if (target == null && m.ClickCount == 2)
                    {
                        Animate(Yaw, _defaultYaw, RecenterMs);
                        Animate(Pitch, _defaultPitch, RecenterMs);
                        Changed();
                    }
                    Post(new HitReply(target, true));
                }
                else
                {
                    var over = target != null;
                    if (over != _lastHover)
                    {
                        _lastHover = over;
                        Post(new HitReply(target, false));
                    }
                }
                break;
            case ReleaseMessage:
                _released = true;
                DropSnapshot();
                _opacityPaint.Dispose();
                _renderer.Dispose();
                break;
        }
    }

    private void StartIntro()
    {
        _introPending = false;
        Animate(Yaw, _defaultYaw, IntroMs);
    }

    /// <summary>L'etat a change : l'image figee n'est plus bonne, un repos devra etre de nouveau annonce.</summary>
    private void Changed()
    {
        _generation++;
        _idleSent = false;
        _settle = false;
        DropSnapshot();
        Invalidate();
    }

    private void Post(object reply) => Dispatcher.UIThread.Post(() => _reply(reply));

    /// <summary>Plus d'animation ni de glisser : on le dit a l'UI, qui demandera de figer l'image apres un delai.</summary>
    private void NotifyIdle()
    {
        if (_idleSent || !_active || _dragging || _snapshot != null || HasTweens()) return;
        _idleSent = true;
        Post(new IdleReply(_generation));
    }

    // ---------------------------------------------------------------- animation, cadencee par le compositeur

    private void Animate(int index, double to, double milliseconds)
    {
        if (!_active || milliseconds <= 0)
        {
            _values[index] = to;
            _tweens[index] = null;
            return;
        }
        if (_tweens[index] is { } pending && pending.To == to) return;
        if (Math.Abs(_values[index] - to) < 1e-6 && _tweens[index] == null) return;
        _tweens[index] = new Tween(_values[index], to, CompositionNow, milliseconds);
        RegisterForNextAnimationFrameUpdate();
    }

    private bool HasTweens()
    {
        foreach (var tween in _tweens)
            if (tween != null) return true;
        return false;
    }

    public override void OnAnimationFrameUpdate()
    {
        if (_released || !_active) return;
        var now = CompositionNow;
        var changed = false;
        for (var i = 0; i < _tweens.Length; i++)
        {
            if (_tweens[i] is not { } t) continue;
            var x = Math.Min(1, (now - t.Start).TotalMilliseconds / t.Milliseconds);
            var eased = x < .5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
            var value = x >= 1 ? t.To : t.From + (t.To - t.From) * eased;
            changed |= _values[i] != value;
            _values[i] = value;
            if (x >= 1) _tweens[i] = null;
        }
        if (changed) Invalidate();
        if (HasTweens()) RegisterForNextAnimationFrameUpdate();
    }

    // ---------------------------------------------------------------- dessin

    public override void OnRender(ImmediateDrawingContext context)
    {
        var size = EffectiveSize;
        if (_released || size.X <= 0 || size.Y <= 0) return;
        if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not ISkiaSharpApiLeaseFeature feature) return;
        using var lease = feature.Lease();
        var canvas = lease.SkCanvas;
        var m = canvas.TotalMatrix;
        var device = (float)Math.Sqrt(m.ScaleX * m.ScaleX + m.SkewY * m.SkewY);
        _renderer.Layout(size.X, size.Y);

        if (_snapshot != null && _snapshotKey != (size, device))
        {
            // Taille ou echelle d'ecran changee : on redessine, puis on figera de nouveau
            DropSnapshot();
            _generation++;
            _idleSent = false;
        }
        if (_snapshot == null && _settle)
        {
            _settle = false;
            TakeSnapshot(size, device);
        }
        SKPaint? opacityPaint = null;
        if (lease.CurrentOpacity < 1)
        {
            _opacityPaint.Color = new SKColor(255, 255, 255, (byte)Math.Round(lease.CurrentOpacity * 255));
            opacityPaint = _opacityPaint;
        }
        if (_snapshot != null)
            canvas.DrawImage(_snapshot, new SKRect(0, 0, _snapshot.Width / device, _snapshot.Height / device),
                new SKSamplingOptions(SKFilterMode.Nearest), opacityPaint);
        else
        {
            var restore = canvas.Save();
            if (opacityPaint != null) canvas.SaveLayer(opacityPaint);
            canvas.Translate(_renderer.Offset.X, _renderer.Offset.Y);
            canvas.Scale((float)_renderer.Scale);
            DrawFrame(canvas);
            canvas.RestoreToCount(restore);
        }
        NotifyIdle();
    }

    private void DrawFrame(SKCanvas canvas)
    {
        _renderer.Yaw = _values[Yaw];
        _renderer.Pitch = _values[Pitch];
        var channels = _renderer.Channels.Values;
        for (var i = 0; i < _info.Inputs.Count; i++) channels[_info.Inputs[i].Index] = _values[FirstInput + i];
        _renderer.DrawFrame(canvas);
    }

    /// <summary>Rend l'image une derniere fois dans un bitmap a la resolution de l'ecran, puis libere la scene.</summary>
    private void TakeSnapshot(Vector size, float device)
    {
        var info = new SKImageInfo((int)Math.Ceiling(size.X * device), (int)Math.Ceiling(size.Y * device),
            SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        if (surface == null) return;
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(device);
        canvas.Translate(_renderer.Offset.X, _renderer.Offset.Y);
        canvas.Scale((float)_renderer.Scale);
        DrawFrame(canvas);
        _snapshot = surface.Snapshot();
        _snapshotKey = (size, device);
        _renderer.DropScene();
    }

    private void DropSnapshot()
    {
        _snapshot?.Dispose();
        _snapshot = null;
    }
}

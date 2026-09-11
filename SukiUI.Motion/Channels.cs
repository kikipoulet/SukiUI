using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// The single entry point and access vocabulary of the motion layer: everything
    /// animable hangs off a <see cref="Surface"/> obtained from <see cref="For"/> — the
    /// element itself, one of its template parts (<see cref="Surface.Part"/>), or its
    /// template popup (<see cref="Surface.Popup"/>, a surface root plus the IsOpen
    /// lifecycle). Surfaces differ only by how their target resolves, never by the
    /// channels they expose.
    /// </summary>
    internal static class Motion
    {
        private static readonly ConditionalWeakTable<Visual, Surface> Surfaces = new();

        /// <summary>The single entry point: one surface per element (a single arbitration
        /// state per animated property).</summary>
        internal static Surface For(Visual visual) =>
            Surfaces.GetValue(visual, v => new Surface(v, () => v));
    }

    /// <summary>
    /// One animatable target: a resolved visual and its channels. Everything animable is
    /// a surface — the element, a template part, a popup root — differing only by the
    /// resolver; the channel vocabulary is identical everywhere. Channels are lazily
    /// created, one instance per property per surface, all writing through the resolver.
    /// The semantic factories carry the write protocols that cannot be derived from a
    /// property reference: the transform channels (scale, translate, rotate) compose
    /// through one shared block per target (see <see cref="Transforms"/>); blur manages
    /// the BlurEffect lifecycle.
    /// </summary>
    internal sealed class Surface
    {
        private static readonly ConditionalWeakTable<TemplatedControl, Dictionary<string, Surface>> Parts = new();

        private readonly Visual _owner; // ticker fallback owner; only used by the Offer path
        private readonly Func<Visual?> _target;

        private Channel? _scale, _scaleX, _scaleY, _blur, _translateX, _translateY, _rotate;
        private Dictionary<StyledProperty<double>, Channel>? _properties;

        internal Surface(Visual owner, Func<Visual?> target)
        {
            _owner = owner;
            _target = target;
        }

        /// <summary>Uniform render scale (transform block, X+Y, render-only — no layout).
        /// The first write attaches the block and schedules the frame the animation rides
        /// on.</summary>
        internal Channel Scale => _scale ??= Channel.ForScale(_owner, _target);

        /// <summary>Horizontal render scale — through the shared transform block.</summary>
        internal Channel ScaleX => _scaleX ??= Channel.ForScaleX(_owner, _target);

        /// <summary>Vertical render scale — through the shared transform block.</summary>
        internal Channel ScaleY => _scaleY ??= Channel.ForScaleY(_owner, _target);

        /// <summary>Horizontal translation in parent coordinates — through the shared
        /// transform block (composes with scale and rotate instead of fighting over
        /// RenderTransform).</summary>
        internal Channel TranslateX => _translateX ??= Channel.ForTranslateX(_owner, _target);

        /// <summary>Vertical translation in parent coordinates — through the shared
        /// transform block.</summary>
        internal Channel TranslateY => _translateY ??= Channel.ForTranslateY(_owner, _target);

        /// <summary>Rotation in degrees around the target's RenderTransformOrigin —
        /// through the shared transform block.</summary>
        internal Channel Rotate => _rotate ??= Channel.ForRotate(_owner, _target);

        /// <summary>Target opacity — the generic property channel over <see cref="Visual.OpacityProperty"/>.</summary>
        internal Channel Opacity => Property(Visual.OpacityProperty);

        /// <summary>One live BlurEffect while blurring, dropped entirely below the 0.5
        /// threshold — no shader pass is paid once the blur has dissipated.</summary>
        internal Channel Blur => _blur ??= Channel.ForBlur(_owner, _target);

        /// <summary>Any double styled property — the generic channel: reading and writing
        /// the property IS the whole semantics. The same property always yields the same
        /// channel (one arbitration state per animated property).</summary>
        internal Channel Property(StyledProperty<double> property)
        {
            _properties ??= new Dictionary<StyledProperty<double>, Channel>();
            if (!_properties.TryGetValue(property, out var channel))
                _properties[property] = channel = Channel.ForProperty(_owner, property, _target);
            return channel;
        }

        // ---- descending from the element --------------------------------------------------

        /// <summary>A named template part of this element as an animatable surface — one
        /// surface per (host, name). The target re-resolves at every TemplateApplied (the
        /// proven popup-root rule); while the part does not resolve (template not applied
        /// yet, or the name does not exist) the channels no-op silently — the popup
        /// precedent — with a discrete trace.</summary>
        internal Surface Part(string name)
        {
            if (_owner is not TemplatedControl host)
                throw new InvalidOperationException(
                    $"Motion: template parts require a TemplatedControl host, got {_owner.GetType().Name}.");

            var parts = Parts.GetValue(host, _ => new Dictionary<string, Surface>());
            if (parts.TryGetValue(name, out var existing))
                return existing;

            Visual? target = null;
            var surface = new Surface(host, () => target);
            parts[name] = surface;
            Resolve();
            // Never unwired on purpose: the surface is held through the Parts table keyed
            // on the host — both live and die with it.
            host.TemplateApplied += (_, _) => Resolve();

            void Resolve()
            {
                target = host.GetTemplateDescendants()
                    .OfType<Control>()
                    .FirstOrDefault(c => c.Name == name);
                if (target is null)
                    Debug.WriteLine($"Motion.Part: '{name}' not found in the template of {host.GetType().Name} — channels no-op until it resolves.");
            }
            return surface;
        }

        /// <summary>The template popup of this element — a popup root surface plus the
        /// IsOpen lifecycle (see <see cref="PopupHandle"/>). Not cached: one handle per
        /// Attach, disposed with its Mover — a second call would observe a disposed handle.</summary>
        internal PopupHandle Popup(
            string popupPart,
            string rootPart,
            string itemsPart,
            Func<bool> isHostOpen)
        {
            if (_owner is not TemplatedControl host)
                throw new InvalidOperationException(
                    $"Motion: popups require a TemplatedControl host, got {_owner.GetType().Name}.");
            return new PopupHandle(host, popupPart, rootPart, itemsPart, isHostOpen);
        }
    }

    /// <summary>
    /// One strongly typed, writable visual property of one target, carrying its own physics
    /// (<see cref="Value"/>, <see cref="Velocity"/>) and the channel arbitration — each rule
    /// a generalized branch of the proven press/popup engines:
    /// interrupting captures the on-screen pose (velocity only survives inside a spring);
    /// a MustFinish() chain owns the channel to completion (a spring offered meanwhile is
    /// memorized, a re-offered chain re-arms from the pose, hover is dropped); a spring in
    /// flight is preempted ONLY by a press chain — an incoming hover never preempts it, it
    /// re-resolves its lazy target (retarget without snap, pose and velocity kept); a plain
    /// timed trajectory is preemptable; a pose write wins over everything; settle is
    /// |Δtarget| &lt; 0.0005 and |v| &lt; 0.02 with an exact snap; an idle channel costs zero
    /// frame callbacks.
    /// Choreographies (popup) do not go through <see cref="Offer"/>: they use <see cref="Run"/>
    /// (forced preemption, spring velocity carry) and <see cref="PrePoseIfIdle"/> (the From
    /// rule), and own their ticker subscription themselves.
    /// </summary>
    internal sealed class Channel
    {
        // A trajectory begun with From but never given a To fails loudly at Start.
        private static readonly Func<double> MissingTo =
            static () => throw new InvalidOperationException("A From trajectory was never given a To.");

        private readonly Visual _owner; // ticker fallback owner; only used by the Offer path
        private readonly Func<double> _read;
        private readonly Action<double> _write;
        private Program? _active;
        private IDisposable? _subscription;

        // Defensive pose-clamp window (the old engines clamped starts to
        // [DeepFloor, HoverScale]): grows as trajectory targets and Froms resolve; every
        // pose the behaviors can produce already fits it, so the clamp is a no-op unless
        // the transform was written by something else.
        private double _seenMin = double.PositiveInfinity;
        private double _seenMax = double.NegativeInfinity;

        private Channel(Visual owner, Func<double> read, Action<double> write)
        {
            _owner = owner;
            _read = read;
            _write = write;
        }

        // ---- channel factories: one write protocol each, all over a resolved target ------

        internal static Channel ForScale(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadScaleX(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteScale(t, v);
            });

        internal static Channel ForScaleX(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadScaleX(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteScaleX(t, v);
            });

        internal static Channel ForScaleY(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadScaleY(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteScaleY(t, v);
            });

        internal static Channel ForTranslateX(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadTranslateX(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteTranslateX(t, v);
            });

        internal static Channel ForTranslateY(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadTranslateY(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteTranslateY(t, v);
            });

        internal static Channel ForRotate(Visual owner, Func<Visual?> target) => new(
            owner,
            () => Transforms.ReadRotate(target()),
            v =>
            {
                if (target() is { } t)
                    Transforms.WriteRotate(t, v);
            });

        /// <summary>Any double styled property of one target — the generic channel: reading
        /// and writing the property IS the whole semantics.</summary>
        internal static Channel ForProperty(Visual owner, StyledProperty<double> property, Func<Visual?> target) => new(
            owner,
            () => target()?.GetValue(property) ?? 1.0,
            v =>
            {
                if (target() is { } t)
                    t.SetValue(property, v);
            });

        /// <summary>One live BlurEffect while blurring, dropped entirely below the 0.5
        /// threshold — no shader pass is paid once the blur has dissipated (the proven rule).</summary>
        internal static Channel ForBlur(Visual owner, Func<Visual?> target) => new(
            owner,
            () => target()?.Effect is BlurEffect b ? b.Radius : 0.0,
            v =>
            {
                if (target() is not { } t)
                    return;
                if (v >= 0.5)
                {
                    if (t.Effect is not BlurEffect blur)
                    {
                        blur = new BlurEffect();
                        t.Effect = blur;
                    }
                    blur.Radius = v;
                }
                else if (t.Effect is BlurEffect)
                {
                    t.Effect = null;
                }
            });

        /// <summary>Current on-screen pose of the channel.</summary>
        internal double Value => _read();

        /// <summary>Live velocity of the running spring (0 for timed trajectories).</summary>
        internal double Velocity => _active is SpringTrajectory s ? s.Velocity : 0.0;

        // ---- description surface -----------------------------------------------------

        /// <summary>A timed trajectory toward a constant target.</summary>
        internal TimedTrajectory To(double to)
        {
            Track(to);
            return new TimedTrajectory(this, () => to, lazyTarget: false);
        }

        /// <summary>A timed trajectory whose target resolves at each Start (per-gesture
        /// snapshot semantics) — and whose paired spring is retargetable mid-flight.</summary>
        internal TimedTrajectory To(Func<double> to) => new(this, to, lazyTarget: true);

        /// <summary>
        /// Begins a trajectory with an explicit start pose — the plan's From rule: the pose
        /// is PRE-POSED on this channel when at rest (written before the popup shows, no
        /// flash) and ignored while a program is in flight (a reopen mid-collapse resumes
        /// pose + velocity instead). Complete it with the trajectory's To. Reads as
        /// <c>x.From(0.92).To(1.0).Spring(...)</c>.
        /// </summary>
        internal TimedTrajectory From(double from) => new TimedTrajectory(this, MissingTo, lazyTarget: true).From(from);

        /// <summary>From resolved per gesture (profile-driven during the port).</summary>
        internal TimedTrajectory From(Func<double> from) => new TimedTrajectory(this, MissingTo, lazyTarget: true).From(from);

        /// <summary>An instantaneous pose write (lifecycle: detach/disable).</summary>
        internal PoseProgram Pose(double value)
        {
            Track(value);
            return new PoseProgram(this, value);
        }

        // ---- engine: arbitration -----------------------------------------------------

        /// <summary>
        /// Offers an incoming program to the channel: the single place where the
        /// "who owns the channel" rules apply (the press rules).
        /// </summary>
        internal void Offer(Program incoming)
        {
            if (incoming is PoseProgram)
            {
                // A pose write wins over everything: stop, rest, nothing runs afterwards.
                _active = null;
                Stop();
                incoming.Start();
                return;
            }

            switch (_active)
            {
                case Chain chain:
                    if (incoming is Chain)
                    {
                        StartProgram(incoming); // re-press: purge the parked release, re-arm from the pose
                        return;
                    }

                    if (incoming is SpringTrajectory chainSpring)
                    {
                        chain.Park(chainSpring); // memorized during the guaranteed descent… or immediate beyond it
                        return;
                    }

                    return; // hover never preempts the press chain

                case SpringTrajectory running:
                    if (incoming is Chain)
                    {
                        StartProgram(incoming); // re-click mid-bounce: the press owns the channel again
                        return;
                    }

                    if (incoming is TimedTrajectory && running.CanRetarget)
                    {
                        // Hover mid-bounce: the resting point moves — no snap, pose and
                        // velocity untouched, the incoming hover is consumed by it.
                        running.Retarget();
                        return;
                    }

                    return; // only a press preempts a spring (a release re-offered is a no-op)

                case TimedTrajectory:
                    StartProgram(incoming); // plain timed trajectories are preemptable
                    return;

                default:
                    StartProgram(incoming); // idle: anything starts
                    return;
            }
        }

        /// <summary>
        /// Forced start used by choreographies (popup): unlike <see cref="Offer"/>, the
        /// incoming program ALWAYS wins — the on-screen pose is captured and, for a spring,
        /// the live velocity of the spring it displaces is carried over (the mid-collapse
        /// reopen: pose + velocity kept, spring constants swapped mid-flight — the old
        /// engine's in-place retarget). The channel does not subscribe here: the choreography
        /// owns the single ticker subscription and advances the program itself.
        /// </summary>
        internal void Run(Program incoming)
        {
            if (incoming is SpringTrajectory spring)
                spring.SeedVelocity(Velocity);
            _active = incoming;
            incoming.Start();
        }

        /// <summary>
        /// The plan's From rule: writes the pose ONLY when the channel is at rest (the open
        /// block's Froms, written before the popup becomes visible — no one-frame flash); a
        /// channel in flight keeps its live pose (the reopen mid-collapse resumes pose +
        /// velocity instead).
        /// </summary>
        internal void PrePoseIfIdle(double value)
        {
            if (_active is not null)
                return;
            Track(value);
            Write(value);
        }

        /// <summary>Releases the channel once its choreography member completed (settle):
        /// idle again, a later From pre-poses it. Never called on preemption — the displacing
        /// spring reads the live velocity through the active program first.</summary>
        internal void Release(Program program)
        {
            if (ReferenceEquals(_active, program))
                _active = null;
        }

        /// <summary>Forgets any program on the channel (instant/abnormal close, template
        /// re-apply, detach, disable) — the next From pre-poses it whatever frozen state it
        /// was left in.</summary>
        internal void Rest() => _active = null;

        private void StartProgram(Program program)
        {
            _active = program;
            program.Start();
            EnsureSubscribed();
        }

        /// <summary>
        /// A chain completed and hands the channel over to its memorized release: start it
        /// WITHOUT a synchronous tick — the spring integrates from the next frame, exactly
        /// like the old completion tick starting the release spring.
        /// </summary>
        internal void Handoff(Program program)
        {
            _active = program;
            program.Start();
        }

        /// <summary>Replaces a still-active program (chain releasing the channel to a spring
        /// beyond its guaranteed descent).</summary>
        internal void Replace(Program oldActive, Program incoming)
        {
            if (ReferenceEquals(_active, oldActive))
                StartProgram(incoming);
        }

        private void EnsureSubscribed()
        {
            if (_subscription is not null)
                return;
            _subscription = SukiTicker.Subscribe(_owner, OnFrame);
            // Prime the pump (the proven engine's trick): the synchronous advance produces
            // the first property write that schedules the frame the first callback rides on.
            OnFrame(SukiTicker.Now);
        }

        private void OnFrame(TimeSpan now)
        {
            var active = _active;
            if (active is null)
            {
                Stop();
                return;
            }

            bool more = active.Advance(now);
            if (!more && ReferenceEquals(_active, active))
            {
                // Settled (or played out — the exact snap is already written): an idle
                // channel costs zero callbacks. (A Handoff replaced _active — keep rolling.)
                _active = null;
                Stop();
            }
        }

        private void Stop()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        // ---- engine: plumbing ----------------------------------------------------------

        internal void Write(double value) => _write(value);

        /// <summary>An instantaneous pose write that also feeds the defensive clamp window —
        /// pose writes win, nothing runs afterwards.</summary>
        internal void SetPose(double value)
        {
            Track(value);
            Write(value);
        }

        /// <summary>Clamps a program start pose into the channel window (defensive, mirrors
        /// the old <c>Math.Clamp(pose, DeepFloor, HoverScale)</c> on press/spring starts).</summary>
        internal double ClampPose(double pose) =>
            _seenMin <= _seenMax ? Math.Clamp(pose, _seenMin, _seenMax) : pose;

        /// <summary>Feeds the defensive pose-clamp window with a resolved target/from.</summary>
        internal void Track(double value)
        {
            if (value < _seenMin) _seenMin = value;
            if (value > _seenMax) _seenMax = value;
        }
    }

    /// <summary>
    /// The shared render-transform block of one target: ONE <c>TransformGroup</c> lazily
    /// attached on the first transform-channel write (the proven attach-once rule — that
    /// write is what schedules the frame the animation rides on), its children created
    /// lazily per channel kind, all transform channels (scale, translate, rotate) writing
    /// through the same group so they COMPOSE instead of fighting over RenderTransform.
    /// Keyed on the rendered visual (two surfaces over the same target share the block; a
    /// re-resolved template part gets a fresh one for free). Fixed composition order —
    /// translate · rotate · scale around the target's RenderTransformOrigin (the proven
    /// dialog order when rotate is absent); children are inserted at their canonical rank
    /// whatever their creation order. A pre-existing bare ScaleTransform is adopted on
    /// first attach (pose continuity); any other RenderTransform value is replaced by the
    /// first write.
    /// </summary>
    internal static class Transforms
    {
        private static readonly ConditionalWeakTable<Visual, Block> Blocks = new();

        private const int TranslateRank = 0, RotateRank = 1, ScaleRank = 2;

        private sealed class Block
        {
            public TransformGroup? Group;
            public TranslateTransform? Translate;
            public RotateTransform? Rotate;
            public ScaleTransform? Scale;
        }

        // ---- reads (never attach; identity until the first write) -------------------------

        internal static double ReadScaleX(Visual? t)
        {
            if (t is null)
                return 1.0;
            if (Blocks.TryGetValue(t, out var b) && b.Scale is { } s)
                return s.ScaleX;
            return t.RenderTransform is ScaleTransform bare ? bare.ScaleX : 1.0;
        }

        internal static double ReadScaleY(Visual? t)
        {
            if (t is null)
                return 1.0;
            if (Blocks.TryGetValue(t, out var b) && b.Scale is { } s)
                return s.ScaleY;
            return t.RenderTransform is ScaleTransform bare ? bare.ScaleY : 1.0;
        }

        internal static double ReadTranslateX(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Translate is { } tr ? tr.X : 0.0;

        internal static double ReadTranslateY(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Translate is { } tr ? tr.Y : 0.0;

        internal static double ReadRotate(Visual? t) =>
            t is not null && Blocks.TryGetValue(t, out var b) && b.Rotate is { } r ? r.Angle : 0.0;

        // ---- writes (attach the block once, then mutate children in place) ----------------

        internal static void WriteScale(Visual t, double v)
        {
            var b = EnsureScale(t);
            b.Scale!.ScaleX = v;
            b.Scale.ScaleY = v;
        }

        internal static void WriteScaleX(Visual t, double v) => EnsureScale(t).Scale!.ScaleX = v;

        internal static void WriteScaleY(Visual t, double v) => EnsureScale(t).Scale!.ScaleY = v;

        internal static void WriteTranslateX(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Translate ??= AddChild(b.Group!, new TranslateTransform(), TranslateRank);
            b.Translate.X = v;
        }

        internal static void WriteTranslateY(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Translate ??= AddChild(b.Group!, new TranslateTransform(), TranslateRank);
            b.Translate.Y = v;
        }

        internal static void WriteRotate(Visual t, double v)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            b.Rotate ??= AddChild(b.Group!, new RotateTransform(), RotateRank);
            b.Rotate.Angle = v;
        }

        // ---- the block --------------------------------------------------------------------

        private static Block BlockOf(Visual t) =>
            Blocks.TryGetValue(t, out var b) ? b : Blocks.GetValue(t, _ => new Block());

        private static Block EnsureScale(Visual t)
        {
            var b = BlockOf(t);
            EnsureGroup(t, b);
            if (b.Scale is null)
                b.Scale = AddChild(b.Group!, new ScaleTransform(1, 1), ScaleRank);
            return b;
        }

        private static void EnsureGroup(Visual t, Block b)
        {
            if (b.Group is { } g && ReferenceEquals(t.RenderTransform, g))
                return;
            var group = new TransformGroup();
            b.Group = group;
            // Adopt a pre-existing bare scale on first attach (pose continuity with the old
            // engines' read rule); anything else is replaced by the first write.
            if (b.Scale is null && t.RenderTransform is ScaleTransform existing)
                b.Scale = new ScaleTransform(existing.ScaleX, existing.ScaleY);
            if (b.Scale is { } adopted)
                AddChild(group, adopted, ScaleRank);
            t.RenderTransform = group; // the attach-once write that schedules the frame
        }

        private static T AddChild<T>(TransformGroup group, T child, int rank) where T : Transform
        {
            int index = 0;
            foreach (var existing in group.Children)
            {
                if (Rank(existing) < rank)
                    index++;
                else
                    break;
            }
            group.Children.Insert(index, child);
            return child;
        }

        private static int Rank(Transform t) => t switch
        {
            TranslateTransform => TranslateRank,
            RotateTransform => RotateRank,
            _ => ScaleRank,
        };
    }
}

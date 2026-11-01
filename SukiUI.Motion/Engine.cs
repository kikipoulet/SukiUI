using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace SukiUI.Motion
{
    /// <summary>
    /// One animated property of one visual — the single arbitration point of the engine.
    /// At most one segment drives it (<see cref="Active"/>); at most one run waits for it
    /// (<see cref="Waiting"/>, behind a MustFinish step). The velocity is the live velocity of
    /// the spring driving it, kept per component so the next spring can carry it.
    /// </summary>
    internal sealed class Track
    {
        private static readonly ConditionalWeakTable<Visual, Dictionary<MotionProperty, Track>> All = new();

        public readonly Visual Target;
        public readonly MotionProperty Property;
        public readonly double[] Velocity;
        public Segment? Active;
        public Run? Waiting;

        private Track(Visual target, MotionProperty property)
        {
            Target = target;
            Property = property;
            Velocity = new double[property.Components];
        }

        public static Track Get(Visual target, MotionProperty property)
        {
            var tracks = All.GetValue(target, static _ => new Dictionary<MotionProperty, Track>());
            if (!tracks.TryGetValue(property, out var track))
                tracks[property] = track = new Track(target, property);
            return track;
        }

        public static IReadOnlyCollection<Track> On(Visual target) =>
            All.TryGetValue(target, out var tracks) ? tracks.Values : Array.Empty<Track>();

        public bool IsMoving => Active is { Done: false };
    }

    internal enum RunState
    {
        Running,
        Waiting,
        Completed,
        Interrupted,
    }

    /// <summary>
    /// One play of a reaction. The arbitration rules all live in <see cref="StartStep"/>:
    /// <list type="number">
    /// <item>The latest motion on a property wins: the run that owned it is interrupted as a
    /// whole, and the newcomer starts from the live pose — a spring also carries the live
    /// velocity.</item>
    /// <item>A <c>MustFinish</c> segment keeps its property until it completes: a run from
    /// another reaction waits behind it (only the latest waiting run is kept) and takes over
    /// at completion. The same reaction triggered again restarts from the pose.</item>
    /// <item>A <c>RetargetIfMoving</c> segment does not replace a spring that is moving (or
    /// waiting): it only moves its target, pose and velocity untouched.</item>
    /// </list>
    /// A run whose clock has no TopLevel completes at once (final poses written, actions run)
    /// so nothing waits forever on a frame that will never come.
    /// </summary>
    internal sealed class Run
    {
        private readonly ReactionDef _def;
        private readonly MotionContext _ctx;
        private readonly Visual _clock;
        private Action? _beforeStart;
        private readonly Action? _onCompleted;
        private readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private List<Segment>? _current;
        private List<Segment>? _pending; // created while waiting behind a MustFinish segment
        private List<Track>? _waitingOn;
        private IDisposable? _subscription;
        private int _step;
        private bool _instant;

        private Run(ReactionDef def, MotionContext ctx, Visual clock, Action? beforeStart, Action? onCompleted)
        {
            _def = def;
            _ctx = ctx;
            _clock = clock;
            _beforeStart = beforeStart;
            _onCompleted = onCompleted;
        }

        public RunState State { get; private set; } = RunState.Running;

        public ReactionDef Definition => _def;

        public Task<bool> Completion => _completion.Task;

        public bool IsAlive => State is RunState.Running or RunState.Waiting;

        /// <summary>Plays <paramref name="def"/> with the frames of <paramref name="clock"/>'s top
        /// level. <paramref name="beforeStart"/> runs once, after the first step's From poses and
        /// before its segments start (a popup is revealed there: its start pose is already
        /// written); <paramref name="onCompleted"/> runs when the last step completes — never on
        /// interruption. <paramref name="ctx"/> is what the value lambdas see.</summary>
        public static Run Play(ReactionDef def, MotionContext ctx, Visual clock,
            Action? beforeStart = null, Action? onCompleted = null)
        {
            var run = new Run(def, ctx, clock, beforeStart, onCompleted);
            run.StartStep(SukiTicker.Now, handedOff: null);
            return run;
        }

        // ---- arbitration ----------------------------------------------------------------

        private void StartStep(TimeSpan now, HashSet<Track>? handedOff)
        {
            while (State == RunState.Running)
            {
                _instant |= TopLevel.GetTopLevel(_clock) is null;

                var segments = _pending ?? Create(_def.Steps[_step]);
                _pending = null;

                if (!_instant && WaitBehindMustFinish(segments))
                    return;

                // Rule 3: a moving spring only changes its target.
                foreach (var segment in segments)
                {
                    if (segment.Consumed || !segment.Retargets || segment.Track is not { Active: { } active })
                        continue;
                    if (active.Run != this && active.IsSpring && !active.Done)
                    {
                        active.RetargetTo(segment);
                        segment.Consume();
                    }
                }

                // From poses only a property at rest: in flight, the motion resumes from the
                // live pose (a reopen mid-collapse) — and a property handed over by a finished
                // MustFinish step continues from where it stopped.
                foreach (var segment in segments)
                {
                    if (segment.Consumed || segment.Track is not { } track)
                        continue;
                    if (track.Active is null && handedOff?.Contains(track) != true)
                        segment.PoseFrom();
                }

                // Rule 1: capture the live velocity BEFORE the owner is interrupted.
                var seeds = new double[]?[segments.Count];
                for (int i = 0; i < segments.Count; i++)
                {
                    if (!segments[i].Consumed && segments[i].Track is { Active: { } active } track && active.Run != this)
                        seeds[i] = (double[])track.Velocity.Clone();
                }

                if (_beforeStart is { } beforeStart)
                {
                    _beforeStart = null;
                    beforeStart();
                }

                foreach (var segment in segments)
                {
                    if (segment.Consumed || segment.Track is not { } track)
                        continue;
                    if (track.Active?.Run is { } owner && owner != this)
                        owner.Interrupt();
                    if (track.Waiting is { } waiting && waiting != this)
                        waiting.Interrupt();
                }

                _current = new List<Segment>(segments.Count);
                // Sources first, derived writes last: a Drive reads the poses its sources
                // reached this frame.
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        var segment = segments[i];
                        if (segment.Consumed || segment.IsDrive != (pass == 1))
                            continue;
                        if (segment.Track is { } track)
                            track.Active = segment;
                        segment.Begin(now, seeds[i]);
                        _current.Add(segment);
                    }
                }

                if (_instant && State == RunState.Running)
                    CompleteCurrent();

                // A Do action (or a property-changed handler reacting to a write) may have
                // interrupted this very run.
                if (State != RunState.Running || _current is null)
                    return;

                if (_current.TrueForAll(static s => s.Done))
                {
                    ReleaseCurrent();
                    if (++_step >= _def.Steps.Count)
                    {
                        Finish();
                        return;
                    }
                    handedOff = null;
                    continue;
                }

                EnsureSubscribed();
                return;
            }
        }

        /// <summary>Rule 2: waits when a property is held by a MustFinish segment of another
        /// reaction. Returns true when this run now waits.</summary>
        private bool WaitBehindMustFinish(List<Segment> segments)
        {
            List<Track>? blocked = null;
            foreach (var segment in segments)
            {
                if (segment.Consumed || !segment.CanWait || segment.Track is not { Active: { } active } track)
                    continue;
                if (active.Run == this || !active.MustFinish || active.Done)
                    continue;
                if (ReferenceEquals(active.Run.Definition, _def))
                    continue; // the same reaction triggered again restarts from the pose

                // A redirect reaches the spring waiting behind the MustFinish step instead.
                if (segment.Retargets && track.Waiting is { } waiting && waiting != this && waiting.RetargetPending(track, segment))
                {
                    segment.Consume();
                    continue;
                }

                (blocked ??= new List<Track>()).Add(track);
            }

            if (blocked is null)
                return false;

            _pending = segments;
            foreach (var track in blocked)
            {
                if (track.Waiting is { } previous && previous != this)
                    previous.Interrupt(); // only the latest waiting run is kept
                track.Waiting = this;
            }
            _waitingOn = blocked;
            State = RunState.Waiting;
            _subscription?.Dispose();
            _subscription = null;
            return true;
        }

        private bool RetargetPending(Track track, Segment incoming)
        {
            if (_pending is null)
                return false;
            foreach (var segment in _pending)
            {
                if (segment.Track == track && segment.IsSpring && !segment.Consumed)
                {
                    segment.RetargetTo(incoming);
                    return true;
                }
            }
            return false;
        }

        /// <summary>The MustFinish segment this run waited for completed: start from the pose it reached.</summary>
        private void Resume(TimeSpan now, HashSet<Track> handedOff)
        {
            if (State != RunState.Waiting)
                return;
            ClearWaiting();
            State = RunState.Running;
            StartStep(now, handedOff);
        }

        // ---- frames ---------------------------------------------------------------------

        private void EnsureSubscribed()
        {
            if (_subscription is not null)
                return;
            _subscription = SukiTicker.Subscribe(_clock, OnFrame);
            // A frame only comes when something invalidates: make sure the first one does.
            _clock.InvalidateVisual();
        }

        private void OnFrame(TimeSpan now)
        {
            if (State != RunState.Running || _current is not { } current)
                return;

            List<(Run Waiting, Track Track)>? handoffs = null;
            foreach (var segment in current)
            {
                if (segment.IsDrive || segment.Done)
                    continue;
                segment.Advance(now);
                if (segment.Done && segment.MustFinish && segment.Track is { Waiting: { } waiting } track && waiting != this)
                    (handoffs ??= new()).Add((waiting, track));
            }
            foreach (var segment in current)
            {
                if (segment.IsDrive && !segment.Done)
                    segment.Advance(now);
            }

            // A write may have triggered a handler that took a property over.
            if (State != RunState.Running || _current is null)
                return;

            if (handoffs is not null)
            {
                // The guaranteed step played to the end: whoever waited takes over, and this
                // run — which would otherwise continue — gives way.
                var tracks = new HashSet<Track>();
                foreach (var (_, track) in handoffs)
                    tracks.Add(track);
                Interrupt();
                foreach (var (waiting, _) in handoffs)
                    waiting.Resume(now, tracks);
                return;
            }

            if (!_current.TrueForAll(static s => s.Done))
                return;

            ReleaseCurrent();
            if (++_step >= _def.Steps.Count)
                Finish();
            else
                StartStep(now, handedOff: null);
        }

        // ---- lifecycle ------------------------------------------------------------------

        public void Interrupt()
        {
            if (!IsAlive)
                return;
            State = RunState.Interrupted;
            _subscription?.Dispose();
            _subscription = null;
            if (_current is not null)
            {
                foreach (var segment in _current)
                {
                    segment.OnInterrupted();
                    segment.Release();
                }
                _current = null;
            }
            _pending = null;
            ClearWaiting();
            _completion.TrySetResult(false);
        }

        private void Finish()
        {
            State = RunState.Completed;
            _subscription?.Dispose();
            _subscription = null;
            _onCompleted?.Invoke();
            _completion.TrySetResult(true);
        }

        private void CompleteCurrent()
        {
            foreach (var segment in _current!)
                if (!segment.IsDrive && !segment.Done)
                    segment.Complete();
            foreach (var segment in _current)
                if (segment.IsDrive && !segment.Done)
                    segment.Complete();
        }

        private void ReleaseCurrent()
        {
            if (_current is null)
                return;
            foreach (var segment in _current)
                segment.Release();
            _current = null;
        }

        private void ClearWaiting()
        {
            if (_waitingOn is null)
                return;
            foreach (var track in _waitingOn)
                if (track.Waiting == this)
                    track.Waiting = null;
            _waitingOn = null;
        }

        private List<Segment> Create(StepDef step)
        {
            var segments = new List<Segment>(step.Actions.Count);
            foreach (var action in step.Actions)
                segments.Add(action.Create(this, _ctx, item: null, TimeSpan.Zero));
            return segments;
        }

        /// <summary>Starts a segment created mid-run (a cascade item): same takeover rules as a step.</summary>
        internal void BeginOwned(Segment segment, TimeSpan now)
        {
            double[]? seed = null;
            if (segment.Track is { } track)
            {
                if (track.Active is null)
                    segment.PoseFrom();
                else if (track.Active.Run != this)
                    seed = (double[])track.Velocity.Clone();
                if (track.Active?.Run is { } owner && owner != this)
                    owner.Interrupt();
                track.Active = segment;
            }
            segment.Begin(now, seed);
        }

        internal bool IsInstant => _instant;

        // ---- control from outside -----------------------------------------------------

        /// <summary>Interrupts every run animating (or waiting to animate) a property of <paramref name="target"/>.</summary>
        internal static void StopAll(Visual target)
        {
            var runs = new HashSet<Run>();
            foreach (var track in Track.On(target))
            {
                if (track.Active?.Run is { } owner)
                    runs.Add(owner);
                if (track.Waiting is { } waiting)
                    runs.Add(waiting);
            }
            foreach (var run in runs)
                run.Interrupt();
        }
    }

    // ---- segments: one action of one step, instantiated for one run --------------------

    internal abstract class Segment
    {
        protected Segment(Run run, Track? track)
        {
            Run = run;
            Track = track;
        }

        public readonly Run Run;
        public readonly Track? Track;
        public bool Done;
        public bool Consumed { get; private set; }

        public virtual bool MustFinish => false;
        public virtual bool Retargets => false;
        public virtual bool IsSpring => false;
        public virtual bool IsDrive => false;

        /// <summary>False for a pose: a Set always wins, even over a MustFinish step.</summary>
        public virtual bool CanWait => Track is not null;

        public void Consume()
        {
            Consumed = true;
            Done = true;
        }

        public virtual void PoseFrom() { }

        public abstract void Begin(TimeSpan now, double[]? seedVelocity);

        public virtual void Advance(TimeSpan now) { }

        /// <summary>Jumps to the end state (no frame will run).</summary>
        public abstract void Complete();

        public virtual void RetargetTo(Segment incoming) { }

        public virtual void OnInterrupted() { }

        public virtual void Release()
        {
            if (Track is { } track && track.Active == this)
                track.Active = null;
        }
    }

    /// <summary>A target that did not resolve (a template part that does not exist yet): a no-op.</summary>
    internal sealed class SkippedSegment : Segment
    {
        public SkippedSegment(Run run) : base(run, null) => Done = true;

        public override void Begin(TimeSpan now, double[]? seedVelocity) => Done = true;

        public override void Complete() => Done = true;
    }

    internal sealed class AnimateSegment<T> : Segment
    {
        private static readonly Easing Linear = new LinearEasing();

        private readonly AnimateDef<T> _def;
        private readonly MotionContext _ctx;
        private readonly Visual _target;
        private readonly MotionValue<T> _value;
        private readonly TimeSpan _extraDelay;
        private readonly bool _isSpring;
        private readonly double[] _x;
        private double[] _from;
        private double[] _to;
        private double[]? _overrideTo;
        private Spring _spring;
        private Easing _easing = Linear;
        private TimeSpan _start, _last, _delay, _duration;
        private bool _begun;

        public AnimateSegment(Run run, Track track, AnimateDef<T> def, MotionContext ctx, Visual target, TimeSpan extraDelay)
            : base(run, track)
        {
            _def = def;
            _ctx = ctx;
            _target = target;
            _value = def.Property.Value;
            _extraDelay = extraDelay;
            _isSpring = def.Duration is null;
            int n = _value.Components;
            _x = new double[n];
            _from = new double[n];
            _to = new double[n];
        }

        public override bool MustFinish => _def.MustFinish;
        public override bool Retargets => _def.Retarget;
        public override bool IsSpring => _isSpring;

        private double[] Velocity => Track!.Velocity;

        internal double[] ResolveTo()
        {
            var to = new double[_value.Components];
            _value.Split(_def.To!(_ctx), to);
            return to;
        }

        public override void RetargetTo(Segment incoming)
        {
            if (incoming is not AnimateSegment<T> other)
                return;
            var to = other.ResolveTo();
            if (_begun)
                _to = to;
            else
                _overrideTo = to;
        }

        public override void PoseFrom()
        {
            if (_def.From is not { } from)
                return;
            var pose = new double[_value.Components];
            _value.Split(from(_ctx), pose);
            _value.Split(_def.Property.Read(_target), _x);
            if (!SameAs(pose, _x))
                _def.Property.Write(_target, _value.Join(pose));
        }

        public override void Begin(TimeSpan now, double[]? seedVelocity)
        {
            _begun = true;
            _start = now;
            _delay = (_def.Delay?.Invoke(_ctx) ?? TimeSpan.Zero) + _extraDelay;
            _last = now + _delay;
            _value.Split(_def.Property.Read(_target), _x);
            Array.Copy(_x, _from, _x.Length);
            _to = _overrideTo ?? ResolveTo();

            if (_isSpring)
            {
                _spring = _def.Spring?.Invoke(_ctx) ?? Spring.Smooth;
                if (!_spring.IsValid)
                    throw new InvalidOperationException(
                        $"Animate({_def.Property.Name}): default(Spring) never settles — use new Spring(omega, decay) or a preset.");
                var v = Velocity;
                if (seedVelocity is not null)
                    Array.Copy(seedVelocity, v, v.Length);
                else
                    Array.Clear(v);
                if (_def.AddVelocity is { } add)
                {
                    var kick = new double[v.Length];
                    _value.Split(add(_ctx), kick);
                    for (int i = 0; i < v.Length; i++)
                        v[i] += kick[i];
                }
            }
            else
            {
                _duration = _def.Duration!(_ctx);
                _easing = _def.Easing?.Invoke(_ctx) ?? Linear;
                Array.Clear(Velocity); // a tween reports no velocity (nothing for a spring to carry)
            }

            // Already there and not moving: nothing to animate, nothing to write (an item
            // cascade with no offset never even creates a transform block).
            Done = SameAs(_x, _to) && Array.TrueForAll(Velocity, static v => v == 0.0);
        }

        public override void Advance(TimeSpan now)
        {
            if (now < _start + _delay)
                return;

            if (!_isSpring)
            {
                double t = _duration <= TimeSpan.Zero
                    ? 1.0
                    : Math.Min((now - _start - _delay).TotalMilliseconds / _duration.TotalMilliseconds, 1.0);
                double eased = _easing.Ease(t);
                for (int i = 0; i < _x.Length; i++)
                    _x[i] = Integrator.Lerp(_from[i], _to[i], eased);
                Write();
                if (t >= 1.0)
                    Done = true;
                return;
            }

            double dt = Math.Min((now - _last).TotalSeconds, 0.05);
            _last = now;
            var v = Velocity;
            bool settled = true;
            for (int i = 0; i < _x.Length; i++)
            {
                Integrator.Step(ref _x[i], ref v[i], _to[i], dt, _spring);
                if (Math.Abs(_x[i] - _to[i]) >= Integrator.SettlePosition || Math.Abs(v[i]) >= Integrator.SettleVelocity)
                    settled = false;
            }
            if (settled)
            {
                Array.Copy(_to, _x, _x.Length); // settled: snap exactly onto the resting point
                Array.Clear(v);
                Done = true;
            }
            Write();
        }

        public override void Complete()
        {
            if (!_begun)
                _to = _overrideTo ?? ResolveTo();
            Array.Copy(_to, _x, _x.Length);
            Array.Clear(Velocity);
            Write();
            Done = true;
        }

        private void Write() => _def.Property.Write(_target, _value.Join(_x));

        private static bool SameAs(double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
    }

    internal sealed class SetSegment<T> : Segment
    {
        private readonly SetDef<T> _def;
        private readonly MotionContext _ctx;
        private readonly Visual _target;

        public SetSegment(Run run, Track track, SetDef<T> def, MotionContext ctx, Visual target) : base(run, track)
        {
            _def = def;
            _ctx = ctx;
            _target = target;
        }

        public override bool CanWait => false;

        public override void Begin(TimeSpan now, double[]? seedVelocity) => Complete();

        public override void Complete()
        {
            _def.Property.Write(_target, _def.To!(_ctx));
            Array.Clear(Track!.Velocity);
            Done = true;
        }
    }

    internal sealed class DriveSegment : Segment
    {
        private readonly DriveDef _def;
        private readonly MotionContext _ctx;
        private readonly Visual _target;
        private readonly Track[] _sources;

        public DriveSegment(Run run, Track track, DriveDef def, MotionContext ctx, Visual target, Track[] sources)
            : base(run, track)
        {
            _def = def;
            _target = target;
            _sources = sources;
            _ctx = def.CreateContext(ctx);
            ((IDriveSources)_ctx).Bind(sources);
        }

        public override bool IsDrive => true;

        public override void Begin(TimeSpan now, double[]? seedVelocity)
        {
            Array.Clear(Track!.Velocity);
            Done = false;
        }

        public override void Advance(TimeSpan now)
        {
            _def.Property.Write(_target, _def.With!(_ctx));
            foreach (var source in _sources)
                if (source.IsMoving)
                    return;
            Done = true; // the sources came to rest: the value just written is the final one
        }

        public override void Complete()
        {
            _def.Property.Write(_target, _def.With!(_ctx));
            Done = true;
        }
    }

    internal sealed class CascadeSegment : Segment
    {
        private readonly CascadeDef _def;
        private readonly MotionContext _ctx;
        private readonly List<Segment> _items = new();
        private bool _collected;

        public CascadeSegment(Run run, CascadeDef def, MotionContext ctx) : base(run, null)
        {
            _def = def;
            _ctx = ctx;
        }

        // Items are collected on the first frame, not at start: a popup's content is only
        // realized once the popup has opened.
        public override void Begin(TimeSpan now, double[]? seedVelocity) => Done = false;

        public override void Advance(TimeSpan now)
        {
            if (!_collected)
                Collect(now);
            bool done = true;
            foreach (var item in _items)
            {
                if (item.Done)
                    continue;
                item.Advance(now);
                if (!item.Done)
                    done = false;
            }
            Done = done;
        }

        public override void Complete()
        {
            if (!_collected)
                Collect(SukiTicker.Now);
            foreach (var item in _items)
                if (!item.Done)
                    item.Complete();
            Done = true;
        }

        public override void OnInterrupted()
        {
            if (!_def.CompleteIfInterrupted)
                return;
            foreach (var item in _items)
                if (!item.Done)
                    item.Complete();
        }

        public override void Release()
        {
            foreach (var item in _items)
                item.Release();
        }

        private void Collect(TimeSpan now)
        {
            _collected = true;
            var visuals = new List<Visual>(_def.Items(_ctx));
            int count = visuals.Count;
            if (count == 0 || (_def.FewerThan is { } limit && count >= limit(_ctx)))
                return; // too many items: the cascade would drag on — they show at once

            var initial = _def.InitialDelay(_ctx);
            var stagger = _def.Stagger(_ctx, count);
            for (int i = 0; i < count; i++)
            {
                foreach (var action in _def.Item.Actions)
                {
                    var segment = action.Create(Run, _ctx, visuals[i], initial + stagger * i);
                    _items.Add(segment);
                    Run.BeginOwned(segment, now);
                    if (Run.IsInstant)
                        segment.Complete();
                }
            }
        }
    }

    internal sealed class DoSegment : Segment
    {
        private readonly DoDef _def;
        private readonly MotionContext _ctx;

        public DoSegment(Run run, DoDef def, MotionContext ctx) : base(run, null)
        {
            _def = def;
            _ctx = ctx;
        }

        public override void Begin(TimeSpan now, double[]? seedVelocity) => Complete();

        public override void Complete()
        {
            if (Done)
                return;
            Done = true;
            _def.Action(_ctx);
        }
    }
}

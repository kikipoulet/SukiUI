using System;
using System.Collections.Generic;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    /// <summary>
    /// Anything runnable on a channel. A program object is a REUSABLE DESCRIPTION: its
    /// runtime state is (re)initialized by <see cref="Start"/> every time it wins the
    /// channel, so the same object may run many times (one per gesture). <see cref="Done"/>
    /// is part of that runtime state — set by <see cref="Advance"/> when the program
    /// finishes, readable by the members observing it (derived writes) and by the
    /// choreography that steps it.
    /// </summary>
    internal abstract class Program
    {
        /// <summary>
        /// The channel this program writes, bound at construction — the strong typing of the
        /// layer: channels are compiled properties of a <see cref="MotionContext"/>, so a
        /// program simply cannot point at the wrong property. Channel-less programs (the
        /// item cascade) leave it null.
        /// </summary>
        internal Channel? Channel;

        internal bool Done;

        /// <summary>Capture the start pose (clamped where the behavior requires it), resolve
        /// lazy arguments (per-gesture snapshot semantics), reset runtime state.</summary>
        internal abstract void Start();

        /// <summary>Advance to <paramref name="now"/> and write the pose; false when finished.</summary>
        internal abstract bool Advance(TimeSpan now);

        /// <summary>
        /// Starts the program on its channel at choreography start: forced preemption with
        /// spring velocity carry through <see cref="Channel.Run(Program)"/>; channel-less
        /// programs (the cascade) simply start.
        /// </summary>
        internal void Run()
        {
            if (Channel is { } channel)
                channel.Run(this);
            else
                Start();
        }

        /// <summary>
        /// The plan's From rule: a From PRE-POSES its channel when at rest — written before
        /// the choreography's preamble makes the popup visible (no flash) — and is ignored
        /// while a program is in flight (a reopen mid-collapse resumes pose + velocity).
        /// Called by the choreography before its preamble; only trajectory programs carry
        /// one, the default is a no-op.
        /// </summary>
        internal virtual void PrePose() { }
    }

    /// <summary>
    /// An instantaneous pose write. A pose wins over everything: whatever was running stops,
    /// the pose lands, the channel rests (detach/disable lifecycle).
    /// </summary>
    internal sealed class PoseProgram : Program
    {
        private readonly double _value;

        internal PoseProgram(Channel channel, double value)
        {
            Channel = channel;
            _value = value;
        }

        internal override void Start()
        {
            Done = true;
            Channel!.Write(_value);
        }

        internal override bool Advance(TimeSpan now) => false;
    }

    /// <summary>
    /// A timed, analytically evaluated trajectory: <c>Lerp(from, to, easing(t))</c> per frame.
    /// Usable standalone (hover ramps) or as a <see cref="Chain"/> step. Lazy arguments are
    /// resolved once, at <see cref="Start"/> — the "per gesture" snapshot semantics of the
    /// proven engine: a live profile switch applies to the NEXT gesture, never mid-flight.
    /// Starts from the channel's current pose; an explicit <see cref="From(double)"/> is a
    /// PRE-POSE, not a start override (see <see cref="PrePose"/>).
    /// </summary>
    internal sealed class TimedTrajectory : Program
    {
        private static readonly Easing DefaultEase = new LinearEasing(); // stateless, shared

        private Func<double> _to;
        private bool _lazyTarget; // lazy targets back retargetable springs
        private Func<double?>? _from; // the plan's From rule (pre-pose when idle)
        private Func<TimeSpan> _duration = static () => TimeSpan.FromMilliseconds(150);
        private Func<Easing> _easing = static () => DefaultEase;

        // Runtime (resolved at Start).
        private double _fromValue, _toValue;
        private TimeSpan _durationValue, _start;
        private Easing _easingValue = DefaultEase;

        internal TimedTrajectory(Channel channel, Func<double> to, bool lazyTarget)
        {
            Channel = channel;
            _to = to;
            _lazyTarget = lazyTarget;
        }

        /// <summary>Sets the constant target of a trajectory begun with the channel's From.</summary>
        internal TimedTrajectory To(double to)
        {
            _to = () => to;
            _lazyTarget = false;
            Channel!.Track(to);
            return this;
        }

        /// <summary>Sets a lazy target (resolved at each Start) on a trajectory begun with
        /// the channel's From — and makes the paired spring retargetable mid-flight.</summary>
        internal TimedTrajectory To(Func<double> to)
        {
            _to = to;
            _lazyTarget = true;
            return this;
        }

        /// <summary>Explicit start pose — the plan's From rule: PRE-POSED on an idle channel
        /// by the hosting choreography (before the popup shows), ignored while a program is
        /// in flight. Propagates to the spring derived from this trajectory.</summary>
        internal TimedTrajectory From(double from)
        {
            _from = () => from;
            return this;
        }

        /// <summary>From resolved per gesture (profile-driven during the port).</summary>
        internal TimedTrajectory From(Func<double> from)
        {
            _from = () => from();
            return this;
        }

        internal TimedTrajectory Over(TimeSpan duration)
        {
            _duration = () => duration;
            return this;
        }

        /// <summary>Duration resolved per gesture (profile-driven during the port).</summary>
        internal TimedTrajectory Over(Func<TimeSpan> duration)
        {
            _duration = duration;
            return this;
        }

        internal TimedTrajectory Ease(Easing easing) => Ease(() => easing);

        /// <summary>Easing resolved per gesture (size- or profile-driven during the port) —
        /// the recipe form of <see cref="Ease(Easing)"/>, mirroring
        /// <see cref="Over(TimeSpan)"/>/<see cref="Over(Func{TimeSpan})"/>.</summary>
        internal TimedTrajectory Ease(Func<Easing> easing)
        {
            _easing = easing;
            return this;
        }

        /// <summary>
        /// This step must finish: the descent is played to the end even on the shortest
        /// gesture — the returned chain owns the channel until that step completes (see
        /// <see cref="Chain"/>).
        /// </summary>
        internal Chain MustFinish() => new(this, mustFinishFirst: true);

        /// <summary>Spring-driven trajectory toward this trajectory's (lazy) target —
        /// the release physics. Carries the From rule of this trajectory.</summary>
        internal SpringTrajectory Spring(Spring spring) => new(Channel!, () => spring, _to, _lazyTarget, _from);

        /// <summary>Spring parameters resolved per gesture (profile-driven during the port).</summary>
        internal SpringTrajectory Spring(Func<Spring> spring) => new(Channel!, spring, _to, _lazyTarget, _from);

        internal override void PrePose()
        {
            if (_from?.Invoke() is { } value && Channel is { } channel)
                channel.PrePoseIfIdle(value);
        }

        internal override void Start()
        {
            _fromValue = Channel!.Value; // a From already pre-posed the idle channel; in flight, the pose is live
            _toValue = _to();
            Channel.Track(_toValue);
            _durationValue = _duration();
            _easingValue = _easing();
            _start = SukiTicker.Now;
            Done = false;
        }

        internal override bool Advance(TimeSpan now)
        {
            double t = Progress(now);
            Channel!.Write(Integrator.Lerp(_fromValue, _toValue, _easingValue.Ease(t)));
            if (t < 1.0)
                return true;
            Done = true;
            return false;
        }

        private double Progress(TimeSpan now) =>
            _durationValue <= TimeSpan.Zero ? 1.0
            : Math.Min((now - _start).TotalMilliseconds / _durationValue.TotalMilliseconds, 1.0);

        // ---- chain-step configuration ----------------------------------------------

        internal Func<double> ToFactory => _to;
        internal Func<TimeSpan> DurationFactory => _duration;
        // Chain steps bypass Start: the factory resolves on access. Wrapping a fixed
        // instance (the press) makes this a constant — no allocation, no drift.
        internal Easing EasingValue => _easing();
    }

    /// <summary>
    /// A real damped-spring integration toward a (possibly lazy) target. Starts from the
    /// channel pose — clamped to the channel window — from rest (timed phases carry no
    /// velocity, exactly like the proven release spring) unless a choreography seeded the
    /// live velocity of the spring it displaced (<see cref="SeedVelocity"/> — the popup's
    /// mid-collapse reopen). Settles at 0.0005/0.02 with an exact snap onto the resting
    /// point. A lazy target can be re-resolved mid-flight without touching pose or velocity:
    /// the mid-bounce retarget where "the target moves without a snap".
    /// </summary>
    internal sealed class SpringTrajectory : Program
    {
        private readonly Func<Spring> _spring;
        private readonly Func<double> _target;
        private readonly bool _retargetable;
        private readonly Func<double?>? _from; // the plan's From rule (pre-pose when idle)
        private Spring _springValue;
        private double _x, _v, _targetValue;
        private double? _seedV; // an armed kick (dialog shake) or a carried velocity (popup reopen)
        private TimeSpan _last;

        internal SpringTrajectory(
            Channel channel,
            Func<Spring> spring,
            Func<double> target,
            bool retargetable,
            Func<double?>? from = null)
        {
            Channel = channel;
            _spring = spring;
            _target = target;
            _retargetable = retargetable;
            _from = from;
        }

        /// <summary>Live velocity of the spring (0 before Start / after settle).</summary>
        internal double Velocity => _v;

        internal bool CanRetarget => _retargetable;

        /// <summary>Arms an initial velocity — a scripted kick (the dialog shake impulse)
        /// or the carried velocity of a displaced spring (the popup reopen). Consumed by
        /// the next Start; null = released from rest.</summary>
        internal void SeedVelocity(double v) => _seedV = v;

        /// <summary>True when an explicit kick is armed — the ambient-velocity carry of
        /// <see cref="Channel.Run(Program)"/> must not overwrite it.</summary>
        internal bool HasKick => _seedV.HasValue;

        internal override void PrePose()
        {
            if (_from?.Invoke() is { } value && Channel is { } channel)
                channel.PrePoseIfIdle(value);
        }

        internal override void Start()
        {
            _springValue = _spring();
            _x = Channel!.ClampPose(Channel.Value);
            _v = _seedV ?? 0.0;
            _seedV = null;
            _targetValue = _target();
            Channel.Track(_targetValue);
            _last = SukiTicker.Now;
            Done = false;
        }

        internal override bool Advance(TimeSpan now)
        {
            double dt = Math.Min((now - _last).TotalSeconds, 0.05);
            _last = now;
            Integrator.Step(ref _x, ref _v, _targetValue, dt, _springValue);
            Channel!.Write(_x);

            if (Math.Abs(_x - _targetValue) < Integrator.SettlePosition &&
                Math.Abs(_v) < Integrator.SettleVelocity)
            {
                _x = _targetValue; // settled: snap exactly onto the resting point
                Channel.Write(_x);
                Done = true;
                return false;
            }

            return true;
        }

        /// <summary>The resting point moves without a snap — pose and velocity are kept.</summary>
        internal void Retarget()
        {
            if (!_retargetable)
                return;
            _targetValue = _target();
            Channel!.Track(_targetValue);
        }
    }

    /// <summary>
    /// An ordered sequence of timed steps — the press chain. <see cref="TimedTrajectory.MustFinish"/>
    /// semantics, each rule a generalized branch of the proven engine:
    /// the FIRST step plays to completion even on the shortest gesture — a spring offered
    /// meanwhile is MEMORIZED and fires at that completion (from the depth actually reached);
    /// a new chain (re-press) purges the memorized release and re-arms from the current pose;
    /// hover programs are ignored while the chain runs. Beyond the first step (the deep
    /// stretch) an offered spring interrupts immediately. The last step finishing leaves the
    /// channel resting wherever it is (hold at the bottom).
    /// </summary>
    internal sealed class Chain : Program
    {
        private readonly List<TimedTrajectory> _steps;
        private readonly bool _mustFinishFirst;
        private SpringTrajectory? _parked;
        private double _from, _to;
        private TimeSpan _duration, _stepStart;
        private int _index;

        internal Chain(TimedTrajectory first, bool mustFinishFirst)
        {
            Channel = first.Channel;
            _steps = new List<TimedTrajectory> { first };
            _mustFinishFirst = mustFinishFirst;
        }

        /// <summary>Appends the next step; it only begins if the intention still holds at the
        /// previous step's completion (no memorized release is waiting).</summary>
        internal Chain Then(TimedTrajectory next)
        {
            _steps.Add(next);
            return this;
        }

        internal override void Start()
        {
            _parked = null; // a re-armed chain purges the memorized release
            _index = 0;
            Done = false;
            BeginStep(SukiTicker.Now, from: Channel!.ClampPose(Channel.Value));
        }

        internal override bool Advance(TimeSpan now)
        {
            double t = _duration <= TimeSpan.Zero
                ? 1.0
                : Math.Min((now - _stepStart).TotalMilliseconds / _duration.TotalMilliseconds, 1.0);
            Channel!.Write(Integrator.Lerp(_from, _to, _steps[_index].EasingValue.Ease(t)));
            if (t < 1.0)
                return true;

            if (_index == 0 && _mustFinishFirst && _parked is { } release)
            {
                // The descent played to the end: the release memorized meanwhile fires now,
                // from the pose just reached — the old completion tick.
                Done = true;
                Channel.Handoff(release);
                return false;
            }

            if (_index + 1 < _steps.Count)
            {
                _index++;
                BeginStep(now, from: _to); // the deep stretch continues from the depth reached
                return true;
            }

            Done = true; // hold at the bottom
            return false;
        }

        /// <summary>Called by the channel when a spring is offered while this chain runs.</summary>
        internal void Park(SpringTrajectory release)
        {
            if (_mustFinishFirst && _index == 0)
                _parked = release; // memorized: the guaranteed descent keeps the channel
            else
                Channel!.Replace(this, release); // deep stretch: the release takes over now
        }

        private void BeginStep(TimeSpan now, double from)
        {
            var step = _steps[_index];
            _from = from;
            _to = step.ToFactory();
            Channel!.Track(_to);
            _duration = step.DurationFactory();
            _stepStart = now;
        }
    }
}

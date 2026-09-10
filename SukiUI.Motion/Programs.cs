using System;
using System.Collections.Generic;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    /// <summary>
    /// Anything runnable on a channel. A program object is a REUSABLE DESCRIPTION: its
    /// runtime state is (re)initialized by <see cref="Start"/> every time it wins the
    /// channel, so the same object may run many times (one per gesture).
    /// </summary>
    internal abstract class Program
    {
        /// <summary>
        /// The channel this program writes, bound at construction — the strong typing of the
        /// layer: channels are compiled properties of a <see cref="MotionContext"/>, so a
        /// program simply cannot point at the wrong property.
        /// </summary>
        internal Channel Channel = null!;

        /// <summary>Capture the start pose (clamped where the behavior requires it), resolve
        /// lazy arguments (per-gesture snapshot semantics), reset runtime state.</summary>
        internal abstract void Start();

        /// <summary>Advance to <paramref name="now"/> and write the pose; false when finished.</summary>
        internal abstract bool Advance(TimeSpan now);
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

        internal override void Start() => Channel.Write(_value);

        internal override bool Advance(TimeSpan now) => false;
    }

    /// <summary>
    /// A timed, analytically evaluated trajectory: <c>Lerp(from, to, easing(t))</c> per frame.
    /// Usable standalone (hover ramps) or as a <see cref="Chain"/> step. Lazy arguments are
    /// resolved once, at <see cref="Start"/> — the "per gesture" snapshot semantics of the
    /// proven engine: a live profile switch applies to the NEXT gesture, never mid-flight.
    /// </summary>
    internal sealed class TimedTrajectory : Program
    {
        private readonly Func<double> _to;
        private readonly bool _lazyTarget; // lazy targets back retargetable springs
        private Func<TimeSpan> _duration = static () => TimeSpan.FromMilliseconds(150);
        private Easing _easing = new LinearEasing();
        private double? _fromConst;

        // Runtime (resolved at Start).
        private double _fromValue, _toValue;
        private TimeSpan _durationValue, _start;

        internal TimedTrajectory(Channel channel, Func<double> to, bool lazyTarget)
        {
            Channel = channel;
            _to = to;
            _lazyTarget = lazyTarget;
        }

        /// <summary>Explicit start pose (pre-poses the channel when it is at rest; ignored
        /// when a program is in flight — the popup reopen rule of the plan).</summary>
        internal TimedTrajectory From(double from)
        {
            _fromConst = from;
            Channel.Track(from);
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

        internal TimedTrajectory Ease(Easing easing)
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
        /// the release physics.</summary>
        internal SpringTrajectory Spring(Spring spring)
        {
            Spring captured = spring;
            return new SpringTrajectory(Channel, () => captured, _to, _lazyTarget);
        }

        /// <summary>Spring parameters resolved per gesture (profile-driven during the port).</summary>
        internal SpringTrajectory Spring(Func<Spring> spring) => new(Channel, spring, _to, _lazyTarget);

        internal override void Start()
        {
            _fromValue = _fromConst ?? Channel.Value;
            _toValue = _to();
            Channel.Track(_toValue);
            _durationValue = _duration();
            _start = SukiTicker.Now;
        }

        internal override bool Advance(TimeSpan now)
        {
            double t = Progress(now);
            Channel.Write(Integrator.Lerp(_fromValue, _toValue, _easing.Ease(t)));
            return t < 1.0;
        }

        private double Progress(TimeSpan now) =>
            _durationValue <= TimeSpan.Zero ? 1.0
            : Math.Min((now - _start).TotalMilliseconds / _durationValue.TotalMilliseconds, 1.0);

        // ---- chain-step configuration ----------------------------------------------

        internal Func<double> ToFactory => _to;
        internal Func<TimeSpan> DurationFactory => _duration;
        internal Easing EasingValue => _easing;
        internal double? FromConst => _fromConst;
    }

    /// <summary>
    /// A real damped-spring integration toward a (possibly lazy) target. Starts from the
    /// channel pose — clamped to the channel window, from rest: timed phases carry no
    /// velocity, exactly like the proven release spring (<c>_springV = 0</c>). Settles at
    /// 0.0005/0.02 with an exact snap onto the resting point. A lazy target can be
    /// re-resolved mid-flight without touching pose or velocity: the mid-bounce retarget
    /// where "the target moves without a snap".
    /// </summary>
    internal sealed class SpringTrajectory : Program
    {
        private readonly Func<Spring> _spring;
        private readonly Func<double> _target;
        private readonly bool _retargetable;
        private Spring _springValue;
        private double _x, _v, _targetValue;
        private TimeSpan _last;

        internal SpringTrajectory(Channel channel, Func<Spring> spring, Func<double> target, bool retargetable)
        {
            Channel = channel;
            _spring = spring;
            _target = target;
            _retargetable = retargetable;
        }

        /// <summary>Live velocity of the spring (0 before Start / after settle).</summary>
        internal double Velocity => _v;

        internal bool CanRetarget => _retargetable;

        internal override void Start()
        {
            _springValue = _spring();
            _x = Channel.ClampPose(Channel.Value);
            _v = 0.0; // released from rest
            _targetValue = _target();
            Channel.Track(_targetValue);
            _last = SukiTicker.Now;
        }

        internal override bool Advance(TimeSpan now)
        {
            double dt = Math.Min((now - _last).TotalSeconds, 0.05);
            _last = now;
            Integrator.Step(ref _x, ref _v, _targetValue, dt, _springValue);
            Channel.Write(_x);

            if (Math.Abs(_x - _targetValue) < Integrator.SettlePosition &&
                Math.Abs(_v) < Integrator.SettleVelocity)
            {
                _x = _targetValue; // settled: snap exactly onto the resting point
                Channel.Write(_x);
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
            Channel.Track(_targetValue);
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
            BeginStep(SukiTicker.Now, from: _steps[0].FromConst ?? Channel.ClampPose(Channel.Value));
        }

        internal override bool Advance(TimeSpan now)
        {
            double t = _duration <= TimeSpan.Zero
                ? 1.0
                : Math.Min((now - _stepStart).TotalMilliseconds / _duration.TotalMilliseconds, 1.0);
            Channel.Write(Integrator.Lerp(_from, _to, _steps[_index].EasingValue.Ease(t)));
            if (t < 1.0)
                return true;

            if (_index == 0 && _mustFinishFirst && _parked is { } release)
            {
                // The descent played to the end: the release memorized meanwhile fires now,
                // from the pose just reached — the old completion tick.
                Channel.Handoff(release);
                return false;
            }

            if (_index + 1 < _steps.Count)
            {
                _index++;
                BeginStep(now, from: _to); // the deep stretch continues from the depth reached
                return true;
            }

            return false; // hold at the bottom
        }

        /// <summary>Called by the channel when a spring is offered while this chain runs.</summary>
        internal void Park(SpringTrajectory release)
        {
            if (_mustFinishFirst && _index == 0)
                _parked = release; // memorized: the guaranteed descent keeps the channel
            else
                Channel.Replace(this, release); // deep stretch: the release takes over now
        }

        private void BeginStep(TimeSpan now, double from)
        {
            var step = _steps[_index];
            _from = from;
            _to = step.ToFactory();
            Channel.Track(_to);
            _duration = step.DurationFactory();
            _stepStart = now;
        }
    }
}

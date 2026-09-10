using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// Entry point of the motion layer: one <see cref="MotionContext"/> per visual, holding
    /// its strongly typed channels. Channels are compiled properties of the context — a
    /// wrong channel is a compile error; the only strings left in the layer are PART_ names.
    /// </summary>
    internal static class Motion
    {
        private static readonly ConditionalWeakTable<Visual, MotionContext> Contexts = new();

        /// <summary>Gets (or lazily creates) the motion context of one visual.</summary>
        internal static MotionContext For(Visual visual) => Contexts.GetValue(visual, v => new MotionContext(v));
    }

    /// <summary>The motion surface of one visual: its channels.</summary>
    internal sealed class MotionContext
    {
        private readonly Visual _owner;
        private Channel? _scale;

        internal MotionContext(Visual owner) => _owner = owner;

        /// <summary>Uniform render scale (ScaleTransform X+Y, attach-once, render-only — no
        /// layout). Writes reproduce the proven engine exactly, including the first write
        /// that attaches the transform and schedules the frame the animation rides on.</summary>
        internal Channel Scale => _scale ??= Channel.ForScale(_owner);
    }

    /// <summary>
    /// One strongly typed, writable visual property of one target, carrying its own physics
    /// (<see cref="Value"/>, <see cref="Velocity"/>) and the channel arbitration — each rule
    /// a generalized branch of the proven press/popup engines:
    /// interrupting captures the on-screen pose (velocity only survives inside a spring);
    /// a MustFinish() chain owns the channel to completion (a spring offered meanwhile is
    /// memorized, a re-offered chain re-arms from the pose, hover is dropped); a spring in
    /// flight is preempted ONLY by a press chain (a mid-bounce re-click owns the channel
    /// again, from the pose) — an incoming hover never preempts it, it re-resolves its lazy
    /// target (retarget without snap, pose and velocity kept); a plain timed trajectory is
    /// preemptable; a pose write wins over everything; settle is |Δtarget| &lt; 0.0005 and
    /// |v| &lt; 0.02 with an exact snap; an idle channel costs zero frame callbacks.
    /// </summary>
    internal sealed class Channel
    {
        private readonly Visual _owner;
        private readonly Func<double> _read;
        private readonly Action<double> _write;
        private Program? _active;
        private IDisposable? _subscription;

        // Defensive pose-clamp window (the old engines clamped starts to
        // [DeepFloor, HoverScale]): grows as trajectory targets resolve; every pose the
        // behaviors can produce already fits it, so the clamp is a no-op unless the
        // transform was written by something else.
        private double _seenMin = double.PositiveInfinity;
        private double _seenMax = double.NegativeInfinity;

        private Channel(Visual owner, Func<double> read, Action<double> write)
        {
            _owner = owner;
            _read = read;
            _write = write;
        }

        internal static Channel ForScale(Visual owner) => new(
            owner,
            () => owner.RenderTransform is ScaleTransform t ? t.ScaleX : 1.0,
            v =>
            {
                var t = owner.RenderTransform as ScaleTransform;
                if (t is null)
                {
                    // First write attaches the transform — a real property change that
                    // schedules the very frame this channel's first callback will ride on.
                    t = new ScaleTransform(1, 1);
                    owner.RenderTransform = t;
                }

                t.ScaleX = v;
                t.ScaleY = v;
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

        /// <summary>An instantaneous pose write (lifecycle: detach/disable).</summary>
        internal PoseProgram Pose(double value)
        {
            Track(value);
            return new PoseProgram(this, value);
        }

        // ---- engine: arbitration -----------------------------------------------------

        /// <summary>
        /// Offers an incoming program to the channel: the single place where the
        /// "who owns the channel" rules apply.
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
}

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SukiUI.Motion
{
    /// <summary>
    /// A multi-channel choreography — the popup model, the old engine's single Tick() told
    /// declaratively: ONE ticker subscription advances every member per frame, and the
    /// choreography settles only when EVERY member is done (the real Popup.IsOpen=false
    /// flips at settle, never before). The start sequence is the plan's open rule, in order:
    /// members PRE-POSE their Froms (idle channels only), the PREAMBLE runs (the real
    /// Show — the poses are already written, no one-frame flash), then members start on
    /// their channels (forced preemption, spring velocity carry — a reopen mid-collapse
    /// resumes pose + velocity). Member ORDER matters: derived writes must be registered
    /// after the members they observe.
    /// </summary>
    /// <remarks>
    /// No synchronous prime here (popup semantics, the old StartTimer): the Froms' writes —
    /// or the property change that triggered the choreography — already scheduled the frame
    /// the first member advance rides on.
    /// </remarks>
    public sealed class Choreography
    {
        private readonly List<Program> _members = new();
        private readonly Action? _preamble;
        private Action? _settle;
        private IDisposable? _subscription;

        /// <param name="preamble">Runs after the From pre-poses and before the members
        /// start — the real popup Show() (see <see cref="PopupHandle.Show"/>).</param>
        public Choreography(Action? preamble = null) => _preamble = preamble;

        public Choreography And(Program member)
        {
            _members.Add(member);
            return this;
        }

        /// <summary>Runs when the choreography settles (every member done) — the popup's
        /// only legitimate Hide(). Never runs on <see cref="Stop"/>.</summary>
        public Choreography Then(Action onSettle)
        {
            _settle = onSettle;
            return this;
        }

        public bool Running => _subscription is not null;

        public void Start(Visual owner)
        {
            foreach (var member in _members)
                member.PrePose();    // the Froms: pre-posed only on idle channels
            _preamble?.Invoke();     // the real Show(): the poses are already written
            foreach (var member in _members)
                member.Run();        // forced preemption, spring velocity carry, pose capture
            _subscription = SukiTicker.Subscribe(owner, OnFrame);
        }

        /// <summary>External termination (preemption by a new choreography, template
        /// re-apply, detach, disable): members freeze at their current pose; the settle
        /// action does NOT run.</summary>
        public void Stop()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnFrame(TimeSpan now)
        {
            bool all = true;
            foreach (var member in _members)
            {
                if (member.Done)
                    continue;
                member.Advance(now);
                if (!member.Done)
                    all = false;
            }
            if (!all)
                return;

            Stop();
            // Settled: release every channel — idle again, a later From pre-poses it.
            foreach (var member in _members)
                member.Channel?.Release(member);
            _settle?.Invoke();
        }
    }

    /// <summary>Seeds a choreography from its first member — the close spelling of the
    /// plan: <c>x.To(...).Spring(...).And(...).Then(popup.Hide())</c>.</summary>
    public static class ChoreographyExtensions
    {
        public static Choreography And(this Program first, Program next) =>
            new Choreography().And(first).And(next);
    }

    /// <summary>
    /// A per-frame derived write — the popup's motion blur (a function of the live spring
    /// velocities) and dissolve blur (a function of the fading opacity). Its value lambda is
    /// evaluated every frame, AFTER the members it observes have advanced; it is done when
    /// they are done. Register it after its sources.
    /// </summary>
    public sealed class DerivedTrajectory : Program
    {
        private readonly Func<double> _value;
        private readonly Func<bool> _done;

        public DerivedTrajectory(Channel channel, Func<double> value, Func<bool> done)
        {
            Channel = channel;
            _value = value;
            _done = done;
        }

        public override void Start() => Done = false;

        public override bool Advance(TimeSpan now)
        {
            Channel!.Write(_value());
            if (_done())
            {
                Done = true;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// The staggered item cascade of an open popup — N item opacities, one program. Items
    /// are collected on the FIRST advance (the popup content only attaches once IsOpen=true),
    /// each item fading 0 → 1 with a per-index delay while emerging from a blur that lands
    /// (radius 0) at 70% of the appearance duration and rising to its resting pose; the
    /// per-item stagger is a function of the item count, and too many items skip the
    /// cascade entirely (shown immediately).
    /// <see cref="Reset"/> rests the items at their normal pose — the close start and every
    /// non-settle termination of the popup handle call it.
    /// </summary>
    public sealed class CascadeProgram : Program
    {
        // The item's blur reaches 0 at this fraction of its appearance duration — crisp
        // for the fade's tail, the effect slot freed early.
        private const double BlurSettleRatio = 0.7;

        private readonly Func<Control[]> _collect;
        private readonly Func<TimeSpan> _duration;
        private readonly Func<double> _initialDelayMs;
        private readonly Func<int, double> _staggerMs;
        private readonly Func<int> _skipAbove;
        private readonly Func<double> _itemBlur;
        private readonly Func<double> _itemOffsetY;

        private Control[] _items = Array.Empty<Control>();
        private bool _pending;
        private long _start;
        private double _durationMs, _delayMs, _stagger;
        private double _blurMax, _offsetY;

        internal CascadeProgram(
            Func<Control[]> collect,
            Func<TimeSpan> duration,
            Func<double> initialDelayMs,
            Func<int, double> staggerMs,
            Func<int> skipAbove,
            Func<double> itemBlur,
            Func<double> itemOffsetY)
        {
            _collect = collect;
            _duration = duration;
            _initialDelayMs = initialDelayMs;
            _staggerMs = staggerMs;
            _skipAbove = skipAbove;
            _itemBlur = itemBlur;
            _itemOffsetY = itemOffsetY;
        }

        public override void Start()
        {
            // Collected on the first advance: the popup content attaches only once IsOpen=true.
            _pending = true;
            Done = false;
        }

        /// <summary>Items stop cascading and rest at their normal pose (close start,
        /// abnormal close, template re-apply, detach, disable).</summary>
        public void Reset()
        {
            foreach (var item in _items)
            {
                item.Opacity = 1.0;
                if (item.Effect is BlurEffect)
                    item.Effect = null;
                if (item.RenderTransform is TranslateTransform)
                    item.RenderTransform = null;
            }
            _items = Array.Empty<Control>();
            _pending = false;
        }

        public override bool Advance(TimeSpan now)
        {
            if (_pending)
            {
                _pending = false;
                var items = _collect();
                // Too many items: the cascade would drag on — show them all immediately.
                _items = items.Length > 0 && items.Length < _skipAbove() ? items : Array.Empty<Control>();
                if (_items.Length == 0)
                {
                    Done = true;
                    return false;
                }
                foreach (var item in _items)
                    item.Opacity = 0;
                _start = SukiTicker.Timestamp;
                _durationMs = _duration().TotalMilliseconds;
                _delayMs = _initialDelayMs();
                _stagger = _staggerMs(_items.Length);
                _blurMax = _itemBlur();
                _offsetY = _itemOffsetY();
            }

            if (_items.Length == 0)
            {
                Done = true;
                return false;
            }

            // The cascade starts a moment after the popup itself has begun opening.
            double elapsed = SukiTicker.ElapsedMilliseconds(_start) - _delayMs;
            bool anyActive = false;
            for (int i = 0; i < _items.Length; i++)
            {
                double t = Math.Min(Math.Max((elapsed - i * _stagger) / _durationMs, 0.0), 1.0);
                _items[i].Opacity = t;
                BlurItem(_items[i], _blurMax * (1.0 - Math.Min(t / BlurSettleRatio, 1.0)));
                RiseItem(_items[i], _offsetY * (1.0 - t));
                if (t < 1.0)
                    anyActive = true;
            }
            if (anyActive)
                return true;

            _items = Array.Empty<Control>();
            Done = true;
            return false;
        }

        /// <summary>The proven Blur channel rule (see Channels.ForBlur), inlined for the
        /// transient item set: attach-once above 0.5 DIP, mutate in place, dropped below —
        /// no shader pass at rest, and the slot is left null when the item lands.</summary>
        private static void BlurItem(Control item, double radius)
        {
            if (radius >= 0.5)
            {
                if (item.Effect is not BlurEffect blur)
                {
                    blur = new BlurEffect();
                    item.Effect = blur;
                }
                blur.Radius = radius;
            }
            else if (item.Effect is BlurEffect)
                item.Effect = null;
        }

        /// <summary>The rise: a TranslateTransform over the item's RenderTransform slot,
        /// same adopt-or-replace, dropped-below-threshold lifecycle as the blur — the slot
        /// is left null at rest.</summary>
        private static void RiseItem(Control item, double offsetY)
        {
            if (offsetY >= 0.5)
            {
                if (item.RenderTransform is not TranslateTransform translate)
                {
                    translate = new TranslateTransform();
                    item.RenderTransform = translate;
                }
                translate.Y = offsetY;
            }
            else if (item.RenderTransform is TranslateTransform)
                item.RenderTransform = null;
        }
    }
}

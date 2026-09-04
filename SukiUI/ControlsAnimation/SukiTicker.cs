using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// One shared frame loop per <see cref="TopLevel"/> for the whole press/popup/dialog
    /// animation subsystem — the equivalent of the single synchronized clock that drives
    /// XAML Transitions, replacing the previous one-<see cref="DispatcherTimer"/>-per-control
    /// model. All animated controls register a callback here; exactly one callback fires per
    /// rendered frame while at least one subscriber is active, and zero when nothing animates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Timing comes from <see cref="TopLevel.RequestAnimationFrame"/>: browser semantics,
    /// ONE registration = ONE frame, so the end of every dispatch re-registers the next frame
    /// while work remains (see <see cref="TickerState"/>), and subscribers that settle stop
    /// the chain by simply unsubscribing. The time BASE is always <see cref="Stopwatch"/>
    /// (see <see cref="Now"/>): the frame callback only provides pacing, so every consumer
    /// shares one monotonic clock regardless of the driver.
    /// </para>
    /// <para>
    /// Subscribe against the TopLevel of the VISUAL being animated — popup content lives
    /// under its own PopupRoot, which <see cref="TopLevel.GetTopLevel"/> resolves. A frame
    /// only renders when something invalidates: subscribers must produce at least one
    /// property write (even a no-op-looking initial pose) to schedule the frame their first
    /// callback will ride on — the physics classes do this by advancing once synchronously
    /// at subscription time.
    /// </para>
    /// <para>
    /// Everything here runs on the UI thread (the same guarantee a DispatcherTimer gave).
    /// </para>
    /// </remarks>
    public static class SukiTicker
    {
        // Safety switch: set to false to drive every TopLevel with one shared 16ms
        // DispatcherTimer instead of RequestAnimationFrame (still a N -> 1 improvement
        // over the per-control timers this class replaces).
        private const bool UseRequestAnimationFrame = true;

        private static readonly ConditionalWeakTable<TopLevel, TickerState> States = new();

        private static readonly double SecondsPerTick = 1.0 / Stopwatch.Frequency;
        private static readonly long Epoch = Stopwatch.GetTimestamp();

        // Lightweight process-wide instrumentation, updated on the UI thread at each dispatch
        // (two Stopwatch reads + field writes — zero allocation, unmeasurable scheduling cost).
        private static double _totalDispatchMs;
        private static long _dispatchCount;

        /// <summary>
        /// Total milliseconds spent invoking subscribers (all TopLevels) since process start.
        /// Read deltas around a window — e.g. a benchmark run — to get that window's engine cost.
        /// </summary>
        public static double TotalDispatchMs => _totalDispatchMs;

        /// <summary>Number of frame dispatches since process start.</summary>
        public static long DispatchCount => _dispatchCount;

        /// <summary>Average ms per dispatch — subscriber callbacks only, scheduling excluded.</summary>
        public static double AverageDispatchMs => _dispatchCount == 0 ? 0 : _totalDispatchMs / _dispatchCount;

        /// <summary>Monotonic elapsed time since the ticker's epoch — the single time base of the subsystem.</summary>
        public static TimeSpan Now => TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - Epoch) * SecondsPerTick);

        /// <summary>Raw high-resolution timestamp (cheap); compare with the elapsed helpers below.</summary>
        public static long Timestamp => Stopwatch.GetTimestamp();

        /// <summary>Seconds elapsed since a <see cref="Timestamp"/> snapshot.</summary>
        public static double ElapsedSeconds(long then) => (Stopwatch.GetTimestamp() - then) * SecondsPerTick;

        /// <summary>Milliseconds elapsed since a <see cref="Timestamp"/> snapshot.</summary>
        public static double ElapsedMilliseconds(long then) => ElapsedSeconds(then) * 1000.0;

        /// <summary>
        /// Register a per-frame callback driven by the TopLevel hosting <paramref name="visual"/>.
        /// The callback receives <see cref="Now"/> and stays subscribed until the returned token
        /// is disposed (idempotent). Callbacks may subscribe or unsubscribe other callbacks;
        /// additions land on the next frame.
        /// </summary>
        public static IDisposable Subscribe(Visual visual, Action<TimeSpan> onFrame)
        {
            if (onFrame is null)
                throw new ArgumentNullException(nameof(onFrame));
            var topLevel = TopLevel.GetTopLevel(visual)
                ?? throw new InvalidOperationException(
                    "SukiTicker: the visual is not attached to a visual tree yet (no TopLevel).");
            var state = States.GetOrCreateValue(topLevel);
            return state.Add(topLevel, onFrame);
        }

        /// <summary>
        /// Per-TopLevel loop state. At most ONE frame registration (or one running fallback
        /// timer) is in flight at any time; the tail of each dispatch re-arms only if work
        /// remains, so an idle TopLevel pays exactly zero callbacks.
        /// </summary>
        private sealed class TickerState
        {
            private readonly List<Token> _subscribers = new();
            private readonly List<Token> _pending = new();
            private bool _registrationInFlight; // a RAF registration (or timer tick) will deliver Dispatch
            private DispatcherTimer? _fallbackTimer; // fallback driver only

            public Token Add(TopLevel topLevel, Action<TimeSpan> onFrame)
            {
                var token = new Token(this, onFrame);
                _pending.Add(token);
                Arm(topLevel);
                return token;
            }

            public void Remove(Token token) => token.Removed = true; // compacted at dispatch end

            private void Arm(TopLevel topLevel)
            {
                if (_subscribers.Count == 0 && _pending.Count == 0)
                    return;
                if (UseRequestAnimationFrame)
                {
                    // ONE registration at a time: the next frame's dispatch re-arms by itself,
                    // so an Arm between frames (a new subscriber) must not stack a second one.
                    if (_registrationInFlight)
                        return;
                    _registrationInFlight = true;
                    topLevel.RequestAnimationFrame(_ => Dispatch(topLevel));
                }
                else
                {
                    if (_fallbackTimer is null)
                    {
                        _fallbackTimer = new DispatcherTimer(DispatcherPriority.Render)
                        {
                            Interval = TimeSpan.FromMilliseconds(16)
                        };
                        _fallbackTimer.Tick += (_, _) => Dispatch(topLevel);
                    }
                    if (!_fallbackTimer.IsEnabled)
                        _fallbackTimer.Start();
                }
            }

            private void Dispatch(TopLevel topLevel)
            {
                // The registration this dispatch rode on is consumed: clear before anything
                // can re-arm (including re-arms triggered by subscriber callbacks below).
                _registrationInFlight = false;

                // Adoptions registered between frames join this one; additions made from
                // within a callback (phase chaining) wait for the next frame — deterministic.
                if (_pending.Count > 0)
                {
                    foreach (var token in _pending)
                        _subscribers.Add(token);
                    _pending.Clear();
                }

                // Forward iteration over the live list: removals only flag their token and
                // additions only touch _pending, so the loop needs no copy — zero allocation.
                // A throwing subscriber is dropped (like a crashed per-control timer would
                // have been) so one broken behavior can never stall the whole frame loop.
                var now = Now;
                long sw = Stopwatch.GetTimestamp();
                for (int i = 0; i < _subscribers.Count; i++)
                {
                    var token = _subscribers[i];
                    if (token.Removed)
                        continue;
                    try
                    {
                        token.Callback(now);
                    }
                    catch (Exception ex)
                    {
                        token.Removed = true;
                        Debug.WriteLine($"SukiTicker: subscriber dropped after throwing: {ex.Message}");
                    }
                }
                _totalDispatchMs += (Stopwatch.GetTimestamp() - sw) * SecondsPerTick * 1000.0;
                _dispatchCount++;

                // Compact the flagged removals, then keep the loop alive only if work remains.
                int keep = 0;
                for (int i = 0; i < _subscribers.Count; i++)
                {
                    if (!_subscribers[i].Removed)
                        _subscribers[keep++] = _subscribers[i];
                }
                if (keep < _subscribers.Count)
                    _subscribers.RemoveRange(keep, _subscribers.Count - keep);

                if (_subscribers.Count > 0 || _pending.Count > 0)
                    Arm(topLevel);
                else
                    _fallbackTimer?.Stop();
            }

            internal sealed class Token : IDisposable
            {
                public readonly Action<TimeSpan> Callback;
                public bool Removed;
                private readonly TickerState _state;

                public Token(TickerState state, Action<TimeSpan> callback)
                {
                    _state = state;
                    Callback = callback;
                }

                public void Dispose() => Removed = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SukiUI.Motion
{
    /// <summary>
    /// The generic trigger wiring of one behavior description: routes events, property
    /// changes and arbitrary signals of any source to payload actions. The engine knows
    /// NO particular event — every wiring is a declaration:
    /// <see cref="OnEvent{TArgs}(Avalonia.Interactivity.RoutedEvent{TArgs},System.Action,Avalonia.Interactivity.RoutingStrategies,bool,System.Func{AvaloniaObject?})"/>:
    /// any routed event, on the element or on an ambient source (a resolver, lazily
    /// resolved and re-wired across attach/detach cycles — e.g. the element's TopLevel,
    /// which does not exist at style time);
    /// <see cref="OnPropertyChanged"/>: any AvaloniaProperty, with an optional value filter;
    /// <see cref="OnSignal"/>: the escape hatch for plain CLR events (Popup.Opened,
    /// Window.Deactivated…) — a subscribe closure, no reflection, trimmer-safe.
    /// Subscription semantics carried over from the proven engines: AddHandler's default
    /// strategy (Direct | Bubble) matches direct events (pointer entered/exited,
    /// capture-lost) and bubbling ones alike; pressed/released need handledEventsToo
    /// because buttons and combo boxes mark those handled in their class handlers.
    /// Lifecycle stays contractual, not event-like: OnDetachedFromVisualTree poses the
    /// channel; Dispose unwires everything and runs the hooks — the control stays
    /// functional, just unanimated.
    /// </summary>
    public sealed class Mover : IDisposable
    {
        private readonly InputElement _element;
        private readonly List<Trigger> _triggers = new();
        private readonly List<Action> _disposeHooks = new();
        private PoseProgram? _detached;
        private bool _baseWired;

        public Mover(InputElement element) => _element = element;

        // ---- the three primitives -------------------------------------------------------

        /// <summary>Any routed event, on the element or on an optional ambient source.
        /// The default strategy (Direct | Bubble) matches direct and bubbling events
        /// alike; pass <paramref name="handledEventsToo"/> for events the host controls
        /// mark handled in their class handlers (pointer pressed/released on buttons and
        /// combo boxes).</summary>
        public Mover OnEvent<TArgs>(
            RoutedEvent<TArgs> ev,
            Action fire,
            RoutingStrategies strategy = RoutingStrategies.Direct | RoutingStrategies.Bubble,
            bool handledEventsToo = false,
            Func<AvaloniaObject?>? source = null)
            where TArgs : RoutedEventArgs
        {
            var trigger = new Trigger(
                src =>
                {
                    if (src is not Interactive interactive)
                        return null;
                    void Handler(object? sender, TArgs e) => fire();
                    interactive.AddHandler(ev, Handler, strategy, handledEventsToo);
                    return Unwire.On(() => interactive.RemoveHandler(ev, Handler));
                },
                source)
            {
                Event = ev,
                Fire = fire,
            };
            return Add(trigger);
        }

        public Mover OnEvent<TArgs>(
            RoutedEvent<TArgs> ev,
            Program program,
            RoutingStrategies strategy = RoutingStrategies.Direct | RoutingStrategies.Bubble,
            bool handledEventsToo = false,
            Func<AvaloniaObject?>? source = null)
            where TArgs : RoutedEventArgs
            => OnEvent(ev, Fire(program), strategy, handledEventsToo, source);

        /// <summary>Any property change on the element;
        /// <paramref name="when"/> fires only for that value (any type, equality).</summary>
        public Mover OnPropertyChanged(AvaloniaProperty property, Action fire, object? when = null)
        {
            return Add(new Trigger(
                src =>
                {
                    if (src is not AvaloniaObject owner)
                        return null;
                    void Handler(object? sender, AvaloniaPropertyChangedEventArgs e)
                    {
                        if (e.Property != property)
                            return;
                        if (when is not null && !Equals(e.NewValue, when))
                            return;
                        fire();
                    }
                    owner.PropertyChanged += Handler;
                    return Unwire.On(() => owner.PropertyChanged -= Handler);
                },
                Source: null));
        }

        public Mover OnPropertyChanged(AvaloniaProperty property, Program program, object? when = null)
            => OnPropertyChanged(property, Fire(program), when);

        /// <summary>The escape hatch for plain CLR events: the closure receives the
        /// resolved source and the fire action, and owns the whole subscription. Returning
        /// null = not applicable (e.g. the ambient source did not resolve — retried at the
        /// next attach).</summary>
        public Mover OnSignal(
            Func<AvaloniaObject?, Action, IDisposable?> subscribe,
            Action fire,
            Func<AvaloniaObject?>? source = null)
            => Add(new Trigger(src => subscribe(src, fire), source));

        /// <summary>Resolves the current TopLevel of one element — the ambient source for
        /// window-level triggers; null while detached, re-wired on attach.</summary>
        public static Func<AvaloniaObject?> TopLevelOf(InputElement element)
            => () => TopLevel.GetTopLevel(element);

        /// <summary>Fires every trigger registered for <paramref name="ev"/> as if the
        /// event had been raised — the programmatic drive behind
        /// <see cref="SukiMotion{TSelf}.Simulate(AvaloniaObject, Avalonia.Interactivity.RoutedEvent)"/>
        /// (no pointer input can be synthesized in Avalonia). Real events keep working
        /// alongside.</summary>
        internal void Simulate(RoutedEvent ev)
        {
            foreach (var trigger in _triggers)
                if (trigger.Event == ev)
                    trigger.Fire?.Invoke();
        }

        /// <summary>Fires when the hosting window is deactivated (Alt-tab, focus another
        /// app). A no-op on non-Window hosts and while detached.</summary>
        public Mover OnWindowDeactivated(Action fire) => OnSignal(
            (source, act) =>
            {
                if (source is not Window window)
                    return null;
                EventHandler handler = (_, _) => act();
                window.Deactivated += handler;
                return Unwire.On(() => window.Deactivated -= handler);
            },
            fire,
            TopLevelOf(_element));

        // ---- lifecycle (contractual, not event routing) --------------------------------

        public Mover OnDetachedFromVisualTree(PoseProgram pose)
        {
            _detached = pose;
            EnsureBaseWiring();
            return this;
        }

        /// <summary>Runs on <see cref="Dispose"/> — the behavior releases engine-owned
        /// lifetimes (the popup handle) here.</summary>
        public Mover OnDispose(Action hook)
        {
            _disposeHooks.Add(hook);
            return this;
        }

        /// <summary>Unwires everything, runs the dispose hooks and rests the channel at its
        /// detach pose — the control stays functional, just unanimated.</summary>
        public void Dispose()
        {
            foreach (var trigger in _triggers)
                trigger.Subscription?.Dispose();
            _triggers.Clear();
            if (_baseWired)
            {
                _baseWired = false;
                _element.AttachedToVisualTree -= OnAttachedToVisualTree;
                _element.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            }
            foreach (var hook in _disposeHooks)
                hook();
            _detached?.Channel?.Offer(_detached);
        }

        // ---- engine ----------------------------------------------------------------------

        private static Action Fire(Program program) => () => program.Channel?.Offer(program);

        private Mover Add(Trigger trigger)
        {
            _triggers.Add(trigger);
            EnsureBaseWiring();
            // A null Source means "the element itself"; an ambient source resolves now —
            // null while detached stays unsubscribed and is retried on the next attach.
            trigger.Subscription = trigger.Subscribe(trigger.Source is { } source ? source.Invoke() : _element);
            return this;
        }

        private void EnsureBaseWiring()
        {
            if (_baseWired)
                return;
            // Attach/detach machinery is needed for the pose and for ambient sources only.
            if (_detached is null && _triggers.TrueForAll(t => t.Source is null))
                return;
            _baseWired = true;
            _element.AttachedToVisualTree += OnAttachedToVisualTree;
            _element.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // Ambient sources resolve again (the element moved to a new TopLevel).
            foreach (var trigger in _triggers)
            {
                if (trigger.Source is null || trigger.Subscription is not null)
                    continue;
                trigger.Subscription = trigger.Subscribe(trigger.Source.Invoke());
            }
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // The TopLevel is going away with us; a re-attach re-wires.
            foreach (var trigger in _triggers)
            {
                if (trigger.Source is null)
                    continue;
                trigger.Subscription?.Dispose();
                trigger.Subscription = null;
            }
            _detached?.Channel?.Offer(_detached);
        }

        /// <summary>One declared wiring: a subscribe closure over a resolved source.</summary>
        private sealed record Trigger(
            Func<AvaloniaObject?, IDisposable?> Subscribe,
            Func<AvaloniaObject?>? Source)
        {
            public RoutedEvent? Event; // set by OnEvent — the simulated-drive key
            public Action? Fire;       // the payload, re-invokable by Simulate
            public IDisposable? Subscription { get; set; }
        }
    }

    /// <summary>A one-shot, idempotent disposable over an action.</summary>
    internal static class Unwire
    {
        internal static IDisposable On(Action dispose) => new Disposer(dispose);

        private sealed class Disposer : IDisposable
        {
            private Action? _dispose;
            public Disposer(Action dispose) => _dispose = dispose;
            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }
}

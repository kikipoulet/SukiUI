using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Interactivity;

namespace SukiUI.Motion
{
    /// <summary>One "On…" statement of a behavior: its event sources and the reaction they play.</summary>
    internal sealed class TriggerDef
    {
        public readonly List<TriggerSource> Sources = new();
        public ReactionDef? Reaction;
        public Func<Visual, object?, EventArgs?, MotionContext>? CreateContext;

        public TriggerSource Last => Sources[^1];
    }

    internal enum Lifecycle
    {
        None,
        Enabled,
        Disabled,
    }

    internal abstract class TriggerSource
    {
        /// <summary>Optional filter (Where), with the context type of this source's arguments.</summary>
        public Func<MotionContext, bool>? Filter;
        public Func<Visual, object?, EventArgs?, MotionContext>? CreateFilterContext;

        /// <summary>Routed events only: also receive the event when it is already handled.</summary>
        public bool HandledToo;

        public virtual Lifecycle Lifecycle => Lifecycle.None;

        public virtual RoutedEvent? RoutedEvent => null;

        public abstract IDisposable? Subscribe(Interactive control, Action<EventArgs?> fire);

        public abstract string Describe();
    }

    internal sealed class RoutedSource<TArgs> : TriggerSource where TArgs : RoutedEventArgs
    {
        private readonly RoutedEvent<TArgs> _event;

        public RoutedSource(RoutedEvent<TArgs> routedEvent) => _event = routedEvent;

        public override RoutedEvent RoutedEvent => _event;

        public override IDisposable Subscribe(Interactive control, Action<EventArgs?> fire)
        {
            void Handler(object? sender, TArgs e) => fire(e);
            // Direct | Bubble matches direct events (pointer entered/exited) and bubbling ones alike.
            control.AddHandler(_event, Handler, RoutingStrategies.Direct | RoutingStrategies.Bubble, HandledToo);
            return new Unsubscriber(() => control.RemoveHandler(_event, Handler));
        }

        public override string Describe() => $"On({_event.Name})";
    }

    internal sealed class PropertySource : TriggerSource
    {
        private readonly AvaloniaProperty _property;

        public PropertySource(AvaloniaProperty property) => _property = property;

        public override IDisposable Subscribe(Interactive control, Action<EventArgs?> fire)
        {
            void Handler(object? sender, AvaloniaPropertyChangedEventArgs e)
            {
                if (e.Property == _property)
                    fire(e);
            }
            control.PropertyChanged += Handler;
            return new Unsubscriber(() => control.PropertyChanged -= Handler);
        }

        public override string Describe() => $"OnChanged({_property.Name})";
    }

    internal sealed class ClrSource : TriggerSource
    {
        private readonly Func<Interactive, Action<EventArgs?>, IDisposable> _subscribe;
        private readonly string _name;

        public ClrSource(Func<Interactive, Action<EventArgs?>, IDisposable> subscribe, string name)
        {
            _subscribe = subscribe;
            _name = name;
        }

        public override IDisposable Subscribe(Interactive control, Action<EventArgs?> fire) => _subscribe(control, fire);

        public override string Describe() => _name;
    }

    internal sealed class VisualTreeSource : TriggerSource
    {
        private readonly bool _attached;

        public VisualTreeSource(bool attached) => _attached = attached;

        public override IDisposable Subscribe(Interactive control, Action<EventArgs?> fire)
        {
            void Handler(object? sender, VisualTreeAttachmentEventArgs e) => fire(e);
            if (_attached)
            {
                control.AttachedToVisualTree += Handler;
                return new Unsubscriber(() => control.AttachedToVisualTree -= Handler);
            }
            control.DetachedFromVisualTree += Handler;
            return new Unsubscriber(() => control.DetachedFromVisualTree -= Handler);
        }

        public override string Describe() => _attached ? "OnAttachedToVisualTree()" : "OnDetachedFromVisualTree()";
    }

    internal sealed class LifecycleSource : TriggerSource
    {
        private readonly Lifecycle _lifecycle;

        public LifecycleSource(Lifecycle lifecycle) => _lifecycle = lifecycle;

        public override Lifecycle Lifecycle => _lifecycle;

        public override IDisposable? Subscribe(Interactive control, Action<EventArgs?> fire) => null;

        public override string Describe() => _lifecycle == Lifecycle.Enabled ? "OnEnabled()" : "OnDisabled()";
    }

    /// <summary>A one-shot, idempotent disposable over an action.</summary>
    internal sealed class Unsubscriber : IDisposable
    {
        private Action? _dispose;

        public Unsubscriber(Action dispose) => _dispose = dispose;

        public void Dispose()
        {
            _dispose?.Invoke();
            _dispose = null;
        }
    }

    /// <summary>
    /// An event (or several, with <see cref="Or{TOther}"/>) and its reaction. Configure the
    /// event first — <see cref="IncludingHandled"/> and <see cref="Where"/> apply to the event
    /// just before them — then say what happens.
    /// </summary>
    public sealed class TriggerBuilder<TControl, TProfile, TArgs> : StepStarter<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        private readonly TriggerDef _trigger;

        internal TriggerBuilder(TriggerDef trigger, ChainState chain) : base(chain) => _trigger = trigger;

        /// <summary>Also receives the event when a control already handled it. Button and
        /// ComboBox handle PointerPressed/PointerReleased themselves, TextBox handles
        /// TextInput: without this, those events never reach the behavior.</summary>
        public TriggerBuilder<TControl, TProfile, TArgs> IncludingHandled()
        {
            if (_trigger.Last is not { RoutedEvent: not null } last)
                throw new InvalidOperationException("IncludingHandled() applies to a routed event (On(...) or Or(...)).");
            last.HandledToo = true;
            return this;
        }

        /// <summary>The event only fires the reaction when this is true.</summary>
        public TriggerBuilder<TControl, TProfile, TArgs> Where(Func<MotionContext<TControl, TProfile, TArgs>, bool> condition)
        {
            var last = _trigger.Last;
            last.Filter = ctx => condition((MotionContext<TControl, TProfile, TArgs>)ctx);
            last.CreateFilterContext = static (control, profile, args) =>
                new MotionContext<TControl, TProfile, TArgs>(control, profile, args);
            return this;
        }

        /// <summary>Another event playing the same reaction. The arguments become plain
        /// <see cref="EventArgs"/> (the events differ).</summary>
        public TriggerBuilder<TControl, TProfile, EventArgs> Or<TOther>(RoutedEvent<TOther> routedEvent)
            where TOther : RoutedEventArgs
        {
            _trigger.Sources.Add(new RoutedSource<TOther>(routedEvent));
            return new TriggerBuilder<TControl, TProfile, EventArgs>(_trigger, Chain);
        }

        /// <summary>A property change playing the same reaction.</summary>
        public TriggerBuilder<TControl, TProfile, EventArgs> OrChanged(AvaloniaProperty property)
        {
            _trigger.Sources.Add(new PropertySource(property));
            return new TriggerBuilder<TControl, TProfile, EventArgs>(_trigger, Chain);
        }
    }
}

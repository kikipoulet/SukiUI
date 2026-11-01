using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Interactivity;

namespace SukiUI.Motion
{
    /// <summary>Everything a behavior declared in its Configure: triggers and sections.</summary>
    internal sealed class BehaviorDef
    {
        public readonly List<TriggerDef> Triggers = new();
        public readonly List<ISectionDef> Sections = new();

        public void Validate()
        {
            foreach (var trigger in Triggers)
            {
                if (trigger.Reaction is null)
                    throw new InvalidOperationException(
                        $"{trigger.Sources[0].Describe()} has nothing to play: add Animate(...), Set(...), Drive(...) or Do(...).");
                trigger.Reaction.Validate();
            }
            foreach (var section in Sections)
                section.Validate();
        }
    }

    /// <summary>A self-contained part of a behavior with its own lifecycle (the popup section).</summary>
    internal interface ISectionDef
    {
        void Validate();

        /// <summary>Wires the section on one control. <paramref name="createContext"/> builds a
        /// context with the profile of the current gesture.</summary>
        IDisposable? Attach(Interactive control, Func<MotionContext> createContext);
    }

    /// <summary>
    /// Declares what happens on a control: <c>motion.On(event)</c>, <c>motion.OnChanged(property)</c>
    /// and the lifecycle triggers, each followed by what it plays.
    /// </summary>
    public class MotionBuilder<TControl, TProfile> where TControl : Interactive
    {
        internal readonly BehaviorDef Definition;

        internal MotionBuilder(BehaviorDef definition) => Definition = definition;

        private TriggerBuilder<TControl, TProfile, TArgs> NewTrigger<TArgs>(TriggerSource source) where TArgs : EventArgs
        {
            var trigger = new TriggerDef();
            trigger.Sources.Add(source);
            Definition.Triggers.Add(trigger);
            var chain = ChainState.ForTrigger((reaction, createContext) =>
            {
                trigger.Reaction = reaction;
                trigger.CreateContext = createContext;
            });
            return new TriggerBuilder<TControl, TProfile, TArgs>(trigger, chain);
        }

        /// <summary>A routed event of the control — <c>ctx.Args</c> is typed by the event.</summary>
        public TriggerBuilder<TControl, TProfile, TArgs> On<TArgs>(RoutedEvent<TArgs> routedEvent) where TArgs : RoutedEventArgs =>
            NewTrigger<TArgs>(new RoutedSource<TArgs>(routedEvent));

        /// <summary>A property change of the control.</summary>
        public TriggerBuilder<TControl, TProfile, AvaloniaPropertyChangedEventArgs<T>> OnChanged<T>(AvaloniaProperty<T> property) =>
            NewTrigger<AvaloniaPropertyChangedEventArgs<T>>(new PropertySource(property));

        /// <summary>A plain .NET event of the control:
        /// <c>motion.OnEvent&lt;MyArgs&gt;((c, h) =&gt; c.Something += h, (c, h) =&gt; c.Something -= h)</c>.</summary>
        public TriggerBuilder<TControl, TProfile, TArgs> OnEvent<TArgs>(
            Action<TControl, EventHandler<TArgs>> subscribe,
            Action<TControl, EventHandler<TArgs>> unsubscribe)
            where TArgs : EventArgs =>
            NewTrigger<TArgs>(new ClrSource((control, fire) =>
            {
                var typed = (TControl)(object)control;
                EventHandler<TArgs> handler = (_, e) => fire(e);
                subscribe(typed, handler);
                return new Unsubscriber(() => unsubscribe(typed, handler));
            }, $"OnEvent<{typeof(TArgs).Name}>"));

        /// <inheritdoc cref="OnEvent{TArgs}"/>
        public TriggerBuilder<TControl, TProfile, EventArgs> OnEvent(
            Action<TControl, EventHandler> subscribe,
            Action<TControl, EventHandler> unsubscribe) =>
            NewTrigger<EventArgs>(new ClrSource((control, fire) =>
            {
                var typed = (TControl)(object)control;
                EventHandler handler = (_, e) => fire(e);
                subscribe(typed, handler);
                return new Unsubscriber(() => unsubscribe(typed, handler));
            }, "OnEvent"));

        /// <summary>When the behavior is attached to the control (Enable = true).</summary>
        public TriggerBuilder<TControl, TProfile, EventArgs> OnEnabled() =>
            NewTrigger<EventArgs>(new LifecycleSource(Lifecycle.Enabled));

        /// <summary>When the behavior is removed from the control (Enable = false): the control
        /// stays functional, so put it back at rest here.</summary>
        public TriggerBuilder<TControl, TProfile, EventArgs> OnDisabled() =>
            NewTrigger<EventArgs>(new LifecycleSource(Lifecycle.Disabled));

        /// <summary>When the control enters a visual tree.</summary>
        public TriggerBuilder<TControl, TProfile, VisualTreeAttachmentEventArgs> OnAttachedToVisualTree() =>
            NewTrigger<VisualTreeAttachmentEventArgs>(new VisualTreeSource(attached: true));

        /// <summary>When the control leaves its visual tree.</summary>
        public TriggerBuilder<TControl, TProfile, VisualTreeAttachmentEventArgs> OnDetachedFromVisualTree() =>
            NewTrigger<VisualTreeAttachmentEventArgs>(new VisualTreeSource(attached: false));

        /// <summary>A named part of the control's template, looked up each time an animation
        /// starts (a re-applied template is picked up automatically).</summary>
        public MotionTarget TemplatePart(string name) => MotionTarget.TemplatePart(name);
    }

    /// <summary>The builder of a behavior without profile.</summary>
    public sealed class MotionBuilder<TControl> : MotionBuilder<TControl, NoProfile> where TControl : Interactive
    {
        internal MotionBuilder(BehaviorDef definition) : base(definition) { }
    }

    /// <summary>A reusable behavior: what happens on a control, declared once.</summary>
    public abstract class MotionBehavior
    {
        private protected MotionBehavior() { }

        /// <summary>The control type the behavior accepts.</summary>
        internal abstract Type ControlType { get; }

        /// <summary>Wires the behavior on <paramref name="element"/>; null when it is not a
        /// <see cref="ControlType"/>.</summary>
        internal abstract IDisposable? Attach(AvaloniaObject element);

        /// <summary>Builds a definition once and validates it.</summary>
        private protected static BehaviorDef Build(Action<BehaviorDef> configure)
        {
            var definition = new BehaviorDef();
            configure(definition);
            definition.Validate();
            return definition;
        }

        private protected IDisposable? Ignored(AvaloniaObject element)
        {
            Debug.WriteLine($"{GetType().Name}: '{element.GetType().Name}' is not a {ControlType.Name} — ignored.");
            return null;
        }

        /// <summary>
        /// Registers the XAML <c>Enable</c> attached property of a behavior class:
        /// <c>public static readonly AttachedProperty&lt;bool&gt; EnableProperty = RegisterEnable&lt;MyMotion, Button&gt;();</c>
        /// plus the usual <c>GetEnable</c>/<c>SetEnable</c> accessors. Setting it to true attaches
        /// one shared instance of the class to the element (once, even when a style is
        /// re-applied); false removes it. From C#, <see cref="Motion.Attach"/> does the same
        /// with any instance.
        /// </summary>
        protected static AttachedProperty<bool> RegisterEnable<TBehavior, THost>()
            where TBehavior : MotionBehavior, new()
            where THost : AvaloniaObject =>
            EnableRegistration.Register<TBehavior, THost>();
    }

    /// <summary>
    /// A reusable behavior with a profile: <see cref="GetProfile"/> is called each time a
    /// trigger fires, so a live profile switch applies to the next gesture.
    /// </summary>
    public abstract class MotionBehavior<TControl, TProfile> : MotionBehavior where TControl : Interactive
    {
        private BehaviorDef? _definition;

        /// <summary>Declares the behavior. Runs once per behavior instance; the <c>ctx =&gt; …</c>
        /// lambdas run each time an animation starts.</summary>
        protected abstract void Configure(MotionBuilder<TControl, TProfile> motion);

        /// <summary>The profile of one gesture.</summary>
        protected abstract TProfile GetProfile(TControl control);

        internal virtual MotionBuilder<TControl, TProfile> CreateBuilder(BehaviorDef definition) => new(definition);

        internal override Type ControlType => typeof(TControl);

        internal override IDisposable? Attach(AvaloniaObject element)
        {
            if (element is not TControl control)
                return Ignored(element);
            _definition ??= Build(definition => Configure(CreateBuilder(definition)));
            return new BehaviorInstance<TControl, TProfile>(_definition, GetProfile, control);
        }
    }

    /// <summary>A reusable behavior without profile.</summary>
    public abstract class MotionBehavior<TControl> : MotionBehavior<TControl, NoProfile> where TControl : Interactive
    {
        /// <inheritdoc cref="MotionBehavior{TControl, TProfile}.Configure"/>
        protected abstract void Configure(MotionBuilder<TControl> motion);

        protected sealed override void Configure(MotionBuilder<TControl, NoProfile> motion) =>
            Configure((MotionBuilder<TControl>)motion);

        protected sealed override NoProfile GetProfile(TControl control) => NoProfile.Value;

        internal override MotionBuilder<TControl, NoProfile> CreateBuilder(BehaviorDef definition) =>
            new MotionBuilder<TControl>(definition);
    }

    /// <summary>The XAML Enable property of a behavior class.</summary>
    internal static class EnableRegistration
    {
        public static AttachedProperty<bool> Register<TBehavior, THost>()
            where TBehavior : MotionBehavior, new()
            where THost : AvaloniaObject
        {
            // One shared, stateless instance per behavior class: Configure runs once for all
            // the elements it is enabled on. Called from the behavior class's static initializer
            // (through RegisterEnable) — the registration the analyzer cannot see.
            var behavior = new TBehavior();
#pragma warning disable AVP1001
            var enable = AvaloniaProperty.RegisterAttached<TBehavior, THost, bool>("Enable");
            var handle = AvaloniaProperty.RegisterAttached<TBehavior, THost, IDisposable?>("EnableHandle");
#pragma warning restore AVP1001
            enable.Changed.AddClassHandler<AvaloniaObject>((element, e) =>
            {
                if (e.NewValue is true)
                {
                    if (element.GetValue(handle) is not null)
                        return; // already attached (style re-application)
                    element.SetValue(handle, behavior.Attach(element));
                }
                else
                {
                    element.GetValue(handle)?.Dispose();
                    element.SetValue(handle, null);
                }
            });
            return enable;
        }
    }

    /// <summary>The behavior of <see cref="Motion.Configure{TControl}"/>: declared for one control.</summary>
    internal sealed class InlineBehavior<TControl> : MotionBehavior where TControl : Interactive
    {
        private readonly BehaviorDef _definition;

        public InlineBehavior(Action<MotionBuilder<TControl>> configure) =>
            _definition = Build(definition => configure(new MotionBuilder<TControl>(definition)));

        internal override Type ControlType => typeof(TControl);

        internal override IDisposable? Attach(AvaloniaObject element) =>
            element is TControl control
                ? new BehaviorInstance<TControl, NoProfile>(_definition, static _ => NoProfile.Value, control)
                : Ignored(element);
    }

    internal interface IBehaviorInstance
    {
        void Simulate(RoutedEvent routedEvent);
    }

    /// <summary>The behaviors attached to each element (the Simulate entry point).</summary>
    internal static class BehaviorRegistry
    {
        private static readonly ConditionalWeakTable<AvaloniaObject, List<IBehaviorInstance>> Instances = new();

        public static void Add(AvaloniaObject element, IBehaviorInstance instance) =>
            Instances.GetValue(element, static _ => new List<IBehaviorInstance>()).Add(instance);

        public static void Remove(AvaloniaObject element, IBehaviorInstance instance)
        {
            if (Instances.TryGetValue(element, out var list))
                list.Remove(instance);
        }

        public static IBehaviorInstance[] On(AvaloniaObject element) =>
            Instances.TryGetValue(element, out var list) ? list.ToArray() : Array.Empty<IBehaviorInstance>();
    }

    /// <summary>A behavior wired on one control.</summary>
    internal sealed class BehaviorInstance<TControl, TProfile> : IBehaviorInstance, IDisposable where TControl : Interactive
    {
        private readonly BehaviorDef _definition;
        private readonly Func<TControl, TProfile> _profile;
        private readonly TControl _control;
        private readonly List<IDisposable> _subscriptions = new();
        private bool _disposed;

        public BehaviorInstance(BehaviorDef definition, Func<TControl, TProfile> profile, TControl control)
        {
            _definition = definition;
            _profile = profile;
            _control = control;
            BehaviorRegistry.Add(control, this);

            foreach (var trigger in _definition.Triggers)
            {
                foreach (var source in trigger.Sources)
                {
                    var t = trigger;
                    var s = source;
                    if (source.Subscribe(control, args => Fire(t, s, args, simulated: false)) is { } subscription)
                        _subscriptions.Add(subscription);
                }
            }

            foreach (var section in _definition.Sections)
            {
                if (section.Attach(control, () => new MotionContext<TControl, TProfile, EventArgs>(
                        _control, _profile(_control), EventArgs.Empty)) is { } attached)
                    _subscriptions.Add(attached);
            }

            FireLifecycle(Lifecycle.Enabled);
        }

        private void Fire(TriggerDef trigger, TriggerSource source, EventArgs? args, bool simulated)
        {
            if (_disposed || trigger.Reaction is null)
                return;
            object? profile = _profile(_control);
            if (!simulated && source.Filter is { } filter &&
                !filter(source.CreateFilterContext!(_control, profile, args)))
                return;
            var ctx = trigger.CreateContext!(_control, profile, args);
            Run.Play(trigger.Reaction, ctx, clock: _control);
        }

        private void FireLifecycle(Lifecycle lifecycle)
        {
            foreach (var trigger in _definition.Triggers)
                foreach (var source in trigger.Sources)
                    if (source.Lifecycle == lifecycle)
                        Fire(trigger, source, EventArgs.Empty, simulated: false);
        }

        public void Simulate(RoutedEvent routedEvent)
        {
            foreach (var trigger in _definition.Triggers)
                foreach (var source in trigger.Sources)
                    if (source.RoutedEvent == routedEvent)
                        Fire(trigger, source, args: null, simulated: true);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();
            FireLifecycle(Lifecycle.Disabled);
            _disposed = true;
            BehaviorRegistry.Remove(_control, this);
        }
    }

    /// <summary>The entry point of the motion layer.</summary>
    public static class Motion
    {
        /// <summary>Starts a one-off motion on a visual; finish the chain with <c>PlayAsync()</c>.</summary>
        public static StepStarter<TControl, NoProfile, MotionPlayArgs> Of<TControl>(TControl target) where TControl : Visual =>
            new(ChainState.ForImperative(target));

        /// <summary>Attaches an inline behavior to one control. Dispose the result to remove it.</summary>
        public static IDisposable Configure<TControl>(TControl control, Action<MotionBuilder<TControl>> configure)
            where TControl : Interactive =>
            Attach(control, new InlineBehavior<TControl>(configure));

        /// <summary>
        /// Attaches a behavior to a control from code — the C# side of <c>Enable="True"</c>.
        /// Dispose the result to remove it. Each call wires the behavior once more: do not
        /// combine it with Enable for the same behavior on the same control.
        /// </summary>
        /// <exception cref="ArgumentException">The control is not of the type the behavior accepts.</exception>
        public static IDisposable Attach(AvaloniaObject control, MotionBehavior behavior)
        {
            if (!behavior.ControlType.IsInstanceOfType(control))
                throw new ArgumentException(
                    $"{behavior.GetType().Name} animates a {behavior.ControlType.Name}, not a {control.GetType().Name}.",
                    nameof(control));
            return behavior.Attach(control)!;
        }

        /// <summary>Fires the reactions declared for <paramref name="routedEvent"/> on the
        /// behaviors attached to <paramref name="element"/>, as if the event had been raised
        /// (benchmarks, tests). Where filters are skipped and <c>ctx.Args</c> is null.</summary>
        public static void Simulate(AvaloniaObject element, RoutedEvent routedEvent)
        {
            foreach (var instance in BehaviorRegistry.On(element))
                instance.Simulate(routedEvent);
        }

        /// <summary>Stops every motion running on <paramref name="target"/> where it is (their
        /// <c>PlayAsync</c> returns false).</summary>
        public static void Stop(Visual target) => Run.StopAll(target);
    }

    /// <summary>The end of a one-off motion.</summary>
    public static class MotionPlayExtensions
    {
        /// <summary>Plays the motion. True when it went to the end, false when another motion
        /// interrupted it.</summary>
        public static Task<bool> PlayAsync<TControl>(this ChainTail<TControl, NoProfile, MotionPlayArgs> chain)
            where TControl : Visual
        {
            var state = chain.Chain;
            var reaction = state.Reaction!;
            reaction.Validate();
            var target = state.ImperativeTarget
                ?? throw new InvalidOperationException("PlayAsync() is for motions started with Motion.Of(...).");
            var ctx = new MotionContext<TControl, NoProfile, MotionPlayArgs>(target, NoProfile.Value, MotionPlayArgs.Instance);
            return Run.Play(reaction, ctx, reaction.Clock ?? target).Completion;
        }

        /// <summary>Drives the frames from another visual — for a target that is not in the
        /// visual tree yet (an item just added to a list: its host drives the motion).</summary>
        public static StepStarter<TControl, NoProfile, MotionPlayArgs> UsingClockOf<TControl>(
            this StepStarter<TControl, NoProfile, MotionPlayArgs> starter, Visual clock)
            where TControl : Visual
        {
            starter.Chain.Reaction!.Clock = clock;
            return starter;
        }
    }
}

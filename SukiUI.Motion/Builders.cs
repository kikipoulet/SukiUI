using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    /// <summary>Where the actions of a fluent chain are written: the reaction and its current step.</summary>
    internal sealed class ChainState
    {
        private readonly Action<ReactionDef, Func<Visual, object?, EventArgs?, MotionContext>>? _onCreated;

        private ChainState(ReactionDef? reaction, StepDef? step, bool allowThen,
            Action<ReactionDef, Func<Visual, object?, EventArgs?, MotionContext>>? onCreated)
        {
            Reaction = reaction;
            Step = step;
            AllowThen = allowThen;
            _onCreated = onCreated;
        }

        public ReactionDef? Reaction { get; private set; }
        public StepDef? Step { get; private set; }
        public bool AllowThen { get; }
        public Visual? ImperativeTarget { get; private init; }

        /// <summary>A trigger's reaction, created with the first action (its context type is
        /// the trigger's arguments type at that point).</summary>
        public static ChainState ForTrigger(Action<ReactionDef, Func<Visual, object?, EventArgs?, MotionContext>> onCreated) =>
            new(null, null, allowThen: true, onCreated);

        public static ChainState ForReaction(ReactionDef reaction) => new(reaction, reaction.Steps[0], allowThen: true, null);

        public static ChainState ForImperative(Visual target)
        {
            var reaction = new ReactionDef();
            return new ChainState(reaction, reaction.Steps[0], allowThen: true, null) { ImperativeTarget = target };
        }

        public static ChainState ForCascadeItem(StepDef item) => new(null, item, allowThen: false, null);

        public void Add(ActionDef action, Func<Visual, object?, EventArgs?, MotionContext> createContext)
        {
            if (Step is null)
            {
                Reaction = new ReactionDef();
                Step = Reaction.Steps[0];
                _onCreated?.Invoke(Reaction, createContext);
            }
            Step.Actions.Add(action);
        }

        public void Then()
        {
            if (!AllowThen)
                throw new InvalidOperationException("Then() is not available inside a cascade item: an item plays one step.");
            Step = Reaction!.AddStep();
        }
    }

    /// <summary>
    /// The start of a step: pick what happens — <c>Animate</c> a property, <c>Set</c> it at
    /// once, <c>Drive</c> it from other properties, <c>Cascade</c> over items, or <c>Do</c>
    /// something.
    /// </summary>
    public class StepStarter<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        internal readonly ChainState Chain;

        internal StepStarter(ChainState chain) => Chain = chain;

        private static MotionContext NewContext(Visual control, object? profile, EventArgs? args) =>
            new MotionContext<TControl, TProfile, TArgs>(control, profile, args);

        private T Add<T>(ActionDef action, T builder)
        {
            Chain.Add(action, NewContext);
            return builder;
        }

        private static MotionTarget Or(MotionTarget? target) => target ?? (MotionTarget)(Visual?)null;

        // ---- Animate ----------------------------------------------------------------------

        /// <summary>Animates a property of the control (inside a cascade: of the item).</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Animate<T>(AvaloniaProperty<T> property) =>
            Animate(MotionTarget.Default, AvaloniaMotionProperty<T>.For(property));

        /// <inheritdoc cref="Animate{T}(AvaloniaProperty{T})"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Animate<T>(MotionProperty<T> property) =>
            Animate(MotionTarget.Default, property);

        /// <summary>Animates a property of another visual (a template part, a child…).</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Animate<T>(MotionTarget target, AvaloniaProperty<T> property) =>
            Animate(target, AvaloniaMotionProperty<T>.For(property));

        /// <inheritdoc cref="Animate{T}(MotionTarget, AvaloniaProperty{T})"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Animate<T>(MotionTarget target, MotionProperty<T> property)
        {
            var def = new AnimateDef<T>(property) { Target = Or(target) };
            return Add(def, new AnimateBuilder<TControl, TProfile, TArgs, T>(Chain, def));
        }

        // ---- Set --------------------------------------------------------------------------

        /// <summary>Sets a property at once, with no animation. A Set always wins: it stops
        /// whatever animates the property, even a MustFinish step.</summary>
        public SetBuilder<TControl, TProfile, TArgs, T> Set<T>(AvaloniaProperty<T> property) =>
            Set(MotionTarget.Default, AvaloniaMotionProperty<T>.For(property));

        /// <inheritdoc cref="Set{T}(AvaloniaProperty{T})"/>
        public SetBuilder<TControl, TProfile, TArgs, T> Set<T>(MotionProperty<T> property) =>
            Set(MotionTarget.Default, property);

        /// <inheritdoc cref="Set{T}(AvaloniaProperty{T})"/>
        public SetBuilder<TControl, TProfile, TArgs, T> Set<T>(MotionTarget target, AvaloniaProperty<T> property) =>
            Set(target, AvaloniaMotionProperty<T>.For(property));

        /// <inheritdoc cref="Set{T}(AvaloniaProperty{T})"/>
        public SetBuilder<TControl, TProfile, TArgs, T> Set<T>(MotionTarget target, MotionProperty<T> property)
        {
            var def = new SetDef<T>(property) { Target = Or(target) };
            return Add(def, new SetBuilder<TControl, TProfile, TArgs, T>(Chain, def));
        }

        // ---- Drive ------------------------------------------------------------------------

        /// <summary>Writes a property every frame from other properties (their value or their
        /// velocity) for as long as those sources move.</summary>
        public DriveBuilder<TControl, TProfile, TArgs> Drive(AvaloniaProperty<double> property) =>
            Drive(MotionTarget.Default, AvaloniaMotionProperty<double>.For(property));

        /// <inheritdoc cref="Drive(AvaloniaProperty{double})"/>
        public DriveBuilder<TControl, TProfile, TArgs> Drive(MotionProperty<double> property) =>
            Drive(MotionTarget.Default, property);

        /// <inheritdoc cref="Drive(AvaloniaProperty{double})"/>
        public DriveBuilder<TControl, TProfile, TArgs> Drive(MotionTarget target, AvaloniaProperty<double> property) =>
            Drive(target, AvaloniaMotionProperty<double>.For(property));

        /// <inheritdoc cref="Drive(AvaloniaProperty{double})"/>
        public DriveBuilder<TControl, TProfile, TArgs> Drive(MotionTarget target, MotionProperty<double> property)
        {
            var def = new DriveDef(property)
            {
                Target = Or(target),
                CreateContext = static ctx =>
                {
                    var drive = new DriveContext<TControl, TProfile, TArgs>();
                    drive.CopyFrom(ctx);
                    return drive;
                },
            };
            return Add(def, new DriveBuilder<TControl, TProfile, TArgs>(Chain, def));
        }

        // ---- Cascade ----------------------------------------------------------------------

        /// <summary>Plays the same animations on every item, one after the other (staggered).
        /// Inside <paramref name="item"/>, <c>Animate(property)</c> targets the item.</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> Cascade(
            MotionItems items,
            Action<StepStarter<TControl, TProfile, TArgs>> item) =>
            Cascade(items.Resolve, item);

        /// <inheritdoc cref="Cascade(MotionItems, Action{StepStarter{TControl, TProfile, TArgs}})"/>
        public CascadeBuilder<TControl, TProfile, TArgs> Cascade(
            Func<MotionContext<TControl, TProfile, TArgs>, IEnumerable<Visual>> items,
            Action<StepStarter<TControl, TProfile, TArgs>> item) =>
            Cascade(ChainTail<TControl, TProfile, TArgs>.Wrap(items), item);

        private CascadeBuilder<TControl, TProfile, TArgs> Cascade(
            Func<MotionContext, IEnumerable<Visual>> items,
            Action<StepStarter<TControl, TProfile, TArgs>> item)
        {
            var def = new CascadeDef(items);
            item(new StepStarter<TControl, TProfile, TArgs>(ChainState.ForCascadeItem(def.Item)));
            return Add(def, new CascadeBuilder<TControl, TProfile, TArgs>(Chain, def));
        }

        // ---- Do ---------------------------------------------------------------------------

        /// <summary>Runs an action when the step starts.</summary>
        public ChainTail<TControl, TProfile, TArgs> Do(Action<MotionContext<TControl, TProfile, TArgs>> action) =>
            Add(new DoDef(ctx => action((MotionContext<TControl, TProfile, TArgs>)ctx)), new ChainTail<TControl, TProfile, TArgs>(Chain));

        /// <inheritdoc cref="Do(Action{MotionContext{TControl, TProfile, TArgs}})"/>
        public ChainTail<TControl, TProfile, TArgs> Do(Action action) =>
            Add(new DoDef(_ => action()), new ChainTail<TControl, TProfile, TArgs>(Chain));
    }

    /// <summary>The end of an action: continue with <see cref="And"/> (in parallel, same step)
    /// or <see cref="Then"/> (a new step, once every action of this one has finished).</summary>
    public class ChainTail<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        internal readonly ChainState Chain;

        internal ChainTail(ChainState chain) => Chain = chain;

        /// <summary>The next action plays together with this one.</summary>
        public StepStarter<TControl, TProfile, TArgs> And() => new(Chain);

        /// <summary>The next action starts once every action of this step has finished.</summary>
        public StepStarter<TControl, TProfile, TArgs> Then()
        {
            Chain.Then();
            return new StepStarter<TControl, TProfile, TArgs>(Chain);
        }

        internal static Func<MotionContext, TResult> Wrap<TResult>(Func<MotionContext<TControl, TProfile, TArgs>, TResult> f) =>
            ctx => f((MotionContext<TControl, TProfile, TArgs>)ctx);
    }

    /// <summary>One animated property: where it starts, where it goes, and how.</summary>
    public sealed class AnimateBuilder<TControl, TProfile, TArgs, T> : ChainTail<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        private readonly AnimateDef<T> _def;

        internal AnimateBuilder(ChainState chain, AnimateDef<T> def) : base(chain) => _def = def;

        /// <summary>The start pose, written when the animation starts — only if the property
        /// is at rest: in flight, the animation continues from the pose on screen.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> From(T value)
        {
            _def.From = _ => value;
            return this;
        }

        /// <inheritdoc cref="From(T)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> From(Func<MotionContext<TControl, TProfile, TArgs>, T> value)
        {
            _def.From = Wrap(value);
            return this;
        }

        /// <summary>The value to reach.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> To(T value)
        {
            _def.To = _ => value;
            return this;
        }

        /// <summary>The value to reach, computed when the animation starts.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> To(Func<MotionContext<TControl, TProfile, TArgs>, T> value)
        {
            _def.To = Wrap(value);
            return this;
        }

        /// <summary>A timed curve: reaches the target in exactly this duration (linear unless
        /// <see cref="WithEasing(Easing)"/>).</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Over(TimeSpan duration)
        {
            _def.Duration = _ => duration;
            return this;
        }

        /// <inheritdoc cref="Over(TimeSpan)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Over(Func<MotionContext<TControl, TProfile, TArgs>, TimeSpan> duration)
        {
            _def.Duration = Wrap(duration);
            return this;
        }

        /// <summary>The easing of a timed curve (any Avalonia easing).</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> WithEasing(Easing easing)
        {
            _def.Easing = _ => easing;
            return this;
        }

        /// <inheritdoc cref="WithEasing(Easing)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> WithEasing(Func<MotionContext<TControl, TProfile, TArgs>, Easing> easing)
        {
            _def.Easing = Wrap(easing);
            return this;
        }

        /// <summary>A physical curve: a damped spring that settles on the target, carrying the
        /// velocity of whatever it interrupts. Without Over(...) or WithSpring(...), an
        /// animation uses <see cref="Spring.Smooth"/>.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> WithSpring(Spring spring)
        {
            _def.Spring = _ => spring;
            return this;
        }

        /// <inheritdoc cref="WithSpring(Spring)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> WithSpring(Func<MotionContext<TControl, TProfile, TArgs>, Spring> spring)
        {
            _def.Spring = Wrap(spring);
            return this;
        }

        /// <summary>Adds an impulse to the spring's live velocity (units per second): a pulse,
        /// a keystroke kick, a shake.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> AddVelocity(T velocity)
        {
            _def.AddVelocity = _ => velocity;
            return this;
        }

        /// <inheritdoc cref="AddVelocity(T)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> AddVelocity(Func<MotionContext<TControl, TProfile, TArgs>, T> velocity)
        {
            _def.AddVelocity = Wrap(velocity);
            return this;
        }

        /// <summary>Waits before moving (the start pose is already written).</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Delay(TimeSpan delay)
        {
            _def.Delay = _ => delay;
            return this;
        }

        /// <inheritdoc cref="Delay(TimeSpan)"/>
        public AnimateBuilder<TControl, TProfile, TArgs, T> Delay(Func<MotionContext<TControl, TProfile, TArgs>, TimeSpan> delay)
        {
            _def.Delay = Wrap(delay);
            return this;
        }

        /// <summary>This animation always plays to the end. Another reaction arriving
        /// meanwhile waits (only the latest one is kept) and takes over from the pose reached;
        /// the same reaction triggered again restarts from the pose.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> MustFinish()
        {
            _def.MustFinish = true;
            return this;
        }

        /// <summary>If a spring is already moving this property (or waiting to), do not
        /// replace it: only move its target to this animation's value — pose and velocity
        /// are kept, so a bounce is never cut short.</summary>
        public AnimateBuilder<TControl, TProfile, TArgs, T> RetargetIfMoving()
        {
            _def.Retarget = true;
            return this;
        }
    }

    /// <summary>A property set at once.</summary>
    public sealed class SetBuilder<TControl, TProfile, TArgs, T> : ChainTail<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        private readonly SetDef<T> _def;

        internal SetBuilder(ChainState chain, SetDef<T> def) : base(chain) => _def = def;

        public SetBuilder<TControl, TProfile, TArgs, T> To(T value)
        {
            _def.To = _ => value;
            return this;
        }

        public SetBuilder<TControl, TProfile, TArgs, T> To(Func<MotionContext<TControl, TProfile, TArgs>, T> value)
        {
            _def.To = Wrap(value);
            return this;
        }
    }

    /// <summary>A property written every frame from its sources, until they come to rest.</summary>
    public sealed class DriveBuilder<TControl, TProfile, TArgs> : ChainTail<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        private readonly DriveDef _def;

        internal DriveBuilder(ChainState chain, DriveDef def) : base(chain) => _def = def;

        /// <summary>A source property of the control.</summary>
        public DriveBuilder<TControl, TProfile, TArgs> From(AvaloniaProperty<double> source) =>
            From(MotionTarget.Default, AvaloniaMotionProperty<double>.For(source));

        /// <inheritdoc cref="From(AvaloniaProperty{double})"/>
        public DriveBuilder<TControl, TProfile, TArgs> From(MotionProperty<double> source) =>
            From(MotionTarget.Default, source);

        /// <summary>A source property of another visual.</summary>
        public DriveBuilder<TControl, TProfile, TArgs> From(MotionTarget target, AvaloniaProperty<double> source) =>
            From(target, AvaloniaMotionProperty<double>.For(source));

        /// <summary>One or more source properties of another visual.</summary>
        public DriveBuilder<TControl, TProfile, TArgs> From(MotionTarget target, params MotionProperty<double>[] sources)
        {
            foreach (var source in sources)
                _def.Sources.Add((target ?? (MotionTarget)(Visual?)null, source));
            return this;
        }

        /// <summary>The value written every frame, from the sources' live value or velocity.</summary>
        public DriveBuilder<TControl, TProfile, TArgs> With(Func<DriveContext<TControl, TProfile, TArgs>, double> value)
        {
            _def.With = ctx => value((DriveContext<TControl, TProfile, TArgs>)ctx);
            return this;
        }
    }

    /// <summary>The same animations played on every item, staggered.</summary>
    public sealed class CascadeBuilder<TControl, TProfile, TArgs> : ChainTail<TControl, TProfile, TArgs>
        where TControl : Visual
        where TArgs : EventArgs
    {
        private readonly CascadeDef _def;

        internal CascadeBuilder(ChainState chain, CascadeDef def) : base(chain) => _def = def;

        /// <summary>The delay between two items.</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> WithStagger(TimeSpan stagger)
        {
            _def.Stagger = (_, _) => stagger;
            return this;
        }

        /// <inheritdoc cref="WithStagger(TimeSpan)"/>
        public CascadeBuilder<TControl, TProfile, TArgs> WithStagger(Func<MotionContext<TControl, TProfile, TArgs>, TimeSpan> stagger)
        {
            var f = Wrap(stagger);
            _def.Stagger = (ctx, _) => f(ctx);
            return this;
        }

        /// <summary>The delay between two items, as a function of the item count.</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> WithStagger(Func<MotionContext<TControl, TProfile, TArgs>, int, TimeSpan> stagger)
        {
            _def.Stagger = (ctx, count) => stagger((MotionContext<TControl, TProfile, TArgs>)ctx, count);
            return this;
        }

        /// <summary>The delay before the first item starts.</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> WithInitialDelay(TimeSpan delay)
        {
            _def.InitialDelay = _ => delay;
            return this;
        }

        /// <inheritdoc cref="WithInitialDelay(TimeSpan)"/>
        public CascadeBuilder<TControl, TProfile, TArgs> WithInitialDelay(Func<MotionContext<TControl, TProfile, TArgs>, TimeSpan> delay)
        {
            _def.InitialDelay = Wrap(delay);
            return this;
        }

        /// <summary>Cascades only when there are fewer items than this; otherwise the items
        /// are left as they are (a long cascade would drag on).</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> OnlyWhenFewerThan(int count)
        {
            _def.FewerThan = _ => count;
            return this;
        }

        /// <inheritdoc cref="OnlyWhenFewerThan(int)"/>
        public CascadeBuilder<TControl, TProfile, TArgs> OnlyWhenFewerThan(Func<MotionContext<TControl, TProfile, TArgs>, int> count)
        {
            _def.FewerThan = Wrap(count);
            return this;
        }

        /// <summary>If the reaction is interrupted, the items jump to their final pose instead
        /// of freezing half-way.</summary>
        public CascadeBuilder<TControl, TProfile, TArgs> CompleteIfInterrupted()
        {
            _def.CompleteIfInterrupted = true;
            return this;
        }
    }
}

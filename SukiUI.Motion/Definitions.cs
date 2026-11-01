using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;

namespace SukiUI.Motion
{
    // The immutable descriptions the fluent builders produce. A description is built once
    // (per behavior type, or per Motion.Of call) and played many times: every play is a Run
    // that instantiates the description's segments with the context of that gesture.

    /// <summary>A reaction: steps played one after the other (Then), each step's actions together (And).</summary>
    internal sealed class ReactionDef
    {
        public readonly List<StepDef> Steps = new() { new StepDef() };

        /// <summary>Imperative plays only: the visual whose frames drive the run (UsingClockOf).</summary>
        public Visual? Clock;

        public StepDef AddStep()
        {
            var step = new StepDef();
            Steps.Add(step);
            return step;
        }

        public void Validate()
        {
            foreach (var step in Steps)
                step.Validate();
        }
    }

    internal sealed class StepDef
    {
        public readonly List<ActionDef> Actions = new();

        public void Validate()
        {
            foreach (var action in Actions)
                action.Validate();
        }
    }

    internal abstract class ActionDef
    {
        public MotionTarget Target = MotionTarget.Default;

        /// <summary>Creates this action's segment for one run. <paramref name="item"/> is the
        /// current cascade item (null outside a cascade); <paramref name="extraDelay"/> is the
        /// cascade stagger.</summary>
        public abstract Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay);

        public virtual void Validate() { }

        protected Visual? ResolveTarget(MotionContext ctx, Visual? item) => Target.Resolve(ctx, item);
    }

    internal sealed class AnimateDef<T> : ActionDef
    {
        public readonly MotionProperty<T> Property;
        public Func<MotionContext, T>? From;
        public Func<MotionContext, T>? To;
        public Func<MotionContext, T>? AddVelocity;
        public Func<MotionContext, TimeSpan>? Duration;
        public Func<MotionContext, TimeSpan>? Delay;
        public Func<MotionContext, Easing>? Easing;
        public Func<MotionContext, Spring>? Spring;
        public bool MustFinish;
        public bool Retarget;

        public AnimateDef(MotionProperty<T> property) => Property = property;

        public override Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay) =>
            ResolveTarget(ctx, item) is { } target
                ? new AnimateSegment<T>(run, Track.Get(target, Property), this, ctx, target, extraDelay)
                : new SkippedSegment(run);

        public override void Validate()
        {
            if (To is null)
                throw new InvalidOperationException($"Animate({Property.Name}) needs a To(...).");
            if (Duration is not null && Spring is not null)
                throw new InvalidOperationException(
                    $"Animate({Property.Name}) has both Over(...) and WithSpring(...): pick one curve.");
            if (Easing is not null && Duration is null)
                throw new InvalidOperationException(
                    $"Animate({Property.Name}) has WithEasing(...) but no Over(...): an easing needs a duration.");
        }
    }

    internal sealed class SetDef<T> : ActionDef
    {
        public readonly MotionProperty<T> Property;
        public Func<MotionContext, T>? To;

        public SetDef(MotionProperty<T> property) => Property = property;

        public override Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay) =>
            ResolveTarget(ctx, item) is { } target
                ? new SetSegment<T>(run, Track.Get(target, Property), this, ctx, target)
                : new SkippedSegment(run);

        public override void Validate()
        {
            if (To is null)
                throw new InvalidOperationException($"Set({Property.Name}) needs a To(...).");
        }
    }

    internal sealed class DriveDef : ActionDef
    {
        public readonly MotionProperty<double> Property;
        public readonly List<(MotionTarget Target, MotionProperty<double> Property)> Sources = new();
        public Func<MotionContext, double>? With;
        public Func<MotionContext, MotionContext> CreateContext = null!;

        public DriveDef(MotionProperty<double> property) => Property = property;

        public override Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay)
        {
            if (ResolveTarget(ctx, item) is not { } target)
                return new SkippedSegment(run);
            var sources = new List<Track>(Sources.Count);
            foreach (var (sourceTarget, sourceProperty) in Sources)
                if (sourceTarget.Resolve(ctx, item) is { } visual)
                    sources.Add(Track.Get(visual, sourceProperty));
            return new DriveSegment(run, Track.Get(target, Property), this, ctx, target, sources.ToArray());
        }

        public override void Validate()
        {
            if (Sources.Count == 0)
                throw new InvalidOperationException($"Drive({Property.Name}) needs at least one From(...).");
            if (With is null)
                throw new InvalidOperationException($"Drive({Property.Name}) needs a With(...).");
        }
    }

    internal sealed class CascadeDef : ActionDef
    {
        public readonly StepDef Item = new();
        public readonly Func<MotionContext, IEnumerable<Visual>> Items;
        public Func<MotionContext, int, TimeSpan> Stagger = static (_, _) => TimeSpan.FromMilliseconds(30);
        public Func<MotionContext, TimeSpan> InitialDelay = static _ => TimeSpan.Zero;
        public Func<MotionContext, int>? FewerThan;
        public bool CompleteIfInterrupted;

        public CascadeDef(Func<MotionContext, IEnumerable<Visual>> items) => Items = items;

        public override Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay) =>
            new CascadeSegment(run, this, ctx);

        public override void Validate() => Item.Validate();
    }

    internal sealed class DoDef : ActionDef
    {
        public readonly Action<MotionContext> Action;

        public DoDef(Action<MotionContext> action) => Action = action;

        public override Segment Create(Run run, MotionContext ctx, Visual? item, TimeSpan extraDelay) =>
            new DoSegment(run, this, ctx);
    }
}

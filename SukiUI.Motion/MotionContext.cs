using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace SukiUI.Motion
{
    /// <summary>
    /// What a value lambda sees when its animation starts: the control the behavior is
    /// attached to, the profile resolved for this gesture, and the arguments of the event
    /// that fired.
    /// </summary>
    public abstract class MotionContext
    {
        internal Visual ControlVisual = null!;
        internal object? ProfileObject;
        internal EventArgs? ArgsObject;

        internal void CopyFrom(MotionContext other)
        {
            ControlVisual = other.ControlVisual;
            ProfileObject = other.ProfileObject;
            ArgsObject = other.ArgsObject;
        }
    }

    /// <inheritdoc cref="MotionContext"/>
    public class MotionContext<TControl, TProfile, TArgs> : MotionContext
        where TControl : Visual
        where TArgs : EventArgs
    {
        internal MotionContext() { }

        internal MotionContext(Visual control, object? profile, EventArgs? args)
        {
            ControlVisual = control;
            ProfileObject = profile;
            ArgsObject = args;
        }

        /// <summary>The control the motion runs for.</summary>
        public TControl Control => (TControl)ControlVisual;

        /// <summary>The profile resolved when the event fired (a live profile switch applies to the next gesture).</summary>
        public TProfile Profile => (TProfile)ProfileObject!;

        /// <summary>The arguments of the event that fired (null when the trigger is simulated).</summary>
        public TArgs Args => (TArgs)ArgsObject!;
    }

    /// <summary>
    /// The context of a <c>Drive</c>: the motion context plus the live state of the source
    /// properties, re-read every frame.
    /// </summary>
    public sealed class DriveContext<TControl, TProfile, TArgs> : MotionContext<TControl, TProfile, TArgs>, IDriveSources
        where TControl : Visual
        where TArgs : EventArgs
    {
        private Track[] _sources = Array.Empty<Track>();

        void IDriveSources.Bind(Track[] sources) => _sources = sources;

        /// <summary>Live velocity of a source property (units per second; 0 when it is not moving).</summary>
        public double VelocityOf(MotionProperty<double> property)
        {
            var track = Find(property);
            return track.IsMoving ? track.Velocity[0] : 0.0;
        }

        /// <inheritdoc cref="VelocityOf(MotionProperty{double})"/>
        public double VelocityOf(AvaloniaProperty<double> property) => VelocityOf(AvaloniaMotionProperty<double>.For(property));

        /// <summary>Current value of a source property.</summary>
        public double ValueOf(MotionProperty<double> property) => property.Read(Find(property).Target);

        /// <inheritdoc cref="ValueOf(MotionProperty{double})"/>
        public double ValueOf(AvaloniaProperty<double> property) => ValueOf(AvaloniaMotionProperty<double>.For(property));

        private Track Find(MotionProperty property)
        {
            foreach (var track in _sources)
                if (ReferenceEquals(track.Property, property))
                    return track;
            throw new InvalidOperationException(
                $"Drive: '{property.Name}' is not one of the sources declared with From(...).");
        }
    }

    internal interface IDriveSources
    {
        void Bind(Track[] sources);
    }

    /// <summary>The marker profile of behaviors that do not use one.</summary>
    public sealed class NoProfile
    {
        public static readonly NoProfile Value = new();

        private NoProfile() { }
    }

    /// <summary>The arguments of a motion played directly with <see cref="Motion.Of{TControl}"/>.</summary>
    public sealed class MotionPlayArgs : EventArgs
    {
        public static readonly MotionPlayArgs Instance = new();

        private MotionPlayArgs() { }
    }

    /// <summary>
    /// Which visual an animation writes to, resolved when the animation starts. By default
    /// it is the control itself; any <see cref="Visual"/> converts implicitly (a null visual
    /// makes the animation a no-op), and behaviors get template parts through
    /// <c>motion.TemplatePart(name)</c>.
    /// </summary>
    public sealed class MotionTarget
    {
        private readonly Func<MotionContext, Visual?, Visual?> _resolve;

        private MotionTarget(Func<MotionContext, Visual?, Visual?> resolve) => _resolve = resolve;

        /// <summary>The control itself — or, inside a cascade, the current item.</summary>
        internal static readonly MotionTarget Default = new((ctx, item) => item ?? ctx.ControlVisual);

        internal Visual? Resolve(MotionContext ctx, Visual? item) => _resolve(ctx, item);

        internal static MotionTarget FromContext(Func<MotionContext, Visual?, Visual?> resolve) => new(resolve);

        public static implicit operator MotionTarget(Visual? visual) => new((_, _) => visual);

        /// <summary>A visual found from the control when the animation starts.</summary>
        public static MotionTarget Find(Func<Visual, Visual?> resolve) => new((ctx, _) => resolve(ctx.ControlVisual));

        /// <summary>A named part of the control's template, looked up when the animation starts
        /// (a re-applied template is picked up by the next animation).</summary>
        internal static MotionTarget TemplatePart(string name) => new((ctx, _) => FindTemplatePart(ctx.ControlVisual, name));

        internal static Control? FindTemplatePart(Visual host, string name)
        {
            if (host is not TemplatedControl templated)
                return null;
            foreach (var child in templated.GetTemplateDescendants())
                if (child is Control control && control.Name == name)
                    return control;
            return null;
        }
    }

    /// <summary>A set of visuals resolved when a cascade starts (popup items, list children…).</summary>
    public sealed class MotionItems
    {
        private readonly Func<MotionContext, IEnumerable<Visual>> _resolve;

        internal MotionItems(Func<MotionContext, IEnumerable<Visual>> resolve) => _resolve = resolve;

        internal IEnumerable<Visual> Resolve(MotionContext ctx) => _resolve(ctx);
    }
}

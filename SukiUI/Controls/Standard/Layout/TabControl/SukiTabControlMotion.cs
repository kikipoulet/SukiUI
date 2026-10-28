using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.VisualTree;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    using Motion = SukiUI.Motion.Motion;

    /// <summary>The shape of the selection indicator a tab asks for.</summary>
    public enum SukiTabIndicator
    {
        /// <summary>No indicator for this tab.</summary>
        None,
        /// <summary>A bar under the header.</summary>
        Underline,
        /// <summary>A filled pill behind the header.</summary>
        Pill,
        /// <summary>A glass card behind the header.</summary>
        Glass,
    }

    /// <summary>
    /// The tab strip. One indicator travels from tab to tab instead of each tab lighting its
    /// own: its two edges ride separate springs, the leading one faster, so it stretches on the
    /// way and gathers on arrival; the tabs never move. The indicator takes the bounds of the
    /// selected tab's PART_IndicatorAnchor and the shape its <see cref="IndicatorProperty"/>
    /// asks for. A strip that reflows (resize, compactness, placement) has its indicator follow
    /// at once, or retargeted if it is travelling. The page change is
    /// <see cref="SukiTabPageTransition"/>, the headers <see cref="SukiTabItemMotion"/>.
    /// Template contract: PART_IndicatorLayer (a canvas sharing its parent with the items, under
    /// them), PART_Indicator (in the layer) holding PART_IndicatorUnderline, PART_IndicatorPill
    /// and PART_IndicatorGlass. A template without them has no indicator.
    /// </summary>
    public sealed class SukiTabControlMotion : MotionBehavior<TabControl, SukiTabControlProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiTabControlMotion, TabControl>();

        public static bool GetEnable(TabControl element) => element.GetValue(EnableProperty);
        public static void SetEnable(TabControl element, bool value) => element.SetValue(EnableProperty, value);

        /// <summary>The calibration of the strip, its headers and its pages (inherited by the tabs).</summary>
        public static readonly AttachedProperty<SukiTabControlPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiTabControlMotion, TemplatedControl, SukiTabControlPreset>(
                "Preset", SukiTabControlPreset.TabControl, inherits: true);

        public static SukiTabControlPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiTabControlPreset value) => element.SetValue(PresetProperty, value);

        /// <summary>The indicator a tab asks for when it is selected (its theme sets it).</summary>
        public static readonly AttachedProperty<SukiTabIndicator> IndicatorProperty =
            AvaloniaProperty.RegisterAttached<SukiTabControlMotion, TabItem, SukiTabIndicator>(
                "Indicator", SukiTabIndicator.Underline);

        public static SukiTabIndicator GetIndicator(TabItem element) => element.GetValue(IndicatorProperty);
        public static void SetIndicator(TabItem element, SukiTabIndicator value) => element.SetValue(IndicatorProperty, value);

        // The far edges of the indicator, in the layer's coordinates. The near edges are its
        // translation; width and height follow from both.
        internal static readonly AttachedProperty<double> RightProperty =
            AvaloniaProperty.RegisterAttached<SukiTabControlMotion, Visual, double>("Right");

        internal static readonly AttachedProperty<double> BottomProperty =
            AvaloniaProperty.RegisterAttached<SukiTabControlMotion, Visual, double>("Bottom");

        private static readonly Easing FadeEasing = SukiEase.Classic.Cubic.Out;

        /// <summary>Where the indicator stands (or is heading), per strip.</summary>
        private sealed class Strip
        {
            public Rect Goal;
            public SukiTabIndicator Kind;
            public bool Shown;
        }

        private static readonly ConditionalWeakTable<TabControl, Strip> Strips = new();

        protected override SukiTabControlProfile GetProfile(TabControl control) =>
            SukiAnimationTheme.For(control).TabControl[GetPreset(control)];

        protected override void Configure(MotionBuilder<TabControl, SukiTabControlProfile> motion)
        {
            var indicator = motion.TemplatePart("PART_Indicator");

            // ---- a new template: the indicator is placed by the first layout pass -------------
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Do(ctx => Strips.Remove(ctx.Control))
                  .And()
                  .Set(indicator, Visual.OpacityProperty).To(0.0);

            // ---- a new selection: the indicator travels -----------------------------------------
            // Leading edge on the fast spring, trailing edge on the slow one: it stretches toward
            // the new tab and gathers there.
            Travel(motion.OnChanged(SelectingItemsControl.SelectedIndexProperty)
                         .Where(ctx => CanTravel(ctx.Control))
                         .Do(ctx => Remember(ctx.Control))
                         .And(), indicator);

            // ... from nothing, or toward a tab of another shape: it appears there.
            motion.OnChanged(SelectingItemsControl.SelectedIndexProperty)
                  .Where(ctx => !CanTravel(ctx.Control) && Goal(ctx.Control) is not null)
                  .Do(ctx => Appear(ctx.Control))
                  .And()
                  .Animate(indicator, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.IndicatorFade)
                      .WithEasing(FadeEasing);

            // ... no tab, or a tab without indicator: it fades out where it is.
            motion.OnChanged(SelectingItemsControl.SelectedIndexProperty)
                  .Where(ctx => Goal(ctx.Control) is null)
                  .Do(ctx => Hide(ctx.Control))
                  .And()
                  .Animate(indicator, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.IndicatorFade)
                      .WithEasing(FadeEasing);

            // ---- the strip reflows (first layout, resize, compactness, placement) -----------------
            // At rest, the indicator follows at once: it belongs to the tab, it does not chase it.
            motion.OnEvent((c, h) => c.LayoutUpdated += h, (c, h) => c.LayoutUpdated -= h)
                  .Where(ctx => Reflowed(ctx.Control) && !IsTravelling(ctx.Control))
                  .Do(ctx => Place(ctx.Control, reveal: true));

            // ... while it travels, it heads for the new place from where it is, with its speed
            // (a fresh travel: retargeting only some edges would let the others cut the run short).
            Travel(motion.OnEvent((c, h) => c.LayoutUpdated += h, (c, h) => c.LayoutUpdated -= h)
                         .Where(ctx => Reflowed(ctx.Control) && IsTravelling(ctx.Control))
                         .Do(ctx => Remember(ctx.Control))
                         .And(), indicator);
        }

        /// <summary>The indicator's travel toward the goal: each edge on its spring, the size
        /// following both edges.</summary>
        private static void Travel<TArgs>(StepStarter<TabControl, SukiTabControlProfile, TArgs> step, MotionTarget indicator)
            where TArgs : EventArgs =>
            step.Animate(indicator, SukiMotion.TranslateXProperty)
                    .To(ctx => Goal(ctx.Control)!.Value.Left)
                    .WithSpring(ctx => NearSpring(ctx.Control, ctx.Profile, vertical: false))
                .And()
                .Animate(indicator, RightProperty)
                    .To(ctx => Goal(ctx.Control)!.Value.Right)
                    .WithSpring(ctx => FarSpring(ctx.Control, ctx.Profile, vertical: false))
                .And()
                .Animate(indicator, SukiMotion.TranslateYProperty)
                    .To(ctx => Goal(ctx.Control)!.Value.Top)
                    .WithSpring(ctx => NearSpring(ctx.Control, ctx.Profile, vertical: true))
                .And()
                .Animate(indicator, BottomProperty)
                    .To(ctx => Goal(ctx.Control)!.Value.Bottom)
                    .WithSpring(ctx => FarSpring(ctx.Control, ctx.Profile, vertical: true))
                .And()
                .Drive(indicator, Layoutable.WidthProperty)
                    .From(indicator, SukiMotion.TranslateXProperty)
                    .From(indicator, RightProperty)
                    .With(ctx => Math.Max(0.0, ctx.ValueOf(RightProperty) - ctx.ValueOf(SukiMotion.TranslateXProperty)))
                .And()
                .Drive(indicator, Layoutable.HeightProperty)
                    .From(indicator, SukiMotion.TranslateYProperty)
                    .From(indicator, BottomProperty)
                    .With(ctx => Math.Max(0.0, ctx.ValueOf(BottomProperty) - ctx.ValueOf(SukiMotion.TranslateYProperty)));

        // ---- the goal --------------------------------------------------------------------------

        /// <summary>The bounds of the selected tab's anchor in the indicator layer, or null when
        /// there is nothing to point at (no selection, no anchor, a tab without indicator).
        /// Through the transforms between them: a strip on a side mirrors its tabs (FlowDirection).
        /// The anchor is never what sinks under a press, so a press does not drag the indicator.</summary>
        internal static Rect? Goal(TabControl tabs)
        {
            if (Part(tabs, "PART_IndicatorLayer") is not { } layer)
                return null;
            if (tabs.SelectedIndex < 0 || tabs.ContainerFromIndex(tabs.SelectedIndex) is not TabItem item)
                return null;
            if (GetIndicator(item) == SukiTabIndicator.None || !item.IsEffectivelyVisible)
                return null;
            if (Part(item, "PART_IndicatorAnchor") is not { } anchor || anchor.Bounds.Width <= 0 || anchor.Bounds.Height <= 0)
                return null;
            if (anchor.TransformToVisual(layer) is not { } transform)
                return null;
            return new Rect(anchor.Bounds.Size).TransformToAABB(transform);
        }

        private static SukiTabIndicator Kind(TabControl tabs) =>
            tabs.SelectedIndex >= 0 && tabs.ContainerFromIndex(tabs.SelectedIndex) is TabItem item
                ? GetIndicator(item)
                : SukiTabIndicator.None;

        // ---- the state the reactions read ------------------------------------------------------

        /// <summary>The indicator is on screen with the shape the new tab asks for: it can travel.</summary>
        private static bool CanTravel(TabControl tabs) =>
            Strips.TryGetValue(tabs, out var strip) && strip.Shown && strip.Kind == Kind(tabs) && Goal(tabs) is not null;

        /// <summary>The anchor moved since the indicator was last aimed at it.</summary>
        private static bool Reflowed(TabControl tabs)
        {
            var goal = Goal(tabs);
            if (!Strips.TryGetValue(tabs, out var strip) || !strip.Shown)
                return goal is not null; // first placement
            return goal is { } g && !Near(g, strip.Goal);
        }

        /// <summary>The indicator has not reached its last goal yet.</summary>
        private static bool IsTravelling(TabControl tabs)
        {
            if (!Strips.TryGetValue(tabs, out var strip) || !strip.Shown || Part(tabs, "PART_Indicator") is not { } indicator)
                return false;
            return !Near(Current(indicator), strip.Goal);
        }

        private static Rect Current(Visual indicator)
        {
            var right = indicator.GetValue(RightProperty);
            var bottom = indicator.GetValue(BottomProperty);
            var width = indicator is Layoutable { Width: var w } && double.IsFinite(w) ? w : 0.0;
            var height = indicator is Layoutable { Height: var h } && double.IsFinite(h) ? h : 0.0;
            return new Rect(right - width, bottom - height, width, height);
        }

        private static bool Near(Rect a, Rect b) =>
            Math.Abs(a.X - b.X) < 0.5 && Math.Abs(a.Y - b.Y) < 0.5 &&
            Math.Abs(a.Right - b.Right) < 0.5 && Math.Abs(a.Bottom - b.Bottom) < 0.5;

        /// <summary>The near edge (left or top) leads when the indicator travels toward it.</summary>
        private static Spring NearSpring(TabControl tabs, SukiTabControlProfile p, bool vertical) =>
            TowardNear(tabs, vertical) ? p.Lead : p.Trail;

        private static Spring FarSpring(TabControl tabs, SukiTabControlProfile p, bool vertical) =>
            TowardNear(tabs, vertical) ? p.Trail : p.Lead;

        private static bool TowardNear(TabControl tabs, bool vertical)
        {
            if (Goal(tabs) is not { } goal || Part(tabs, "PART_Indicator") is not { } indicator)
                return false;
            var now = Current(indicator);
            return vertical ? goal.Top < now.Top : goal.Left < now.Left;
        }

        // ---- the writes --------------------------------------------------------------------------

        private static Strip StripOf(TabControl tabs) => Strips.GetValue(tabs, _ => new Strip());

        private static void Remember(TabControl tabs)
        {
            if (Goal(tabs) is not { } goal)
                return;
            var strip = StripOf(tabs);
            strip.Goal = goal;
            strip.Kind = Kind(tabs);
            strip.Shown = true;
        }

        /// <summary>Puts the indicator on its goal at once, with the shape of the selected tab.
        /// <paramref name="reveal"/>: a strip born with a selection shows it at once.</summary>
        private static void Place(TabControl tabs, bool reveal)
        {
            if (Goal(tabs) is not { } goal || Part(tabs, "PART_Indicator") is not { } indicator)
                return;
            var first = !Strips.TryGetValue(tabs, out var strip) || !strip.Shown;
            Remember(tabs);
            Shape(indicator, Kind(tabs));
            // A Set stops whatever moves the same property: the opacity keeps its own fade.
            Motion.Of(indicator)
                  .Set(SukiMotion.TranslateXProperty).To(goal.Left)
                  .And()
                  .Set(SukiMotion.TranslateYProperty).To(goal.Top)
                  .And()
                  .Set(RightProperty).To(goal.Right)
                  .And()
                  .Set(BottomProperty).To(goal.Bottom)
                  .And()
                  .Set(Layoutable.WidthProperty).To(goal.Width)
                  .And()
                  .Set(Layoutable.HeightProperty).To(goal.Height)
                  .PlayAsync();
            if (first && reveal)
                Motion.Of(indicator).Set(Visual.OpacityProperty).To(1.0).PlayAsync();
        }

        /// <summary>The indicator jumps to the new tab, takes its shape and fades in there.</summary>
        private static void Appear(TabControl tabs)
        {
            if (Strips.TryGetValue(tabs, out var strip))
                strip.Shown = false;
            Place(tabs, reveal: false);
        }

        private static void Hide(TabControl tabs)
        {
            if (Strips.TryGetValue(tabs, out var strip))
                strip.Shown = false;
        }

        /// <summary>Shows the one shape of the indicator the tab asks for.</summary>
        private static void Shape(Visual indicator, SukiTabIndicator kind)
        {
            if (indicator is not Panel panel)
                return;
            foreach (var child in panel.Children)
                child.IsVisible = child.Name switch
                {
                    "PART_IndicatorUnderline" => kind == SukiTabIndicator.Underline,
                    "PART_IndicatorPill" => kind == SukiTabIndicator.Pill,
                    "PART_IndicatorGlass" => kind == SukiTabIndicator.Glass,
                    _ => child.IsVisible,
                };
        }

        internal static Control? Part(TemplatedControl host, string name)
        {
            foreach (var child in host.GetTemplateDescendants())
                if (child is Control control && control.Name == name)
                    return control;
            return null;
        }
    }
}

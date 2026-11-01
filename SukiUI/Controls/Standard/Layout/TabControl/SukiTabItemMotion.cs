using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// A tab header. The pointer lights a plate under a tab that is not selected (the selected
    /// one already has the indicator); a press sinks the header — the descent always plays out —
    /// and it springs back on release. The selection itself is the strip's indicator
    /// (<see cref="SukiTabControlMotion"/>). Template contract: PART_HeaderPresenter (what sinks),
    /// PART_HoverPlate.
    /// </summary>
    public sealed class SukiTabItemMotion : MotionBehavior<TabItem, SukiTabControlProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiTabItemMotion, TabItem>();

        public static bool GetEnable(TabItem element) => element.GetValue(EnableProperty);
        public static void SetEnable(TabItem element, bool value) => element.SetValue(EnableProperty, value);

        private static readonly Easing HoverEasing = SukiEase.Classic.Cubic.Out;
        private static readonly Easing PressEasing = SukiEase.Classic.Cubic.Out;

        protected override SukiTabControlProfile GetProfile(TabItem control) =>
            SukiAnimationTheme.For(control).TabControl[SukiTabControlMotion.GetPreset(control)];

        protected override void Configure(MotionBuilder<TabItem, SukiTabControlProfile> motion)
        {
            // The header text sinks, not the tab: the indicator's anchor must stay where it is.
            var header = motion.TemplatePart("PART_HeaderPresenter");
            var hover = motion.TemplatePart("PART_HoverPlate");

            // ---- at start, and with every new template --------------------------------------
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Set(hover, Visual.OpacityProperty).To(ctx => IsLit(ctx.Control) ? 1.0 : 0.0)
                  .And()
                  .Set(header, SukiMotion.ScaleProperty).To(1.0);

            // ---- the pointer is over a tab that is not selected -------------------------------
            motion.OnChanged(InputElement.IsPointerOverProperty)
                  .OrChanged(TabItem.IsSelectedProperty)
                  .OrChanged(InputElement.IsEffectivelyEnabledProperty)
                  .Animate(hover, Visual.OpacityProperty)
                      .To(ctx => IsLit(ctx.Control) ? 1.0 : 0.0)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing);

            // ---- a press: the descent always plays out, the release springs back ---------------
            // The strip selects on press and handles the event: it is received handled too.
            motion.On(InputElement.PointerPressedEvent)
                  .IncludingHandled()
                  .Where(ctx => ctx.Args.GetCurrentPoint(ctx.Control).Properties.IsLeftButtonPressed)
                  .Animate(header, SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Profile.PressScale)
                      .Over(ctx => ctx.Profile.PressDuration)
                      .WithEasing(PressEasing)
                      .MustFinish();

            // A tab does not capture the pointer: leaving it releases the press too.
            motion.On(InputElement.PointerReleasedEvent)
                  .IncludingHandled()
                  .Or(InputElement.PointerCaptureLostEvent)
                  .Or(InputElement.PointerExitedEvent)
                  .Animate(header, SukiMotion.ScaleProperty)
                      .To(1.0)
                      .WithSpring(ctx => ctx.Profile.Release);

            // ---- detached or disabled: back to rest ------------------------------------------------
            motion.OnDetachedFromVisualTree()
                  .Set(header, SukiMotion.ScaleProperty).To(1.0);

            motion.OnDisabled()
                  .Set(header, SukiMotion.ScaleProperty).To(1.0)
                  .And()
                  .Set(hover, Visual.OpacityProperty).To(0.0);
        }

        private static bool IsLit(TabItem item) =>
            item.IsPointerOver && !item.IsSelected && item.IsEffectivelyEnabled;
    }
}

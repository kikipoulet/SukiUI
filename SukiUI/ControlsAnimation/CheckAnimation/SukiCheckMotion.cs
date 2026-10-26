using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The CheckBox: the box grows under the pointer, sinks when pressed and springs back; the
    /// check mark is drawn stroke by stroke with a soft trail behind its head; the box pops
    /// slightly when the state changes. Template contract: PART_Box (RenderTransformOrigin
    /// 50%,50%), PART_Check (a <see cref="CheckStroke"/>) and PART_Indeterminate (a bar with
    /// RenderTransformOrigin 50%,50%).
    /// </summary>
    public sealed class SukiCheckMotion : MotionBehavior<CheckBox, SukiCheckProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiCheckMotion, CheckBox>();

        public static bool GetEnable(CheckBox element) => element.GetValue(EnableProperty);
        public static void SetEnable(CheckBox element, bool value) => element.SetValue(EnableProperty, value);

        private static readonly Easing HoverEasing = new CubicEaseOut();
        private static readonly Easing PressEasing = new CubicEaseOut();

        protected override SukiCheckProfile GetProfile(CheckBox control) =>
            SukiAnimationTheme.Current.Check[SukiCheckPreset.CheckBox];

        protected override void Configure(MotionBuilder<CheckBox, SukiCheckProfile> motion)
        {
            var box = motion.TemplatePart("PART_Box");
            var fill = motion.TemplatePart("PART_Fill");
            var check = motion.TemplatePart("PART_Check");
            var bar = motion.TemplatePart("PART_Indeterminate");

            // At start, with every new template and once loaded (the template parts are only
            // in the tree by then): the mark is placed at once — a box born checked never
            // draws itself.
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Or(Control.LoadedEvent)
                  .Set(check, CheckStroke.ProgressProperty).To(ctx => MarkPosition(ctx))
                  .And()
                  .Set(check, CheckStroke.TrailProperty).To(ctx => MarkPosition(ctx))
                  .And()
                  .Set(bar, SukiMotion.ScaleXProperty).To(ctx => BarPosition(ctx))
                  .And()
                  .Set(fill, SukiMotion.ScaleProperty).To(ctx => FillRestScale(ctx))
                  .And()
                  .Set(fill, Visual.OpacityProperty).To(ctx => FillRest(ctx));

            // Checked, unchecked or indeterminate: the head draws (or erases) the mark on a
            // fast spring, the trail follows on a softer one, and the bar slides in or out.
            // The box is not part of this reaction: a run is interrupted as a whole as soon
            // as another one takes one of its properties, and the press and release own the
            // box's scale — sharing it would cut the drawing off at the click's release.
            motion.OnChanged(ToggleButton.IsCheckedProperty)
                  .Animate(check, CheckStroke.ProgressProperty)
                      .To(ctx => MarkPosition(ctx))
                      .WithSpring(ctx => ctx.Profile.DrawSpring)
                  .And()
                  .Animate(check, CheckStroke.TrailProperty)
                      .To(ctx => MarkPosition(ctx))
                      .WithSpring(ctx => ctx.Profile.TrailSpring)
                  .And()
                  .Animate(bar, SukiMotion.ScaleXProperty)
                      .To(ctx => BarPosition(ctx))
                      .WithSpring(ctx => ctx.Profile.BarSpring)
                  .And()
                  // The fill follows the mark (or the bar): it grows and fills in on the same
                  // spring, a little ahead of the stroke so the white mark never lands on bare card.
                  .Drive(fill, Visual.OpacityProperty)
                      .From(check, CheckStroke.ProgressProperty)
                      .From(bar, SukiMotion.ScaleXProperty)
                      .With(ctx => Math.Min(1.0, FillAmount(ctx) * ctx.Profile.FillLead))
                  .And()
                  .Drive(fill, SukiMotion.ScaleProperty)
                      .From(check, CheckStroke.ProgressProperty)
                      .From(bar, SukiMotion.ScaleXProperty)
                      .With(ctx => ctx.Profile.FillStartScale + (1.0 - ctx.Profile.FillStartScale) * FillAmount(ctx));

            // The same change kicks the box, which pops it on its spring.
            motion.OnChanged(ToggleButton.IsCheckedProperty)
                  .Animate(box, SukiMotion.ScaleProperty)
                      .To(ctx => RestScale(ctx))
                      .WithSpring(ctx => ctx.Profile.BoxSpring)
                      .AddVelocity(ctx => ctx.Profile.PopKick);

            // Hover: the box grows slightly — not while held down. If a spring is still
            // bouncing, only its target moves.
            motion.On(InputElement.PointerEnteredEvent)
                  .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                  .Animate(box, SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Profile.HoverScale)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing)
                      .RetargetIfMoving();

            motion.On(InputElement.PointerExitedEvent)
                  .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                  .Animate(box, SukiMotion.ScaleProperty)
                      .To(1.0)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing)
                      .RetargetIfMoving();

            // Press: the box sinks, the descent always plays to the end — even the shortest
            // click shows it. CheckBox handles the pointer events itself.
            motion.On(InputElement.PointerPressedEvent)
                  .IncludingHandled()
                  .Animate(box, SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Profile.PressDepth)
                      .Over(ctx => ctx.Profile.PressDuration)
                      .WithEasing(PressEasing)
                      .MustFinish();

            // Release: a spring back to rest — or to the hover size if the pointer is still over.
            motion.On(InputElement.PointerReleasedEvent)
                  .IncludingHandled()
                  .Or(InputElement.PointerCaptureLostEvent)
                  .Animate(box, SukiMotion.ScaleProperty)
                      .To(ctx => RestScale(ctx))
                      .WithSpring(ctx => ctx.Profile.BoxSpring);

            // Detached or disabled: the box goes back to its normal size.
            motion.OnDetachedFromVisualTree()
                  .Set(box, SukiMotion.ScaleProperty).To(1.0);

            motion.OnChanged(InputElement.IsEnabledProperty)
                  .Where(ctx => !ctx.Control.IsEnabled)
                  .Set(box, SukiMotion.ScaleProperty).To(1.0);

            motion.OnDisabled()
                  .Set(box, SukiMotion.ScaleProperty).To(1.0);
        }

        private static double MarkPosition<TArgs>(MotionContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsChecked == true ? 1.0 : 0.0;

        private static double BarPosition<TArgs>(MotionContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsChecked is null ? 1.0 : 0.0;

        // Checked or indeterminate: the fill is there.
        private static double FillRest<TArgs>(MotionContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsChecked != false ? 1.0 : 0.0;

        private static double FillRestScale<TArgs>(MotionContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsChecked != false ? 1.0 : ctx.Profile.FillStartScale;

        // How much of the fill is there, from the live poses of the mark and of the bar.
        private static double FillAmount<TArgs>(DriveContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            Math.Clamp(Math.Max(ctx.ValueOf(CheckStroke.ProgressProperty), ctx.ValueOf(SukiMotion.ScaleXProperty)), 0.0, 1.0);

        private static double RestScale<TArgs>(MotionContext<CheckBox, SukiCheckProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsPointerOver ? ctx.Profile.HoverScale : 1.0;
    }
}

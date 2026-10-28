using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The ToggleSwitch knob as a physical puck: it snaps between its two positions on a
    /// lightly damped spring (keeping its momentum when re-toggled mid-flight), smearing and
    /// stretching with its speed, crisp and round at rest. Template contract: a knob part
    /// named PART_Knob with RenderTransformOrigin 50%,50% — the template keeps the knob's
    /// layout and resting shadow, the motion only renders the travel on top of it.
    /// </summary>
    public sealed class SukiToggleSwitchMotion : MotionBehavior<ToggleButton, SukiToggleProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiToggleSwitchMotion, ToggleButton>();

        public static bool GetEnable(ToggleButton element) => element.GetValue(EnableProperty);
        public static void SetEnable(ToggleButton element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiTogglePreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiToggleSwitchMotion, TemplatedControl, SukiTogglePreset>(
                "Preset", SukiTogglePreset.ToggleSwitch);

        public static SukiTogglePreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiTogglePreset value) => element.SetValue(PresetProperty, value);

        /// <summary>
        /// Multiplies <see cref="SukiToggleProfile.Travel"/> (the knob's travel at Normal compactness):
        /// the template sets it from the current compactness level, so the knob follows the track width.
        /// </summary>
        public static readonly AttachedProperty<double> TravelScaleProperty =
            AvaloniaProperty.RegisterAttached<SukiToggleSwitchMotion, TemplatedControl, double>("TravelScale", 1.0);

        public static double GetTravelScale(TemplatedControl element) => element.GetValue(TravelScaleProperty);
        public static void SetTravelScale(TemplatedControl element, double value) => element.SetValue(TravelScaleProperty, value);

        protected override SukiToggleProfile GetProfile(ToggleButton control) =>
            SukiAnimationTheme.For(control).Toggle[GetPreset(control)];

        protected override void Configure(MotionBuilder<ToggleButton, SukiToggleProfile> motion)
        {
            var knob = motion.TemplatePart("PART_Knob");
            
            // At start and with every new template: the knob is placed at once — a switch born
            // checked never slides in.
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Set(knob, SukiMotion.TranslateXProperty).To(ctx => KnobPosition(ctx));

            // A new compactness level resizes the track: a checked knob takes its new rest pose at once.
            motion.OnChanged(TravelScaleProperty)
                  .Set(knob, SukiMotion.TranslateXProperty).To(ctx => KnobPosition(ctx));

            // Checked or unchecked: the knob snaps over on a spring; while it moves it blurs
            // and stretches along its travel, both driven by its speed.
            motion.OnChanged(ToggleButton.IsCheckedProperty)
                  .Animate(knob, SukiMotion.TranslateXProperty)
                      .To(ctx => KnobPosition(ctx))
                      .WithSpring(ctx => ctx.Profile.Spring)
                  .And()
                  .Drive(knob, SukiMotion.BlurProperty)
                      .From(knob, SukiMotion.TranslateXProperty)
                      .With(ctx => Math.Min(Speed(ctx) * ctx.Profile.BlurFactor, ctx.Profile.MaxBlur))
                  .And()
                  .Drive(knob, SukiMotion.ScaleXProperty)
                      .From(knob, SukiMotion.TranslateXProperty)
                      .With(ctx => 1.0 + Math.Min(Speed(ctx) * ctx.Profile.SquashFactor, ctx.Profile.MaxSquash))
                  .And()
                  .Drive(knob, SukiMotion.ScaleYProperty)
                      .From(knob, SukiMotion.TranslateXProperty)
                      .With(ctx => 1.0 - Math.Min(Speed(ctx) * ctx.Profile.SquashFactor, ctx.Profile.MaxSquash));
        }

        private static double KnobPosition<TArgs>(MotionContext<ToggleButton, SukiToggleProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.IsChecked == true ? ctx.Profile.Travel * GetTravelScale(ctx.Control) : 0.0;

        private static double Speed<TArgs>(DriveContext<ToggleButton, SukiToggleProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            Math.Abs(ctx.VelocityOf(SukiMotion.TranslateXProperty));
    }
}

using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Input;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The press feel of buttons and combo boxes: the control grows slightly under the
    /// pointer, sinks when pressed — the descent always plays to the end, then keeps sinking
    /// slowly while held — and springs back on release. Enable/Preset/PressDepth attached
    /// properties; the profile is read from <see cref="SukiAnimationTheme"/> at each gesture.
    /// </summary>
    public sealed class SukiPressMotion : MotionBehavior<InputElement, SukiPressProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiPressMotion, InputElement>();

        public static bool GetEnable(InputElement element) => element.GetValue(EnableProperty);
        public static void SetEnable(InputElement element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiPressPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiPressMotion, InputElement, SukiPressPreset>(
                "Preset", SukiPressPreset.Button);

        // NaN = follow the preset's DefaultPressDepth (Button 0.96, ComboBox 0.982).
        public static readonly AttachedProperty<double> PressDepthProperty =
            AvaloniaProperty.RegisterAttached<SukiPressMotion, InputElement, double>(
                "PressDepth", double.NaN);

        public static SukiPressPreset GetPreset(InputElement element) => element.GetValue(PresetProperty);
        public static void SetPreset(InputElement element, SukiPressPreset value) => element.SetValue(PresetProperty, value);

        public static double GetPressDepth(InputElement element) => element.GetValue(PressDepthProperty);
        public static void SetPressDepth(InputElement element, double value) => element.SetValue(PressDepthProperty, value);

        private static readonly Easing HoverEasing = new CubicEaseOut();
        private static readonly Easing PressEasing = new SukiEaseElasticIn { Damping = 2.5, Frequency = 3 };

        protected override SukiPressProfile GetProfile(InputElement control) =>
            SukiAnimationTheme.For(control).Press[GetPreset(control)];

        protected override void Configure(MotionBuilder<InputElement, SukiPressProfile> motion)
        {
            // Hover: the control grows slightly — not while it is held down. If the release
            // spring is still bouncing, only its target moves: the bounce is never cut short.
            motion.On(InputElement.PointerEnteredEvent)
                  .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                  .Animate(SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Profile.HoverScale)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing)
                      .RetargetIfMoving();

            motion.On(InputElement.PointerExitedEvent)
                  .Where(ctx => !ctx.Args.Properties.IsLeftButtonPressed)
                  .Animate(SukiMotion.ScaleProperty)
                      .To(1.0)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing)
                      .RetargetIfMoving();

            // Press: the descent always plays to the end — a release during it waits, so even
            // the shortest click shows the full press — then the control keeps sinking slowly
            // while held (a release there takes over at once). Buttons and combo boxes handle
            // the pointer events themselves.
            motion.On(InputElement.PointerPressedEvent)
                  .IncludingHandled()
                  .Animate(SukiMotion.ScaleProperty)
                      .To(ctx => Depth(ctx))
                      .Over(ctx => ctx.Profile.PressDuration)
                      .WithEasing(PressEasing)
                      .MustFinish()
                  .Then()
                  .Animate(SukiMotion.ScaleProperty)
                      .To(ctx => Depth(ctx) - ctx.Profile.ExtraDeepRange)
                      .Over(ctx => ctx.Profile.DeepDuration);

            // Release: a spring back to rest — or to the hover size if the pointer is still over.
            motion.On(InputElement.PointerReleasedEvent)
                  .IncludingHandled()
                  .Or(InputElement.PointerCaptureLostEvent)
                  .Animate(SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Control.IsPointerOver ? ctx.Profile.HoverScale : 1.0)
                      .WithSpring(ctx => ctx.Profile.ReleaseSpring);

            // Detached or disabled: the control goes back to its normal size.
            motion.OnDetachedFromVisualTree()
                  .Set(SukiMotion.ScaleProperty).To(1.0);

            motion.OnDisabled()
                  .Set(SukiMotion.ScaleProperty).To(1.0);
        }

        private static double Depth<TArgs>(MotionContext<InputElement, SukiPressProfile, TArgs> ctx)
            where TArgs : EventArgs
        {
            double depth = GetPressDepth(ctx.Control);
            return double.IsNaN(depth) ? ctx.Profile.DefaultPressDepth : depth;
        }
    }
}

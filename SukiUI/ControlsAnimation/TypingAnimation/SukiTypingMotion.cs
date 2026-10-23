using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SukiUI.Motion;
using SukiUI.Theme;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The TextBox typing glow: the primary border overlay rests at 0, at 0.5 while the box is
    /// focused or hovered, and every keystroke kicks it brighter before it settles back on a
    /// spring — so it brightens with typing speed, with no steps and no timers. Renders through
    /// an overlay Border independent of GlassCardBorder, identical in light and dark themes.
    /// </summary>
    public sealed class SukiTypingMotion : MotionBehavior<TextBox, SukiTypingProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiTypingMotion, TextBox>();

        public static bool GetEnable(TextBox element) => element.GetValue(EnableProperty);
        public static void SetEnable(TextBox element, bool value) => element.SetValue(EnableProperty, value);

        protected override SukiTypingProfile GetProfile(TextBox control) =>
            SukiAnimationTheme.Current.Typing[SukiTypingPreset.TextBox];

        protected override void Configure(MotionBuilder<TextBox, SukiTypingProfile> motion)
        {
            // Hover and focus: the glow reaches its resting level in 150 ms. If it is still
            // settling after a keystroke, only its target moves.
            motion.On(InputElement.PointerEnteredEvent)
                  .Or(InputElement.PointerExitedEvent)
                  .OrChanged(InputElement.IsFocusedProperty)
                  .Animate(TextBoxExtensions.TypingIntensityProperty)
                      .To(ctx => RestLevel(ctx.Control))
                      .Over(TimeSpan.FromMilliseconds(150))
                      .RetargetIfMoving();

            // Every keystroke kicks the glow, which then settles back on an overdamped spring:
            // a kick is nearly invisible the frame it lands and builds into a delayed hump that
            // merges with the next keystrokes. TextBox handles TextInput itself.
            motion.On(InputElement.TextInputEvent)
                  .IncludingHandled()
                  .Where(ctx => ctx.Profile.Kick > 0)
                  .Animate(TextBoxExtensions.TypingIntensityProperty)
                      .To(ctx => RestLevel(ctx.Control))
                      .WithSpring(ctx => ctx.Profile.Spring)
                      .AddVelocity(ctx => ctx.Profile.Kick);

            motion.OnDetachedFromVisualTree()
                  .Set(TextBoxExtensions.TypingIntensityProperty).To(0.0);

            motion.OnDisabled()
                  .Set(TextBoxExtensions.TypingIntensityProperty).To(0.0);
        }

        private static double RestLevel(TextBox box) => box.IsFocused || box.IsPointerOver ? 0.5 : 0.0;
    }
}

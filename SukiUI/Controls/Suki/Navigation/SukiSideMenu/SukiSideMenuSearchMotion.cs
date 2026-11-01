using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using SukiUI.CompactnessTheme;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The side menu's search field: a round button until the pointer is over it or the field has
    /// the focus, then it stretches into a field (width and corner radius together, on one timed
    /// curve) and closes back when both are gone. Enabled on the search GlassCard of the
    /// SukiSideMenu template.
    /// </summary>
    public sealed class SukiSideMenuSearchMotion : MotionBehavior<GlassCard, SukiSideMenuProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiSideMenuSearchMotion, GlassCard>();

        public static bool GetEnable(GlassCard element) => element.GetValue(EnableProperty);
        public static void SetEnable(GlassCard element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiSideMenuPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiSideMenuSearchMotion, TemplatedControl, SukiSideMenuPreset>(
                "Preset", SukiSideMenuPreset.SideMenu);

        public static SukiSideMenuPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiSideMenuPreset value) => element.SetValue(PresetProperty, value);

        protected override SukiSideMenuProfile GetProfile(GlassCard control) =>
            SukiAnimationTheme.For(control).SideMenu[GetPreset(control)];

        protected override void Configure(MotionBuilder<GlassCard, SukiSideMenuProfile> motion)
        {
            // At start: the field is placed at once, a menu never opens with its search stretching.
            motion.OnEnabled()
                  .Set(Layoutable.WidthProperty).To(ctx => Width(ctx.Control, ctx.Profile))
                  .And()
                  .Set(GlassCard.CornerRadiusProperty).To(ctx => Radius(ctx.Control, ctx.Profile));

            // A new compactness level resizes the button; an open field keeps up as well.
            motion.OnCompactnessChanged()
                  .Set(Layoutable.WidthProperty).To(ctx => Width(ctx.Control, ctx.Profile))
                  .And()
                  .Set(GlassCard.CornerRadiusProperty).To(ctx => Radius(ctx.Control, ctx.Profile));

            // The pointer is over it, or the field has the focus: it stretches; both gone: it closes.
            motion.OnChanged(InputElement.IsPointerOverProperty)
                  .OrChanged(InputElement.IsKeyboardFocusWithinProperty)
                  .Animate(Layoutable.WidthProperty)
                      .To(ctx => Width(ctx.Control, ctx.Profile))
                      .Over(ctx => ctx.Profile.SearchDuration)
                      .WithEasing(ctx => ctx.Profile.SearchEase)
                  .And()
                  .Animate(GlassCard.CornerRadiusProperty)
                      .To(ctx => Radius(ctx.Control, ctx.Profile))
                      .Over(ctx => ctx.Profile.SearchDuration)
                      .WithEasing(ctx => ctx.Profile.SearchEase);
        }

        private static bool IsOpen(GlassCard card) => card.IsPointerOver || card.IsKeyboardFocusWithin;

        // The sizes follow the compactness level (SukiSideMenu.Compactness.axaml).
        private static double Width(GlassCard card, SukiSideMenuProfile p) =>
            card.GetCompactness<double>(IsOpen(card)
                ? "SukiCompactness.SukiSideMenu.Search.OpenWidth"
                : "SukiCompactness.SukiSideMenu.Search.Size");

        private static CornerRadius Radius(GlassCard card, SukiSideMenuProfile p) =>
            card.GetCompactness<CornerRadius>(IsOpen(card)
                ? "SukiCompactness.SukiSideMenu.Search.Open.CornerRadius"
                : "SukiCompactness.SukiSideMenu.Search.CornerRadius");
    }
}

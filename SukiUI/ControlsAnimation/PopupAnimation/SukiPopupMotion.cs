using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The drop-down popup of a templated control (ComboBox, MenuItem…), animated open and
    /// closed: the popup springs open with a motion blur and an item cascade, and collapses
    /// while dissolving — the real popup closing only at the end of the collapse. Configured
    /// from XAML: Enable, Preset, PopupPart, OpenProperty (the host's bool open property,
    /// e.g. <c>{x:Static ComboBox.IsDropDownOpenProperty}</c>) and CloseOnOutsidePress. The
    /// motion owns the template popup's IsOpen (no binding on it); root and items follow the
    /// PART_LayoutTransform / PART_ItemsPresenter convention.
    /// </summary>
    public sealed class SukiPopupMotion : MotionBehavior<TemplatedControl, SukiPopupProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiPopupMotion, TemplatedControl>();

        public static bool GetEnable(TemplatedControl element) => element.GetValue(EnableProperty);
        public static void SetEnable(TemplatedControl element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiPopupPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, SukiPopupPreset>(
                "Preset", SukiPopupPreset.ComboBox);

        /// <summary>The name of the template Popup the motion owns. Defaults to PART_SukiPopup.</summary>
        public static readonly AttachedProperty<string> PopupPartProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, string>(
                "PopupPart", "PART_SukiPopup");

        /// <summary>The host's bool open property — watched and written (outside press,
        /// deactivation, abnormal close).</summary>
        public static readonly AttachedProperty<AvaloniaProperty> OpenPropertyProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, AvaloniaProperty>(
                "OpenProperty");

        /// <summary>Any press in the host's window closes the popup (the ComboBox rule). Hosts
        /// whose own logic already dismisses on outside press opt out: menus open on
        /// PointerPressed, so this guard would close them in the very event that opened them.</summary>
        public static readonly AttachedProperty<bool> CloseOnOutsidePressProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, bool>(
                "CloseOnOutsidePress", true);

        public static SukiPopupPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiPopupPreset value) => element.SetValue(PresetProperty, value);

        public static string GetPopupPart(TemplatedControl element) => element.GetValue(PopupPartProperty);
        public static void SetPopupPart(TemplatedControl element, string value) => element.SetValue(PopupPartProperty, value);

        public static AvaloniaProperty GetOpenProperty(TemplatedControl element) => element.GetValue(OpenPropertyProperty);
        public static void SetOpenProperty(TemplatedControl element, AvaloniaProperty value) => element.SetValue(OpenPropertyProperty, value);

        public static bool GetCloseOnOutsidePress(TemplatedControl element) => element.GetValue(CloseOnOutsidePressProperty);
        public static void SetCloseOnOutsidePress(TemplatedControl element, bool value) => element.SetValue(CloseOnOutsidePressProperty, value);

        protected override SukiPopupProfile GetProfile(TemplatedControl control) =>
            SukiAnimationTheme.Current.Popup[GetPreset(control)];

        protected override void Configure(MotionBuilder<TemplatedControl, SukiPopupProfile> motion)
        {
            var popup = motion.Popup()
                .PopupPart(host => GetPopupPart(host))
                .RootPart("PART_LayoutTransform")
                .ItemsPresenterPart("PART_ItemsPresenter")
                .OpenWhen(host => GetOpenProperty(host))
                .CloseOnPointerPressedOutside(host => GetCloseOnOutsidePress(host))
                .CloseInstantlyWhenWindowDeactivated();

            SukiPopupFeel.Apply(popup);
        }
    }

    /// <summary>The popup feel shared by every popup motion (drop-downs and context menus).</summary>
    internal static class SukiPopupFeel
    {
        internal static void Apply<TControl>(PopupSection<TControl, SukiPopupProfile> popup)
            where TControl : TemplatedControl
        {
            // Opening: the popup springs to full size from a collapsed pose and fades in,
            // blurred by its own expansion speed; its items cascade in, one after the other.
            popup.OnOpening()
                 .Animate(popup.Root, SukiMotion.ScaleXProperty)
                     .From(ctx => ctx.Profile.ClosedScaleX)
                     .To(1.0)
                     .WithSpring(ctx => ctx.Profile.OpenSpring)
                 .And()
                 .Animate(popup.Root, SukiMotion.ScaleYProperty)
                     .From(ctx => ctx.Profile.ClosedScaleY)
                     .To(1.0)
                     .WithSpring(ctx => ctx.Profile.OpenSpring)
                 .And()
                 .Animate(popup.Root, Visual.OpacityProperty)
                     .From(0.0)
                     .To(1.0)
                     .Over(ctx => ctx.Profile.OpenOpacityDuration)
                 .And()
                 .Drive(popup.Root, SukiMotion.BlurProperty)
                     .From(popup.Root, SukiMotion.ScaleXProperty, SukiMotion.ScaleYProperty)
                     .With(ctx => Math.Min(
                         (Math.Abs(ctx.VelocityOf(SukiMotion.ScaleXProperty)) + Math.Abs(ctx.VelocityOf(SukiMotion.ScaleYProperty)))
                             * ctx.Profile.BlurFactor,
                         ctx.Profile.MaxBlurRadius))
                 .And()
                 .Cascade(popup.Items, item => item
                         .Animate(Visual.OpacityProperty)
                             .From(0.0)
                             .To(1.0)
                             .Over(ctx => Ms(ctx.Profile.CascadeDurationMs))
                         .And()
                         // Crisp at 70% of the appearance: the blur leaves before the fade ends.
                         .Animate(SukiMotion.BlurProperty)
                             .From(ctx => ctx.Profile.CascadeItemBlur)
                             .To(0.0)
                             .Over(ctx => Ms(ctx.Profile.CascadeDurationMs * 0.7))
                         .And()
                         .Animate(SukiMotion.TranslateYProperty)
                             .From(ctx => ctx.Profile.CascadeItemOffsetY)
                             .To(0.0)
                             .Over(ctx => Ms(ctx.Profile.CascadeDurationMs))
                         .And()
                         .Animate(SukiMotion.ScaleProperty)
                             .From(ctx => ctx.Profile.CascadeItemScale)
                             .To(1.0)
                             .Over(ctx => Ms(ctx.Profile.CascadeDurationMs)))
                     .WithInitialDelay(ctx => Ms(ctx.Profile.CascadeInitialDelayMs))
                     .WithStagger((ctx, count) => Ms(ctx.Profile.CascadeStaggerMs(count)))
                     .OnlyWhenFewerThan(ctx => ctx.Profile.CascadeMaxItems)
                     // The collapse shows the list: closing mid-cascade lands every item first.
                     .CompleteIfInterrupted();

            // Closing: a partial collapse on springs while the popup fades out, dissolving
            // into blur as it disappears.
            popup.OnClosing()
                 .Animate(popup.Root, SukiMotion.ScaleXProperty)
                     .To(ctx => ctx.Profile.CloseScaleX)
                     .WithSpring(ctx => ctx.Profile.CloseSpring)
                 .And()
                 .Animate(popup.Root, SukiMotion.ScaleYProperty)
                     .To(ctx => ctx.Profile.CloseScaleY)
                     .WithSpring(ctx => ctx.Profile.CloseSpring)
                 .And()
                 .Animate(popup.Root, Visual.OpacityProperty)
                     .To(0.0)
                     .Over(ctx => ctx.Profile.CloseOpacityDuration)
                 .And()
                 .Drive(popup.Root, SukiMotion.BlurProperty)
                     .From(popup.Root, Visual.OpacityProperty)
                     .With(ctx => (1.0 - ctx.ValueOf(Visual.OpacityProperty)) * ctx.Profile.CloseBlurRadius);
        }

        private static TimeSpan Ms(double milliseconds) => TimeSpan.FromMilliseconds(milliseconds);
    }
}

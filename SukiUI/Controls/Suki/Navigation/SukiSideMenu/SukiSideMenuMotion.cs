using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using SukiUI.CompactnessTheme;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The side menu itself. The pane opens and closes on a timed curve that never overshoots (it
    /// replaces the SplitView: it moves the page next to it); the pane toggle turns; header,
    /// footer and search fold their room in and out on the very same curve, so the rows below
    /// never jump, and their content fades once the pane has made room. The rows are
    /// <see cref="SukiSideMenuItemMotion"/>. Template contract: PART_Pane, PART_ExpandIcon,
    /// PART_HeaderFold/PART_HeaderHost, PART_FooterFold/PART_FooterHost,
    /// PART_SearchFold/PART_SearchHost (each fold is a clipping panel around its host).
    /// </summary>
    public sealed class SukiSideMenuMotion : MotionBehavior<SukiSideMenu, SukiSideMenuProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiSideMenuMotion, SukiSideMenu>();

        public static bool GetEnable(SukiSideMenu element) => element.GetValue(EnableProperty);
        public static void SetEnable(SukiSideMenu element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiSideMenuPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiSideMenuMotion, TemplatedControl, SukiSideMenuPreset>(
                "Preset", SukiSideMenuPreset.SideMenu);

        public static SukiSideMenuPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiSideMenuPreset value) => element.SetValue(PresetProperty, value);

        private static readonly Easing RevealEasing = SukiEase.Classic.Cubic.Out;

        // The width of the compact pane, for the compactness level: the rows (SukiSideMenuItemMotion) aim at it too.
        internal const string CompactWidthKey = "SukiCompactness.SukiSideMenu.Pane.CompactWidth";

        private static readonly string[] Folds = { "PART_HeaderFold", "PART_FooterFold", "PART_SearchFold" };

        protected override SukiSideMenuProfile GetProfile(SukiSideMenu control) =>
            SukiAnimationTheme.Current.SideMenu[GetPreset(control)];

        protected override void Configure(MotionBuilder<SukiSideMenu, SukiSideMenuProfile> motion)
        {
            var pane = motion.TemplatePart("PART_Pane");
            var toggle = motion.TemplatePart("PART_ExpandIcon");
            var headerFold = motion.TemplatePart("PART_HeaderFold");
            var footerFold = motion.TemplatePart("PART_FooterFold");
            var searchFold = motion.TemplatePart("PART_SearchFold");
            var header = motion.TemplatePart("PART_HeaderHost");
            var footer = motion.TemplatePart("PART_FooterHost");
            var search = motion.TemplatePart("PART_SearchHost");

            // A menu born collapsed never animates into it.
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Do(ctx => SetFoldsVisible(ctx.Control, ctx.Control.IsMenuExpanded))
                  .And()
                  .Set(headerFold, Layoutable.MaxHeightProperty).To(ctx => FoldHeight(ctx.Control))
                  .And()
                  .Set(footerFold, Layoutable.MaxHeightProperty).To(ctx => FoldHeight(ctx.Control))
                  .And()
                  .Set(searchFold, Layoutable.MaxHeightProperty).To(ctx => FoldHeight(ctx.Control))
                  .And()
                  .Set(pane, Layoutable.WidthProperty).To(ctx => PaneWidth(ctx.Control, ctx.Profile))
                  .And()
                  .Set(toggle, SukiMotion.RotateProperty).To(ctx => ctx.Control.IsMenuExpanded ? 0.0 : 180.0)
                  .And()
                  .Set(header, Visual.OpacityProperty).To(ctx => ctx.Control.IsMenuExpanded ? 1.0 : 0.0)
                  .And()
                  .Set(header, SukiMotion.BlurProperty).To(0.0)
                  .And()
                  .Set(footer, Visual.OpacityProperty).To(ctx => ctx.Control.IsMenuExpanded ? 1.0 : 0.0)
                  .And()
                  .Set(search, Visual.OpacityProperty).To(ctx => ctx.Control.IsMenuExpanded ? 1.0 : 0.0);

            // The pane itself: it opens and closes on a spring (an open pane follows a new OpenPaneLength).
            motion.OnChanged(SukiSideMenu.IsMenuExpandedProperty)
                  .Animate(pane, Layoutable.WidthProperty)
                      .To(ctx => PaneWidth(ctx.Control, ctx.Profile))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase);

            // A new compactness level changes the compact width: the pane takes it at once.
            motion.OnCompactnessChanged()
                  .Set(pane, Layoutable.WidthProperty).To(ctx => PaneWidth(ctx.Control, ctx.Profile));

            motion.OnChanged(SukiSideMenu.OpenPaneLengthProperty)
                  .Where(ctx => ctx.Control.IsMenuExpanded)
                  .Animate(pane, Layoutable.WidthProperty)
                      .To(ctx => PaneWidth(ctx.Control, ctx.Profile))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase);

            motion.OnChanged(SukiSideMenu.IsMenuExpandedProperty)
                  .Animate(toggle, SukiMotion.RotateProperty)
                      .To(ctx => ctx.Control.IsMenuExpanded ? 0.0 : 180.0)
                      .Over(ctx => ctx.Profile.RotateDuration)
                      .WithEasing(ctx => ctx.Profile.RotateEase);

            // The pane opens: header, footer and search take their room on the pane's spring and
            // their content comes in once there is room for it.
            motion.OnChanged(SukiSideMenu.IsMenuExpandedProperty)
                  .Where(ctx => ctx.Control.IsMenuExpanded)
                  .Do(ctx => SetFoldsVisible(ctx.Control, true))
                  .Then()
                  .Animate(headerFold, Layoutable.MaxHeightProperty)
                      .From(0.0).To(ctx => Natural(ctx.Control, "PART_HeaderFold"))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .And()
                  .Animate(footerFold, Layoutable.MaxHeightProperty)
                      .From(0.0).To(ctx => Natural(ctx.Control, "PART_FooterFold"))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .And()
                  .Animate(searchFold, Layoutable.MaxHeightProperty)
                      .From(0.0).To(ctx => Natural(ctx.Control, "PART_SearchFold"))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .And()
                  .Animate(header, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(RevealEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(header, SukiMotion.TranslateXProperty)
                      .From(ctx => ctx.Profile.RevealOffsetX).To(0.0)
                      .WithSpring(ctx => ctx.Profile.Child)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  // The header comes out of a very light blur, crisp before the end.
                  .Animate(header, SukiMotion.BlurProperty)
                      .From(ctx => ctx.Profile.RevealBlur).To(0.0)
                      .Over(ctx => ctx.Profile.RevealDuration/1.5)
                      .WithEasing(RevealEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(footer, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(RevealEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(search, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(RevealEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(search, SukiMotion.TranslateXProperty)
                      .From(ctx => ctx.Profile.RevealOffsetX).To(0.0)
                      .WithSpring(ctx => ctx.Profile.Child)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .Then()
                  .Set(headerFold, Layoutable.MaxHeightProperty).To(double.PositiveInfinity)
                  .And()
                  .Set(footerFold, Layoutable.MaxHeightProperty).To(double.PositiveInfinity)
                  .And()
                  .Set(searchFold, Layoutable.MaxHeightProperty).To(double.PositiveInfinity);

            // ... and they leave first when it closes, giving their room back as the pane narrows.
            motion.OnChanged(SukiSideMenu.IsMenuExpandedProperty)
                  .Where(ctx => !ctx.Control.IsMenuExpanded)
                  .Animate(header, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .And()
                  // ... dissolving into the same light blur it comes out of.
                  .Animate(header, SukiMotion.BlurProperty)
                      .To(ctx => ctx.Profile.RevealBlur)
                      .Delay(ctx => ctx.Profile.HideDuration -( ctx.Profile.HideDuration/1.5))
                      .Over(ctx => ctx.Profile.HideDuration/1.5)
                      .WithEasing(RevealEasing)
                  .And()
                  .Animate(footer, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .And()
                  .Animate(search, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .And()
                  .Animate(headerFold, Layoutable.MaxHeightProperty)
                      .From(ctx => Height(ctx.Control, "PART_HeaderFold")).To(0.0)
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .And()
                  .Animate(footerFold, Layoutable.MaxHeightProperty)
                      .From(ctx => Height(ctx.Control, "PART_FooterFold")).To(0.0)
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .And()
                  .Animate(searchFold, Layoutable.MaxHeightProperty)
                      .From(ctx => Height(ctx.Control, "PART_SearchFold")).To(0.0)
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase)
                  .Then()
                  .Do(ctx => SetFoldsVisible(ctx.Control, false))
                  .And()
                  .Set(header, SukiMotion.BlurProperty).To(0.0); // drop the blur shader once it is hidden
        }

        // The compact width follows the compactness level (SukiSideMenu.Compactness.axaml).
        private static double PaneWidth(SukiSideMenu menu, SukiSideMenuProfile p) =>
            menu.IsMenuExpanded ? menu.OpenPaneLength : menu.GetCompactness<double>(CompactWidthKey);

        private static double FoldHeight(SukiSideMenu menu) => menu.IsMenuExpanded ? double.PositiveInfinity : 0.0;

        private static double Height(SukiSideMenu menu, string fold) => menu.Part(fold)?.Bounds.Height ?? 0.0;

        /// <summary>The room a fold needs once open, measured at the width of the open pane.</summary>
        private static double Natural(SukiSideMenu menu, string fold)
        {
            if (menu.Part(fold) is not Panel { Children.Count: > 0 } panel)
                return 0.0;
            var content = panel.Children[0];
            content.Measure(new Size(menu.OpenPaneLength, double.PositiveInfinity));
            return content.DesiredSize.Height;
        }

        // The folds' visibility belongs to the motion, not to bindings that would cut the fades.
        // The search keeps its own switch (IsSearchEnabled) on its gate.
        private static void SetFoldsVisible(SukiSideMenu menu, bool visible)
        {
            foreach (var name in Folds)
                if (menu.Part(name) is { } fold)
                    fold.IsVisible = visible;
        }
    }
}

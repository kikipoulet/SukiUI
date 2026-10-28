using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using SukiUI.CompactnessTheme;
using SukiUI.Controls;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// A row of the side menu. The pointer lights a plate under the row, a press sinks it and
    /// springs it back. The selection is one liquid plate: it drains out of the row that is left
    /// toward the new one and fills it from the side the choice came from, while the title
    /// slides over the icon it replaces. When the pane opens or closes, the row slides on the
    /// pane's own curve (it keeps the width of the open pane, so nothing in it reflows) and the
    /// titles fade. Sub-menus unroll and their items arrive one after the other; rows filtered
    /// out by the search fold away. Template contract: PART_Border (clipped row), PART_HoverPlate,
    /// PART_SelectionPlate, PART_Icon, PART_TitleBox, PART_Header, PART_SideDisplay,
    /// PART_ChevronHost, PART_ExpandedButton, PART_Content, PART_ChildrenHost, PART_ItemsPresenter.
    /// </summary>
    public sealed class SukiSideMenuItemMotion : MotionBehavior<SukiSideMenuItem, SukiSideMenuProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiSideMenuItemMotion, SukiSideMenuItem>();

        public static bool GetEnable(SukiSideMenuItem element) => element.GetValue(EnableProperty);
        public static void SetEnable(SukiSideMenuItem element, bool value) => element.SetValue(EnableProperty, value);

        public static readonly AttachedProperty<SukiSideMenuPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiSideMenuItemMotion, TemplatedControl, SukiSideMenuPreset>(
                "Preset", SukiSideMenuPreset.SideMenu);

        public static SukiSideMenuPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiSideMenuPreset value) => element.SetValue(PresetProperty, value);

        // The plate never reaches a flat zero: a singular render transform is not worth the risk.
        internal const double Drained = 0.02;

        // The row's margin follows the compactness level; a filtered row gives up its vertical share.
        private static Thickness RowMargin(SukiSideMenuItem item) =>
            item.GetCompactness<Thickness>("SukiCompactness.SukiSideMenuItem.Row.Margin");

        private static Thickness FilteredMargin(SukiSideMenuItem item)
        {
            var margin = RowMargin(item);
            return new Thickness(margin.Left, 0, margin.Right, 0);
        }

        private static readonly Easing HoverEasing = SukiEase.Classic.Cubic.Out;
        private static readonly Easing PressEasing = SukiEase.Classic.Cubic.Out;

        protected override SukiSideMenuProfile GetProfile(SukiSideMenuItem control) =>
            SukiAnimationTheme.For(control).SideMenu[GetPreset(control)];

        protected override void Configure(MotionBuilder<SukiSideMenuItem, SukiSideMenuProfile> motion)
        {
            var row = motion.TemplatePart("PART_Border");
            var hover = motion.TemplatePart("PART_HoverPlate");
            var plate = motion.TemplatePart("PART_SelectionPlate");
            var icon = motion.TemplatePart("PART_Icon");
            var titleBox = motion.TemplatePart("PART_TitleBox");
            // The title lives inside a Viewbox, which a template-part lookup does not cross.
            var header = MotionTarget.Find(c => (c as SukiSideMenuItem)?.Part("PART_Header"));
            var side = motion.TemplatePart("PART_SideDisplay");
            var chevronHost = motion.TemplatePart("PART_ChevronHost");
            var chevron = motion.TemplatePart("PART_ExpandedButton");
            var children = motion.TemplatePart("PART_ChildrenHost");

            // ---- at start, and with every new template: everything is placed at once ----------
            // A menu born with a selected row or an open sub-menu never animates into it.
            motion.OnEnabled()
                  .Or(TemplatedControl.TemplateAppliedEvent)
                  .Do(ctx => Place(ctx.Control))
                  .And()
                  .Set(plate, Visual.OpacityProperty).To(ctx => ctx.Control.IsPlateActive ? 1.0 : 0.0)
                  .And()
                  .Set(plate, SukiMotion.ScaleYProperty).To(ctx => ctx.Control.IsPlateActive ? 1.0 : Drained)
                  .And()
                  .Set(hover, Visual.OpacityProperty).To(ctx => ctx.Control.IsHeaderPointerOver ? 1.0 : 0.0)
                  .And()
                  .Set(icon, SukiMotion.TranslateXProperty).To(ctx => IconOffset(ctx.Control, ctx.Profile))
                  .And()
                  .Set(icon, SukiMotion.ScaleProperty).To(ctx => IconScale(ctx.Control, ctx.Profile))
                  .And()
                  .Set(icon, Visual.OpacityProperty).To(ctx => IsShifted(ctx.Control) ? 0.0 : 1.0)
                  .And()
                  .Set(titleBox, SukiMotion.TranslateXProperty).To(ctx => IconOffset(ctx.Control, ctx.Profile))
                  .And()
                  .Set(row, SukiMotion.TranslateXProperty).To(ctx => RowOffset(ctx.Control, ctx.Profile))
                  .And()
                  .Set(header, Visual.OpacityProperty).To(ctx => ctx.Control.IsTopMenuExpanded ? 1.0 : 0.0)
                  .And()
                  .Set(header, SukiMotion.TranslateXProperty).To(0.0)
                  .And()
                  .Set(header, SukiMotion.BlurProperty).To(0.0)
                  .And()
                  .Set(side, Visual.OpacityProperty).To(ctx => ctx.Control.IsTopMenuExpanded ? 1.0 : 0.0)
                  .And()
                  .Set(chevronHost, Visual.OpacityProperty).To(ctx => ctx.Control.IsTopMenuExpanded ? 1.0 : 0.0)
                  .And()
                  .Set(chevron, SukiMotion.RotateProperty).To(ctx => ctx.Control.IsExpanded ? 180.0 : 0.0)
                  .And()
                  .Set(children, Layoutable.MaxHeightProperty).To(ctx => ShowChildren(ctx.Control) ? double.PositiveInfinity : 0.0)
                  .And()
                  .Set(row, Layoutable.MaxHeightProperty).To(ctx => ctx.Control.IsFiltered ? 0.0 : double.PositiveInfinity)
                  .And()
                  .Set(row, Visual.OpacityProperty).To(ctx => ctx.Control.IsFiltered ? 0.0 : 1.0);

            // ---- the pointer is over the row ---------------------------------------------------
            motion.OnChanged(SukiSideMenuItem.IsHeaderPointerOverProperty)
                  .Animate(hover, Visual.OpacityProperty)
                      .To(ctx => ctx.Control.IsHeaderPointerOver ? 1.0 : 0.0)
                      .Over(ctx => ctx.Profile.HoverDuration)
                      .WithEasing(HoverEasing);

            // ---- a press: the descent always plays out, the release springs back -------------------
            motion.OnChanged(SukiSideMenuItem.IsHeaderPressedProperty)
                  .Where(ctx => ctx.Control.IsHeaderPressed)
                  .Animate(row, SukiMotion.ScaleProperty)
                      .To(ctx => ctx.Profile.PressScale)
                      .Over(ctx => ctx.Profile.PressDuration)
                      .WithEasing(PressEasing)
                      .MustFinish();

            motion.OnChanged(SukiSideMenuItem.IsHeaderPressedProperty)
                  .Where(ctx => !ctx.Control.IsHeaderPressed)
                  .Animate(row, SukiMotion.ScaleProperty)
                      .To(1.0)
                      .WithSpring(ctx => ctx.Profile.Release);

            // ---- the selection plate: filled from the side the choice came from ----------------------
            motion.OnChanged(SukiSideMenuItem.IsPlateActiveProperty)
                  .Where(ctx => ctx.Control.IsPlateActive)
                  .Do(ctx => Orient(ctx.Control, incoming: true))
                  .Then()
                  .Animate(plate, SukiMotion.ScaleYProperty)
                      .To(1.0)
                      .WithSpring(ctx => ctx.Profile.PlateGrow)
                      .Delay(HandoffDelay)
                  .And()
                  .Animate(plate, Visual.OpacityProperty)
                      .To(1.0)
                      .Over(ctx => ctx.Profile.PlateFade)
                      .Delay(HandoffDelay);

            // ... and drained toward the row that takes over.
            motion.OnChanged(SukiSideMenuItem.IsPlateActiveProperty)
                  .Where(ctx => !ctx.Control.IsPlateActive)
                  .Do(ctx => Orient(ctx.Control, incoming: false))
                  .Then()
                  .Animate(plate, SukiMotion.ScaleYProperty)
                      .To(Drained)
                      .WithSpring(ctx => ctx.Profile.PlateDrain)
                  .And()
                  .Animate(plate, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.PlateFade)
                      .Delay(ctx => ctx.Profile.PlateFade / 2);

            // ---- the content of the row: icon and title ----------------------------------------------
            // A chosen row of an open menu slides its title over the icon, which shrinks away; in a
            // collapsed menu the icon swells instead. The pointer also swells the icon.
            motion.OnChanged(SukiSideMenuItem.IsPlateActiveProperty)
                  .OrChanged(SukiSideMenuItem.IsContentMovableProperty)
                  .OrChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .OrChanged(SukiSideMenuItem.IsHeaderPointerOverProperty)
                  .Animate(icon, SukiMotion.TranslateXProperty)
                      .To(ctx => IconOffset(ctx.Control, ctx.Profile))
                      .WithSpring(ctx => ctx.Profile.Pose)
                  .And()
                  .Animate(icon, SukiMotion.ScaleProperty)
                      .To(ctx => IconScale(ctx.Control, ctx.Profile))
                      .WithSpring(ctx => ctx.Profile.Pose)
                  .And()
                  .Animate(icon, Visual.OpacityProperty)
                      .To(ctx => IsShifted(ctx.Control) ? 0.0 : 1.0)
                      .Over(ctx => ctx.Profile.PlateFade)
                  .And()
                  .Animate(titleBox, SukiMotion.TranslateXProperty)
                      .To(ctx => IconOffset(ctx.Control, ctx.Profile))
                      .WithSpring(ctx => ctx.Profile.Pose);

            // ---- a new compactness level moves the icon, the title and the compact row ---------------
            motion.OnCompactnessChanged()
                  .Set(row, SukiMotion.TranslateXProperty).To(ctx => RowOffset(ctx.Control, ctx.Profile))
                  .And()
                  .Set(icon, SukiMotion.TranslateXProperty).To(ctx => IconOffset(ctx.Control, ctx.Profile))
                  .And()
                  .Set(titleBox, SukiMotion.TranslateXProperty).To(ctx => IconOffset(ctx.Control, ctx.Profile));

            // ---- the pane opens or closes: the row slides with it ---------------------------------------
            // The row keeps the width of the open pane; closing the pane only shifts it left until
            // its icon sits in the middle of what is left, so the icon travels with the pane's edge,
            // on the same curve, and nothing in the row ever reflows.
            motion.OnChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .Animate(row, SukiMotion.TranslateXProperty)
                      .To(ctx => RowOffset(ctx.Control, ctx.Profile))
                      .Over(ctx => ctx.Profile.PaneDuration)
                      .WithEasing(ctx => ctx.Profile.PaneEase);

            // ... titles come in once there is room for them,
            motion.OnChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .Where(ctx => ctx.Control.IsTopMenuExpanded)
                  .Do(ctx => SetTitlesVisible(ctx.Control, true))
                  .Then()
                  .Animate(header, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(HoverEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(header, SukiMotion.TranslateXProperty)
                      .From(ctx => ctx.Profile.RevealOffsetX).To(0.0)
                      .WithSpring(ctx => ctx.Profile.Child)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  // The title comes out of a very light blur, crisp before the end.
                  .Animate(header, SukiMotion.BlurProperty)
                      .From(ctx => ctx.Profile.RevealBlur).To(0.0)
                      .Over(ctx => ctx.Profile.RevealDuration/1.5)
                      .WithEasing(HoverEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(side, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(HoverEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay)
                  .And()
                  .Animate(chevronHost, Visual.OpacityProperty)
                      .From(0.0).To(1.0)
                      .Over(ctx => ctx.Profile.RevealDuration)
                      .WithEasing(HoverEasing)
                      .Delay(ctx => ctx.Profile.RevealDelay);

            // ... and leave first when it closes, so nothing is cut by the narrowing pane.
            motion.OnChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .Where(ctx => !ctx.Control.IsTopMenuExpanded)
                  .Animate(header, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .And()
                  .Animate(header, SukiMotion.TranslateXProperty)
                      .To(ctx => ctx.Profile.RevealOffsetX)
                      .Over(ctx => ctx.Profile.HideDuration)
                      .WithEasing(HoverEasing)
                  .And()
                  // ... dissolving into the same light blur it comes out of.
                  .Animate(header, SukiMotion.BlurProperty)
                      .To(ctx => ctx.Profile.RevealBlur)
                      .Delay(ctx => ctx.Profile.HideDuration -( ctx.Profile.HideDuration/1.5))
                      .Over(ctx => ctx.Profile.HideDuration/1.5)
                      .WithEasing(HoverEasing)
                  .And()
                  .Animate(side, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .And()
                  .Animate(chevronHost, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.HideDuration)
                  .Then()
                  .Do(ctx => SetTitlesVisible(ctx.Control, false))
                  .And()
                  .Set(header, SukiMotion.BlurProperty).To(0.0); // drop the blur shader once it is hidden

            // ---- sub-menus: the chevron turns, the list unrolls, its items arrive in turn --------------
            motion.OnChanged(TreeViewItem.IsExpandedProperty)
                  .Animate(chevron, SukiMotion.RotateProperty)
                      .To(ctx => ctx.Control.IsExpanded ? 180.0 : 0.0)
                      .Over(ctx => ctx.Profile.RotateDuration)
                      .WithEasing(ctx => ctx.Profile.RotateEase);

            motion.OnChanged(TreeViewItem.IsExpandedProperty)
                  .Where(ctx => ShowChildren(ctx.Control))
                  .OrChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .Where(ctx => ShowChildren(ctx.Control))
                  .Do(ctx => SetChildrenVisible(ctx.Control, true))
                  .Then()
                  .Animate(children, Layoutable.MaxHeightProperty)
                      .From(0.0)
                      .To(ctx => NaturalChildrenHeight(ctx.Control))
                      .Over(ctx => ctx.Profile.UnrollDuration)
                      .WithEasing(ctx => ctx.Profile.UnrollEase)
                  .And()
                  .Cascade(ctx => ChildItems(ctx.Control), item => item
                      .Animate(Visual.OpacityProperty)
                          .From(0.0).To(1.0)
                          .WithSpring(ctx => ctx.Profile.Child)
                      .And()
                      .Animate(SukiMotion.TranslateXProperty)
                          .From(ctx => ctx.Profile.ChildOffsetX).To(0.0)
                          .WithSpring(ctx => ctx.Profile.Child))
                      .WithStagger(ctx => ctx.Profile.ChildStagger)
                      .WithInitialDelay(ctx => ctx.Profile.ChildInitialDelay)
                      .OnlyWhenFewerThan(12)
                      .CompleteIfInterrupted()
                  .Then()
                  .Set(children, Layoutable.MaxHeightProperty).To(double.PositiveInfinity);

            motion.OnChanged(TreeViewItem.IsExpandedProperty)
                  .Where(ctx => !ShowChildren(ctx.Control))
                  .OrChanged(SukiSideMenuItem.IsTopMenuExpandedProperty)
                  .Where(ctx => !ShowChildren(ctx.Control))
                  .Animate(children, Layoutable.MaxHeightProperty)
                      .From(ctx => ChildrenHeight(ctx.Control))
                      .To(0.0)
                      .Over(ctx => ctx.Profile.UnrollDuration)
                      .WithEasing(ctx => ctx.Profile.UnrollEase)
                  .Then()
                  .Do(ctx => SetChildrenVisible(ctx.Control, false));

            // ---- the search folds a row away, and unfolds it back --------------------------------------
            motion.OnChanged(SukiSideMenuItem.IsFilteredProperty)
                  .Where(ctx => ctx.Control.IsFiltered)
                  .Animate(row, Layoutable.MaxHeightProperty)
                      .From(ctx => RowHeight(ctx.Control))
                      .To(0.0)
                      .Over(ctx => ctx.Profile.FilterDuration)
                      .WithEasing(ctx => ctx.Profile.FilterEase)
                  .And()
                  .Animate(row, Layoutable.MarginProperty)
                      .To(ctx => FilteredMargin(ctx.Control))
                      .Over(ctx => ctx.Profile.FilterDuration)
                      .WithEasing(ctx => ctx.Profile.FilterEase)
                  .And()
                  .Animate(row, Visual.OpacityProperty)
                      .To(0.0)
                      .Over(ctx => ctx.Profile.PlateFade);

            motion.OnChanged(SukiSideMenuItem.IsFilteredProperty)
                  .Where(ctx => !ctx.Control.IsFiltered)
                  .Animate(row, Layoutable.MaxHeightProperty)
                      .To(ctx => NaturalRowHeight(ctx.Control))
                      .Over(ctx => ctx.Profile.FilterDuration)
                      .WithEasing(ctx => ctx.Profile.FilterEase)
                  .And()
                  .Animate(row, Layoutable.MarginProperty)
                      .To(ctx => RowMargin(ctx.Control))
                      .Over(ctx => ctx.Profile.FilterDuration)
                      .WithEasing(ctx => ctx.Profile.FilterEase)
                  .And()
                  .Animate(row, Visual.OpacityProperty)
                      .To(1.0)
                      .Over(ctx => ctx.Profile.PlateFade)
                  .Then()
                  .Set(row, Layoutable.MaxHeightProperty).To(double.PositiveInfinity)
                  .And()
                  // back to the template's own margin, so a later compactness level reaches it
                  .Do(ctx => ctx.Control.Row?.ClearValue(Layoutable.MarginProperty));

            // ---- detached or disabled: back to rest -----------------------------------------------------
            motion.OnDisabled()
                  .Set(row, SukiMotion.ScaleProperty).To(1.0);
        }

        // ---- the state the poses read ------------------------------------------------------------------

        /// <summary>The title has taken the place of the icon: a chosen row of an open menu.</summary>
        private static bool IsShifted(SukiSideMenuItem item) =>
            item.IsPlateActive && item.IsContentMovable && item.IsTopMenuExpanded;

        /// <summary>How far the title slides to take the icon's place: the icon and the gap before the title.</summary>
        private static double Shift(SukiSideMenuItem item)
        {
            var iconWidth = item.Part("PART_Icon") is { Width: > 0 } icon ? icon.Width : 24.0;
            var gap = item.Part("PART_Header")?.Margin.Left ?? 15.0;
            return iconWidth + gap;
        }

        private static double IconOffset(SukiSideMenuItem item, SukiSideMenuProfile p) =>
            IsShifted(item) ? -Shift(item) : 0.0;

        private static double IconScale(SukiSideMenuItem item, SukiSideMenuProfile p)
        {
            if (IsShifted(item))
                return p.HiddenIconScale;
            var scale = item.IsPlateActive && item.IsContentMovable ? p.SelectedIconScale : 1.0;
            return item.IsHeaderPointerOver ? scale * p.HoverIconScale : scale;
        }

        /// <summary>How far the row sits from its open position: nothing when the pane is open;
        /// when it is compact, enough to centre the icon in the pane's compact width.</summary>
        private static double RowOffset(SukiSideMenuItem item, SukiSideMenuProfile p)
        {
            if (item.IsTopMenuExpanded || item.Row is not { } row)
                return 0.0;
            var iconWidth = item.Part("PART_Icon") is { Width: > 0 } icon ? icon.Width : 24.0;
            var padding = item.Part("PART_Content") is Border content ? content.Padding.Left : 30.0;
            var open = RowMargin(item).Left + padding;             // where the icon sits in an open pane
            var compact = (item.GetCompactness<double>(SukiSideMenuMotion.CompactWidthKey) - iconWidth) / 2.0; // ... and in a compact one
            return compact - open;
        }

        private static TimeSpan HandoffDelay<TArgs>(MotionContext<SukiSideMenuItem, SukiSideMenuProfile, TArgs> ctx)
            where TArgs : EventArgs =>
            ctx.Control.SelectionFlow == 0 ? TimeSpan.Zero : ctx.Profile.PlateHandoffDelay;

        /// <summary>Sets the plate's origin on the side the selection comes from (or goes to). Only
        /// while the plate is at rest: a plate in flight keeps the origin it is turning around.</summary>
        private static void Orient(SukiSideMenuItem item, bool incoming)
        {
            if (item.Part("PART_SelectionPlate") is not { } plate)
                return;
            var scale = (plate.RenderTransform?.Value ?? Matrix.Identity).M22;
            if (Math.Abs(scale - 1.0) > 0.03 && Math.Abs(scale - Drained) > 0.03)
                return;
            var y = item.SelectionFlow switch
            {
                1 => incoming ? 0.0 : 1.0,
                -1 => incoming ? 1.0 : 0.0,
                _ => 0.5,
            };
            plate.RenderTransformOrigin = new RelativePoint(0.5, y, RelativeUnit.Relative);
        }

        // ---- sub-menus --------------------------------------------------------------------------------------

        private static bool ShowChildren(SukiSideMenuItem item) =>
            item.IsExpanded && item.IsTopMenuExpanded && item.ItemCount > 0;

        private static IEnumerable<Visual> ChildItems(SukiSideMenuItem item) =>
            item.Part("PART_ItemsPresenter") is ItemsPresenter { Panel: { } panel }
                ? panel.Children
                : Enumerable.Empty<Visual>();

        /// <summary>The height the list needs to be fully shown, measured with the room it will have.</summary>
        private static double NaturalChildrenHeight(SukiSideMenuItem item)
        {
            if (item.Part("PART_ItemsPresenter") is not { } presenter)
                return 0.0;
            var width = item.Bounds.Width > 0 ? item.Bounds.Width : double.PositiveInfinity;
            presenter.Measure(new Size(width, double.PositiveInfinity));
            return presenter.DesiredSize.Height;
        }

        private static double ChildrenHeight(SukiSideMenuItem item) =>
            item.Part("PART_ChildrenHost")?.Bounds.Height ?? 0.0;

        private static void SetChildrenVisible(SukiSideMenuItem item, bool visible)
        {
            if (item.Part("PART_ChildrenHost") is { } host)
                host.IsVisible = visible;
        }

        // ---- titles and filtering --------------------------------------------------------------------------

        private static void SetTitlesVisible(SukiSideMenuItem item, bool visible)
        {
            foreach (var name in new[] { "PART_Header", "PART_SideDisplay", "PART_ChevronHost" })
                if (item.Part(name) is { } part)
                    part.IsVisible = visible;
        }

        private static double RowHeight(SukiSideMenuItem item) => item.Row?.Bounds.Height ?? 0.0;

        /// <summary>The height of the row's content, measured without the row's own ceiling.</summary>
        private static double NaturalRowHeight(SukiSideMenuItem item)
        {
            if (item.Part("PART_Content") is not { } content)
                return 0.0;
            var width = item.Row is { Bounds.Width: > 0 } row ? row.Bounds.Width : double.PositiveInfinity;
            content.Measure(new Size(width, double.PositiveInfinity));
            return content.DesiredSize.Height;
        }

        /// <summary>Visibility the motion owns (not bindings, which would fight the fades) and
        /// the placement of everything that depends on the state at start.</summary>
        private static void Place(SukiSideMenuItem item)
        {
            SetTitlesVisible(item, item.IsTopMenuExpanded);
            SetChildrenVisible(item, ShowChildren(item));
            if (item.IsFiltered && item.Row is { } row)
                row.Margin = FilteredMargin(item); // an unfiltered row keeps the margin its template gives it
            if (item.Part("PART_SelectionPlate") is { } plate)
                plate.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        }
    }
}

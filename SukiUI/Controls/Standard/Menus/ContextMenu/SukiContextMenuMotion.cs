using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// The ContextMenu flavor of the popup motion — the same feel as the drop-downs
    /// (<see cref="SukiPopupFeel"/>), a different lifecycle: Avalonia builds and owns the real
    /// popup (ContextMenu.Open(), light dismiss), so the popup is the menu's logical parent and
    /// the close is held through the collapse by cancelling ContextMenu.Closing. A deactivated
    /// window closes the menu at once. The preset is fixed to <see cref="SukiPopupPreset.ContextMenu"/>.
    /// </summary>
    public sealed class SukiContextMenuMotion : MotionBehavior<ContextMenu, SukiPopupProfile>
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            RegisterEnable<SukiContextMenuMotion, ContextMenu>();

        public static bool GetEnable(ContextMenu element) => element.GetValue(EnableProperty);
        public static void SetEnable(ContextMenu element, bool value) => element.SetValue(EnableProperty, value);

        protected override SukiPopupProfile GetProfile(ContextMenu control) =>
            SukiAnimationTheme.For(control).Popup[SukiPopupPreset.ContextMenu];

        protected override void Configure(MotionBuilder<ContextMenu, SukiPopupProfile> motion)
        {
            var popup = motion.Popup()
                .ResolvedBy(menu => menu.GetLogicalParent() as Popup)
                .RootPart("PART_LayoutTransform")
                .ItemsPresenterPart("PART_ItemsPresenter")
                .OpenWhen(MenuBase.IsOpenProperty)
                .CloseHostWith(menu => menu.Close())
                .HoldOpenThroughCollapse();

            SukiPopupFeel.Apply(popup);
        }
    }
}

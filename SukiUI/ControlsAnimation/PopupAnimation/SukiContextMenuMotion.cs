 using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside this namespace the simple name "Motion" would bind to the SIBLING NAMESPACE
    // SukiUI.Motion before any outer using is consulted — same alias as SukiPopupMotion.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The ContextMenu flavor of the popup motion — same feel as the ComboBox drop-down
    /// (<see cref="SukiPopupMotion"/>, the choreographies shared through
    /// <see cref="SukiPopupChoreographies"/>), different lifecycle: Avalonia builds and
    /// owns the real popup (ContextMenu.Open(), light-dismiss), so the popup resolves as
    /// the host's logical parent and the animated root as the host's own template part;
    /// the close is held through the collapse by cancelling ContextMenu.Closing (raised
    /// synchronously by Popup.CloseCore) — the real IsOpen=false flips only at settle. A
    /// deactivated window closes instantly (the ComboBox rule); outside presses need no
    /// wiring (light-dismiss owns them). The preset is fixed to
    /// <see cref="SukiPopupPreset.ContextMenu"/> — an attached Preset will earn its place
    /// when a second menu feel exists.
    /// </summary>
    public class SukiContextMenuMotion : SukiMotion<SukiContextMenuMotion>
    {
        /// <summary>
        /// The popup behavior description. Null (Enable = logged no-op) on anything that is
        /// not a ContextMenu.
        /// </summary>
        internal override Mover? Attach(AvaloniaObject owner)
        {
            if (owner is not ContextMenu menu)
            {
                Debug.WriteLine($"SukiContextMenuMotion: '{owner.GetType().Name}' is not a ContextMenu — Enable ignored.");
                return null;
            }

            // Per-transition profile snapshot: every factory resolves when its program
            // starts, so a live SukiAnimationTheme switch applies to the NEXT transition.
            SukiPopupProfile P() => SukiAnimationTheme.Current.Popup[SukiPopupPreset.ContextMenu];
            bool HostIsOpen() => menu.IsOpen;

            // The real popup is code-built by ContextMenu.Open(), which parents the menu
            // in logically — resolve it as the host's logical parent; the animated root
            // and the items are the host's own template parts (the Suki ContextMenu
            // template already follows the PART_ convention).
            var popup = Motion.For(menu).Popup(
                resolvePopup: () => menu.GetLogicalParent() as Popup,
                resolveRoot: _ => menu.GetTemplateDescendants()
                    .OfType<Control>()
                    .FirstOrDefault(c => c.Name == "PART_LayoutTransform"),
                resolveItems: root => root?.GetLogicalDescendants()
                    .OfType<ItemsPresenter>()
                    .FirstOrDefault(i => i.Name == "PART_ItemsPresenter"),
                isHostOpen: HostIsOpen);

            // The settle close must pass through the Closing interception below — the
            // flag scopes the synchronous flip inside Popup.CloseCore's Closing raise.
            bool engineClosing = false;
            void SettleClose()
            {
                engineClosing = true;
                try { popup.Hide(); }
                finally { engineClosing = false; }
            }

            var (open, close, cascade) = SukiPopupChoreographies.Build(popup, P, SettleClose);

            // Popup lifecycle guards owned by the engine, decisions owned here.
            popup.SafetyReopen = () => popup.Play(open);
            popup.AbnormalClose += menu.Close;
            popup.Stopped += cascade.Reset;

            void Close()
            {
                if (popup.IsOpen)
                {
                    // Items stop cascading and rest at their normal pose while the popup
                    // collapses (the collapse shows the menu — it must be fully visible).
                    cascade.Reset();
                    popup.Play(close);
                }
                // Already closed (deactivated window): nothing animates a hidden popup.
            }

            // The host's window through the popup's placement target (the menu's own
            // TopLevel is the PopupRoot, not the window). ponytail: IsActive at Closing
            // time is the deactivation probe — a race would only animate ~150ms early.
            bool HostWindowActive() =>
                menu.GetLogicalParent() is not Popup { PlacementTarget: { } target }
                || TopLevel.GetTopLevel(target) is not Window window
                || window.IsActive;

            return new Mover(menu)
                .OnPropertyChanged(MenuBase.IsOpenProperty, () => popup.Play(open), when: true)
                // Safety net once PopupClosed set IsOpen=false: the popup is already
                // hidden — Close() no-ops.
                .OnPropertyChanged(MenuBase.IsOpenProperty, Close, when: false)
                .OnSignal(
                    (_, fire) =>
                    {
                        void HoldThroughCollapse(object? sender, CancelEventArgs e)
                        {
                            // Alt-tab / focus another app: close instantly — animating over
                            // a deactivated window would be wrong. Otherwise hold the popup
                            // open through the collapse and play it.
                            e.Cancel = !engineClosing && HostWindowActive() && popup.IsOpen;
                            if (e.Cancel)
                                fire();
                        }
                        menu.Closing += HoldThroughCollapse;
                        return Unwire.On(() => menu.Closing -= HoldThroughCollapse);
                    },
                    Close)
                // No outside-press / deactivation triggers here: light-dismiss owns
                // dismissal, and the deactivated-window close passes through above.
                .OnDispose(popup.Dispose);
        }
    }
}

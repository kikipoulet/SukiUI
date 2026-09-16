using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SukiUI.Motion;

namespace SukiUI.ControlsAnimation
{
    // Inside this namespace the simple name "Motion" would bind to the SIBLING NAMESPACE
    // SukiUI.Motion (a member of SukiUI) before any outer using is consulted — this alias,
    // declared in the namespace body, restores the class binding so descriptions read
    // Motion.Popup(...) exactly as written in Plan.md.
    using Motion = SukiUI.Motion.Motion;

    /// <summary>
    /// The template-popup behavior described declaratively over the SukiUI.Motion engine
    /// (see SukiUI.Motion/Plan.md): open = springs to full scale + opacity fade +
    /// velocity-driven motion blur + staggered item cascade; close = partial collapse
    /// springs + dissolve blur, the real IsOpen=false flipping only at settle. Fully
    /// configured from XAML through attached properties — Enable (from
    /// <see cref="SukiMotion{TSelf}"/>), Preset, PopupPart, and the host's OpenProperty —
    /// profile resolved per open/close through <see cref="SukiAnimationTheme"/> so a live
    /// switch applies to the NEXT transition. Contract: a settable bool-typed open
    /// property on the host and a named template Popup the motion owns (no binding on its
    /// IsOpen); root and items parts follow the PART_LayoutTransform /
    /// PART_ItemsPresenter convention.
    /// </summary>
    public class SukiPopupMotion : SukiMotion<SukiPopupMotion>
    {
        public static readonly AttachedProperty<SukiPopupPreset> PresetProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, SukiPopupPreset>(
                "Preset", SukiPopupPreset.ComboBox);

        public static SukiPopupPreset GetPreset(TemplatedControl element) => element.GetValue(PresetProperty);
        public static void SetPreset(TemplatedControl element, SukiPopupPreset value) => element.SetValue(PresetProperty, value);

        /// <summary>The name of the template Popup part the motion owns — the engine
        /// becomes the sole writer of its IsOpen (remove any template binding on it).
        /// Defaults to PART_SukiPopup.</summary>
        public static readonly AttachedProperty<string> PopupPartProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, string>(
                "PopupPart", "PART_SukiPopup");

        public static string GetPopupPart(TemplatedControl element) => element.GetValue(PopupPartProperty);
        public static void SetPopupPart(TemplatedControl element, string value) => element.SetValue(PopupPartProperty, value);

        /// <summary>The host's open/close property — watched AND written (outside-press,
        /// deactivation, abnormal-close sync). A settable bool-typed AvaloniaProperty,
        /// referenced from XAML as {x:Static MenuItem.IsSubMenuOpenProperty}. Replaces
        /// the old per-type adapter registry: new hosts are pure XAML configuration.</summary>
        public static readonly AttachedProperty<AvaloniaProperty> OpenPropertyProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, AvaloniaProperty>(
                "OpenProperty");

        public static AvaloniaProperty GetOpenProperty(TemplatedControl element) => element.GetValue(OpenPropertyProperty);
        public static void SetOpenProperty(TemplatedControl element, AvaloniaProperty value) => element.SetValue(OpenPropertyProperty, value);

        /// <summary>Any press inside the host's window closes the popup — the ComboBox
        /// drop-down rule (the drop-down opens on release, so its own press is never seen
        /// open by this guard). Hosts whose interaction logic ALREADY dismisses on outside
        /// press must opt out: menus open on PointerPressed and Avalonia's menu handler
        /// owns the outside dismissal — this guard would see the just-opened host within
        /// the same event and instantly close it (no popup ever visible).</summary>
        public static readonly AttachedProperty<bool> CloseOnOutsidePressProperty =
            AvaloniaProperty.RegisterAttached<SukiPopupMotion, TemplatedControl, bool>(
                "CloseOnOutsidePress", true);

        public static bool GetCloseOnOutsidePress(TemplatedControl element) => element.GetValue(CloseOnOutsidePressProperty);
        public static void SetCloseOnOutsidePress(TemplatedControl element, bool value) => element.SetValue(CloseOnOutsidePressProperty, value);

        /// <summary>
        /// The popup behavior description. Null (Enable = logged no-op) on hosts without a
        /// settable bool OpenProperty.
        /// </summary>
        internal override Mover? Attach(AvaloniaObject owner)
        {
            if (owner is not TemplatedControl element)
            {
                Debug.WriteLine($"SukiPopupMotion: '{owner.GetType().Name}' is not a TemplatedControl — Enable ignored.");
                return null;
            }

            var openProperty = GetOpenProperty(element);
            if (openProperty is null || openProperty.PropertyType != typeof(bool))
            {
                Debug.WriteLine($"SukiPopupMotion: '{element.GetType().Name}' needs a settable bool OpenProperty — Enable ignored.");
                return null;
            }

            // Per-gesture profile snapshot: every factory below resolves when its program
            // starts, so a live SukiAnimationTheme switch applies to the NEXT transition,
            // never mid-flight (the derived blur values resolve per frame — cosmetic).
            SukiPopupProfile P() => SukiAnimationTheme.Current.Popup[GetPreset(element)];
            bool HostIsOpen() => (bool)element.GetValue(openProperty);
            void SetOpen(bool open) => element.SetValue(openProperty, open);

            var popup = Motion.For(element).Popup(
                popupPart: GetPopupPart(element),
                rootPart: "PART_LayoutTransform",
                itemsPart: "PART_ItemsPresenter",
                isHostOpen: HostIsOpen);

            var (open, close, cascade) = SukiPopupChoreographies.Build(popup, P, popup.Hide);

            // Popup lifecycle guards owned by the engine, decisions owned here.
            popup.SafetyReopen = () => popup.Play(open);
            popup.AbnormalClose += () => SetOpen(false);
            popup.Stopped += cascade.Reset;

            void Close()
            {
                if (popup.IsOpen)
                {
                    // Items stop cascading and rest at their normal pose while the popup
                    // collapses (the collapse shows the list — it must be fully visible).
                    cascade.Reset();
                    popup.Play(close);
                }
                // Already closed (deactivated window): nothing animates a hidden popup —
                // InstantClose and the abnormal paths have already rested everything.
            }

            var mover = new Mover(element)
                .OnPropertyChanged(openProperty, () => popup.Play(open), when: true)
                .OnPropertyChanged(openProperty, Close, when: false);

            // Any press inside the main window closes the drop-down (presses inside
            // the popup never bubble there — it is its own top level). Opt-out for hosts
            // whose interaction logic already dismisses on outside press (menus).
            if (GetCloseOnOutsidePress(element))
            {
                mover.OnEvent(InputElement.PointerPressedEvent,
                    () =>
                    {
                        if (HostIsOpen())
                            SetOpen(false); // animated close via the property change
                    },
                    handledEventsToo: true,
                    source: Mover.TopLevelOf(element));
            }

            // Alt-tab / focus another app: the popup must not linger over a deactivated
            // window, and animating there would be wrong — close instantly.
            return mover
                .OnWindowDeactivated(() =>
                {
                    popup.InstantClose();
                    if (HostIsOpen())
                        SetOpen(false);
                })
                .OnDispose(popup.Dispose);
        }
    }

    /// <summary>
    /// The popup feel — the open/close choreographies shared by every popup motion flavor
    /// (SukiPopupMotion, SukiContextMenuMotion): open = springs to full scale + opacity
    /// fade + velocity-driven motion blur + staggered item cascade; close = partial
    /// collapse springs + dissolve blur, the settle action (the real popup close) owned by
    /// each flavor's lifecycle. The profile accessor resolves per transition, so a live
    /// SukiAnimationTheme switch applies to the NEXT transition, never mid-flight (the
    /// derived blur values resolve per frame — cosmetic).
    /// </summary>
    internal static class SukiPopupChoreographies
    {
        internal static (Choreography Open, Choreography Close, CascadeProgram Cascade) Build(
            PopupHandle popup, Func<SukiPopupProfile> profile, Action onSettle)
        {
            var (x, y, o, blur) = (popup.Root.ScaleX, popup.Root.ScaleY, popup.Root.Opacity, popup.Root.Blur);

            // Show = vrai Popup.IsOpen=true ; les From du bloc sont écrits AVANT (pas de
            // flash) et ignorés si le canal est en vol (reopen mid-collapse = reprise
            // pose + vélocité).
            var openX = x.From(() => profile().ClosedScaleX).To(1.0)
                .Spring(() => new Spring(profile().OpenSpringOmega, profile().OpenSpringDecay));
            var openY = y.From(() => profile().ClosedScaleY).To(1.0)
                .Spring(() => new Spring(profile().OpenSpringOmega, profile().OpenSpringDecay));
            var openOpacity = o.From(0.0).To(1.0).Over(() => profile().OpenOpacityDuration);

            // Canaux dérivés : rééchantillonnés chaque frame, meurent avec le programme.
            var motionBlur = new DerivedTrajectory(blur,
                () => Math.Min(
                    (Math.Abs(openX.Velocity) + Math.Abs(openY.Velocity)) * profile().BlurFactor,
                    profile().MaxBlurRadius),
                () => openX.Done && openY.Done);

            var cascade = new CascadeProgram(
                collect: popup.CollectItems,
                duration: () => TimeSpan.FromMilliseconds(profile().CascadeDurationMs),
                initialDelayMs: () => profile().CascadeInitialDelayMs,
                staggerMs: count => profile().CascadeStaggerMs(count),
                skipAbove: () => profile().CascadeMaxItems,
                itemBlur: () => profile().CascadeItemBlur,
                itemOffsetY: () => profile().CascadeItemOffsetY,
                itemScale: () => profile().CascadeItemScale);

            var open = popup.Show()
                .And(openX)
                .And(openY)
                .And(openOpacity)
                .And(motionBlur)
                .And(cascade);

            var closeX = x.To(() => profile().CloseScaleX)
                .Spring(() => new Spring(profile().CloseSpringOmega, profile().CloseSpringDecay));
            var closeY = y.To(() => profile().CloseScaleY)
                .Spring(() => new Spring(profile().CloseSpringOmega, profile().CloseSpringDecay));
            var closeOpacity = o.To(0.0).Over(() => profile().CloseOpacityDuration);
            var dissolveBlur = new DerivedTrajectory(blur,
                () => (1.0 - o.Value) * profile().CloseBlurRadius,
                () => closeOpacity.Done);

            var close = closeX
                .And(closeY)
                .And(closeOpacity)
                .And(dissolveBlur)
                .Then(onSettle); // IsOpen=false seulement au settle

            return (open, close, cascade);
        }
    }
}

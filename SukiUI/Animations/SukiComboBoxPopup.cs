using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;

namespace SukiUI.Animations
{

    public class SukiComboBoxPopup
    {
        // Spring: the button's frequency with heavier damping — zeta = decay / (2 * omega) = 0.65,
        // i.e. a single subtle overshoot (~7% of travel) and a very smooth settle.
        private const double SpringOmega = 16.0;
        private const double SpringDecay = 20.8;

        // Close-only feel: no motion blur, a collapse target only 40% of the open travel
        // (60% smaller translation), and a spring ~40% faster (omega / 0.6, same zeta).
        private const double ClosedScaleX = 0.92;
        private const double ClosedScaleY = 0.72;
        private const double CloseScaleX = 0.968;
        private const double CloseScaleY = 0.888;
        private const double CloseSpringOmega = 26.7;
        private const double CloseSpringDecay = 34.7;

        private static readonly TimeSpan OpenOpacityDuration = TimeSpan.FromMilliseconds(350);
        private static readonly TimeSpan CloseOpacityDuration = TimeSpan.FromMilliseconds(150);

        // Motion blur: radius proportional to the real expansion speed — opening only.
        private const double BlurFactor = 4.0;
        private const double MaxBlurRadius = 12.0;

        // Closing blur: a progressive dissolve that grows with the fade — 0 at the start
        // of the close, 20 at its end.
        private const double CloseBlurRadius = 20.0;

        // Item cascade: opacity only, skipped entirely for large lists. The stagger adapts
        // to the item count: 40ms apart for tiny lists, 20ms for big ones, interpolated
        // in between so the whole cascade lasts about as long regardless of the count.
        private const double CascadeInitialDelayMs = 150;
        private const double CascadeDurationMs = 250;
        private const int CascadeMaxItems = 20;

        private static double CascadeStaggerMs(int count) => count switch
        {
            < 4 => 40.0,
            > 10 => 20.0,
            _ => 40.0 - (count - 4) * (40.0 - 20.0) / (10.0 - 4.0)
        };

        private const double FrameIntervalMs = 16;

        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<SukiComboBoxPopup, ComboBox, bool>("Enable");

        private sealed class PopupState
        {
            public Popup? Popup;
            public Control? Root;
            public ItemsPresenter? ItemsPresenter;
            public DispatcherTimer? Timer;

            public bool Opening;
            public bool Closing;

            // Springs (scale X/Y) + opacity lerp.
            public double X = ClosedScaleX, Xv, XTarget = ClosedScaleX;
            public double Y = ClosedScaleY, Yv, YTarget = ClosedScaleY;
            public double Opacity, OpacityFrom, OpacityTarget;
            public DateTime OpacityStart;
            public TimeSpan OpacityDuration = OpenOpacityDuration;

            // Cascade.
            public bool CascadePending;
            public DateTime CascadeStart;
            public ComboBoxItem[] CascadeItems = Array.Empty<ComboBoxItem>();

            public BlurEffect? Effect;
            public DateTime LastTick;

            // Stored handlers so disable can unwire everything.
            public EventHandler<AvaloniaPropertyChangedEventArgs>? BoxPropertyChanged;
            public EventHandler<AvaloniaPropertyChangedEventArgs>? PopupPropertyChanged;
            public EventHandler? PopupOpened;

            // Outside-press dismissal wiring: the behavior owns the popup lifecycle.
            public TopLevel? SubscribedTopLevel;
            public EventHandler<PointerPressedEventArgs>? TopLevelPointerPressed;
            public EventHandler? TopLevelDeactivated;
        }

        private static readonly AttachedProperty<PopupState> StateProperty =
            AvaloniaProperty.RegisterAttached<SukiComboBoxPopup, ComboBox, PopupState>("State");

        static SukiComboBoxPopup()
        {
            EnableProperty.Changed.AddClassHandler<ComboBox>(OnEnableChanged);
        }

        public static bool GetEnable(ComboBox element) => element.GetValue(EnableProperty);
        public static void SetEnable(ComboBox element, bool value) => element.SetValue(EnableProperty, value);

        private static void OnEnableChanged(ComboBox box, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                var state = box.GetValue(StateProperty) ?? new PopupState();
                box.SetValue(StateProperty, state);

                state.BoxPropertyChanged = (_, args) => OnBoxPropertyChanged(box, state, args);
                box.PropertyChanged += state.BoxPropertyChanged;
                box.TemplateApplied += OnTemplateApplied;
                box.AttachedToVisualTree += OnBoxAttached;
                box.DetachedFromVisualTree += OnDetached;

                WireParts(box, state);
                WireTopLevel(box, state);
            }
            else
            {
                if (box.GetValue(StateProperty) is not { } state)
                    return;
                box.PropertyChanged -= state.BoxPropertyChanged;
                box.TemplateApplied -= OnTemplateApplied;
                box.AttachedToVisualTree -= OnBoxAttached;
                box.DetachedFromVisualTree -= OnDetached;
                UnwirePopup(state);
                UnwireTopLevel(state);
                StopTimer(state);
                state.Opening = state.Closing = false;
                ResetItems(state);
                ResetToClosedPose(state);
                // Keep the drop-down functional without animation.
                if (state.Popup is { } popup)
                    popup.IsOpen = box.IsDropDownOpen;
            }
        }

        private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            if (sender is ComboBox box && box.GetValue(StateProperty) is { } state)
            {
                StopTimer(state);
                state.Opening = state.Closing = false;
                state.CascadeItems = Array.Empty<ComboBoxItem>();
                state.CascadePending = false;
                WireParts(box, state);
            }
        }

        private static void WireParts(ComboBox box, PopupState state)
        {
            UnwirePopup(state);
            var children = box.GetTemplateChildren();
            state.Popup = children.OfType<Popup>().FirstOrDefault(p => p.Name == "PART_SukiPopup");

            if (state.Popup is { } popup)
            {
                state.PopupPropertyChanged = (_, args) => OnPopupPropertyChanged(box, state, args);
                popup.PropertyChanged += state.PopupPropertyChanged;
                state.PopupOpened = (_, _) => OnPopupOpened(box, state);
                popup.Opened += state.PopupOpened;

                // GetTemplateChildren does not reach INSIDE the popup: the animated root and
                // the items presenter live in the popup's content, so walk it directly. The
                // logical tree is intact even while the popup is closed.
                if (popup.Child is { } content && content.Name == "PART_LayoutTransform")
                {
                    state.Root = content;
                    state.ItemsPresenter = content.GetLogicalDescendants()
                        .OfType<ItemsPresenter>()
                        .FirstOrDefault(i => i.Name == "PART_ItemsPresenter");
                }
            }

            if (state.Root is { } root)
            {
                // Invisible until the first open: prevents a one-frame flash of the open popup.
                ResetToClosedPose(state);
                ApplyVisuals(state, 0);
            }
        }

        private static void UnwirePopup(PopupState state)
        {
            if (state.Popup is { } popup)
            {
                popup.PropertyChanged -= state.PopupPropertyChanged;
                popup.Opened -= state.PopupOpened;
            }
            state.Popup = null;
            state.Root = null;
            state.ItemsPresenter = null;
        }

        private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is not ComboBox box || box.GetValue(StateProperty) is not { } state)
                return;
            StopTimer(state);
            state.Opening = state.Closing = false;
            ResetItems(state);
            if (state.Popup is { IsOpen: true } popup)
                popup.IsOpen = false;
            ResetToClosedPose(state);
            UnwireTopLevel(state);
        }

        private static void OnBoxPropertyChanged(ComboBox box, PopupState state, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != ComboBox.IsDropDownOpenProperty)
                return;
            if (state.Popup is null || state.Root is null)
                WireParts(box, state); // fail-safe if the template was applied before enabling
            if (e.NewValue is true)
                OpenDropDown(box, state);
            else
                CloseDropDown(box, state);
        }

        private static void OpenDropDown(ComboBox box, PopupState state)
        {
            if (state.Popup is not { } popup)
                return;
            bool wasOpen = popup.IsOpen;
            if (wasOpen)
            {
                // Reopening mid-collapse: start from what is currently on screen.
                ReadCurrent(state);
            }
            else
            {
                // Fresh open: the control may still hold the previous open pose (an instant
                // close never writes the collapsed pose), so force it — and apply it BEFORE
                // the popup becomes visible to avoid a one-frame flash.
                ResetToClosedPose(state);
            }
            state.XTarget = 1.0;
            state.YTarget = 1.0;
            state.OpacityFrom = state.Opacity;
            state.OpacityTarget = 1.0;
            state.OpacityStart = DateTime.Now;
            state.OpacityDuration = OpenOpacityDuration;
            state.Opening = true;
            state.Closing = false;

            if (!wasOpen)
            {
                ApplyVisuals(state, 0);
                popup.IsOpen = true;
            }

            // Collected on the first tick: the popup content attaches only once IsOpen is set.
            state.CascadePending = true;
            StartTimer(box, state);
        }

        private static void CloseDropDown(ComboBox box, PopupState state)
        {
            if (state.Popup is not { } popup)
                return;
            if (!popup.IsOpen)
            {
                // The popup is already closed (deactivated window): stop animating a hidden
                // popup and rest at the closed pose, ready for the next open.
                StopTimer(state);
                state.Opening = state.Closing = false;
                ResetItems(state);
                ResetToClosedPose(state);
                return;
            }

            ReadCurrent(state);
            state.XTarget = CloseScaleX;
            state.YTarget = CloseScaleY;
            state.OpacityFrom = state.Opacity;
            state.OpacityTarget = 0.0;
            state.OpacityStart = DateTime.Now;
            state.OpacityDuration = CloseOpacityDuration;
            state.Opening = false;
            state.Closing = true;
            ResetItems(state); // items stop cascading and rest at their normal pose
            StartTimer(box, state);
        }

        private static void OnPopupPropertyChanged(ComboBox box, PopupState state, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Popup.IsOpenProperty)
                return;
            // The behavior is the only legitimate closer of the real popup, so this branch
            // means an abnormal close (window teardown and the like): sync instantly.
            if (e.NewValue is false && box.IsDropDownOpen)
                CloseInstantly(box, state);
        }

        private static void OnBoxAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is ComboBox box && box.GetValue(StateProperty) is { } state)
                WireTopLevel(box, state);
        }

        private static void WireTopLevel(ComboBox box, PopupState state)
        {
            if (state.SubscribedTopLevel != null || TopLevel.GetTopLevel(box) is not { } topLevel)
                return;
            state.SubscribedTopLevel = topLevel;
            // Any press inside the main window closes the drop-down. Presses inside the
            // popup never bubble here (the popup is its own top level), so no filtering is
            // needed. handledEventsToo: presses swallowed by other controls must still close.
            state.TopLevelPointerPressed = (_, _) =>
            {
                if (box.IsDropDownOpen)
                    box.IsDropDownOpen = false; // animated close via the property change
            };
            topLevel.AddHandler(InputElement.PointerPressedEvent, state.TopLevelPointerPressed,
                RoutingStrategies.Bubble, handledEventsToo: true);
            // Alt-tab / focus another app: the popup must not linger over a deactivated
            // window, and animating there would be wrong — close instantly.
            state.TopLevelDeactivated = (_, _) =>
            {
                if (box.IsDropDownOpen)
                    CloseInstantly(box, state);
            };
            if (topLevel is Window window)
                window.Deactivated += state.TopLevelDeactivated;
        }

        private static void UnwireTopLevel(PopupState state)
        {
            if (state.SubscribedTopLevel is { } topLevel)
            {
                if (state.TopLevelPointerPressed != null)
                    topLevel.RemoveHandler(InputElement.PointerPressedEvent, state.TopLevelPointerPressed);
                if (topLevel is Window window)
                    window.Deactivated -= state.TopLevelDeactivated;
            }
            state.SubscribedTopLevel = null;
            state.TopLevelPointerPressed = null;
            state.TopLevelDeactivated = null;
        }

        private static void CloseInstantly(ComboBox box, PopupState state)
        {
            StopTimer(state);
            state.Opening = state.Closing = false;
            ResetItems(state);
            if (state.Popup is { IsOpen: true } popup)
                popup.IsOpen = false;
            if (box.IsDropDownOpen)
                box.IsDropDownOpen = false; // CloseDropDown then no-ops (popup already closed)
            ResetToClosedPose(state);
        }

        private static void OnPopupOpened(ComboBox box, PopupState state)
        {
            // Safety net: if some path opened the popup without going through the property
            // change, still play the opening transition.
            if (box.IsDropDownOpen && !state.Opening && !state.Closing)
                OpenDropDown(box, state);
        }

        private static void Tick(ComboBox box, PopupState state)
        {
            double dt = Math.Min((DateTime.Now - state.LastTick).TotalSeconds, 0.05);
            state.LastTick = DateTime.Now;

            // The close spring is faster than the open spring; both share zeta = 0.65.
            double omega = state.Closing ? CloseSpringOmega : SpringOmega;
            double decay = state.Closing ? CloseSpringDecay : SpringDecay;
            StepSpring(ref state.X, ref state.Xv, state.XTarget, dt, omega, decay);
            StepSpring(ref state.Y, ref state.Yv, state.YTarget, dt, omega, decay);

            double opacityT = Math.Min(
                (DateTime.Now - state.OpacityStart).TotalMilliseconds / state.OpacityDuration.TotalMilliseconds, 1.0);
            state.Opacity = Lerp(state.OpacityFrom, state.OpacityTarget, opacityT);

            // Motion blur from the real spring velocity — opening only.
            double speed = Math.Abs(state.Xv) + Math.Abs(state.Yv);
            double blurRadius = state.Closing
                ? (1.0 - state.Opacity) * CloseBlurRadius
                : Math.Min(speed * BlurFactor, MaxBlurRadius);
            ApplyVisuals(state, blurRadius);

            StepCascade(state);

            bool settled =
                Math.Abs(state.X - state.XTarget) < 0.0005 && Math.Abs(state.Xv) < 0.02 &&
                Math.Abs(state.Y - state.YTarget) < 0.0005 && Math.Abs(state.Yv) < 0.02 &&
                opacityT >= 1.0 &&
                state.CascadeItems.Length == 0 && !state.CascadePending;

            if (!settled)
                return;

            state.X = state.XTarget;
            state.Y = state.YTarget;
            state.Xv = state.Yv = 0.0;
            state.Opacity = state.OpacityTarget;
            bool wasClosing = state.Closing;
            state.Opening = state.Closing = false;
            ApplyVisuals(state, 0);
            StopTimer(state);

            if (wasClosing && !box.IsDropDownOpen)
            {
                // The collapse finished: actually close the popup now.
                if (state.Popup is { } popup)
                    popup.IsOpen = false;
                ResetToClosedPose(state);
            }
        }

        private static void StepSpring(ref double x, ref double v, double target, double dt, double omega, double decay)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(dt / 0.008));
            double h = dt / steps;
            for (int i = 0; i < steps; i++)
            {
                double accel = -omega * omega * (x - target) - decay * v;
                v += accel * h;
                x += v * h;
            }
        }

        private static void StepCascade(PopupState state)
        {
            if (state.CascadePending)
            {
                state.CascadePending = false;
                var items = CollectItems(state);
                // Too many items: the cascade would drag on — show them all immediately.
                state.CascadeItems = items.Length < CascadeMaxItems ? items : Array.Empty<ComboBoxItem>();
                if (state.CascadeItems.Length > 0)
                {
                    foreach (var item in state.CascadeItems)
                        item.Opacity = 0;
                    state.CascadeStart = DateTime.Now;
                }
            }

            if (state.CascadeItems.Length == 0)
                return;

            // The cascade starts a moment after the popup itself has begun opening.
            double elapsed = (DateTime.Now - state.CascadeStart).TotalMilliseconds - CascadeInitialDelayMs;
            double stagger = CascadeStaggerMs(state.CascadeItems.Length);
            bool anyActive = false;
            for (int i = 0; i < state.CascadeItems.Length; i++)
            {
                double delay = i * stagger;
                double t = Math.Min(Math.Max((elapsed - delay) / CascadeDurationMs, 0.0), 1.0);
                state.CascadeItems[i].Opacity = t;
                if (t < 1.0)
                    anyActive = true;
            }
            if (!anyActive)
                state.CascadeItems = Array.Empty<ComboBoxItem>();
        }

        private static ComboBoxItem[] CollectItems(PopupState state)
        {
            if (state.ItemsPresenter is not { } presenter || presenter.Panel is not Panel panel)
                return Array.Empty<ComboBoxItem>();
            return panel.Children.OfType<ComboBoxItem>().ToArray();
        }

        private static void ResetItems(PopupState state)
        {
            foreach (var item in state.CascadeItems)
                item.Opacity = 1.0;
            state.CascadeItems = Array.Empty<ComboBoxItem>();
            state.CascadePending = false;
        }

        private static void ReadCurrent(PopupState state)
        {
            if (state.Root is not { } root)
                return;
            if (root.RenderTransform is ScaleTransform transform)
            {
                state.X = transform.ScaleX;
                state.Y = transform.ScaleY;
            }
            state.Opacity = root.Opacity;
        }

        private static void ApplyVisuals(PopupState state, double blurRadius)
        {
            if (state.Root is not { } root)
                return;

            if (root.RenderTransform is not ScaleTransform transform)
            {
                transform = new ScaleTransform(state.X, state.Y);
                root.RenderTransform = transform;
            }
            transform.ScaleX = state.X;
            transform.ScaleY = state.Y;
            root.Opacity = state.Opacity;

            if (blurRadius >= 0.5)
            {
                state.Effect ??= new BlurEffect();
                if (root.Effect != state.Effect)
                    root.Effect = state.Effect;
                state.Effect.Radius = blurRadius;
            }
            else if (state.Effect != null)
            {
                // Drop the effect entirely: no shader pass once the motion blur has dissipated.
                state.Effect = null;
                root.Effect = null;
            }
        }

        private static void ResetToClosedPose(PopupState state)
        {
            state.X = state.XTarget = ClosedScaleX;
            state.Y = state.YTarget = ClosedScaleY;
            state.Xv = state.Yv = 0.0;
            state.Opacity = state.OpacityFrom = state.OpacityTarget = 0.0;
        }

        private static void StartTimer(ComboBox box, PopupState state)
        {
            if (state.Timer != null)
                return; // already running: targets are updated in place (live retargeting)
            var timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(FrameIntervalMs)
            };
            state.LastTick = DateTime.Now;
            timer.Tick += (_, _) => Tick(box, state);
            state.Timer = timer;
            timer.Start();
        }

        private static void StopTimer(PopupState state)
        {
            state.Timer?.Stop();
            state.Timer = null;
        }

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;
    }
}

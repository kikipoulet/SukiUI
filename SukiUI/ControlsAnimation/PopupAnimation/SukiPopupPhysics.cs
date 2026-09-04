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

namespace SukiUI.ControlsAnimation
{
    /// <summary>
    /// How the engine reads and writes the open state of a popup host control. This is the
    /// single host-specific seam of the popup animation system: supporting a new control
    /// type (AutoCompleteBox, …) means adding one adapter here — everything else (springs,
    /// blur, cascade, lifecycle) is generic.
    /// </summary>
    internal interface ISukiPopupHost
    {
        /// <summary>The open/close property watched on the host (drives Open/Close).</summary>
        AvaloniaProperty OpenProperty { get; }

        bool IsOpen(TemplatedControl host);

        void SetOpen(TemplatedControl host, bool open);
    }

    internal sealed class ComboBoxPopupHost : ISukiPopupHost
    {
        public static readonly ComboBoxPopupHost Instance = new();

        public AvaloniaProperty OpenProperty => ComboBox.IsDropDownOpenProperty;

        public bool IsOpen(TemplatedControl host) => ((ComboBox)host).IsDropDownOpen;

        public void SetOpen(TemplatedControl host, bool open) => ((ComboBox)host).IsDropDownOpen = open;
    }

    internal static class SukiPopupHosts
    {
        /// <summary>
        /// Resolves the host adapter for a control type. Unsupported types return null —
        /// <c>SukiPopupAnimation.Enable</c> then no-ops (with a debug trace) instead of throwing.
        /// </summary>
        public static ISukiPopupHost? Resolve(TemplatedControl control) => control switch
        {
            ComboBox => ComboBoxPopupHost.Instance,
            _ => null,
        };
    }

    /// <summary>
    /// The complete popup open/close animation engine, shared by every control enabled
    /// through <see cref="SukiPopupAnimation"/> (see <see cref="SukiPopupProfile"/> for the
    /// calibrated feels), driven frame by frame by the single shared <see cref="SukiTicker"/>
    /// loop. One instance per host control, owning the whole lifecycle of its template
    /// popup (convention: <c>PART_SukiPopup</c> whose content root is
    /// <c>PART_LayoutTransform</c>, with an optional <c>PART_ItemsPresenter</c> for the
    /// item cascade):
    /// open: damped springs on scale X/Y to 1.0 + opacity lerp + motion blur proportional
    /// to the real expansion speed, then a staggered opacity cascade over the items;
    /// close: a ~40% faster spring toward a partial collapse target, a dissolution blur
    /// that grows with the fade — the real <c>Popup.IsOpen</c> is only flipped to false
    /// once the collapse has settled (the engine is the sole legitimate closer);
    /// reopening mid-collapse restarts from the pose currently on screen; outside presses
    /// (any press in the main window) close animated, window deactivation closes instantly.
    /// </summary>
    internal sealed class SukiPopupPhysics
    {
        private readonly TemplatedControl _host;
        private readonly SukiPopupProfile _profile;
        private readonly ISukiPopupHost _hostAdapter;

        private Popup? _popup;
        private Control? _root;
        private ItemsPresenter? _itemsPresenter;
        // One subscription to the shared SukiTicker loop (per TopLevel), never a
        // private DispatcherTimer: N open popups animate for the price of one callback.
        private IDisposable? _ticker;

        private bool _opening;
        private bool _closing;

        // Springs (scale X/Y) + opacity lerp.
        private double _x, _xv, _xTarget;
        private double _y, _yv, _yTarget;
        private double _opacity, _opacityFrom, _opacityTarget;
        private long _opacityStart;
        private TimeSpan _opacityDuration;

        // Cascade (opacity-staggered items; collected on the first open tick).
        private bool _cascadePending;
        private long _cascadeStart;
        private Control[] _cascadeItems = Array.Empty<Control>();

        private BlurEffect? _effect;
        private long _lastTick;

        // Outside-press dismissal wiring: the engine owns the popup lifecycle.
        private TopLevel? _subscribedTopLevel;

        internal SukiPopupPhysics(TemplatedControl host, SukiPopupProfile profile, ISukiPopupHost hostAdapter)
        {
            _host = host;
            _profile = profile;
            _hostAdapter = hostAdapter;
            _x = _xTarget = profile.ClosedScaleX;
            _y = _yTarget = profile.ClosedScaleY;
            _opacityDuration = profile.OpenOpacityDuration;

            _host.PropertyChanged += OnHostPropertyChanged;
            _host.TemplateApplied += OnTemplateApplied;
            _host.AttachedToVisualTree += OnHostAttached;
            _host.DetachedFromVisualTree += OnDetached;

            WireParts();
            WireTopLevel();
        }

        /// <summary>
        /// Disable/detach: unwire everything, reset poses and — the intended behavior of the
        /// original disable path — leave the popup functional without animation.
        /// </summary>
        internal void Dispose()
        {
            _host.PropertyChanged -= OnHostPropertyChanged;
            _host.TemplateApplied -= OnTemplateApplied;
            _host.AttachedToVisualTree -= OnHostAttached;
            _host.DetachedFromVisualTree -= OnDetached;

            var popup = _popup; // captured before UnwirePopup clears it
            UnwirePopup();
            UnwireTopLevel();
            StopTimer();
            _opening = _closing = false;
            ResetItems();
            ResetToClosedPose();
            // Keep the drop-down functional without animation.
            if (popup is { } p)
                p.IsOpen = _hostAdapter.IsOpen(_host);
        }

        // ---- Host wiring -----------------------------------------------------------

        private void OnHostPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != _hostAdapter.OpenProperty)
                return;
            if (_popup is null || _root is null)
                WireParts(); // fail-safe if the template was applied before enabling
            if (e.NewValue is true)
                Open();
            else
                Close();
        }

        private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            StopTimer();
            _opening = _closing = false;
            _cascadeItems = Array.Empty<Control>();
            _cascadePending = false;
            WireParts();
        }

        private void OnHostAttached(object? sender, VisualTreeAttachmentEventArgs e) => WireTopLevel();

        private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopTimer();
            _opening = _closing = false;
            ResetItems();
            if (_popup is { IsOpen: true } popup)
                popup.IsOpen = false;
            ResetToClosedPose();
            UnwireTopLevel();
        }

        // ---- Template parts --------------------------------------------------------

        private void WireParts()
        {
            UnwirePopup();
            var children = _host.GetTemplateChildren();
            _popup = children.OfType<Popup>().FirstOrDefault(p => p.Name == "PART_SukiPopup");

            if (_popup is { } popup)
            {
                popup.PropertyChanged += OnPopupPropertyChanged;
                popup.Opened += OnPopupOpened;

                // GetTemplateChildren does not reach INSIDE the popup: the animated root and
                // the items presenter live in the popup's content, so walk it directly. The
                // logical tree is intact even while the popup is closed.
                if (popup.Child is { } content && content.Name == "PART_LayoutTransform")
                {
                    _root = content;
                    _itemsPresenter = content.GetLogicalDescendants()
                        .OfType<ItemsPresenter>()
                        .FirstOrDefault(i => i.Name == "PART_ItemsPresenter");
                }
            }

            if (_root is { } root)
            {
                // Invisible until the first open: prevents a one-frame flash of the open popup.
                ResetToClosedPose();
                ApplyVisuals(0);
            }
        }

        private void UnwirePopup()
        {
            if (_popup is { } popup)
            {
                popup.PropertyChanged -= OnPopupPropertyChanged;
                popup.Opened -= OnPopupOpened;
            }
            _popup = null;
            _root = null;
            _itemsPresenter = null;
        }

        // ---- Open / close ----------------------------------------------------------

        private void Open()
        {
            if (_popup is not { } popup)
                return;
            bool wasOpen = popup.IsOpen;
            if (wasOpen)
            {
                // Reopening mid-collapse: start from what is currently on screen.
                ReadCurrent();
            }
            else
            {
                // Fresh open: the control may still hold the previous open pose (an instant
                // close never writes the collapsed pose), so force it — and apply it BEFORE
                // the popup becomes visible to avoid a one-frame flash.
                ResetToClosedPose();
            }
            _xTarget = 1.0;
            _yTarget = 1.0;
            _opacityFrom = _opacity;
            _opacityTarget = 1.0;
            _opacityStart = SukiTicker.Timestamp;
            _opacityDuration = _profile.OpenOpacityDuration;
            _opening = true;
            _closing = false;

            if (!wasOpen)
            {
                ApplyVisuals(0);
                popup.IsOpen = true;
            }

            // Collected on the first tick: the popup content attaches only once IsOpen is set.
            _cascadePending = true;
            StartTimer();
        }

        private void Close()
        {
            if (_popup is not { } popup)
                return;
            if (!popup.IsOpen)
            {
                // The popup is already closed (deactivated window): stop animating a hidden
                // popup and rest at the closed pose, ready for the next open.
                StopTimer();
                _opening = _closing = false;
                ResetItems();
                ResetToClosedPose();
                return;
            }

            ReadCurrent();
            _xTarget = _profile.CloseScaleX;
            _yTarget = _profile.CloseScaleY;
            _opacityFrom = _opacity;
            _opacityTarget = 0.0;
            _opacityStart = SukiTicker.Timestamp;
            _opacityDuration = _profile.CloseOpacityDuration;
            _opening = false;
            _closing = true;
            ResetItems(); // items stop cascading and rest at their normal pose
            StartTimer();
        }

        private void OnPopupPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Popup.IsOpenProperty)
                return;
            // The engine is the only legitimate closer of the real popup, so this branch
            // means an abnormal close (window teardown and the like): sync instantly.
            if (e.NewValue is false && _hostAdapter.IsOpen(_host))
                CloseInstantly();
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            // Safety net: if some path opened the popup without going through the property
            // change, still play the opening transition.
            if (_hostAdapter.IsOpen(_host) && !_opening && !_closing)
                Open();
        }

        private void CloseInstantly()
        {
            StopTimer();
            _opening = _closing = false;
            ResetItems();
            if (_popup is { IsOpen: true } popup)
                popup.IsOpen = false;
            if (_hostAdapter.IsOpen(_host))
                _hostAdapter.SetOpen(_host, false); // Close() then no-ops (popup already closed)
            ResetToClosedPose();
        }

        // ---- TopLevel (outside-press dismissal / deactivation) ---------------------

        private void WireTopLevel()
        {
            if (_subscribedTopLevel != null || TopLevel.GetTopLevel(_host) is not { } topLevel)
                return;
            _subscribedTopLevel = topLevel;
            // Any press inside the main window closes the drop-down. Presses inside the
            // popup never bubble here (the popup is its own top level), so no filtering is
            // needed. handledEventsToo: presses swallowed by other controls must still close.
            topLevel.AddHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed,
                RoutingStrategies.Bubble, handledEventsToo: true);
            // Alt-tab / focus another app: the popup must not linger over a deactivated
            // window, and animating there would be wrong — close instantly.
            if (topLevel is Window window)
                window.Deactivated += OnTopLevelDeactivated;
        }

        private void OnTopLevelPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_hostAdapter.IsOpen(_host))
                _hostAdapter.SetOpen(_host, false); // animated close via the property change
        }

        private void OnTopLevelDeactivated(object? sender, EventArgs e)
        {
            if (_hostAdapter.IsOpen(_host))
                CloseInstantly();
        }

        private void UnwireTopLevel()
        {
            if (_subscribedTopLevel is { } topLevel)
            {
                topLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopLevelPointerPressed);
                if (topLevel is Window window)
                    window.Deactivated -= OnTopLevelDeactivated;
            }
            _subscribedTopLevel = null;
        }

        // ---- Frame loop ------------------------------------------------------------

        private void Tick()
        {
            double dt = Math.Min(SukiTicker.ElapsedSeconds(_lastTick), 0.05);
            _lastTick = SukiTicker.Timestamp;

            // The close spring is faster than the open spring; both share zeta = 0.65.
            double omega = _closing ? _profile.CloseSpringOmega : _profile.OpenSpringOmega;
            double decay = _closing ? _profile.CloseSpringDecay : _profile.OpenSpringDecay;
            StepSpring(ref _x, ref _xv, _xTarget, dt, omega, decay);
            StepSpring(ref _y, ref _yv, _yTarget, dt, omega, decay);

            double opacityT = Math.Min(
                SukiTicker.ElapsedMilliseconds(_opacityStart) / _opacityDuration.TotalMilliseconds, 1.0);
            _opacity = Lerp(_opacityFrom, _opacityTarget, opacityT);

            // Motion blur from the real spring velocity — opening only.
            double speed = Math.Abs(_xv) + Math.Abs(_yv);
            double blurRadius = _closing
                ? (1.0 - _opacity) * _profile.CloseBlurRadius
                : Math.Min(speed * _profile.BlurFactor, _profile.MaxBlurRadius);
            ApplyVisuals(blurRadius);

            StepCascade();

            bool settled =
                Math.Abs(_x - _xTarget) < 0.0005 && Math.Abs(_xv) < 0.02 &&
                Math.Abs(_y - _yTarget) < 0.0005 && Math.Abs(_yv) < 0.02 &&
                opacityT >= 1.0 &&
                _cascadeItems.Length == 0 && !_cascadePending;

            if (!settled)
                return;

            _x = _xTarget;
            _y = _yTarget;
            _xv = _yv = 0.0;
            _opacity = _opacityTarget;
            bool wasClosing = _closing;
            _opening = _closing = false;
            ApplyVisuals(0);
            StopTimer();

            if (wasClosing && !_hostAdapter.IsOpen(_host))
            {
                // The collapse finished: actually close the popup now.
                if (_popup is { } popup)
                    popup.IsOpen = false;
                ResetToClosedPose();
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

        // ---- Item cascade ----------------------------------------------------------

        private void StepCascade()
        {
            if (_cascadePending)
            {
                _cascadePending = false;
                var items = CollectItems();
                // Too many items: the cascade would drag on — show them all immediately.
                _cascadeItems = items.Length < _profile.CascadeMaxItems ? items : Array.Empty<Control>();
                if (_cascadeItems.Length > 0)
                {
                    foreach (var item in _cascadeItems)
                        item.Opacity = 0;
                    _cascadeStart = SukiTicker.Timestamp;
                }
            }

            if (_cascadeItems.Length == 0)
                return;

            // The cascade starts a moment after the popup itself has begun opening.
            double elapsed = SukiTicker.ElapsedMilliseconds(_cascadeStart) - _profile.CascadeInitialDelayMs;
            double stagger = _profile.CascadeStaggerMs(_cascadeItems.Length);
            bool anyActive = false;
            for (int i = 0; i < _cascadeItems.Length; i++)
            {
                double delay = i * stagger;
                double t = Math.Min(Math.Max((elapsed - delay) / _profile.CascadeDurationMs, 0.0), 1.0);
                _cascadeItems[i].Opacity = t;
                if (t < 1.0)
                    anyActive = true;
            }
            if (!anyActive)
                _cascadeItems = Array.Empty<Control>();
        }

        private Control[] CollectItems()
        {
            if (_itemsPresenter is not { } presenter || presenter.Panel is not Panel panel)
                return Array.Empty<Control>();
            // Generic: every child of the items panel is a container to cascade (for a
            // ComboBox these are exactly the ComboBoxItem containers — historical behavior).
            return panel.Children.ToArray();
        }

        private void ResetItems()
        {
            foreach (var item in _cascadeItems)
                item.Opacity = 1.0;
            _cascadeItems = Array.Empty<Control>();
            _cascadePending = false;
        }

        // ---- Visuals ---------------------------------------------------------------

        private void ReadCurrent()
        {
            if (_root is not { } root)
                return;
            if (root.RenderTransform is ScaleTransform transform)
            {
                _x = transform.ScaleX;
                _y = transform.ScaleY;
            }
            _opacity = root.Opacity;
        }

        private void ApplyVisuals(double blurRadius)
        {
            if (_root is not { } root)
                return;

            if (root.RenderTransform is not ScaleTransform transform)
            {
                transform = new ScaleTransform(_x, _y);
                root.RenderTransform = transform;
            }
            transform.ScaleX = _x;
            transform.ScaleY = _y;
            root.Opacity = _opacity;

            if (blurRadius >= 0.5)
            {
                _effect ??= new BlurEffect();
                if (root.Effect != _effect)
                    root.Effect = _effect;
                _effect.Radius = blurRadius;
            }
            else if (_effect != null)
            {
                // Drop the effect entirely: no shader pass once the motion blur has dissipated.
                _effect = null;
                root.Effect = null;
            }
        }

        private void ResetToClosedPose()
        {
            _x = _xTarget = _profile.ClosedScaleX;
            _y = _yTarget = _profile.ClosedScaleY;
            _xv = _yv = 0.0;
            _opacity = _opacityFrom = _opacityTarget = 0.0;
        }

        // ---- Ticker ----------------------------------------------------------------

        private void StartTimer()
        {
            if (_ticker != null)
                return; // already running: targets are updated in place (live retargeting)
            _lastTick = SukiTicker.Timestamp;
            // Subscribe on the TopLevel of the animated root when it resolves (popup content
            // may live under its own PopupRoot), otherwise on the host control's window.
            Visual host = _root is { } root && TopLevel.GetTopLevel(root) is not null ? root : _host;
            _ticker = SukiTicker.Subscribe(host, _ => Tick());
        }

        private void StopTimer()
        {
            _ticker?.Dispose();
            _ticker = null;
        }

        private static double Lerp(double from, double to, double t) => from + (to - from) * t;
    }
}

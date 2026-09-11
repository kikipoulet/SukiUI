using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;

namespace SukiUI.Motion
{
    /// <summary>
    /// The popup lifecycle owner of the layer — the proven wiring of the old popup engine
    /// moved in as-is: resolves its parts through host-flavored resolvers (the template-part
    /// convention — the <c>Popup</c> named popupPart whose content root is named rootPart,
    /// with an optional items presenter inside it — or the caller's own rule for code-built
    /// popups), re-wires at every TemplateApplied and logical attach, parks a choreography
    /// played before the stage resolves (a code-built popup's content only gets its
    /// template once hosted), detects abnormal closes of the real popup (the engine is the
    /// only legitimate closer), keeps the Opened safety net, and runs ONE choreography at a
    /// time (open preempts close and back). The channels it exposes write through the
    /// CURRENT root — a re-applied template simply repoints them.
    /// </summary>
    public sealed class PopupHandle
    {
        private readonly TemplatedControl _host;
        private readonly Func<Popup?> _resolvePopup;
        private readonly Func<Popup?, Control?> _resolveRoot;
        private readonly Func<Control?, ItemsPresenter?> _resolveItems;
        private readonly Func<bool> _isHostOpen;

        private Popup? _popup;
        private Control? _root;
        private ItemsPresenter? _itemsPresenter;
        private Choreography? _current;
        private Choreography? _pending;   // parked while the stage does not resolve yet
        private bool _selfClosing;        // our own settle flip — not an abnormal close

        internal PopupHandle(
            TemplatedControl host,
            Func<Popup?> resolvePopup,
            Func<Popup?, Control?> resolveRoot,
            Func<Control?, ItemsPresenter?> resolveItems,
            Func<bool> isHostOpen)
        {
            _host = host;
            _resolvePopup = resolvePopup;
            _resolveRoot = resolveRoot;
            _resolveItems = resolveItems;
            _isHostOpen = isHostOpen;

            // The root resolves through the popup's content (the template walk does not
            // reach INSIDE the popup) — the same Surface vocabulary, a special resolver
            // reading the re-resolved _root field.
            Root = new Surface(host, () => _root);

            _host.TemplateApplied += OnTemplateApplied;
            _host.AttachedToLogicalTree += OnHostAttachedToLogicalTree;
            _host.DetachedFromVisualTree += OnHostDetached;
            ResolveParts(); // fail-safe: the template may already have been applied
        }

        /// <summary>The template-part flavor (ComboBox): the popup is a named part of the
        /// host's template, its content root a named child, the items a named presenter
        /// inside that root.</summary>
        internal PopupHandle(
            TemplatedControl host,
            string popupPart,
            string rootPart,
            string itemsPart,
            Func<bool> isHostOpen)
            : this(
                host,
                () => host.GetTemplateDescendants()
                    .OfType<Popup>()
                    .FirstOrDefault(p => p.Name == popupPart),
                popup => popup?.Child is { } content && content.Name == rootPart ? content : null,
                root => root?.GetLogicalDescendants()
                    .OfType<ItemsPresenter>()
                    .FirstOrDefault(i => i.Name == itemsPart),
                isHostOpen)
        { }

        /// <summary>The popup's animated root — the single surface vocabulary.</summary>
        internal Surface Root { get; }

        public bool IsOpen => _popup?.IsOpen ?? false;

        /// <summary>Re-opened behind the engine's back while the host wants open and nothing
        /// runs: the behavior plays its open choreography here (the Opened safety net).</summary>
        public Action? SafetyReopen { get; set; }

        /// <summary>The real popup closed abnormally (window teardown and the like) while the
        /// host still wants open: the behavior syncs the host state here.</summary>
        public event Action? AbnormalClose;

        /// <summary>Every non-settle termination of the running choreography (template
        /// re-apply, detach, abnormal close, disable): the behavior resets its cascade.</summary>
        public event Action? Stopped;

        // ---- choreography arena --------------------------------------------------------

        /// <summary>One choreography at a time — the single-state-machine rule of the old
        /// engine: starting one stops the running one frozen at its current pose (its
        /// channels stay active: the displacing members read pose + velocity).</summary>
        public void Play(Choreography choreography)
        {
            // Fail-safe of the old engine, re-run at every open-property change: the
            // template walk at TemplateApplied time can still see nothing (the template
            // root is not attached to the visual tree at that instant), so parts are
            // re-resolved here, where the tree is guaranteed ready.
            if (_popup is null || _root is null)
                ResolveParts();
            if (_root is null)
            {
                // The stage does not resolve yet — a code-built popup whose content only
                // gets its template once hosted (ContextMenu): park the choreography. It
                // starts at the TemplateApplied / logical attach that resolves the stage,
                // still before the first render of the open popup.
                StopCurrent();
                _pending = choreography;
                return;
            }
            StopCurrent();
            _current = choreography;
            choreography.Start(TickOwner);
        }

        public void StopCurrent()
        {
            _current?.Stop();
            _current = null;
        }

        /// <summary>
        /// The open choreography seed: its preamble flips the real popup open — a no-op
        /// property-wise on an already-open popup (the reopen mid-collapse). The fresh-open
        /// poses come from the members' Froms, pre-posed by the choreography BEFORE this
        /// preamble runs (no one-frame flash); in flight, the Froms are skipped and the
        /// members resume pose + velocity.
        /// </summary>
        public Choreography Show() => new(() =>
        {
            if (_popup is { } popup)
                popup.IsOpen = true;
        });

        /// <summary>Close settle: drop the blur and flip the real popup closed — the engine's
        /// only legitimate close. The collapsed pose itself is not written (the popup is
        /// already invisible; the next fresh open pre-poses it again).</summary>
        public void Hide()
        {
            Root.Blur.Write(0.0);
            if (_popup is { IsOpen: true } popup)
            {
                // Our own settle flip — the host syncs through its own closed path;
                // OnPopupPropertyChanged must not read it as an abnormal close (a
                // ContextMenu keeps IsOpen=true until its PopupClosed runs).
                _selfClosing = true;
                try { popup.IsOpen = false; }
                finally { _selfClosing = false; }
            }
        }

        /// <summary>Instant close (window deactivated): stop everything, rest the items,
        /// close the real popup — no animation, no settle actions. The channels are RESTED
        /// (not just stopped): the next fresh open's Froms pre-pose them whatever frozen
        /// state this close left them in.</summary>
        public void InstantClose() => StopAndRest(() =>
        {
            if (_popup is { IsOpen: true } popup)
                popup.IsOpen = false;
        });

        /// <summary>Stops the running choreography and rests every channel — the paths that
        /// abandon a popup outright (instant/abnormal close, template re-apply, detach,
        /// disable): unlike a preemption, the next From must pre-pose.</summary>
        private void StopAndRest(Action? after = null)
        {
            StopCurrent();
            Root.ScaleX.Rest();
            Root.ScaleY.Rest();
            Root.Opacity.Rest();
            Root.Blur.Rest();
            Stopped?.Invoke();
            after?.Invoke();
        }

        // The popup content may live under its own PopupRoot: subscribe on the animated
        // root when it resolves, otherwise on the host — the old StartTimer rule.
        private Visual TickOwner =>
            _root is { } root && TopLevel.GetTopLevel(root) is not null ? root : _host;

        /// <summary>The items of the cascade: every child of the items panel.</summary>
        public Control[] CollectItems()
        {
            if (_itemsPresenter is not { } presenter || presenter.Panel is not Panel panel)
                return Array.Empty<Control>();
            return panel.Children.ToArray();
        }

        // ---- parts & lifecycle ---------------------------------------------------------

        private void ResolveParts()
        {
            UnwirePopup();
            _root = null;
            _itemsPresenter = null;
            _popup = _resolvePopup();

            if (_popup is { } popup)
            {
                popup.PropertyChanged += OnPopupPropertyChanged;
                popup.Opened += OnPopupOpened;

                // The root and items resolve through the host flavor's own rule (inside
                // the popup's content for template popups, in the host's own template for
                // code-built ones). The logical tree is intact even while closed.
                if (_resolveRoot(popup) is { } root)
                {
                    _root = root;
                    _itemsPresenter = _resolveItems(root);
                }
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
        }

        private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) =>
            // A re-applied template aborts any running choreography over now-dead parts;
            // the fresh root rests until the next open's Froms pose it — or, for a parked
            // choreography, becomes the stage that starts it.
            StopAndRest(() => { ResolveParts(); StartPending(); });

        private void OnHostAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            // A code-built popup (ContextMenu) parents its host in only at first Open —
            // re-resolve there, and release any choreography parked on the missing stage.
            ResolveParts();
            StartPending();
        }

        private void StartPending()
        {
            if (_root is null || _pending is not { } choreography)
                return;
            _pending = null;
            _current = choreography;
            choreography.Start(TickOwner);
        }

        private void OnHostDetached(object? sender, VisualTreeAttachmentEventArgs e) =>
            StopAndRest(() =>
            {
                if (_popup is { IsOpen: true } popup)
                    popup.IsOpen = false;
            });

        private void OnPopupPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Popup.IsOpenProperty || e.NewValue is not false)
                return;
            if (_selfClosing)
                return; // our own settle flip: the host's closed path syncs the state
            // The engine is the only legitimate closer, so reaching here with the host still
            // open means an abnormal close (window teardown and the like): sync instantly.
            if (_isHostOpen())
                StopAndRest(() => AbnormalClose?.Invoke());
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            // Safety net: some path opened the popup without going through the host
            // property — still play the opening transition. Never while a choreography
            // is current: Opened also fires synchronously inside our own Show preamble,
            // and re-playing there would double-start the open and leak a subscription.
            if (_isHostOpen() && _current is null)
                SafetyReopen?.Invoke();
        }

        /// <summary>Disable (Enable=false): unwire everything and leave the popup functional
        /// without animation — the real popup follows the host state.</summary>
        public void Dispose() =>
            StopAndRest(() =>
            {
                _host.TemplateApplied -= OnTemplateApplied;
                _host.AttachedToLogicalTree -= OnHostAttachedToLogicalTree;
                _host.DetachedFromVisualTree -= OnHostDetached;
                _pending = null;
                UnwirePopup();
                if (_popup is { } popup)
                    popup.IsOpen = _isHostOpen();
            });
    }
}
